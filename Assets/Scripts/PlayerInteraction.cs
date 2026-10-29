using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public Camera playerCamera;
    public InteractionCursorUI cursorUI;
    public ExaminationUI examinationUI;
    public DetectiveJournal detectiveJournal;
    public GameObject interactionPrompt;
    public TMPro.TMP_Text objectiveText;

    private ObjectHighlight currentHighlight;

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
        if (detectiveJournal != null)
            {
        detectiveJournal.ToggleJournal();
            }   
        }
        UpdateHighlight();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (examinationUI != null)
            {
                examinationUI.HidePanel();
            }
        }
    }

    private void UpdateHighlight()
    {
        if (playerCamera == null)
        {
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        ObjectHighlight newHighlight = null;

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            newHighlight = hit.collider.GetComponentInParent<ObjectHighlight>();
        }

        currentHighlight = newHighlight;

        if (cursorUI != null)
        {
            cursorUI.SetInteractable(newHighlight != null);
        }
        if (interactionPrompt != null)
{
    interactionPrompt.SetActive(newHighlight != null);
}
    }

    private void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("Player camera is not assigned.");
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance))
        {
            ObjectHighlight clue = hit.collider.GetComponentInParent<ObjectHighlight>();

            if (clue != null)
            {
                string clueName = clue.gameObject.name;
                string description = GetClueDescription(clueName);

                if (examinationUI != null)
{
    examinationUI.ShowClue(clueName, description);
}
else
{
    Debug.LogWarning("ExaminationUI is not assigned on PlayerInteraction.");
}

if (detectiveJournal != null)
{
    detectiveJournal.AddEvidence(clueName, description);
}
else
{
    Debug.LogWarning("DetectiveJournal is not assigned on PlayerInteraction.");
}
UpdateObjective();

                Debug.Log("You examined: " + clueName);
            }
            else
            {
                Debug.Log("This object has no clue information yet.");
            }
        }
        else
        {
            Debug.Log("No clue nearby. Look at an object and press E.");
        }
    }

    private string GetClueDescription(string clueName)
{
    string objectName = clueName.ToLower();

    if (objectName.Contains("clock"))
    {
        return "The clock has stopped at 11:47. Could this be the time of Dr. Vale's disappearance?";
    }

    if (objectName.Contains("mug") || objectName.Contains("coffee"))
    {
        return "A half-finished cup of coffee remains on the desk. Was Dr. Vale interrupted before finishing it?";
    }

    if (objectName.Contains("monitor") || objectName.Contains("computer") || objectName.Contains("pc"))
    {
        return "The computer may contain important research records. Someone may have left behind evidence.";
    }

    if (objectName.Contains("book") || objectName.Contains("document") || objectName.Contains("paper"))
    {
        return "These research materials may reveal what Dr. Vale was investigating before the incident.";
    }

    return "Something about this object seems unusual. Examine the surrounding area for more clues.";
}

    private void OnDisable()
    {
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);
        }
    }
    private void UpdateObjective()
{
    if (objectiveText == null || detectiveJournal == null)
    {
        return;
    }

    int collected = detectiveJournal.EvidenceCount;
    int required = detectiveJournal.TotalEvidenceRequired;

    if (collected >= required)
    {
        objectiveText.text =
            "CURRENT OBJECTIVE\n" +
            "All required evidence collected! Review your journal and prepare your final deduction.";
    }
    else
    {
        objectiveText.text =
            "CURRENT OBJECTIVE\n" +
            "Investigate Dr. Vale's office and collect evidence about his disappearance.\n" +
            detectiveJournal.GetEvidenceProgress();
    }
}
}