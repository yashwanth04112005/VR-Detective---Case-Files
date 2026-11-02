using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// Automated setup for VR Detective: Case Files.
/// Creates shared game prefabs and maintains scene wiring for the five-scene investigation flow.
/// Run from the Unity menu bar: Tools → VR Detective → Full Project Setup.
/// </summary>
public class VRDetectiveSetup : EditorWindow
{
    private static readonly string SCENES_PATH = "Assets/Scenes/";
    private static readonly string PREFABS_PATH = "Assets/Prefabs/";

    // =====================================================================
    // Menu Entry Point
    // =====================================================================

    [MenuItem("Tools/VR Detective/1. Full Project Setup (Run This First)")]
    public static void RunFullSetup()
    {
        if (!EditorUtility.DisplayDialog(
            "VR Detective: Full Setup",
            "This will:\n" +
            "• Create 3 global prefabs (SceneFader, GameStateManager, AudioManager)\n" +
            "• Refresh Build Settings to use the five-scene investigation flow\n\n" +
            "Existing scenes will NOT be modified.\n" +
            "After this, run step 2 (Patch CaseRoom) and step 3 (Patch CaseConclusion) separately.\n\n" +
            "Proceed?",
            "Yes, run setup",
            "Cancel"))
        {
            return;
        }

        EnsureDirectoryExists(PREFABS_PATH);
        EnsureDirectoryExists(SCENES_PATH);

        // Step 1: Create prefabs
        CreateSceneFaderPrefab();
        CreateGameStateManagerPrefab();
        CreateAudioManagerPrefab();

        // Existing authored scenes, including the Detective Board, are preserved.
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Setup Complete!",
            "✅ Shared prefabs created in Assets/Prefabs/\n" +
            "✅ Shared prefabs created in Assets/Prefabs/\n" +
            "✅ Build Settings updated with the five-scene investigation flow\n\n" +
            "NEXT STEPS:\n" +
            "1. Open 01_CaseRoom and run: Tools → VR Detective → 2. Patch CaseRoom Scene\n" +
            "2. Open 04_CaseConclusion and run: Tools → VR Detective → 3. Patch CaseConclusion Scene\n" +
            "3. Open 00_MainMenu and run: Tools → VR Detective → 4. Patch MainMenu Scene",
            "OK");
    }

    // =====================================================================
    // Step 2: Patch CaseRoom (must be open)
    // =====================================================================

    [MenuItem("Tools/VR Detective/2. Patch CaseRoom Scene (open it first)")]
    public static void PatchCaseRoomScene()
    {
        Scene active = EditorSceneManager.GetActiveScene();
        if (!active.name.Contains("CaseRoom") && !active.name.Contains("01_"))
        {
            EditorUtility.DisplayDialog("Wrong Scene",
                "Please open 01_CaseRoom first, then run this command.", "OK");
            return;
        }

        AddGlobalPrefabsToScene();
        AddUSBDriveToScene();
        AddHUDBadgeToScene();
        AddUIInputHandlerToScene();
        FixDetectiveJournalTotalEvidence();

        EditorSceneManager.MarkSceneDirty(active);
        EditorSceneManager.SaveScene(active);

        EditorUtility.DisplayDialog("CaseRoom Patched!",
            "✅ Global prefabs added\n" +
            "✅ USB_Drive created (behind monitor area)\n" +
            "✅ HUD Evidence Badge added\n" +
            "✅ UIInputHandler added\n" +
            "✅ DetectiveJournal TotalEvidenceRequired set to 5\n\n" +
            "You may need to manually position the USB_Drive near your desk/monitor.\n" +
            "Find it in the Hierarchy — it's named 'USB_Drive'.",
            "OK");
    }

    // =====================================================================
    // Step 3: Patch CaseConclusion (must be open)
    // =====================================================================

    [MenuItem("Tools/VR Detective/3. Patch CaseConclusion Scene (open it first)")]
    public static void PatchCaseConclusionScene()
    {
        Scene active = EditorSceneManager.GetActiveScene();
        if (!active.name.Contains("CaseConclusion") && !active.name.Contains("02_"))
        {
            EditorUtility.DisplayDialog("Wrong Scene",
                "Please open 04_CaseConclusion first, then run this command.", "OK");
            return;
        }

        AddGlobalPrefabsToScene();

        // Find CaseConclusionController and update its colors
        CaseConclusionController controller = Object.FindAnyObjectByType<CaseConclusionController>();
        if (controller != null)
        {
            controller.selectedColor = new Color(0.8f, 0.6f, 0.1f, 1f);
            controller.defaultColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            if (controller.submitButton != null)
            {
                controller.submitButton.interactable = false;
            }

            EditorUtility.SetDirty(controller);
        }

        EditorSceneManager.MarkSceneDirty(active);
        EditorSceneManager.SaveScene(active);

        EditorUtility.DisplayDialog("CaseConclusion Patched!",
            "✅ Global prefabs added\n" +
            "✅ Button colors configured (gold / dark grey)\n" +
            "✅ Submit button set to non-interactable by default",
            "OK");
    }

    // =====================================================================
    // Step 4: Patch MainMenu (must be open)
    // =====================================================================

    [MenuItem("Tools/VR Detective/4. Patch MainMenu Scene (open it first)")]
    public static void PatchMainMenuScene()
    {
        Scene active = EditorSceneManager.GetActiveScene();
        if (!active.name.Contains("MainMenu") && !active.name.Contains("00_"))
        {
            EditorUtility.DisplayDialog("Wrong Scene",
                "Please open 00_MainMenu first, then run this command.", "OK");
            return;
        }

        AddGlobalPrefabsToScene();

        EditorSceneManager.MarkSceneDirty(active);
        EditorSceneManager.SaveScene(active);

        EditorUtility.DisplayDialog("MainMenu Patched!",
            "✅ Global prefabs added (SceneFader, GameStateManager, AudioManager)\n\n" +
            "The Play button now routes through the Loading Interstitial scene automatically.",
            "OK");
    }

    // =====================================================================
    // Prefab Creators
    // =====================================================================

    private static void CreateSceneFaderPrefab()
    {
        string path = PREFABS_PATH + "SceneFader.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[VRDetectiveSetup] SceneFader prefab already exists — skipping.");
            return;
        }

        // Root object
        GameObject root = new GameObject("SceneFader");

        // Add Canvas
        Canvas canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        // Fade panel child
        GameObject fadePanel = new GameObject("FadePanel");
        fadePanel.transform.SetParent(root.transform, false);

        Image img = fadePanel.AddComponent<Image>();
        img.color = Color.black;

        // Stretch to fill
        RectTransform rt = fadePanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // CanvasGroup on the panel
        CanvasGroup cg = fadePanel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;

        // Attach SceneFader script
        SceneFader fader = root.AddComponent<SceneFader>();
        fader.fadeCanvasGroup = cg;
        fader.fadeDuration = 0.5f;

        // Save prefab
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log("[VRDetectiveSetup] ✅ SceneFader prefab created.");
    }

    private static void CreateGameStateManagerPrefab()
    {
        string path = PREFABS_PATH + "GameStateManager.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[VRDetectiveSetup] GameStateManager prefab already exists — skipping.");
            return;
        }

        GameObject go = new GameObject("GameStateManager");
        GameStateManager gsm = go.AddComponent<GameStateManager>();
        gsm.TotalEvidenceRequired = 5;

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[VRDetectiveSetup] ✅ GameStateManager prefab created.");
    }

    private static void CreateAudioManagerPrefab()
    {
        string path = PREFABS_PATH + "AudioManager.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[VRDetectiveSetup] AudioManager prefab already exists — skipping.");
            return;
        }

        GameObject go = new GameObject("AudioManager");
        AudioManager am = go.AddComponent<AudioManager>();

        // Pre-populate the sounds array with the 7 expected entries (clips left null for user to assign).
        am.sounds = new AudioManager.SoundEntry[]
        {
            new AudioManager.SoundEntry { name = "ambient_office",    volume = 0.3f, loop = true },
            new AudioManager.SoundEntry { name = "sfx_clue_found",    volume = 0.8f, loop = false },
            new AudioManager.SoundEntry { name = "sfx_usb_found",     volume = 1.0f, loop = false },
            new AudioManager.SoundEntry { name = "sfx_page_turn",     volume = 0.6f, loop = false },
            new AudioManager.SoundEntry { name = "sting_case_solved", volume = 1.0f, loop = false },
            new AudioManager.SoundEntry { name = "sting_unresolved",  volume = 0.8f, loop = false },
            new AudioManager.SoundEntry { name = "ambient_terminal",  volume = 0.4f, loop = true },
        };

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log("[VRDetectiveSetup] ✅ AudioManager prefab created.");
    }

    // =====================================================================
    // Scene Creators
    // =====================================================================

    private static void CreateLoadingInterstitialScene()
    {
        string scenePath = SCENES_PATH + "05_LoadingInterstitial.unity";
        if (File.Exists(scenePath))
        {
            Debug.Log("[VRDetectiveSetup] 05_LoadingInterstitial already exists — skipping.");
            return;
        }

        // Save current scene first
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Add Camera (needed for UI rendering in some setups)
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.03f, 1f);
        cam.tag = "MainCamera";

        // === Canvas ===
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreateUIChild(canvasObj, "Background", true);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.02f, 0.02f, 0.03f, 1f);

        // Case File Text
        GameObject caseFileObj = CreateUIChild(canvasObj, "CaseFileText", false);
        TMP_Text caseFileText = AddTMPText(caseFileObj, "CASE FILE #1147", 52,
            new Color(0.78f, 0.66f, 0.29f), TextAlignmentOptions.Center);
        caseFileObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 80);
        SetRectSize(caseFileObj, 800, 80);

        // Subtitle Text
        GameObject subtitleObj = CreateUIChild(canvasObj, "SubtitleText", false);
        TMP_Text subtitleText = AddTMPText(subtitleObj, "The Vale Disappearance", 28,
            Color.white, TextAlignmentOptions.Center);
        subtitleObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 20);
        SetRectSize(subtitleObj, 800, 50);

        // Crime Tape
        GameObject tapeObj = CreateUIChild(canvasObj, "CrimeTapeRect", false);
        Image tapeImg = tapeObj.AddComponent<Image>();
        tapeImg.color = new Color(0.9f, 0.75f, 0.1f, 1f); // Yellow
        RectTransform tapeRT = tapeObj.GetComponent<RectTransform>();
        tapeRT.anchoredPosition = new Vector2(0, -60);
        tapeRT.sizeDelta = new Vector2(2400, 50);

        // Tape text child
        GameObject tapeTextObj = CreateUIChild(tapeObj, "TapeText", false);
        AddTMPText(tapeTextObj, "████ CRIME SCENE — DO NOT CROSS ████ CRIME SCENE — DO NOT CROSS ████ CRIME SCENE — DO NOT CROSS ████",
            18, Color.black, TextAlignmentOptions.Center);
        StretchToParent(tapeTextObj);

        // Loading Text
        GameObject loadingObj = CreateUIChild(canvasObj, "LoadingText", false);
        TMP_Text loadingText = AddTMPText(loadingObj, "Loading Crime Scene...", 20,
            new Color(0.53f, 0.53f, 0.53f), TextAlignmentOptions.Center);
        loadingObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150);
        SetRectSize(loadingObj, 600, 40);

        // === Controller ===
        GameObject controllerObj = new GameObject("LoadingController");
        LoadingScreenController controller = controllerObj.AddComponent<LoadingScreenController>();
        controller.targetScene = "01_CaseRoom";
        controller.caseFileText = caseFileText;
        controller.subtitleText = subtitleText;
        controller.loadingText = loadingText;
        controller.crimeTapeRect = tapeRT;
        controller.tapeScrollSpeed = 80f;
        controller.minimumDisplayTime = 2.5f;

        // Add global prefabs
        InstantiatePrefabInScene("SceneFader");
        InstantiatePrefabInScene("GameStateManager");
        InstantiatePrefabInScene("AudioManager");

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[VRDetectiveSetup] ✅ 05_LoadingInterstitial scene created.");
    }

    private static void CreateUSBRevealScene()
    {
        string scenePath = SCENES_PATH + "03_USBReveal.unity";
        if (File.Exists(scenePath))
        {
            Debug.Log("[VRDetectiveSetup] 03_USBReveal already exists — skipping.");
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.tag = "MainCamera";

        // === Canvas ===
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreateUIChild(canvasObj, "Background", true);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = Color.black;

        Color terminalGreen = new Color(0f, 1f, 0.255f, 1f); // #00FF41
        Color dimGreen = new Color(0f, 0.87f, 0.2f, 1f);

        // Header Text
        GameObject headerObj = CreateUIChild(canvasObj, "HeaderText", false);
        TMP_Text headerText = AddTMPText(headerObj, "", 22, terminalGreen, TextAlignmentOptions.TopLeft);
        headerObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 200);
        SetRectSize(headerObj, 900, 120);

        // Fragment 1
        GameObject frag1Obj = CreateUIChild(canvasObj, "Fragment1Text", false);
        TMP_Text frag1Text = AddTMPText(frag1Obj, "", 17, dimGreen, TextAlignmentOptions.TopLeft);
        frag1Obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 70);
        SetRectSize(frag1Obj, 900, 120);
        frag1Obj.SetActive(false);

        // Fragment 2
        GameObject frag2Obj = CreateUIChild(canvasObj, "Fragment2Text", false);
        TMP_Text frag2Text = AddTMPText(frag2Obj, "", 17, dimGreen, TextAlignmentOptions.TopLeft);
        frag2Obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -60);
        SetRectSize(frag2Obj, 900, 120);
        frag2Obj.SetActive(false);

        // Fragment 3
        GameObject frag3Obj = CreateUIChild(canvasObj, "Fragment3Text", false);
        TMP_Text frag3Text = AddTMPText(frag3Obj, "", 17, dimGreen, TextAlignmentOptions.TopLeft);
        frag3Obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -190);
        SetRectSize(frag3Obj, 900, 120);
        frag3Obj.SetActive(false);

        // Proceed Button
        GameObject btnObj = CreateButtonTMP(canvasObj, "ProceedButton",
            "> CLOSE FILE — MAKE YOUR DEDUCTION",
            new Color(0.78f, 0.66f, 0.29f), Color.black,
            new Vector2(0, -310), new Vector2(500, 50));
        btnObj.SetActive(false);

        // === Controller ===
        GameObject controllerObj = new GameObject("USBRevealController");
        USBRevealController controller = controllerObj.AddComponent<USBRevealController>();
        controller.headerText = headerText;
        controller.fragment1Text = frag1Text;
        controller.fragment2Text = frag2Text;
        controller.fragment3Text = frag3Text;
        controller.proceedButton = btnObj;
        controller.typewriterSpeed = 0.025f;

        // Wire button OnClick
        Button btn = btnObj.GetComponent<Button>();
        if (btn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btn.onClick, controller.ProceedToConclusion);
        }

        // Global prefabs
        InstantiatePrefabInScene("SceneFader");
        InstantiatePrefabInScene("GameStateManager");
        InstantiatePrefabInScene("AudioManager");

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[VRDetectiveSetup] ✅ 03_USBReveal scene created.");
    }

    private static void CreateVerdictRevealScene()
    {
        string scenePath = SCENES_PATH + "04_VerdictReveal.unity";
        if (File.Exists(scenePath))
        {
            Debug.Log("[VRDetectiveSetup] 04_VerdictReveal already exists — skipping.");
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.06f, 0.04f, 0.03f, 1f);
        cam.tag = "MainCamera";

        // === Canvas ===
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Background
        GameObject bg = CreateUIChild(canvasObj, "Background", true);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.06f, 0.04f, 0.03f, 1f);

        Color gold = new Color(0.78f, 0.66f, 0.29f);
        Color cardBG = new Color(0.08f, 0.08f, 0.08f, 0.86f);
        Color stampRed = new Color(0.8f, 0.13f, 0.13f);

        // Heading
        GameObject headingObj = CreateUIChild(canvasObj, "HeadingText", false);
        TMP_Text headingText = AddTMPText(headingObj, "CASE CLOSED", 64, gold, TextAlignmentOptions.Center);
        headingObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 280);
        SetRectSize(headingObj, 800, 100);

        // === Evidence Cards ===
        string[] cardTexts = {
            "⏰  STOPPED CLOCK — At 11:47, Dr. Vale disconnected the clock. A prearranged signal to his contact.",
            "☕  UNFINISHED COFFEE — Left deliberately. Staged to suggest a hasty, forced departure.",
            "💻  COMPUTER RECORDS — Browser history revealed: researching non-extradition countries for 3 weeks.",
            "🔌  USB DRIVE — New identity, offshore funds, a farewell note. Dr. Vale planned everything."
        };
        string[] cardNames = { "ClockCard", "CoffeeCard", "ComputerCard", "USBCard" };
        float startY = 150f;

        GameObject[] cardObjects = new GameObject[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject cardObj = CreateUIChild(canvasObj, cardNames[i], false);
            Image cardImg = cardObj.AddComponent<Image>();
            cardImg.color = cardBG;
            RectTransform cardRT = cardObj.GetComponent<RectTransform>();
            cardRT.anchoredPosition = new Vector2(0, startY - i * 105);
            cardRT.sizeDelta = new Vector2(750, 90);

            // Card text child
            GameObject textChild = CreateUIChild(cardObj, "Text", false);
            AddTMPText(textChild, cardTexts[i], 18, Color.white, TextAlignmentOptions.MidlineLeft);
            RectTransform textRT = textChild.GetComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0, 0);
            textRT.anchorMax = new Vector2(1, 1);
            textRT.offsetMin = new Vector2(20, 10);
            textRT.offsetMax = new Vector2(-20, -10);

            cardObj.SetActive(false);
            cardObjects[i] = cardObj;
        }

        // Stamp
        GameObject stampObj = CreateUIChild(canvasObj, "StampObject", false);
        TMP_Text stampText = AddTMPText(stampObj, "CASE SOLVED\nSELF-STAGED DISAPPEARANCE", 40,
            stampRed, TextAlignmentOptions.Center);
        stampText.fontStyle = FontStyles.Bold;
        RectTransform stampRT = stampObj.GetComponent<RectTransform>();
        stampRT.anchoredPosition = new Vector2(0, -30);
        stampRT.localRotation = Quaternion.Euler(0, 0, -8f);
        SetRectSize(stampObj, 800, 120);
        stampObj.SetActive(false);

        // Return Button
        GameObject returnBtnObj = CreateButtonTMP(canvasObj, "ReturnButton",
            "Return to Main Menu",
            new Color(0.15f, 0.15f, 0.15f), gold,
            new Vector2(0, -280), new Vector2(350, 50));
        returnBtnObj.SetActive(false);

        // === Controller ===
        GameObject controllerObj = new GameObject("VerdictRevealController");
        VerdictRevealController controller = controllerObj.AddComponent<VerdictRevealController>();
        controller.headingText = headingText;
        controller.evidenceCards = cardObjects;
        controller.stampObject = stampObj;
        controller.returnButton = returnBtnObj;
        controller.cardRevealInterval = 0.9f;
        controller.cardSlideDistance = 300f;

        // Wire button
        Button btn = returnBtnObj.GetComponent<Button>();
        if (btn != null)
        {
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btn.onClick, controller.ReturnToMainMenu);
        }

        // Global prefabs
        InstantiatePrefabInScene("SceneFader");
        InstantiatePrefabInScene("GameStateManager");
        InstantiatePrefabInScene("AudioManager");

        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log("[VRDetectiveSetup] ✅ 04_VerdictReveal scene created.");
    }

    // =====================================================================
    // Scene Patchers (for existing scenes)
    // =====================================================================

    private static void AddGlobalPrefabsToScene()
    {
        InstantiatePrefabInSceneIfMissing<SceneFader>("SceneFader");
        InstantiatePrefabInSceneIfMissing<GameStateManager>("GameStateManager");
        InstantiatePrefabInSceneIfMissing<AudioManager>("AudioManager");
    }

    private static void AddUSBDriveToScene()
    {
        // Check if it already exists
        GameObject existing = GameObject.Find("USB_Drive");
        if (existing != null)
        {
            Debug.Log("[VRDetectiveSetup] USB_Drive already exists — skipping.");
            return;
        }

        // Create a small cube to represent the USB drive
        GameObject usb = GameObject.CreatePrimitive(PrimitiveType.Cube);
        usb.name = "USB_Drive";
        usb.transform.localScale = new Vector3(0.05f, 0.02f, 0.1f);

        // Try to position it near the desk/monitor — find any object with "monitor" or "desk" in name
        GameObject monitor = FindObjectWithNameContaining("monitor");
        GameObject desk = FindObjectWithNameContaining("desk");

        if (monitor != null)
        {
            usb.transform.position = monitor.transform.position + new Vector3(0.3f, -0.1f, 0.15f);
        }
        else if (desk != null)
        {
            usb.transform.position = desk.transform.position + new Vector3(0.5f, 0.8f, 0.3f);
        }
        else
        {
            usb.transform.position = new Vector3(0.5f, 1.0f, 0.3f);
        }

        // Add ObjectHighlight component
        ObjectHighlight highlight = usb.AddComponent<ObjectHighlight>();
        highlight.highlightColor = new Color(0f, 1f, 0.53f, 1f); // Green glow

        // Darken the material to look like a USB
        Renderer rend = usb.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.15f, 0.15f, 0.18f, 1f); // Dark grey
            rend.material = mat;
        }

        EditorUtility.SetDirty(usb);
        Debug.Log("[VRDetectiveSetup] ✅ USB_Drive created.");
    }

    private static void AddHUDBadgeToScene()
    {
        if (Object.FindAnyObjectByType<HUDBadge>() != null)
        {
            Debug.Log("[VRDetectiveSetup] HUDBadge already exists — skipping.");
            return;
        }

        // Find existing Canvas or create one
        Canvas existingCanvas = null;
        Canvas[] allCanvases = Object.FindObjectsByType<Canvas>();
        foreach (Canvas c in allCanvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay && c.sortingOrder < 900)
            {
                existingCanvas = c;
                break;
            }
        }

        GameObject canvasObj;
        if (existingCanvas != null)
        {
            canvasObj = existingCanvas.gameObject;
        }
        else
        {
            canvasObj = new GameObject("HUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create the badge text
        GameObject badgeObj = new GameObject("EvidenceBadge");
        badgeObj.transform.SetParent(canvasObj.transform, false);

        RectTransform rt = badgeObj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-30, -30);
        rt.sizeDelta = new Vector2(200, 50);

        TextMeshProUGUI badgeText = badgeObj.AddComponent<TextMeshProUGUI>();
        badgeText.text = "🔍 0 / 5";
        badgeText.fontSize = 22;
        badgeText.color = new Color(0.78f, 0.66f, 0.29f);
        badgeText.alignment = TextAlignmentOptions.Right;

        // Create HUDBadge component holder
        GameObject hudBadgeObj = new GameObject("HUDBadge");
        HUDBadge badge = hudBadgeObj.AddComponent<HUDBadge>();
        badge.badgeText = badgeText;
        badge.pulseDuration = 0.2f;
        badge.pulseScale = 1.3f;

        EditorUtility.SetDirty(badgeObj);
        EditorUtility.SetDirty(hudBadgeObj);
        Debug.Log("[VRDetectiveSetup] ✅ HUD Badge created.");
    }

    private static void AddUIInputHandlerToScene()
    {
        if (Object.FindAnyObjectByType<UIInputHandler>() != null)
        {
            Debug.Log("[VRDetectiveSetup] UIInputHandler already exists — skipping.");
            return;
        }

        GameObject handlerObj = new GameObject("UIInputHandler");
        UIInputHandler handler = handlerObj.AddComponent<UIInputHandler>();

        // Try to find and wire references
        ExaminationUI examUI = Object.FindAnyObjectByType<ExaminationUI>();
        DetectiveJournal journal = Object.FindAnyObjectByType<DetectiveJournal>();
        MouseLook mouseLook = Object.FindAnyObjectByType<MouseLook>();

        if (examUI != null) handler.examinationUI = examUI;
        if (journal != null) handler.detectiveJournal = journal;
        if (mouseLook != null) handler.mouseLook = mouseLook;

        EditorUtility.SetDirty(handlerObj);
        Debug.Log("[VRDetectiveSetup] ✅ UIInputHandler created.");
    }

    private static void FixDetectiveJournalTotalEvidence()
    {
        DetectiveJournal journal = Object.FindAnyObjectByType<DetectiveJournal>();
        if (journal != null)
        {
            journal.TotalEvidenceRequired = 5;
            EditorUtility.SetDirty(journal);
            Debug.Log("[VRDetectiveSetup] ✅ DetectiveJournal TotalEvidenceRequired set to 5.");
        }
    }

    // =====================================================================
    // Build Settings
    // =====================================================================

    private static void UpdateBuildSettings()
    {
        string[] scenePaths = new string[]
        {
            SCENES_PATH + "00_MainMenu.unity",
            SCENES_PATH + "01_CaseRoom.unity",
            SCENES_PATH + "02_ResearchArchive.unity",
            SCENES_PATH + "03_DetectiveBoard.unity",
            SCENES_PATH + "04_CaseConclusion.unity",
        };

        EditorBuildSettingsScene[] buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
        for (int i = 0; i < scenePaths.Length; i++)
        {
            buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
        }

        EditorBuildSettings.scenes = buildScenes;
        Debug.Log("[VRDetectiveSetup] Build Settings updated with the 5-scene investigation flow.");
    }

    // =====================================================================
    // Utility Helpers
    // =====================================================================

    private static void EnsureDirectoryExists(string path)
    {
        if (!AssetDatabase.IsValidFolder(path.TrimEnd('/')))
        {
            string parent = Path.GetDirectoryName(path.TrimEnd('/')).Replace("\\", "/");
            string folder = Path.GetFileName(path.TrimEnd('/'));
            if (!AssetDatabase.IsValidFolder(parent))
            {
                AssetDatabase.CreateFolder("Assets", Path.GetFileName(parent));
            }
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static GameObject CreateUIChild(GameObject parent, string name, bool stretchToFill)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        RectTransform rt = obj.AddComponent<RectTransform>();

        if (stretchToFill)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        return obj;
    }

    private static TMP_Text AddTMPText(GameObject obj, string text, float fontSize,
        Color color, TextAlignmentOptions alignment)
    {
        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.overflowMode = TextOverflowModes.Overflow;
        return tmp;
    }

    private static void SetRectSize(GameObject obj, float width, float height)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = new Vector2(width, height);
        }
    }

    private static void StretchToParent(GameObject obj)
    {
        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }

    private static GameObject CreateButtonTMP(GameObject parent, string name, string label,
        Color bgColor, Color textColor, Vector2 position, Vector2 size)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent.transform, false);

        RectTransform rt = btnObj.AddComponent<RectTransform>();
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;

        Button btn = btnObj.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        btn.colors = colors;

        // Text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(btnObj.transform, false);

        RectTransform textRT = textObj.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 5);
        textRT.offsetMax = new Vector2(-10, -5);

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 18;
        tmp.color = textColor;
        tmp.alignment = TextAlignmentOptions.Center;

        return btnObj;
    }

    private static void InstantiatePrefabInScene(string prefabName)
    {
        string path = PREFABS_PATH + prefabName + ".prefab";
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab != null)
        {
            PrefabUtility.InstantiatePrefab(prefab);
        }
        else
        {
            Debug.LogWarning($"[VRDetectiveSetup] Prefab not found: {path}");
        }
    }

    private static void InstantiatePrefabInSceneIfMissing<T>(string prefabName) where T : MonoBehaviour
    {
        if (Object.FindAnyObjectByType<T>() != null)
        {
            Debug.Log($"[VRDetectiveSetup] {prefabName} already in scene — skipping.");
            return;
        }
        InstantiatePrefabInScene(prefabName);
    }

    private static GameObject FindObjectWithNameContaining(string partialName)
    {
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>();
        string lowerPartial = partialName.ToLower();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains(lowerPartial))
            {
                return obj;
            }
        }
        return null;
    }
}


