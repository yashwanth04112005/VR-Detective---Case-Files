using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// First-person interaction and progression for the Research Archive.
/// Players examine three records before filing the case in the conclusion scene.
/// </summary>
public class ArchiveSceneController : MonoBehaviour
{
    [Header("Interaction")]
    public Camera playerCamera;
    public float interactionDistance = 4f;

    [Header("Archive UI")]
    public TMP_Text objectiveText;
    public TMP_Text promptText;
    public TMP_Text clueTitleText;
    public TMP_Text clueBodyText;
    public GameObject cluePanel;

    [Header("Progression")]
    public int requiredRecords = 3;
    public string conclusionScene = "03_DetectiveBoard";

    private readonly HashSet<string> discoveredRecords = new HashSet<string>();
    private ArchiveClue currentClue;
    private ObjectHighlight currentHighlight;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (cluePanel != null)
        {
            cluePanel.SetActive(false);
        }

        UpdateObjective();
        UpdatePrompt(null);
    }

    private void Update()
    {
        UpdateTarget();

        if (Input.GetKeyDown(KeyCode.E))
        {
            ExamineTarget();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && cluePanel != null)
        {
            cluePanel.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            FileCase();
        }
    }

    private void UpdateTarget()
    {
        ArchiveClue nextClue = null;

        if (playerCamera != null)
        {
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
            {
                nextClue = hit.collider.GetComponentInParent<ArchiveClue>();
            }
        }

        if (currentHighlight != null && (nextClue == null || currentHighlight.gameObject != nextClue.gameObject))
        {
            currentHighlight.SetHighlight(false);
        }

        currentClue = nextClue;
        currentHighlight = currentClue != null
            ? currentClue.GetComponent<ObjectHighlight>()
            : null;

        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(true);
        }

        UpdatePrompt(currentClue);
    }

    private void UpdatePrompt(ArchiveClue clue)
    {
        if (promptText == null)
        {
            return;
        }

        promptText.text = clue == null
            ? "WASD move • Shift sprint • Ctrl crouch • Space jump • E examine • F file"
            : "Press E to read: " + clue.clueTitle;
    }

    private void ExamineTarget()
    {
        AudioManager.Instance?.PlayOneShot("sfx_ui_click");
        if (currentClue == null)
        {
            return;
        }

        string id = string.IsNullOrWhiteSpace(currentClue.clueId)
            ? currentClue.gameObject.name
            : currentClue.clueId;

        if (discoveredRecords.Add(id))
        {
            AudioManager.Instance?.PlayOneShot("sfx_clue_found");
            string category = id.ToLowerInvariant().Contains("access") ? "access"
                : id.ToLowerInvariant().Contains("financial") ? "ledger" : "research";
            GameStateManager.Instance?.RegisterCollectedEvidence(
                "archive_" + id.ToLowerInvariant(), currentClue.clueTitle,
                currentClue.description, "Research Archive", category);
            Debug.Log("[ResearchArchive] Record found: " + currentClue.clueTitle);
        }
        else
        {
            AudioManager.Instance?.PlayOneShot("sfx_page_turn");
        }

        if (clueTitleText != null)
        {
            clueTitleText.text = currentClue.clueTitle;
        }

        if (clueBodyText != null)
        {
            clueBodyText.text = currentClue.description;
        }

        if (cluePanel != null)
        {
            cluePanel.SetActive(true);
        }

        UpdateObjective();
    }

    private void UpdateObjective()
    {
        if (objectiveText == null)
        {
            return;
        }

        if (discoveredRecords.Count >= requiredRecords)
        {
            objectiveText.text =
                "ARCHIVE COMPLETE\n" +
                "All three records are pinned in your case file. Press F to build your evidence board.";
        }
        else
        {
            objectiveText.text =
                "VALE RESEARCH ARCHIVE\n" +
                "Review the research log, access record, and financial ledger. Records found: " +
                discoveredRecords.Count + " / " + requiredRecords;
        }
    }

    private void FileCase()
    {
        if (discoveredRecords.Count < requiredRecords)
        {
            if (objectiveText != null)
            {
                objectiveText.text =
                    "RECORDS REMAINING\n" +
                    "Read all three archive records before filing the case (" +
                    discoveredRecords.Count + " / " + requiredRecords + ").";
            }

            return;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene(conclusionScene);
        }
        else
        {
            SceneManager.LoadScene(conclusionScene);
        }
    }

    private void OnDisable()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);
        }
    }
}
