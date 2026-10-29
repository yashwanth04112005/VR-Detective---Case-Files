using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class DetectiveJournal : MonoBehaviour
{
    [Header("Journal UI")]
    public GameObject journalPanel;
    public TMP_Text evidenceListText;

    private readonly Dictionary<string, string> discoveredEvidence =
        new Dictionary<string, string>();

    public int EvidenceCount => discoveredEvidence.Count;

    public int TotalEvidenceRequired = 4;
    private void Start()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }

        UpdateEvidenceList();
    }

    public void AddEvidence(string clueName, string description)
    {
        if (string.IsNullOrWhiteSpace(clueName))
        {
            return;
        }

        // Prevent the same clue from being recorded twice.
        if (discoveredEvidence.ContainsKey(clueName))
        {
            return;
        }

        discoveredEvidence.Add(clueName, description);
        UpdateEvidenceList();

        Debug.Log("New evidence recorded: " + clueName);
    }

    public void ToggleJournal()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(!journalPanel.activeSelf);
        }
    }

    public void CloseJournal()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }
    }

    private void UpdateEvidenceList()
    {
        if (evidenceListText == null)
        {
            return;
        }

        if (discoveredEvidence.Count == 0)
{
    evidenceListText.text =
        "No evidence collected yet.\n\n" +
        "Examine objects around the office to discover clues.";

    return;
}
        StringBuilder text = new StringBuilder();
        

        foreach (KeyValuePair<string, string> clue in discoveredEvidence)
        {
            text.AppendLine(clue.Value);
            text.AppendLine();
        }

        evidenceListText.text = text.ToString();
    }
    public string GetEvidenceProgress()
    {
    return "Evidence Collected: " + EvidenceCount +
           " / " + TotalEvidenceRequired;
    }
}