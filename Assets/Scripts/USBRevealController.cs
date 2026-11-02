using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the <c>03_USBReveal</c> scene.
/// Presents a digital terminal that reveals the contents of the USB drive one fragment at a
/// time using a typewriter effect, then surfaces a "CLOSE FILE" button that leads to the
/// case conclusion scene.
/// </summary>
public class USBRevealController : MonoBehaviour
{
    // ── Inspector Fields ─────────────────────────────────────────────────────

    [Header("Terminal Header")]
    [Tooltip("TMP_Text that shows the decryption header.")]
    public TMP_Text headerText;

    [Header("File Fragments")]
    [Tooltip("TMP_Text for FAREWELL.txt")]
    public TMP_Text fragment1Text;

    [Tooltip("TMP_Text for ACCOUNTS.dat")]
    public TMP_Text fragment2Text;

    [Tooltip("TMP_Text for PASSPORT.img")]
    public TMP_Text fragment3Text;

    [Header("UI Controls")]
    [Tooltip("Gold 'CLOSE FILE' button — hidden until the sequence finishes.")]
    public GameObject proceedButton;

    [Header("Typewriter Settings")]
    [Tooltip("Delay in seconds between each character being appended.")]
    public float typewriterSpeed = 0.03f;

    // ── Hardcoded Content ─────────────────────────────────────────────────────

    private const string HeaderContent =
        "> DECRYPTING USB_DRIVE_01...\n> ACCESS GRANTED\n> 3 FILES FOUND";

    private const string Fragment1Content =
        "[ FAREWELL.txt ]\n" +
        "\"If you're reading this, I made it out.\n" +
        "I'm sorry for the confusion I left behind.\n" +
        "By the time you find this, I'll be long gone.\n" +
        "Don't look for me. — V\"";

    private const string Fragment2Content =
        "[ ACCOUNTS.dat ]\n" +
        "Cayman Islands — Account #4471-B\n" +
        "Transfer Amount: USD 2,400,000\n" +
        "Transaction Time: 11:43 PM, Nov 14\n" +
        "Status: CLEARED";

    private const string Fragment3Content =
        "[ PASSPORT.img ]\n" +
        "New identity confirmed.\n" +
        "Issued under: R. CALLOWAY\n" +
        "Photo: MATCHES VALE\n" +
        "Issued: 3 weeks prior to disappearance";

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Start()
    {
        // Hide all fragment GameObjects and the proceed button before the sequence starts.
        if (fragment1Text != null) fragment1Text.gameObject.SetActive(false);
        if (fragment2Text != null) fragment2Text.gameObject.SetActive(false);
        if (fragment3Text != null) fragment3Text.gameObject.SetActive(false);
        if (proceedButton != null) proceedButton.SetActive(false);

        StartCoroutine(RevealSequence());
    }

    // ── Coroutines ────────────────────────────────────────────────────────────

    /// <summary>
    /// Orchestrates the full reveal: header → fragment 1 → fragment 2 → fragment 3 → button.
    /// </summary>
    private IEnumerator RevealSequence()
    {
        // Brief pause before anything appears.
        yield return new WaitForSeconds(0.5f);

        // Typewriter the terminal header.
        yield return StartCoroutine(TypewriterEffect(headerText, HeaderContent));

        yield return new WaitForSeconds(1.0f);

        // Fragment 1 — FAREWELL.txt
        if (fragment1Text != null) fragment1Text.gameObject.SetActive(true);
        yield return StartCoroutine(TypewriterEffect(fragment1Text, Fragment1Content));

        yield return new WaitForSeconds(0.8f);

        // Fragment 2 — ACCOUNTS.dat
        if (fragment2Text != null) fragment2Text.gameObject.SetActive(true);
        yield return StartCoroutine(TypewriterEffect(fragment2Text, Fragment2Content));

        yield return new WaitForSeconds(0.8f);

        // Fragment 3 — PASSPORT.img
        if (fragment3Text != null) fragment3Text.gameObject.SetActive(true);
        yield return StartCoroutine(TypewriterEffect(fragment3Text, Fragment3Content));

        yield return new WaitForSeconds(1.0f);

        // Reveal the proceed button and play an audio cue if AudioManager exists.
        if (proceedButton != null) proceedButton.SetActive(true);

        // Null-conditional call — safe if AudioManager isn't present in the scene.
        AudioManager.Instance?.Play("sfx_clue_found");
    }

    /// <summary>
    /// Reveals <paramref name="fullText"/> one character at a time into <paramref name="textObj"/>.
    /// </summary>
    private IEnumerator TypewriterEffect(TMP_Text textObj, string fullText)
    {
        if (textObj == null) yield break;

        textObj.text = string.Empty;

        foreach (char c in fullText)
        {
            textObj.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    // ── Button Callback ───────────────────────────────────────────────────────

    /// <summary>
    /// Called by the "CLOSE FILE" button's OnClick event.
    /// Transitions to the case conclusion scene via <see cref="SceneFader"/> if available,
    /// otherwise falls back to a direct scene load.
    /// </summary>
    public void ProceedToConclusion()
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene("03_DetectiveBoard");
        }
        else
        {
            SceneManager.LoadScene("03_DetectiveBoard");
        }
    }
}

