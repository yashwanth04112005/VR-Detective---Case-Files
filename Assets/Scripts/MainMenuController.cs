using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the Main Menu scene:
///   - PlayGame()     — starts the game by loading the loading interstitial.
///   - ShowHowToPlay() / HideHowToPlay() — toggles the How To Play panel.
///   - QuitGame()     — exits the application.
///
/// All scene transitions prefer SceneFader.FadeToScene() when the singleton
/// is present and fall back to a direct SceneManager.LoadScene() otherwise.
/// </summary>
public class MainMenuController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields (all existing fields preserved)
    // -------------------------------------------------------------------------

    [Header("How To Play Panel")]
    public GameObject howToPlayPanel;

    // -------------------------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts a new playthrough by loading the loading interstitial scene.
    /// Fades out if SceneFader is present; direct load otherwise.
    /// </summary>
    public void PlayGame()
    {
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene("01_CaseRoom");
        }
        else
        {
            SceneManager.LoadScene("01_CaseRoom");
        }
    }

    /// <summary>Shows the How To Play overlay panel.</summary>
    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("[MainMenuController] HowToPlayPanel is not assigned.");
        }
    }

    /// <summary>Hides the How To Play overlay panel.</summary>
    public void HideHowToPlay()
    {
        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }
    }

    /// <summary>Exits the application (no-op in the Unity Editor).</summary>
    public void QuitGame()
    {
        Debug.Log("[MainMenuController] Quit button clicked.");
        Application.Quit();
    }
}
