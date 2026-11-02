using UnityEngine;

// ============================================================
// NOTE FOR SETUP — add these properties to the listed scripts:
//
//   ExaminationUI.cs:
//     public bool IsPanelOpen => examinationPanel != null && examinationPanel.activeSelf;
//
//   DetectiveJournal.cs:
//     public bool IsJournalOpen => journalPanel != null && journalPanel.activeSelf;
//
// Both have been added automatically by the code-generation pass.
// ============================================================

/// <summary>
/// Handles global UI keyboard shortcuts.
///
/// Escape key priority (highest first):
///   1. Close the Detective Journal if it is open.
///   2. Otherwise close the Examination panel if it is open.
///
/// SETUP:
///   Attach this to a persistent UI manager GameObject in the scene.
///   Wire up ExaminationUI, DetectiveJournal, and MouseLook references in the Inspector.
/// </summary>
public class UIInputHandler : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("UI Panels")]
    [Tooltip("Reference to the ExaminationUI component that shows clue detail panels.")]
    public ExaminationUI examinationUI;

    [Tooltip("Reference to the DetectiveJournal component that shows the evidence journal.")]
    public DetectiveJournal detectiveJournal;

    [Header("Player")]
    [Tooltip("Reference to MouseLook – used to detect cursor lock state (game vs. UI mode).")]
    public MouseLook mouseLook;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Processes the Escape key with ordered priority:
    /// Journal > Examination Panel.
    /// </summary>
    private void HandleEscapeKey()
    {
        // Priority 1 – close the journal if it is currently open.
        if (detectiveJournal != null && detectiveJournal.IsJournalOpen)
        {
            detectiveJournal.CloseJournal();
            return;
        }

        // Priority 2 – close the examination panel if it is currently open.
        if (examinationUI != null && examinationUI.IsPanelOpen)
        {
            examinationUI.HidePanel();
            return;
        }

        // Nothing was open; Escape has no additional effect here.
        // (A separate script such as PauseMenu can subscribe to Escape independently.)
    }
}
