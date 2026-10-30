using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CaseConclusionController : MonoBehaviour
{
    [Header("Answer Buttons")]
    public Button answerButton1;
    public Button answerButton2;
    public Button answerButton3;
    public Button submitButton;

    [Header("Verdict")]
    public TMP_Text verdictText;

    [Header("Navigation")]
    public Button mainMenuButton;

    private int selectedAnswer = 0;

    private void Start()
    {
        verdictText.text = "Choose your deduction.";

        answerButton1.onClick.AddListener(() => SelectAnswer(1));
        answerButton2.onClick.AddListener(() => SelectAnswer(2));
        answerButton3.onClick.AddListener(() => SelectAnswer(3));

        submitButton.onClick.AddListener(SubmitDeduction);
        mainMenuButton.onClick.AddListener(ReturnToMainMenu);
    }

    private void SelectAnswer(int answer)
    {
        selectedAnswer = answer;
        verdictText.text = "Deduction selected. Submit your answer.";
    }

    private void SubmitDeduction()
    {
        if (selectedAnswer == 0)
        {
            verdictText.text = "Please select an answer first.";
            return;
        }

        if (selectedAnswer == 3)
        {
            verdictText.text = "CASE SOLVED! You uncovered the truth about Dr. Vale's disappearance.";
        }
        else
        {
            verdictText.text = "CASE UNRESOLVED. Review the evidence and try again.";
        }
    }

    private void ReturnToMainMenu()
    {
        SceneManager.LoadScene("00_MainMenu");
    }
}