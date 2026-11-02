using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls the Case Conclusion scene UI:
///   - Presents a four-question deduction quiz using the evidence board.
///   - Scores each answer and explains how the evidence supports the result.
///   - Reveals the final case report after the quiz is complete.
///   - Offers a "Return to Main Menu" button that resets global game state.
/// </summary>
public class CaseConclusionController : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields (all existing fields preserved)
    // -------------------------------------------------------------------------

    [Header("Answer Buttons")]
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button submitButton;

    [Header("Verdict")]
    public TMP_Text verdictText;

    [Header("Navigation")]
    public Button mainMenuButton;

    // -------------------------------------------------------------------------
    // Visual Feedback Colours
    // -------------------------------------------------------------------------

    [Header("Button Colours")]
    /// <summary>Colour applied to the currently selected answer button (gold).</summary>
    public Color selectedColor  = new Color(0.8f, 0.6f, 0.1f, 1f);

    /// <summary>Default resting colour for un-selected answer buttons (dark grey).</summary>
    public Color defaultColor   = new Color(0.15f, 0.15f, 0.15f, 1f);

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>The currently chosen answer index (1, 2, or 3). 0 means nothing selected.</summary>
    private int selectedAnswer = 0;
    private int questionIndex;
    private int score;
    private bool answerSubmitted;
    private TMP_Text questionText;
    private TMP_Text progressText;

    private readonly string[] questions =
    {
        "What does 11:47 mean alongside the Archive access record?",
        "What does the financial ledger reveal about the transfer?",
        "Why is the passport scan on the USB important?",
        "Which explanation best fits the complete evidence board?"
    };

    private readonly string[][] choices =
    {
        new[] { "It marks the moment a second badge entered the office.", "Vale used the service lift at 11:42; the office clock stopped five minutes later.", "The archive clock had been broken for weeks." },
        new[] { "A payment made only after Vale disappeared.", "A transfer prepared weeks earlier to the offshore account named on the USB.", "No money left Vale's accounts." },
        new[] { "It suggests Vale prepared a new identity and planned an exit.", "It proves that Vale was taken by force.", "It is an unrelated archive document." },
        new[] { "Vale was accidentally trapped in the archive.", "A colleague stole the money and fabricated every clue.", "Dr. Vale staged his disappearance with funds and a new identity." }
    };

    // Choice indices are one-based to match the existing button selection code.
    private readonly int[] correctChoices = { 2, 2, 1, 3 };

    private readonly string[] explanations =
    {
        "The service access record places Vale in the archive at 11:42 PM. The stopped office clock fits a five-minute timeline, and no forced entry was recorded.",
        "The $2.4 million transfer was prepared three weeks earlier. Its destination matches the offshore account listed on the USB.",
        "The passport scan, farewell note, and account records point to a prepared departure under a new identity.",
        "The archive log, transfer timing, and USB contents all support a planned disappearance rather than an abduction."
    };

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        ApplyVisualPolish();

        TMP_Text[] labels = FindObjectsByType<TMP_Text>();
        foreach (TMP_Text label in labels)
        {
            if (label.text.ToLowerInvariant().Contains("what do you believe"))
            {
                questionText = label;
                break;
            }
        }
        GameObject progressObject = GameObject.Find("CaseStatusText");
        progressText = progressObject != null ? progressObject.GetComponent<TMP_Text>() : null;
        GameObject oldEvidencePanel = GameObject.Find("EvidencePanel");
        if (oldEvidencePanel != null) oldEvidencePanel.SetActive(false);
        GameObject oldEvidenceDescription = GameObject.Find("EvidenceDescription");
        if (oldEvidenceDescription != null) oldEvidenceDescription.SetActive(false);
        string[] oldCardObjects =
        {
            "ClockEvidenceCard", "CoffeeEvidenceCard", "ComputerEvidenceCard", "BookEvidenceCard",
            "ClockClueTitle", "ClockClueDescription", "CoffeeClueTitle", "CoffeeClueDescription",
            "ComputerClueTitle", "ComputerClueDescription", "BookClueTitle", "BookClueDescription"
        };
        foreach (string objectName in oldCardObjects)
        {
            GameObject oldCardPart = GameObject.Find(objectName);
            if (oldCardPart != null) oldCardPart.SetActive(false);
        }
        GameObject headingObject = GameObject.Find("EvidenceHeading");
        TMP_Text heading = headingObject != null ? headingObject.GetComponent<TMP_Text>() : null;
        if (heading != null) heading.text = "FINAL DEDUCTION";
        ConfigureQuizLayout();

        // Submit is disabled until the player picks an answer.
        submitButton.interactable = false;

        // Wire button listeners.
        answerButton1.onClick.AddListener(() => SelectAnswer(1));
        answerButton2.onClick.AddListener(() => SelectAnswer(2));
        answerButton3.onClick.AddListener(() => SelectAnswer(3));

        submitButton.onClick.AddListener(SubmitDeduction);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);

        // Make sure all buttons start in the default visual state.
        ResetButtonVisuals();
        PrepareQuestion();
    }

    private void ConfigureQuizLayout()
    {
        PositionRect(questionText != null ? questionText.rectTransform : null, new Vector2(0, 130), new Vector2(1420, 72));
        PositionRect(progressText != null ? progressText.rectTransform : null, new Vector2(0, 60), new Vector2(1200, 30));
        PositionRect(answerButton1 != null ? answerButton1.GetComponent<RectTransform>() : null, new Vector2(0, 8), new Vector2(1120, 54));
        PositionRect(answerButton2 != null ? answerButton2.GetComponent<RectTransform>() : null, new Vector2(0, -58), new Vector2(1120, 54));
        PositionRect(answerButton3 != null ? answerButton3.GetComponent<RectTransform>() : null, new Vector2(0, -124), new Vector2(1120, 54));
        PositionRect(submitButton != null ? submitButton.GetComponent<RectTransform>() : null, new Vector2(0, -184), new Vector2(330, 52));
        PositionRect(verdictText != null ? verdictText.rectTransform : null, new Vector2(0, -250), new Vector2(1080, 88));
        PositionRect(mainMenuButton != null ? mainMenuButton.GetComponent<RectTransform>() : null, new Vector2(0, -330), new Vector2(250, 44));
    }

    private void PositionRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        if (rect == null) return;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    /// <summary>Applies a consistent evidence-dossier palette while keeping the existing scene layout and flow.</summary>
    private void ApplyVisualPolish()
    {
        Color ink = new Color(0.025f, 0.045f, 0.075f, 1f);
        Color panel = new Color(0.075f, 0.12f, 0.18f, 1f);
        Color gold = new Color(0.94f, 0.75f, 0.35f, 1f);
        Color paper = new Color(0.89f, 0.91f, 0.91f, 1f);
        Color muted = new Color(0.57f, 0.69f, 0.75f, 1f);

        Image background = FindImage("BackgroundImage");
        if (background != null) background.color = ink;
        Image evidencePanel = FindImage("EvidencePanel");
        if (evidencePanel != null) evidencePanel.color = panel;
        if (evidencePanel != null)
        {
            TMP_Text[] evidenceLabels = evidencePanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (TMP_Text label in evidenceLabels)
            {
                label.color = paper;
                label.fontSize = Mathf.Max(label.fontSize, 14f);
            }
        }

        StyleLabel("CaseTitle", gold, 34, FontStyles.Bold);
        StyleLabel("EvidenceHeading", gold, 23, FontStyles.Bold);
        StyleLabel("CaseStatusText", muted, 16, FontStyles.Bold);
        TMP_Text[] allLabels = GetComponentInParent<Canvas>() != null
            ? GetComponentInParent<Canvas>().GetComponentsInChildren<TMP_Text>(true)
            : FindObjectsByType<TMP_Text>();
        foreach (TMP_Text label in allLabels)
            if (label.text.ToLowerInvariant().Contains("what do you believe"))
            {
                label.color = paper;
                label.fontSize = 20;
                label.fontStyle = FontStyles.Bold;
            }
        if (verdictText != null)
        {
            verdictText.color = paper;
            verdictText.fontSize = 17;
            verdictText.fontStyle = FontStyles.Bold;
            verdictText.alignment = TextAlignmentOptions.Center;
            verdictText.textWrappingMode = TextWrappingModes.Normal;
        }

        StyleButton(answerButton1, panel, paper, 21);
        StyleButton(answerButton2, panel, paper, 21);
        StyleButton(answerButton3, panel, paper, 21);
        StyleButton(submitButton, gold, ink, 18);
        StyleButton(mainMenuButton, new Color(0.10f, 0.15f, 0.21f, 1f), muted, 15);

        // Make the selected-answer feedback match the dossier's warm accent.
        defaultColor = panel;
        selectedColor = new Color(0.68f, 0.48f, 0.16f, 1f);
    }

    private Image FindImage(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.GetComponent<Image>() : null;
    }

    private void StyleLabel(string objectName, Color color, float size, FontStyles style)
    {
        GameObject target = GameObject.Find(objectName);
        TMP_Text label = target != null ? target.GetComponent<TMP_Text>() : null;
        if (label == null) return;
        label.color = color;
        label.fontSize = size;
        label.fontStyle = style;
        label.textWrappingMode = TextWrappingModes.Normal;
    }

    private void StyleButton(Button button, Color background, Color foreground, float fontSize)
    {
        if (button == null) return;
        Image image = button.GetComponent<Image>();
        if (image != null) image.color = background;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 0.9f, 0.68f, 1f);
        colors.selectedColor = Color.white;
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.5f);
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.color = foreground;
            label.fontSize = fontSize;
            label.fontStyle = FontStyles.Bold;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    // -------------------------------------------------------------------------
    // Answer Selection
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the player clicks one of the three answer buttons.
    /// Applies gold highlight to the chosen button and resets the others.
    /// Enables the Submit button.
    /// </summary>
    /// <param name="answer">Button index: 1, 2, or 3.</param>
    private void SelectAnswer(int answer)
    {
        if (answerSubmitted) return;
        selectedAnswer = answer;
        verdictText.text = "Answer selected. Check your reasoning.";

        // --- Reset all three buttons to default appearance ---
        ResetButtonVisuals();

        // --- Highlight the chosen button ---
        Button chosenButton = GetButtonByIndex(answer);
        if (chosenButton != null)
        {
            // Colour feedback.
            Image img = chosenButton.GetComponent<Image>();
            if (img != null)
            {
                img.color = selectedColor;
            }

            // Scale feedback — chosen button slightly enlarged.
            chosenButton.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        }

        // Enable submission once an answer has been chosen.
        submitButton.interactable = true;
    }

    // -------------------------------------------------------------------------
    // Submission / Verdict
    // -------------------------------------------------------------------------

    /// <summary>
    /// Evaluates the selected answer:
    ///   - Answer 3 is correct → fade to the Verdict Reveal scene.
    ///   - Any other answer    → display a retry message.
    /// </summary>
    private void SubmitDeduction()
    {
        if (answerSubmitted)
        {
            questionIndex++;
            if (questionIndex >= questions.Length)
            {
                FinishQuiz();
                return;
            }
            PrepareQuestion();
            return;
        }

        if (selectedAnswer == 0)
        {
            verdictText.text = "Please select an answer first.";
            return;
        }

        bool correct = selectedAnswer == correctChoices[questionIndex];
        if (correct)
        {
            score++;
            AudioManager.Instance?.PlayOneShot("sfx_clue_found");
            verdictText.text = "SUPPORTED BY THE EVIDENCE\n" + explanations[questionIndex];
        }
        else
        {
            AudioManager.Instance?.PlayOneShot("sting_unresolved");
            verdictText.text = "NOT SUPPORTED\n" + explanations[questionIndex];
        }

        answerSubmitted = true;
        foreach (Button button in new[] { answerButton1, answerButton2, answerButton3 }) button.interactable = false;
        SetButtonText(submitButton, questionIndex == questions.Length - 1 ? "VIEW CASE RESULT" : "NEXT QUESTION  →");
        if (progressText != null) progressText.text = $"QUESTION {questionIndex + 1} OF {questions.Length}  •  SCORE {score}";
    }

    private void PrepareQuestion()
    {
        selectedAnswer = 0;
        answerSubmitted = false;
        if (questionText != null) questionText.text = questions[questionIndex];
        if (progressText != null) progressText.text = $"QUESTION {questionIndex + 1} OF {questions.Length}  •  SCORE {score}";
        verdictText.text = "Choose the answer best supported by the evidence board.";
        SetButtonText(answerButton1, choices[questionIndex][0]);
        SetButtonText(answerButton2, choices[questionIndex][1]);
        SetButtonText(answerButton3, choices[questionIndex][2]);
        foreach (Button button in new[] { answerButton1, answerButton2, answerButton3 }) button.interactable = true;
        ResetButtonVisuals();
        submitButton.interactable = false;
        SetButtonText(submitButton, "CHECK ANSWER");
    }

    private void FinishQuiz()
    {
        GameStateManager.Instance?.SetDeductionResult(score, questions.Length);
        if (score == questions.Length) AudioManager.Instance?.PlayOneShot("sting_case_solved");
        else AudioManager.Instance?.PlayOneShot("sting_unresolved");

        foreach (Button button in new[] { answerButton1, answerButton2, answerButton3, submitButton })
            button.gameObject.SetActive(false);

        string rating = score == 4 ? "LEAD DETECTIVE" : score >= 3 ? "CASE ANALYST" : score >= 2 ? "FIELD INVESTIGATOR" : "CASE REVIEW REQUIRED";
        if (questionText != null) questionText.text = "FINAL CASE REPORT";
        if (progressText != null) progressText.text = $"FINAL SCORE  •  {score} / {questions.Length}  •  {rating}";
        verdictText.text =
            $"DEDUCTION SCORE: {score} / {questions.Length}  •  {rating}\n\n" +
            "DR. VALE PLANNED HIS OWN DISAPPEARANCE.\n" +
            "The archive records place him on the service lift shortly before the office clock stopped. The early offshore transfer, farewell note, and passport scan show a prepared exit under a new identity.";
    }

    private void SetButtonText(Button button, string value)
    {
        if (button == null) return;
        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null) label.text = value;
    }

    // -------------------------------------------------------------------------
    // Navigation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resets all global game state and navigates back to the main menu.
    /// </summary>
    private void ReturnToMainMenu()
    {
        // Clear persistent evidence data so a new run starts fresh.
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

    // -------------------------------------------------------------------------
    // Visual Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resets all three answer buttons to the default dark-grey colour and
    /// normal (1, 1, 1) scale.
    /// </summary>
    private void ResetButtonVisuals()
    {
        Button[] buttons = { answerButton1, answerButton2, answerButton3 };

        foreach (Button btn in buttons)
        {
            if (btn == null) continue;

            Image img = btn.GetComponent<Image>();
            if (img != null)
            {
                img.color = defaultColor;
            }

            btn.transform.localScale = Vector3.one;
        }
    }

    /// <summary>
    /// Maps an integer index (1–3) to the corresponding answer Button reference.
    /// </summary>
    private Button GetButtonByIndex(int index)
    {
        switch (index)
        {
            case 1:  return answerButton1;
            case 2:  return answerButton2;
            case 3:  return answerButton3;
            default: return null;
        }
    }
}

