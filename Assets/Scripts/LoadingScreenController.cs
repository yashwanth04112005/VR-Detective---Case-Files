using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the <c>05_LoadingInterstitial</c> scene.
/// Asynchronously loads <see cref="targetScene"/> in the background while displaying
/// an atmospheric loading screen with scrolling crime-scene tape and fade-in text.
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    // ── Inspector Fields ─────────────────────────────────────────────────────

    [Tooltip("Name of the scene to load in the background.")]
    public string targetScene = "01_CaseRoom";

    [Header("Text Elements")]
    [Tooltip("Displays the case-file identifier, e.g. 'CASE FILE #1147'.")]
    public TMP_Text caseFileText;

    [Tooltip("Displays the case subtitle, e.g. 'The Vale Disappearance'.")]
    public TMP_Text subtitleText;

    [Tooltip("Animated loading label, e.g. 'Loading Crime Scene...'")]
    public TMP_Text loadingText;

    [Header("Crime Tape")]
    [Tooltip("RectTransform of the scrolling crime-tape UI image.")]
    public RectTransform crimeTapeRect;

    [Tooltip("Horizontal scroll speed in pixels per second (negative direction).")]
    public float tapeScrollSpeed = 80f;

    [Header("Timing")]
    [Tooltip("Minimum seconds to display this screen before activating the loaded scene.")]
    public float minimumDisplayTime = 2.5f;

    // ── Private state ─────────────────────────────────────────────────────────

    /// <summary>Base label for the animated dots effect.</summary>
    private const string LoadingBase = "Loading Crime Scene";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // Hide all text elements immediately (alpha, not SetActive, so layout is preserved).
        if (caseFileText != null) caseFileText.alpha = 0f;
        if (subtitleText != null) subtitleText.alpha = 0f;
        if (loadingText  != null) loadingText.alpha  = 0f;

        StartCoroutine(RunLoadingSequence());
    }

    private void Update()
    {
        // Scroll crime tape continuously to the left.
        if (crimeTapeRect == null) return;

        crimeTapeRect.anchoredPosition += Vector2.left * (tapeScrollSpeed * Time.deltaTime);

        // Loop back when the tape has scrolled far enough off-screen.
        if (crimeTapeRect.anchoredPosition.x < -1200f)
        {
            crimeTapeRect.anchoredPosition = new Vector2(0f, crimeTapeRect.anchoredPosition.y);
        }
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    /// <summary>
    /// Main loading sequence:
    /// kicks off background loading, staggers text fade-ins, then activates the scene
    /// once both the minimum display time and Unity's load threshold are reached.
    /// </summary>
    private IEnumerator RunLoadingSequence()
    {
        // Begin loading in the background without immediately switching scenes.
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetScene);
        asyncOp.allowSceneActivation = false;

        // ── Staggered text fade-ins ──────────────────────────────────────────
        StartCoroutine(FadeInText(caseFileText, 0.8f));
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(FadeInText(subtitleText, 0.8f));
        yield return new WaitForSeconds(0.5f);

        StartCoroutine(FadeInText(loadingText, 0.4f));

        // ── Loading loop with animated dots ─────────────────────────────────
        float elapsed = 0f;
        int dotCount = 0;
        float dotTimer = 0f;
        const float dotInterval = 0.4f; // seconds between dot increments

        while (!asyncOp.isDone)
        {
            elapsed   += Time.deltaTime;
            dotTimer  += Time.deltaTime;

            // Cycle through one, two, and three trailing dots.
            if (dotTimer >= dotInterval)
            {
                dotTimer = 0f;
                dotCount = (dotCount % 3) + 1;

                if (loadingText != null)
                {
                    string dots = new string('.', dotCount);
                    loadingText.text = LoadingBase + dots;
                }
            }

            // Once both conditions are satisfied, allow Unity to switch scenes.
            if (elapsed >= minimumDisplayTime && asyncOp.progress >= 0.9f)
            {
                asyncOp.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Fades a TMP_Text element's alpha from 0 to 1 over <paramref name="duration"/> seconds.
    /// </summary>
    private IEnumerator FadeInText(TMP_Text textObj, float duration)
    {
        if (textObj == null) yield break;

        float elapsed = 0f;
        textObj.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            textObj.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        textObj.alpha = 1f;
    }
}
