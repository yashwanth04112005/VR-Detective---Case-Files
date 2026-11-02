using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the <c>04_VerdictReveal</c> scene — a cinematic epilogue that slides in
/// evidence cards one by one, stamps the case as solved, and returns the player to the
/// main menu.
/// </summary>
public class VerdictRevealController : MonoBehaviour
{
    // ── Inspector Fields ─────────────────────────────────────────────────────

    [Header("Text")]
    [Tooltip("Large heading that reads 'CASE CLOSED'.")]
    public TMP_Text headingText;

    [Header("Evidence Cards")]
    [Tooltip("Assign the four evidence card GameObjects in order: Clock, Coffee, Computer, USB.")]
    public GameObject[] evidenceCards;

    [Header("Stamp & Buttons")]
    [Tooltip("The 'CASE SOLVED — SELF-STAGED DISAPPEARANCE' stamp graphic.")]
    public GameObject stampObject;

    [Tooltip("'Return to Main Menu' button revealed after the stamp.")]
    public GameObject returnButton;

    [Header("Animation Settings")]
    [Tooltip("Seconds to wait between each card sliding in.")]
    public float cardRevealInterval = 0.9f;

    [Tooltip("Distance in pixels each card travels from right before reaching its final position.")]
    public float cardSlideDistance = 300f;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // Hide all cards, stamp and button before the sequence begins.
        if (evidenceCards != null)
        {
            foreach (GameObject card in evidenceCards)
            {
                if (card != null) card.SetActive(false);
            }
        }

        if (stampObject  != null) stampObject.SetActive(false);
        if (returnButton != null) returnButton.SetActive(false);

        StartCoroutine(RevealCards());
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    /// <summary>
    /// Slides in each evidence card, then punches in the stamp and reveals the return button.
    /// </summary>
    private IEnumerator RevealCards()
    {
        yield return new WaitForSeconds(0.5f);

        if (evidenceCards != null)
        {
            foreach (GameObject card in evidenceCards)
            {
                if (card == null) continue;

                card.SetActive(true);

                RectTransform rect = card.GetComponent<RectTransform>();
                if (rect != null)
                {
                    // Slide-in runs concurrently; we advance to the next card after the interval.
                    StartCoroutine(SlideIn(rect));
                }

                yield return new WaitForSeconds(cardRevealInterval);
            }
        }

        yield return new WaitForSeconds(0.5f);

        // Show and animate the case-solved stamp.
        if (stampObject != null)
        {
            stampObject.SetActive(true);
            yield return StartCoroutine(PunchScale(stampObject.transform));
        }

        yield return new WaitForSeconds(0.8f);

        // Reveal the return button.
        if (returnButton != null) returnButton.SetActive(true);

        // Play the victory sting — null-safe if AudioManager is not present.
        AudioManager.Instance?.Play("sting_case_solved");
    }

    /// <summary>
    /// Slides a card's <see cref="RectTransform"/> from an offset position to its original
    /// anchored position over 0.4 seconds.
    /// </summary>
    private IEnumerator SlideIn(RectTransform rect)
    {
        if (rect == null) yield break;

        Vector2 destination = rect.anchoredPosition;
        Vector2 origin      = destination + new Vector2(cardSlideDistance, 0f);

        float elapsed  = 0f;
        float duration = 0.4f;

        rect.anchoredPosition = origin;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(origin, destination, elapsed / duration);
            yield return null;
        }

        // Guarantee exact final position.
        rect.anchoredPosition = destination;
    }

    /// <summary>
    /// Scales <paramref name="t"/> from (1.5, 1.5, 1) down to (1, 1, 1) over 0.5 seconds,
    /// creating a satisfying "punch" effect when the stamp appears.
    /// </summary>
    private IEnumerator PunchScale(Transform t)
    {
        if (t == null) yield break;

        Vector3 startScale = new Vector3(1.5f, 1.5f, 1f);
        Vector3 endScale   = Vector3.one;

        float elapsed  = 0f;
        float duration = 0.5f;

        t.localScale = startScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(startScale, endScale, elapsed / duration);
            yield return null;
        }

        t.localScale = endScale;
    }

    // ── Button Callback ───────────────────────────────────────────────────────

    /// <summary>
    /// Called by the "Return to Main Menu" button's OnClick event.
    /// Resets game state and transitions back to the main menu.
    /// </summary>
    public void ReturnToMainMenu()
    {
        // Null-conditional — safe if GameStateManager doesn't exist yet.
        GameStateManager.Instance?.ResetState();

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene("00_MainMenu");
        }
        else
        {
            SceneManager.LoadScene("00_MainMenu");
        }
    }
}
