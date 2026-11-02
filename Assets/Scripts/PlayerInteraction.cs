using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles first-person player interaction with scene objects:
///   - Raycasts from the player camera to detect interactable objects.
///   - Displays examination UI and logs clues to the Detective Journal.
///   - Registers evidence with the global GameStateManager.
///   - Listens for [E] to examine, [Tab] to toggle journal, [Esc] to close
///     examination panel, and [F] to proceed to the next scene when all
///     evidence has been collected.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector Fields (all existing fields preserved)
    // -------------------------------------------------------------------------

    [Header("Interaction Settings")]
    public float interactionDistance = 3f;
    public Camera playerCamera;
    public InteractionCursorUI cursorUI;
    public ExaminationUI examinationUI;
    public DetectiveJournal detectiveJournal;
    public GameObject interactionPrompt;
    public TMPro.TMP_Text objectiveText;

    // -------------------------------------------------------------------------
    // Private State
    // -------------------------------------------------------------------------

    /// <summary>The ObjectHighlight component currently aimed at (null if none).</summary>
    private ObjectHighlight currentHighlight;
    private GameObject usbDrive;

    // Track each clue object once so pressing E again never inflates the evidence count.
    private readonly HashSet<ObjectHighlight> examinedObjects = new HashSet<ObjectHighlight>();

    // -------------------------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        // Ensure evidence progression exists even when the scene has no manager object.
        if (GameStateManager.Instance == null)
        {
            new GameObject("GameStateManager").AddComponent<GameStateManager>();
        }
        // Fall back to the main camera if no camera is assigned in the Inspector.
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        EnsureUSBDriveExists();

        // Show the initial objective message.
        UpdateObjective();
    }

    /// <summary>Keeps the saved USB clue clearly visible on the vending machine.</summary>
    private void EnsureUSBDriveExists()
    {
        GameObject vendingObject = GameObject.Find("(Prb)VendingMachine");
        Transform vendingMachine = vendingObject != null ? vendingObject.transform : null;
        if (vendingMachine == null)
        {
            foreach (Transform candidate in FindObjectsByType<Transform>())
            {
                if (candidate.name.ToLowerInvariant().Contains("vendingmachine"))
                {
                    vendingMachine = candidate;
                    break;
                }
            }
        }

        usbDrive = GameObject.Find("USB_Drive");
        if (usbDrive == null)
        {
            usbDrive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            usbDrive.name = "USB_Drive";
            ObjectHighlight highlight = usbDrive.AddComponent<ObjectHighlight>();
            highlight.highlightColor = new Color(1f, 0.88f, 0.30f, 1f);
            usbDrive.GetComponent<Renderer>().material.color = new Color(0.02f, 0.75f, 0.68f, 1f);
        }

        if (vendingMachine != null)
        {
            Renderer[] renderers = vendingMachine.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                // Center it on the machine's top so it cannot slip behind the front lip.
                // The slight size increase also keeps the clue readable from normal play distance.
                usbDrive.transform.localScale = new Vector3(0.19f, 0.045f, 0.28f);
                Vector3 surfacePosition = new Vector3(bounds.center.x, bounds.max.y + 0.025f, bounds.center.z);
                usbDrive.transform.position = surfacePosition;
                usbDrive.transform.rotation = Quaternion.Euler(0f, vendingMachine.eulerAngles.y, 0f);
            }
        }

        Renderer driveRenderer = usbDrive.GetComponent<Renderer>();
        if (driveRenderer != null)
        {
            driveRenderer.enabled = true;
            driveRenderer.forceRenderingOff = false;
            driveRenderer.material.color = new Color(0.015f, 0.9f, 0.78f, 1f);
        }

        CreateUSBLocator();
        bool reveal = GameStateManager.Instance != null &&
                      (GameStateManager.Instance.USBFound || GameStateManager.Instance.EvidenceCollected >= 4);
        usbDrive.SetActive(reveal);
    }

    /// <summary>Adds a restrained locator that makes the tiny drive easy to spot after the four room clues.</summary>
    private void CreateUSBLocator()
    {
        Transform existing = usbDrive.transform.Find("USB_Reveal_Locator");
        if (existing != null)
        {
            existing.localPosition = new Vector3(0f, 1.25f, 0f);
            existing.localScale = new Vector3(0.42f, 1.75f, 0.28f);
            return;
        }

        GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker.name = "USB_Reveal_Locator";
        marker.transform.SetParent(usbDrive.transform, false);
        marker.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        marker.transform.localScale = new Vector3(0.42f, 1.75f, 0.28f);
        Collider markerCollider = marker.GetComponent<Collider>();
        if (markerCollider != null) Destroy(markerCollider);
        Renderer markerRenderer = marker.GetComponent<Renderer>();
        if (markerRenderer != null)
        {
            Material beacon = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
            beacon.color = new Color(0.18f, 0.92f, 1f, 1f);
            if (beacon.HasProperty("_EmissionColor"))
            {
                beacon.EnableKeyword("_EMISSION");
                beacon.SetColor("_EmissionColor", new Color(0.05f, 0.65f, 1f) * 2.2f);
            }
            markerRenderer.material = beacon;
        }
        Light locatorLight = marker.AddComponent<Light>();
        locatorLight.type = LightType.Point;
        locatorLight.color = new Color(0.12f, 0.75f, 1f);
        locatorLight.intensity = 1.2f;
        locatorLight.range = 1.6f;
        locatorLight.shadows = LightShadows.None;
    }

    private void Update()
    {
        // Tab — toggle detective journal.
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (detectiveJournal != null)
            {
                detectiveJournal.ToggleJournal();
            }
        }

        // Always keep the highlight cursor fresh every frame.
        UpdateHighlight();

        // E — examine the object being looked at.
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteract();
        }

        // Escape — close the examination panel if it is open.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (examinationUI != null)
            {
                examinationUI.HidePanel();
            }
        }

        // F — proceed to the next scene once all evidence is collected.
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryProceedToConclusion();
        }
    }

    // -------------------------------------------------------------------------
    // Highlight / Cursor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Casts a ray from the player camera every frame and updates the cursor
    /// and interaction-prompt visibility based on what is in front of the player.
    /// </summary>
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

        // Update the cursor ring.
        if (cursorUI != null)
        {
            cursorUI.SetInteractable(newHighlight != null);
        }

        // Show / hide the world-space "Press E" prompt.
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(newHighlight != null);
        }
    }

    // -------------------------------------------------------------------------
    // Interaction
    // -------------------------------------------------------------------------

    /// <summary>
    /// Fires a ray from the camera and, if a valid clue object is hit:
    ///   1. Shows the ExaminationUI panel.
    ///   2. Logs the clue to the DetectiveJournal.
    ///   3. Registers evidence with GameStateManager (respecting USB logic).
    ///   4. Refreshes the objective text.
    /// </summary>
    private void TryInteract()
    {
        AudioManager.Instance?.PlayOneShot("sfx_ui_click");
        if (playerCamera == null)
        {
            Debug.LogWarning("[PlayerInteraction] Player camera is not assigned.");
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
                string clueName    = clue.gameObject.name;
                string objectName  = clueName.ToLower();
                string description = GetClueDescription(clueName);

                // Determine whether this object is the USB drive.
                bool isUSB = objectName.Contains("usb")
                          || objectName.Contains("drive")
                          || objectName.Contains("flashdrive");

                // Keep showing clue details on repeat interactions, but register evidence only once.
                bool firstExamination = examinedObjects.Add(clue);

                // --- Examination UI ---
                if (examinationUI != null)
                {
                    examinationUI.ShowClue(clueName, description);
                }
                else
                {
                    Debug.LogWarning("[PlayerInteraction] ExaminationUI is not assigned.");
                }

                // --- Detective Journal ---
                if (detectiveJournal != null)
                {
                    detectiveJournal.AddEvidence(clueName, description);
                }
                else
                {
                    Debug.LogWarning("[PlayerInteraction] DetectiveJournal is not assigned.");
                }

                // --- GameStateManager evidence registration ---
                // USB: RegisterUSB() already calls RegisterEvidence() internally,
                //      so we must NOT call RegisterEvidence() again to avoid
                //      double-counting.
                // Non-USB: call RegisterEvidence() directly.
                if (firstExamination)
                {
                    if (isUSB)
                    {
                        GameStateManager.Instance?.RegisterCollectedEvidence(
                            "case_usb_drive", "Encrypted USB Drive", description, "Case Room", "usb", true);
                        AudioManager.Instance?.PlayOneShot("sfx_usb_found");
                    }
                    else
                    {
                        RegisterCaseRoomEvidence(clueName, description);
                        AudioManager.Instance?.PlayOneShot("sfx_clue_found");
                        RevealUSBIfReady();
                    }
                }

                // Refresh HUD objective.
                UpdateObjective();

                Debug.Log("[PlayerInteraction] You examined: " + clueName);
            }
            else
            {
                Debug.Log("[PlayerInteraction] This object has no clue information yet.");
            }
        }
        else
        {
            Debug.Log("[PlayerInteraction] No clue nearby. Look at an object and press E.");
        }
    }

    private void RegisterCaseRoomEvidence(string clueName, string description)
    {
        if (GameStateManager.Instance == null) return;

        string normalized = clueName.ToLowerInvariant();
        string id = "case_" + normalized.Replace(" ", "_");
        string title = clueName.Replace("_", " ");
        string category = "document";

        if (normalized.Contains("clock"))
        {
            id = "case_stopped_clock";
            title = "Stopped Clock";
            category = "clock";
        }
        else if (normalized.Contains("mug") || normalized.Contains("coffee"))
        {
            id = "case_coffee_mug";
            title = "Unfinished Coffee";
            category = "coffee";
        }
        else if (normalized.Contains("monitor") || normalized.Contains("computer") || normalized.Contains("pc"))
        {
            id = "case_computer_records";
            title = "Computer Records";
            category = "computer";
        }
        else if (normalized.Contains("book") || normalized.Contains("document") || normalized.Contains("paper"))
        {
            id = "case_research_book";
            title = "Research Book";
            category = "book";
        }

        GameStateManager.Instance.RegisterCollectedEvidence(id, title, description, "Case Room", category);
    }

    private void RevealUSBIfReady()
    {
        if (usbDrive == null || GameStateManager.Instance == null) return;
        if (GameStateManager.Instance.EvidenceCollected >= 4 && !GameStateManager.Instance.USBFound)
        {
            usbDrive.SetActive(true);
            AudioManager.Instance?.PlayOneShot("sfx_clue_found");
        }
    }

    // -------------------------------------------------------------------------
    // Scene Transition
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called when the player presses [F]. Loads the USB reveal scene if all
    /// evidence is collected, otherwise shows a hint in the objective text.
    /// </summary>
    private void TryProceedToConclusion()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("[PlayerInteraction] GameStateManager not found.");
            return;
        }

        if (!GameStateManager.Instance.AllEvidenceFound)
        {
            // Display a hint — USB is the most likely missing piece.
            if (objectiveText != null)
            {
                objectiveText.text =
                    "CURRENT OBJECTIVE\n" +
                    "Collect all 5 clues first. Find the hidden USB drive.";
            }
            return;
        }

        // All office evidence found — transition to the Research Archive scene.
        // Use SceneFader if available; otherwise fall back to a direct load.
        if (SceneFader.Instance != null)
        {
            SceneFader.Instance.FadeToScene("02_ResearchArchive");
        }
        else
        {
            SceneManager.LoadScene("02_ResearchArchive");
        }
    }

    // -------------------------------------------------------------------------
    // Clue Descriptions
    // -------------------------------------------------------------------------

    /// <summary>
    /// Maps an object's GameObject name to a narrative clue description.
    /// The USB clue also triggers GameStateManager.RegisterUSB() as a side-effect.
    /// </summary>
    private string GetClueDescription(string clueName)
    {
        string objectName = clueName.ToLower();

        // --- USB Drive (critical evidence) ---
        if (objectName.Contains("usb") || objectName.Contains("drive") || objectName.Contains("flashdrive"))
        {
            // Note: RegisterUSB() is called from TryInteract(), not here, to keep
            // this method a pure data lookup with no side-effects.
            return "USB DRIVE — CRITICAL EVIDENCE\n" +
                   "An encrypted USB drive resting on top of the vending machine.\n" +
                   "Contents: a farewell note, offshore account records, and a new passport scan.\n" +
                   "Dr. Vale planned this. Everything was staged.";
        }

        // --- Stopped clock ---
        if (objectName.Contains("clock"))
        {
            return "The clock has stopped at 11:47. Could this be the time of Dr. Vale's disappearance?";
        }

        // --- Coffee mug ---
        if (objectName.Contains("mug") || objectName.Contains("coffee"))
        {
            return "A half-finished cup of coffee remains on the desk. Was Dr. Vale interrupted before finishing it?";
        }

        // --- Computer / monitor ---
        if (objectName.Contains("monitor") || objectName.Contains("computer") || objectName.Contains("pc"))
        {
            return "The computer may contain important research records. Someone may have left behind evidence.";
        }

        // --- Research documents / books ---
        if (objectName.Contains("book") || objectName.Contains("document") || objectName.Contains("paper"))
        {
            return "These research materials may reveal what Dr. Vale was investigating before the incident.";
        }

        // --- Generic fallback ---
        return "Something about this object seems unusual. Examine the surrounding area for more clues.";
    }

    // -------------------------------------------------------------------------
    // HUD / Objective
    // -------------------------------------------------------------------------

    /// <summary>
    /// Refreshes the objective text based on how many clues have been collected
    /// and whether the USB has been found.
    /// </summary>
    private void UpdateObjective()
    {
        if (objectiveText == null)
        {
            return;
        }

        // If GameStateManager is not present, fall back to the journal count.
        if (GameStateManager.Instance == null)
        {
            if (detectiveJournal != null)
            {
                objectiveText.text =
                    "CURRENT OBJECTIVE\n" +
                    "Investigate Dr. Vale's office. Examine objects with [E].\n" +
                    detectiveJournal.GetEvidenceProgress();
            }
            return;
        }

        int collected = GameStateManager.Instance.EvidenceCollected;
        bool usbFound = GameStateManager.Instance.USBFound;
        int required  = GameStateManager.Instance.TotalEvidenceRequired;

        if (collected >= required && usbFound)
        {
            // All 5 pieces collected.
            objectiveText.text =
                "CURRENT OBJECTIVE\n" +
                "All Case Room evidence gathered! Press [F] to search the Research Archive.";
        }
        else if (collected == 0)
        {
            // No clues yet — give an introductory prompt.
            objectiveText.text =
                "CURRENT OBJECTIVE\n" +
                "Investigate Dr. Vale's office. Examine objects with [E].";
        }
        else if (usbFound && collected < required)
        {
            // USB found but still missing other scene clues.
            objectiveText.text =
                "CURRENT OBJECTIVE\n" +
                $"Evidence: {collected} / {required} — Check the remaining objects.";
        }
        else if (collected >= 4 && !usbFound)
        {
            objectiveText.text =
                "CASE FILES UPDATED\n" +
                "The final clue is exposed. Look on top of the vending machine for a glowing cyan marker, then press [E].";
        }
        else
        {
            // 1-4 clues collected, USB not yet found.
            objectiveText.text =
                "CURRENT OBJECTIVE\n" +
                $"Evidence: {collected} / {required} — Keep searching. Find the hidden USB drive.";
        }
    }

    // -------------------------------------------------------------------------
    // Cleanup
    // -------------------------------------------------------------------------

    private void OnDisable()
    {
        // Remove highlight when this component is disabled (e.g., paused/menu).
        if (currentHighlight != null)
        {
            currentHighlight.SetHighlight(false);
        }
    }
}
