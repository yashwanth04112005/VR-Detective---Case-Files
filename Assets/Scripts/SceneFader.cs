using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton that wraps every scene transition in a black fade-out / fade-in.
/// Attach to a GameObject in the first scene. A full-screen black Image with a CanvasGroup
/// component should be assigned to <see cref="fadeCanvasGroup"/>. The Canvas must be set to
/// Screen Space - Overlay with a Sort Order of 999 so it renders on top of everything.
/// </summary>
public class SceneFader : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────────
    public static SceneFader Instance { get; private set; }

    // ── Inspector Fields ─────────────────────────────────────────────────────

    [Tooltip("CanvasGroup on the full-screen black overlay Image.")]
    public CanvasGroup fadeCanvasGroup;

    [Tooltip("Total seconds for each fade-out or fade-in pass.")]
    public float fadeDuration = 0.5f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Destroy duplicate — only one SceneFader may exist at a time.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Make sure the overlay starts fully transparent and non-blocking.
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Fades the screen to black, loads <paramref name="sceneName"/>, then fades back in.
    /// </summary>
    /// <param name="sceneName">The exact name of the scene to load (must be in Build Settings).</param>
    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    // ── Private Coroutines ────────────────────────────────────────────────────

    /// <summary>
    /// Full transition: fade out → load scene → fade in.
    /// </summary>
    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        // Block player input while transitioning.
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = true;
        }

        // Fade out (0 → 1).
        yield return StartCoroutine(FadeAlpha(0f, 1f));

        // Load the requested scene.
        SceneManager.LoadScene(sceneName);

        // Fade in (1 → 0).
        yield return StartCoroutine(FadeAlpha(1f, 0f));

        // Restore interactivity once the fade-in is complete.
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// Smoothly transitions <see cref="fadeCanvasGroup"/>'s alpha from
    /// <paramref name="from"/> to <paramref name="to"/> over <see cref="fadeDuration"/> seconds.
    /// </summary>
    private IEnumerator FadeAlpha(float from, float to)
    {
        if (fadeCanvasGroup == null) yield break;

        float elapsed = 0f;
        fadeCanvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        // Guarantee we land exactly on the target value.
        fadeCanvasGroup.alpha = to;
    }
}
