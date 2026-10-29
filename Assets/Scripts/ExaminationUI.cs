using System.Collections;
using UnityEngine;
using TMPro;

public class ExaminationUI : MonoBehaviour
{
    [Header("Examination Panel")]
    public GameObject examinationPanel;
    public TMP_Text examinationText;

    [Header("Display Settings")]
    public float displayDuration = 5f;

    private Coroutine hideCoroutine;

    private void Start()
    {
        HidePanel();
    }

    public void ShowClue(string clueName, string description)
    {
        if (examinationPanel != null)
        {
            examinationPanel.SetActive(true);
        }

        if (examinationText != null)
        {
            examinationText.text = description;
        }

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        HidePanel();
        hideCoroutine = null;
    }

    public void HidePanel()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        if (examinationPanel != null)
        {
            examinationPanel.SetActive(false);
        }
    }
}