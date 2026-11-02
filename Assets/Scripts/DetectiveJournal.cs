using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the in-game detective journal:
///   - Stores discovered evidence in a dictionary (duplicate-proof).
///   - Updates the journal UI text whenever new evidence is added.
///   - Exposes an event so other systems can react to new discoveries.
///   - Handles toggling the journal panel open/closed.
/// 
/// NOTE: GameStateManager.RegisterEvidence() is called directly by
/// PlayerInteraction.cs, NOT from here, to avoid double-counting.
/// This class stays focused purely on journal UI and evidence storage.
/// </summary>
public class DetectiveJournal : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields (all existing fields preserved)
    // -------------------------------------------------------------------------

    [Header("Journal UI")]
    public GameObject journalPanel;
    public TMP_Text evidenceListText;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>
    /// Dictionary mapping clue name → description.
    /// Using the GameObject name as the key prevents the same object from being
    /// logged twice even if the player examines it repeatedly.
    /// </summary>
    private readonly Dictionary<string, string> discoveredEvidence =
        new Dictionary<string, string>();

    // -------------------------------------------------------------------------
    // Properties
    // -------------------------------------------------------------------------

    /// <summary>Number of unique clues currently recorded in the journal.</summary>
    public int EvidenceCount => discoveredEvidence.Count;

    /// <summary>Returns true when the journal panel is visible.</summary>
    public bool IsJournalOpen => journalPanel != null && journalPanel.activeSelf;

    /// <summary>
    /// Total evidence required, kept here so UI elements wired to this
    /// component can read it directly without touching GameStateManager.
    /// </summary>
    public int TotalEvidenceRequired = 5;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fired after a new piece of evidence is successfully added to the journal.
    /// Listeners can use this to animate the journal icon, play audio, etc.
    /// </summary>
    public event System.Action OnEvidenceAdded;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        // Always start with the journal hidden.
        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }

        // Populate with the "empty" placeholder text.
        UpdateEvidenceList();
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Records a clue in the journal and refreshes the evidence list UI.
    /// Silently ignored if the clue name is blank or already recorded.
    /// </summary>
    /// <param name="clueName">The GameObject name used as the unique key.</param>
    /// <param name="description">The narrative text to display in the journal.</param>
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

        Debug.Log("[DetectiveJournal] New evidence recorded: " + clueName);

        // Notify subscribers (e.g., journal icon animation, audio cue).
        OnEvidenceAdded?.Invoke();
    }

    /// <summary>Toggles the journal panel open or closed.</summary>
    public void ToggleJournal()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(!journalPanel.activeSelf);
        }
    }

    /// <summary>Forces the journal panel closed.</summary>
    public void CloseJournal()
    {
        if (journalPanel != null)
        {
            journalPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Returns a short progress string, e.g. "Evidence Collected: 3 / 4".
    /// Used by PlayerInteraction as a fallback when GameStateManager is absent.
    /// </summary>
    public string GetEvidenceProgress()
    {
        return "Evidence Collected: " + EvidenceCount + " / " + TotalEvidenceRequired;
    }

    // -------------------------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Rebuilds the TMP evidence list from the current dictionary contents.
    /// Displays a placeholder prompt when nothing has been found yet.
    /// </summary>
    private void UpdateEvidenceList()
    {
        if (evidenceListText == null)
        {
            return;
        }

        // Empty-state placeholder.
        if (discoveredEvidence.Count == 0)
        {
            evidenceListText.text =
                "No evidence collected yet.\n\n" +
                "Examine objects around the office to discover clues.";
            return;
        }

        // Build the full evidence log from all discovered clues.
        StringBuilder text = new StringBuilder();

        foreach (KeyValuePair<string, string> clue in discoveredEvidence)
        {
            text.AppendLine(clue.Value);
            text.AppendLine(); // Blank line between entries for readability.
        }

        evidenceListText.text = text.ToString();
    }
}
