using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class EvidenceRecord
{
    public string evidenceId;
    public string title;
    [TextArea(2, 6)] public string description;
    public string source;
    public string category;

    public EvidenceRecord(string id, string evidenceTitle, string evidenceDescription, string evidenceSource, string evidenceCategory)
    {
        evidenceId = id;
        title = evidenceTitle;
        description = evidenceDescription;
        source = evidenceSource;
        category = evidenceCategory;
    }
}

/// <summary>
/// Persistent singleton that tracks global game state across scenes.
/// Handles evidence counts, USB discovery, and completion checks.
/// Survives scene loads via DontDestroyOnLoad.
/// </summary>
public class GameStateManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Singleton
    // -------------------------------------------------------------------------

    /// <summary>Global singleton reference. Access from any script safely.</summary>
    public static GameStateManager Instance { get; private set; }

    // -------------------------------------------------------------------------
    // Evidence Tracking
    // -------------------------------------------------------------------------

    /// <summary>Backing field for EvidenceCollected.</summary>
    private int _evidenceCollected;
    private readonly List<EvidenceRecord> evidenceRecords = new List<EvidenceRecord>();
    private readonly HashSet<string> evidenceIds = new HashSet<string>();

    /// <summary>Number of unique clues the player has found (read-only externally).</summary>
    public int EvidenceCollected => _evidenceCollected;
    public IReadOnlyList<EvidenceRecord> EvidenceRecords => evidenceRecords;
    public int DeductionScore { get; private set; }
    public int DeductionQuestions { get; private set; }

    /// <summary>Total clues required to proceed (4 scene clues + 1 USB).</summary>
    public int TotalEvidenceRequired = 5;

    /// <summary>True once the player has examined the USB drive object.</summary>
    public bool USBFound { get; private set; }

    /// <summary>
    /// Returns true when the player has collected all required evidence
    /// AND has found the USB drive.
    /// </summary>
    public bool AllEvidenceFound => _evidenceCollected >= TotalEvidenceRequired && USBFound;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired every time the evidence count or USB state changes.</summary>
    public event System.Action OnEvidenceUpdated;

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // Singleton enforcement — destroy any duplicate that appears after the
        // first instance has already been created and persisted.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Increments the evidence counter by one and fires OnEvidenceUpdated.
    /// Duplicate-guard is handled by PlayerInteraction before calling this.
    /// </summary>
    public void RegisterEvidence()
    {
        _evidenceCollected++;
        Debug.Log($"[GameStateManager] Evidence registered: {_evidenceCollected} / {TotalEvidenceRequired}");

        OnEvidenceUpdated?.Invoke();
        CheckCompletion();
    }

    /// <summary>Stores a unique evidence item for the Detective Board and updates overall progress.</summary>
    public bool RegisterCollectedEvidence(string id, string title, string description, string source, string category, bool isUSB = false)
    {
        if (string.IsNullOrWhiteSpace(id) || !evidenceIds.Add(id))
        {
            return false;
        }

        evidenceRecords.Add(new EvidenceRecord(id, title, description, source, category));
        if (isUSB) USBFound = true;
        _evidenceCollected++;
        Debug.Log($"[GameStateManager] Evidence recorded: {title} ({_evidenceCollected})");
        OnEvidenceUpdated?.Invoke();
        CheckCompletion();
        return true;
    }

    public void SetDeductionResult(int score, int questionCount)
    {
        DeductionScore = score;
        DeductionQuestions = questionCount;
    }

    /// <summary>
    /// Marks the USB drive as found, then also counts it as a piece of evidence
    /// (USB is both a special flag and one of the five required clues).
    /// </summary>
    public void RegisterUSB()
    {
        if (!USBFound)
        {
            USBFound = true;
            Debug.Log("[GameStateManager] USB drive registered.");
        }

        // USB counts as a regular evidence item too.
        RegisterEvidence();
    }

    /// <summary>
    /// Logs a completion hint when all evidence has been gathered.
    /// </summary>
    private void CheckCompletion()
    {
        if (AllEvidenceFound)
        {
            Debug.Log("[GameStateManager] All evidence found! Press F to proceed.");
        }
    }

    /// <summary>
    /// Resets all state back to defaults. Call this before returning to the
    /// main menu so a new playthrough starts clean.
    /// </summary>
    public void ResetState()
    {
        _evidenceCollected = 0;
        USBFound = false;
        evidenceIds.Clear();
        evidenceRecords.Clear();
        DeductionScore = 0;
        DeductionQuestions = 0;
        Debug.Log("[GameStateManager] State reset.");

        // Notify listeners so any persistent HUD elements refresh.
        OnEvidenceUpdated?.Invoke();
    }

    /// <summary>
    /// Loads the case-conclusion scene directly.
    /// Fader integration is handled inside SceneFader when that singleton exists.
    /// </summary>
    public void LoadConclusionScene()
    {
        SceneManager.LoadScene("04_CaseConclusion");
    }
}
