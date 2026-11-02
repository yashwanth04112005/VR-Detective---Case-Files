#if UNITY_EDITOR
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds the compact Research Archive scene and adds it between the office
/// and conclusion in Build Settings. Run from Tools > VR Detective > Build Research Archive.
/// </summary>
public static class ResearchArchiveSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/02_ResearchArchive.unity";
    private const string MaterialFolder = "Assets/Materials";

    [MenuItem("Tools/VR Detective/Build Research Archive")]
    public static void BuildResearchArchive()
    {
        if (File.Exists(ScenePath))
        {
            if (SceneManager.GetActiveScene().name != "02_ResearchArchive")
            {
                EditorSceneManager.OpenScene(ScenePath);
            }

            foreach (Light light in Object.FindObjectsByType<Light>())
            {
                if (light.name == "Warm Desk Light") light.intensity = 320f;
                else if (light.name == "Cool Archive Light") light.intensity = 220f;
                else if (light.name == "Archive Directional Light") light.intensity = 0.22f;
            }

            RenderSettings.ambientIntensity = 0.45f;
            HideLegacyBlockoutProps();
            HideLegacyArchiveClues();
            ApplyCaseRoomLighting();
            AddNappinArchiveProps();
            AddNappinArchiveClues();
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            AssetDatabase.Refresh();
            AddToBuildSettings();
            AssetDatabase.SaveAssets();
            Debug.Log("Research Archive lighting refined.");
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        EnsureFolder(MaterialFolder);
        Material wall = GetOrCreateMaterial("Archive_DeepBlue.mat", new Color(0.075f, 0.11f, 0.16f));
        Material floor = GetOrCreateMaterial("Archive_Floor.mat", new Color(0.17f, 0.19f, 0.21f));
        Material wood = GetOrCreateMaterial("Archive_Walnut.mat", new Color(0.24f, 0.14f, 0.08f));
        Material gold = GetOrCreateMaterial("Archive_Brass.mat", new Color(0.72f, 0.48f, 0.17f));
        Material paper = GetOrCreateMaterial("Archive_Paper.mat", new Color(0.73f, 0.67f, 0.52f));
        Material screen = GetOrCreateMaterial("Archive_Terminal.mat", new Color(0.12f, 0.42f, 0.39f));

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "02_ResearchArchive";

        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientLight = new Color(0.28f, 0.30f, 0.34f);
        RenderSettings.ambientIntensity = 1f;

        BuildRoom(wall, floor, wood, gold, paper, screen);
        BuildPlayerAndUI();
        AddNappinArchiveProps();
        AddToBuildSettings();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AddToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("Research Archive created at " + ScenePath);
    }

    private static void BuildRoom(Material wall, Material floor, Material wood, Material gold, Material paper, Material screen)
    {
        // Keep a simple room shell; furniture and props use the same Nappin prefabs as the Case Room.
        CreateCube("Archive Floor", new Vector3(0, -0.12f, 0), new Vector3(12, 0.24f, 12), floor);
        CreateCube("Back Wall", new Vector3(0, 2.5f, 5.9f), new Vector3(12, 5, 0.2f), wall);
        CreateCube("Left Wall", new Vector3(-5.9f, 2.5f, 0), new Vector3(0.2f, 5, 12), wall);
        CreateCube("Right Wall", new Vector3(5.9f, 2.5f, 0), new Vector3(0.2f, 5, 12), wall);

        CreatePrefabClue("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)BookOpen.prefab",
            "Research Log", "research_log",
            "RESEARCH LOG — PROJECT UMBRA\nVale's notes show the research had been closed weeks before he vanished. His final pages form a personal checklist: cash, a passport, and a route out.",
            new Vector3(-0.45f, 0.88f, 1.05f), 1.2f);

        CreatePrefabClue("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)DocumentHolder.prefab",
            "Access Record", "access_record",
            "SERVICE ACCESS RECORD — 11:42 PM\nDr. Vale used the archive service lift five minutes before the office clock stopped. No forced entry or second badge was recorded.",
            new Vector3(-3f, 0.88f, -2.6f), 1.2f);

        CreatePrefabClue("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)BookOpen.prefab",
            "Financial Ledger", "financial_ledger",
            "FINANCIAL LEDGER — 11:43 PM\nA transfer of $2.4 million was sent to an offshore account listed on the USB, and the transfer was prepared three weeks before Dr. Vale vanished.",
            new Vector3(0.45f, 0.88f, 1.05f), 1.2f);
        GameObject keyLight = new GameObject("Archive Directional Light");
        keyLight.transform.position = new Vector3(0f, 3f, 0f);
        Light directional = keyLight.AddComponent<Light>();
        directional.type = LightType.Directional;
        directional.color = Color.white;
        directional.intensity = 2f;
        directional.shadows = LightShadows.Soft;
        keyLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
    }
    private static void BuildPlayerAndUI()
    {
        GameObject player = new GameObject("Archive Player");
        player.transform.position = new Vector3(0, 0, -2.0f);

        CharacterController character = player.AddComponent<CharacterController>();
        character.height = 1.8f;
        character.radius = 0.35f;
        character.center = new Vector3(0, 0.9f, 0);
        character.stepOffset = 0.3f;

        PlayerMovement movement = player.AddComponent<PlayerMovement>();
        movement.moveSpeed = 3.2f;

        GameObject cameraObject = new GameObject("Archive Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.SetParent(player.transform, false);
        cameraObject.transform.localPosition = new Vector3(0, 1.58f, 0);

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
        camera.fieldOfView = 65f;
        cameraObject.AddComponent<AudioListener>();

        MouseLook look = cameraObject.AddComponent<MouseLook>();
        look.mouseSensitivity = 450f;
        look.playerBody = player.transform;

        Canvas canvas = CreateCanvas();
        TMP_Text title = CreateText("Archive Title", canvas.transform,
            "VALE RESEARCH ARCHIVE", 34, new Color(0.93f, 0.76f, 0.43f),
            TextAlignmentOptions.Center, new Vector2(0.2f, 0.90f), new Vector2(0.8f, 0.99f));

        TMP_Text objective = CreateText("Archive Objective", canvas.transform,
            string.Empty, 21, Color.white,
            TextAlignmentOptions.Left, new Vector2(0.04f, 0.80f), new Vector2(0.52f, 0.91f));

        TMP_Text prompt = CreateText("Archive Prompt", canvas.transform,
            string.Empty, 19, new Color(0.95f, 0.86f, 0.66f),
            TextAlignmentOptions.Center, new Vector2(0.12f, 0.03f), new Vector2(0.88f, 0.11f));

        GameObject panel = CreatePanel("Evidence Readout", canvas.transform,
            new Vector2(0.12f, 0.16f), new Vector2(0.88f, 0.47f),
            new Color(0.025f, 0.04f, 0.065f, 0.94f));

        TMP_Text clueTitle = CreateText("Evidence Title", panel.transform,
            string.Empty, 24, new Color(0.96f, 0.78f, 0.43f),
            TextAlignmentOptions.Left, new Vector2(0.05f, 0.65f), new Vector2(0.95f, 0.95f));

        TMP_Text clueBody = CreateText("Evidence Description", panel.transform,
            string.Empty, 19, Color.white,
            TextAlignmentOptions.TopLeft, new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.66f));

        panel.SetActive(false);

        ArchiveSceneController controller = player.AddComponent<ArchiveSceneController>();
        controller.playerCamera = camera;
        controller.interactionDistance = 4f;
        controller.objectiveText = objective;
        controller.promptText = prompt;
        controller.clueTitleText = clueTitle;
        controller.clueBodyText = clueBody;
        controller.cluePanel = panel;
        controller.requiredRecords = 3;
        controller.conclusionScene = "03_DetectiveBoard";
    }

    private static void AddNappinArchiveProps()
    {
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)Desk2.prefab",
            "Archive Nappin Desk", new Vector3(-3f, 0f, -2.9f));
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)OfficeChair.prefab",
            "Archive Office Chair", new Vector3(-3f, 0f, -4.2f));
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)Plant1.prefab",
            "Archive Potted Plant", new Vector3(4.7f, 0f, -4.7f));
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)Desk1.prefab",
            "Archive Main Nappin Desk", new Vector3(0f, 0f, 1.05f));
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)Shelves1.prefab",
            "Archive Nappin Shelves Left", new Vector3(-5.1f, 0f, 2f));
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)Shelves1.prefab",
            "Archive Nappin Shelves Right", new Vector3(5.1f, 0f, 2f), 180f);
        AddPrefabIfMissing("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)DeskLight.prefab",
            "Archive Desk Lamp", new Vector3(1.2f, 0.95f, 1.15f));
    }

    private static void AddNappinArchiveClues()
    {
        CreatePrefabClue("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)BookOpen.prefab",
            "Research Log", "research_log",
            "RESEARCH LOG — PROJECT UMBRA\nVale's notes show the research had been closed weeks before he vanished. His final pages form a personal checklist: cash, a passport, and a route out.",
            new Vector3(-0.45f, 0.88f, 1.05f), 1.2f);
        CreatePrefabClue("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)DocumentHolder.prefab",
            "Access Record", "access_record",
            "SERVICE ACCESS RECORD — 11:42 PM\nDr. Vale used the archive service lift five minutes before the office clock stopped. No forced entry or second badge was recorded.",
            new Vector3(-3f, 0.88f, -2.6f), 1.2f);
        CreatePrefabClue("Assets/nappin/OfficeEssentialsPack/Prefabs/(Prb)BookOpen.prefab",
            "Financial Ledger", "financial_ledger",
            "FINANCIAL LEDGER — 11:43 PM\nA transfer of $2.4 million was sent to an offshore account listed on the USB, and the transfer was prepared three weeks before Dr. Vale vanished.",
            new Vector3(0.45f, 0.88f, 1.05f), 1.2f);
    }

    private static void CreatePrefabClue(string prefabPath, string title, string clueId, string description, Vector3 position, float scale)
    {
        if (GameObject.Find(title) != null) return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("Research clue prefab not found: " + prefabPath);
            return;
        }

        GameObject clueObject = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        clueObject.name = title;
        clueObject.transform.position = position;
        clueObject.transform.localScale = Vector3.one * scale;

        ArchiveClue clue = clueObject.AddComponent<ArchiveClue>();
        clue.clueId = clueId;
        clue.clueTitle = title;
        clue.description = description;

        ObjectHighlight highlight = clueObject.AddComponent<ObjectHighlight>();
        highlight.highlightColor = new Color(1f, 0.78f, 0.15f);
        EditorUtility.SetDirty(clueObject);
    }

    private static void HideLegacyArchiveClues()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>())
        {
            if (obj.name.StartsWith("Legacy Research Log") ||
                obj.name.StartsWith("Legacy Access Record") ||
                obj.name.StartsWith("Legacy Financial Ledger"))
            {
                obj.SetActive(false);
            }
        }
    }
    private static void HideLegacyBlockoutProps()
    {
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>())
        {
            string name = obj.name;
            bool oldFurniture = name.StartsWith("Left Archive Shelves") ||
                name.StartsWith("Right Archive Shelves") ||
                name == "Archive Desk Top" || name.StartsWith("Desk Leg") ||
                name == "Desk Back Support" || name.StartsWith("Terminal ") ||
                name.StartsWith("Archive Storage Crate");
            bool oldColoredLight = name == "Warm Desk Light" || name == "Cool Archive Light";
            if (oldFurniture || oldColoredLight) obj.SetActive(false);
        }
    }

    private static void ApplyCaseRoomLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Skybox;
        RenderSettings.ambientIntensity = 1f;

        foreach (Light light in Object.FindObjectsByType<Light>())
        {
            if (light.name == "Archive Directional Light")
            {
                light.color = Color.white;
                light.intensity = 2f;
                light.shadows = LightShadows.Soft;
                light.transform.position = new Vector3(0f, 3f, 0f);
                light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }
    }

    private static void AddPrefabIfMissing(string prefabPath, string objectName, Vector3 position, float yaw = 0f)
    {
        if (GameObject.Find(objectName) != null) return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("Research archive prefab not found: " + prefabPath);
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = objectName;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        EditorUtility.SetDirty(instance);
    }
    private static void CreateShelf(string name, float x, Material wood, Material paper)
    {
        CreateCube(name + " Left Upright", new Vector3(x - 0.8f, 1.55f, 4.65f), new Vector3(0.12f, 3.1f, 0.55f), wood);
        CreateCube(name + " Right Upright", new Vector3(x + 0.8f, 1.55f, 4.65f), new Vector3(0.12f, 3.1f, 0.55f), wood);

        for (int level = 0; level < 4; level++)
        {
            float y = 0.25f + level * 0.82f;
            CreateCube(name + " Shelf " + level, new Vector3(x, y, 4.65f), new Vector3(1.75f, 0.10f, 0.62f), wood);

            if (level < 3)
            {
                for (int box = 0; box < 3; box++)
                {
                    float boxX = x - 0.52f + box * 0.52f;
                    CreateCube(name + " File Box " + level + "-" + box,
                        new Vector3(boxX, y + 0.31f, 4.58f),
                        new Vector3(0.38f, 0.48f, 0.45f), paper);
                }
            }
        }
    }

    private static void CreateClue(string title, string id, string description, Vector3 position, Vector3 scale, Material material)
    {
        GameObject clueObject = CreateCube(title, position, scale, material);
        clueObject.transform.rotation = Quaternion.Euler(0f, 0f, title == "Access Record" ? 0f : -4f);
        ArchiveClue clue = clueObject.AddComponent<ArchiveClue>();
        clue.clueId = id;
        clue.clueTitle = title;
        clue.description = description;

        ObjectHighlight highlight = clueObject.AddComponent<ObjectHighlight>();
        highlight.highlightColor = new Color(1f, 0.78f, 0.15f);
    }

    private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        cube.GetComponent<Renderer>().sharedMaterial = material;
        return cube;
    }

    private static void CreateLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject lightObject = new GameObject(name);
        lightObject.transform.position = position;
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new GameObject("Archive Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static GameObject CreatePanel(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panel;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, float fontSize, Color color,
        TextAlignmentOptions alignment, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = new Vector2(14, 8);
        rect.offsetMax = new Vector2(-14, -8);

        TMP_Text tmp = textObject.GetComponent<TMP_Text>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = alignment;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Material GetOrCreateMaterial(string fileName, Color color)
    {
        string path = MaterialFolder + "/" + fileName;
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            return existing;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.name = Path.GetFileNameWithoutExtension(fileName);
        material.color = color;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string folder = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, folder);
    }

    private static void AddToBuildSettings()
    {
        var list = EditorBuildSettings.scenes
            .Where(scene => scene.path != ScenePath)
            .ToList();

        int conclusionIndex = list.FindIndex(scene => scene.path.EndsWith("/04_CaseConclusion.unity"));
        int insertIndex = conclusionIndex >= 0 ? conclusionIndex : list.Count;
        list.Insert(insertIndex, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = list.ToArray();

        // Unity 6 can retain a zero GUID for scenes added before its asset refresh.
        // Patch the serialized Build Settings entry from the now-imported scene meta.
        string sceneGuid = AssetDatabase.AssetPathToGUID(ScenePath);
        string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "ProjectSettings", "EditorBuildSettings.asset");
        if (!string.IsNullOrEmpty(sceneGuid) && File.Exists(settingsPath))
        {
            string settings = File.ReadAllText(settingsPath);
            string pattern = @"(path:\s*" + System.Text.RegularExpressions.Regex.Escape(ScenePath) +
                @"\s*\r?\n\s*guid:\s*)[0-9a-fA-F]+";
            string updated = System.Text.RegularExpressions.Regex.Replace(
                settings,
                pattern,
                match => match.Groups[1].Value + sceneGuid);

            if (updated != settings)
            {
                File.WriteAllText(settingsPath, updated);
            }
        }
    }
}
#endif
