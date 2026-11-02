#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Wires game audio and dresses the Research Archive with Nappin props.</summary>
public static class SceneAudioAndDressingSetup
{
    private const string AudioFolder = "Assets/Audio/";
    private const string PrefabFolder = "Assets/nappin/OfficeEssentialsPack/Prefabs/";
    private const string ArchivePath = "Assets/Scenes/02_ResearchArchive.unity";

    [MenuItem("Tools/VR Detective/Set Up Audio and Archive Dressing")]
    public static void SetUpAudioAndArchiveDressing()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string returnPath = SceneManager.GetActiveScene().path;

        string[] scenePaths =
        {
            "Assets/Scenes/00_MainMenu.unity",
            "Assets/Scenes/01_CaseRoom.unity",
            ArchivePath,
            "Assets/Scenes/03_DetectiveBoard.unity",
            "Assets/Scenes/04_CaseConclusion.unity"
        };

        foreach (string scenePath in scenePaths)
        {
            Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            EnsureAudioManager(scene);
            if (scenePath == "Assets/Scenes/01_CaseRoom.unity")
            {
                EnsureFootsteps("Player");
                EnsureUsbClue(scene);
            }
            else if (scenePath == ArchivePath)
            {
                EnsureFootsteps("Archive Player");
                AddArchiveDressing(scene);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (!string.IsNullOrEmpty(returnPath))
            EditorSceneManager.OpenScene(returnPath, OpenSceneMode.Single);
        Debug.Log("Audio is wired and the Research Archive has been dressed with Nappin props.");
    }

    private static void EnsureAudioManager(Scene scene)
    {
        GameObject managerObject = GameObject.Find("Audio Manager");
        if (managerObject == null) managerObject = new GameObject("Audio Manager");
        AudioManager manager = managerObject.GetComponent<AudioManager>();
        if (manager == null) manager = managerObject.AddComponent<AudioManager>();

        manager.sounds = new[]
        {
            Sound("ambient_office", "ambient_office.wav", 0.10f, true),
            Sound("ambient_archive", "ambient_archive.wav", 0.10f, true),
            Sound("suspense_bgm", "suspense_bgm.wav", 0.28f, true),
            Sound("sfx_clue_found", "sfx_clue_found.wav", 0.55f, false),
            Sound("sfx_usb_found", "sfx_usb_found.wav", 0.62f, false),
            Sound("sfx_page_turn", "sfx_page_turn.wav", 0.42f, false),
            Sound("sfx_ui_click", "sfx_ui_click.wav", 0.32f, false),
            Sound("sting_case_solved", "sting_case_solved.wav", 0.65f, false),
            Sound("sting_unresolved", "sting_unresolved.wav", 0.55f, false)
        };
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(managerObject);
    }

    private static AudioManager.SoundEntry Sound(string name, string file, float volume, bool loop)
    {
        AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file);
        if (clip == null) Debug.LogWarning("Audio clip not found: " + AudioFolder + file);
        return new AudioManager.SoundEntry { name = name, clip = clip, volume = volume, loop = loop };
    }

    private static void EnsureFootsteps(string playerName)
    {
        GameObject player = GameObject.Find(playerName);
        if (player == null)
        {
            Debug.LogWarning("Could not find player object for footsteps: " + playerName);
            return;
        }

        FootstepAudio footsteps = player.GetComponent<FootstepAudio>();
        if (footsteps == null) footsteps = player.AddComponent<FootstepAudio>();
        footsteps.footstepClips = new[]
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_footstep_walk_a.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_footstep_walk_b.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_footstep_walk_c.wav")
        };
        footsteps.sprintFootstepClips = new[]
        {
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_footstep_sprint_a.wav"),
            AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + "sfx_footstep_sprint_b.wav")
        };
        footsteps.footstepVolume = 0.34f;
        footsteps.stepInterval = 0.44f;
        footsteps.sprintStepInterval = 0.26f;
        footsteps.sprintPitch = 1.12f;
        EditorUtility.SetDirty(footsteps);
    }

    private static void AddArchiveDressing(Scene scene)
    {
        GameObject mainDesk = GameObject.Find("Archive Main Nappin Desk");
        GameObject sideDesk = GameObject.Find("Archive Nappin Desk");
        if (mainDesk == null || sideDesk == null)
        {
            Debug.LogWarning("Nappin desks were not found; skipping archive surface props.");
            return;
        }

        AddPrefabOnDesk("(Prb)PC.prefab", "Archive Research PC", mainDesk,
            new Vector3(0f, 0f, 0.30f), 0f, 0.82f);
        AddPrefabOnDesk("(Prb)PenHolder.prefab", "Archive Desk Pen Holder", mainDesk,
            new Vector3(0.78f, 0f, -0.15f), 0f, 0.72f);
        AddPrefabOnDesk("(Prb)BookPile4.prefab", "Archive Main Desk Book Stack", mainDesk,
            new Vector3(-0.83f, 0f, -0.18f), -8f, 0.66f);
        AddPrefabOnDesk("(Prb)DeskPrinter.prefab", "Archive Side Desk Printer", sideDesk,
            new Vector3(-0.62f, 0f, -0.12f), -90f, 0.72f);
        AddPrefabOnDesk("(Prb)BookPile6.prefab", "Archive Side Desk Reference Stack", sideDesk,
            new Vector3(0.56f, 0f, -0.12f), 7f, 0.62f);
        AddPrefabOnDesk("(Prb)Mug.prefab", "Archive Late Night Mug", sideDesk,
            new Vector3(-0.02f, 0f, -0.42f), 12f, 0.72f);
        AddPrefabOnDesk("(Prb)Stapler.prefab", "Archive Desk Stapler", mainDesk,
            new Vector3(0.82f, 0f, 0.22f), -8f, 0.65f);

        AddPrefabOnShelf("(Prb)BookPile2.prefab", "Archive Left Shelf Books", "Archive Nappin Shelves Left",
            new Vector3(0f, 1.30f, -0.18f), -4f, 0.58f);
        AddPrefabOnShelf("(Prb)BookPile5.prefab", "Archive Right Shelf Books", "Archive Nappin Shelves Right",
            new Vector3(0f, 1.85f, -0.18f), 5f, 0.58f);
        AddPrefabOnShelf("(Prb)BookPile1.prefab", "Archive Left Shelf Case Files", "Archive Nappin Shelves Left",
            new Vector3(-0.20f, 2.35f, -0.10f), 8f, 0.52f);
        AddPrefabOnShelf("(Prb)BookPile7.prefab", "Archive Right Shelf Journals", "Archive Nappin Shelves Right",
            new Vector3(0.18f, 0.75f, -0.10f), -7f, 0.52f);

        AddPrefabAtFloor("(Prb)TrashCan.prefab", "Archive Waste Basket", new Vector3(-4.45f, 0f, -3.50f), 0f, 0.82f);
        AddPrefabAtFloor("(Prb)ConferenceTable.prefab", "Archive Review Table", new Vector3(0f, 0f, 4.15f), 0f, 0.76f);
        AddPrefabAtFloor("(Prb)OfficeChair.prefab", "Archive Review Chair Left", new Vector3(-1.55f, 0f, 3.85f), 90f, 0.90f);
        AddPrefabAtFloor("(Prb)OfficeChair.prefab", "Archive Review Chair Right", new Vector3(1.55f, 0f, 3.85f), 270f, 0.90f);
        AddPrefabAtFloor("(Prb)LoungeChair.prefab", "Archive Reading Chair", new Vector3(-4.20f, 0f, 4.20f), 90f, 0.82f);
        AddPrefabAtFloor("(Prb)CoffeTable.prefab", "Archive Reading Table", new Vector3(-3.05f, 0f, 4.15f), 0f, 0.78f);
        AddPrefabAtFloor("(Prb)BigDrawer.prefab", "Archive Records Cabinet", new Vector3(4.25f, 0f, 4.55f), 180f, 0.82f);
        AddPrefabAtFloor("(Prb)WaterDispenser.prefab", "Archive Water Station", new Vector3(4.85f, 0f, -3.15f), 0f, 0.82f);

        AddArchiveShellEnhancements(scene);
        AddCeilingFixture(scene, "Archive Ceiling Light Left", new Vector3(-2.8f, 4.82f, 0.6f), new Color(1f, 0.84f, 0.68f), 3.4f);
        AddCeilingFixture(scene, "Archive Ceiling Light Right", new Vector3(2.8f, 4.82f, 0.6f), new Color(1f, 0.91f, 0.79f), 3.4f);
        AddCeilingFixture(scene, "Archive Ceiling Light Rear", new Vector3(0f, 4.82f, 3.4f), new Color(0.78f, 0.86f, 1f), 2.4f);
    }

    private static void EnsureUsbClue(Scene scene)
    {
        GameObject usb = GameObject.Find("USB_Drive");
        if (usb == null)
        {
            usb = GameObject.CreatePrimitive(PrimitiveType.Cube);
            usb.name = "USB_Drive";
            usb.AddComponent<ObjectHighlight>();
        }

        GameObject vending = GameObject.Find("(Prb)VendingMachine");
        if (vending == null) return;
        Renderer[] machineRenderers = vending.GetComponentsInChildren<Renderer>();
        if (machineRenderers.Length == 0) return;
        Bounds machineBounds = machineRenderers[0].bounds;
        for (int i = 1; i < machineRenderers.Length; i++) machineBounds.Encapsulate(machineRenderers[i].bounds);

        usb.transform.localScale = new Vector3(0.16f, 0.04f, 0.24f);
        Vector3 face = Vector3.ProjectOnPlane(vending.transform.forward, Vector3.up).normalized;
        Vector3 top = new Vector3(machineBounds.center.x, machineBounds.max.y + 0.028f,
            machineBounds.center.z) + face * Mathf.Max(0.14f, machineBounds.extents.z * 0.42f);
        usb.transform.position = top;
        usb.transform.rotation = Quaternion.Euler(0f, vending.transform.eulerAngles.y, 0f);

        Material body = GetOrCreateUsbMaterial("USB_Anodized_Teal.mat", new Color(0.02f, 0.75f, 0.68f), 0.55f, 0.35f);
        Material metal = GetOrCreateUsbMaterial("USB_Brushed_Metal.mat", new Color(0.58f, 0.66f, 0.70f), 0.82f, 0.08f);
        Material led = GetOrCreateUsbMaterial("USB_Status_Glow.mat", new Color(0.10f, 1f, 0.58f), 0.12f, 1.2f);
        MeshRenderer bodyRenderer = usb.GetComponent<MeshRenderer>();
        if (bodyRenderer != null) bodyRenderer.sharedMaterial = body;
        ObjectHighlight highlight = usb.GetComponent<ObjectHighlight>();
        if (highlight == null) highlight = usb.AddComponent<ObjectHighlight>();
        highlight.highlightColor = new Color(1f, 0.88f, 0.30f);
        CreateUsbPart(usb.transform, "USB Metal Connector", new Vector3(0f, 0f, 0.135f), new Vector3(0.10f, 0.027f, 0.082f), metal);
        CreateUsbPart(usb.transform, "USB Contact 1", new Vector3(-0.027f, 0.001f, 0.18f), new Vector3(0.012f, 0.012f, 0.025f), led);
        CreateUsbPart(usb.transform, "USB Contact 2", new Vector3(0f, 0.001f, 0.18f), new Vector3(0.012f, 0.012f, 0.025f), led);
        CreateUsbPart(usb.transform, "USB Contact 3", new Vector3(0.027f, 0.001f, 0.18f), new Vector3(0.012f, 0.012f, 0.025f), led);
        CreateUsbPart(usb.transform, "USB Status LED", new Vector3(0f, 0.021f, -0.065f), new Vector3(0.024f, 0.006f, 0.018f), led);
        EditorUtility.SetDirty(usb);
        EditorUtility.SetDirty(usb.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("USB clue placed at vending machine top: " + top);
    }

    private static void CreateUsbPart(Transform parent, string partName, Vector3 localPosition, Vector3 localScale, Material material)
    {
        Transform existing = parent.Find(partName);
        if (existing != null)
        {
            existing.localPosition = localPosition;
            existing.localScale = localScale;
            MeshRenderer existingRenderer = existing.GetComponent<MeshRenderer>();
            if (existingRenderer != null) existingRenderer.sharedMaterial = material;
            return;
        }

        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent, false);
        part.transform.localPosition = localPosition;
        part.transform.localRotation = Quaternion.identity;
        part.transform.localScale = localScale;
        Collider collider = part.GetComponent<Collider>();
        if (collider != null) Object.DestroyImmediate(collider);
        MeshRenderer renderer = part.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.sharedMaterial = material;
        EditorUtility.SetDirty(part);
    }

    private static Material GetOrCreateUsbMaterial(string fileName, Color color, float metallic, float emission)
    {
        string path = "Assets/Materials/" + fileName;
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        material = new Material(shader);
        material.name = System.IO.Path.GetFileNameWithoutExtension(fileName);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", 0.78f);
        if (emission > 0f && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emission);
        }
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void AddArchiveShellEnhancements(Scene scene)
    {
        Material ceiling = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Archive_Floor.mat");
        Material wall = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Archive_DeepBlue.mat");
        Material trim = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Archive_Walnut.mat");
        EnsureCube("Archive Ceiling", new Vector3(0f, 5.02f, 0f), new Vector3(12f, 0.16f, 12f), ceiling);
        EnsureCube("Archive Entrance Wall Left", new Vector3(-3.55f, 2.5f, -5.9f), new Vector3(4.7f, 5f, 0.2f), wall);
        EnsureCube("Archive Entrance Wall Right", new Vector3(3.55f, 2.5f, -5.9f), new Vector3(4.7f, 5f, 0.2f), wall);
        EnsureCube("Archive Entrance Header", new Vector3(0f, 4.4f, -5.9f), new Vector3(2.4f, 1.2f, 0.2f), wall);
        EnsureCube("Archive Back Wall Baseboard", new Vector3(0f, 0.12f, 5.77f), new Vector3(11.7f, 0.24f, 0.08f), trim);
        EnsureCube("Archive Left Wall Baseboard", new Vector3(-5.77f, 0.12f, 0f), new Vector3(0.08f, 0.24f, 11.6f), trim);
        EnsureCube("Archive Right Wall Baseboard", new Vector3(5.77f, 0.12f, 0f), new Vector3(0.08f, 0.24f, 11.6f), trim);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void EnsureCube(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.Find(objectName);
        if (cube == null) cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = objectName;
        cube.transform.position = position;
        cube.transform.rotation = Quaternion.identity;
        cube.transform.localScale = scale;
        MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
        if (renderer != null && material != null) renderer.sharedMaterial = material;
        EditorUtility.SetDirty(cube);
    }

    private static void AddCeilingFixture(Scene scene, string name, Vector3 position, Color lightColor, float intensity)
    {
        GameObject fixture = GameObject.Find(name);
        if (fixture == null) fixture = InstantiatePrefab("(Prb)CeilingLight2.prefab", name, scene);
        if (fixture == null) return;
        fixture.transform.position = position;
        fixture.transform.rotation = Quaternion.identity;
        Light light = fixture.GetComponentInChildren<Light>();
        if (light == null)
        {
            GameObject source = new GameObject("Archive Light Source");
            source.transform.SetParent(fixture.transform, false);
            source.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            light = source.AddComponent<Light>();
        }
        light.type = LightType.Point;
        light.color = lightColor;
        light.intensity = intensity;
        light.range = 7.5f;
        light.shadows = LightShadows.None;
        EditorUtility.SetDirty(fixture);
        EditorUtility.SetDirty(light);
    }

    private static void AddPrefabOnDesk(string prefabFile, string objectName, GameObject desk,
        Vector3 offset, float yaw, float scale)
    {
        if (GameObject.Find(objectName) != null) return;
        if (!TryGetBounds(desk, out Bounds surface)) return;
        GameObject instance = InstantiatePrefab(prefabFile, objectName, desk.scene);
        if (instance == null) return;
        instance.transform.localScale *= scale;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.position = new Vector3(surface.center.x + offset.x,
            surface.max.y + 2f, surface.center.z + offset.z);
        SetBottomAt(instance, surface.max.y + 0.015f);
    }

    private static void AddPrefabOnShelf(string prefabFile, string objectName, string shelfName,
        Vector3 offset, float yaw, float scale)
    {
        if (GameObject.Find(objectName) != null) return;
        GameObject shelf = GameObject.Find(shelfName);
        if (shelf == null || !TryGetBounds(shelf, out Bounds shelfBounds)) return;
        GameObject instance = InstantiatePrefab(prefabFile, objectName, shelf.scene);
        if (instance == null) return;
        instance.transform.localScale *= scale;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        float shelfLevelY = shelfBounds.min.y + offset.y;
        instance.transform.position = new Vector3(shelfBounds.center.x + offset.x,
            shelfLevelY + 2f, shelfBounds.center.z + offset.z);
        SetBottomAt(instance, shelfLevelY + 0.012f);
    }

    private static void AddPrefabAtFloor(string prefabFile, string objectName, Vector3 position, float yaw, float scale)
    {
        if (GameObject.Find(objectName) != null) return;
        GameObject instance = InstantiatePrefab(prefabFile, objectName, SceneManager.GetActiveScene());
        if (instance == null) return;
        instance.transform.localScale *= scale;
        instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        instance.transform.position = position + Vector3.up * 2f;
        SetBottomAt(instance, 0.01f);
    }

    private static GameObject InstantiatePrefab(string prefabFile, string objectName, Scene scene)
    {
        string path = PrefabFolder + prefabFile;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogWarning("Nappin prefab not found: " + path);
            return null;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(prefab, scene) as GameObject;
        if (instance != null) instance.name = objectName;
        return instance;
    }

    private static void SetBottomAt(GameObject instance, float y)
    {
        if (!TryGetBounds(instance, out Bounds bounds))
        {
            Vector3 p = instance.transform.position;
            instance.transform.position = new Vector3(p.x, y, p.z);
            return;
        }
        instance.transform.position += Vector3.up * (y - bounds.min.y);
        EditorUtility.SetDirty(instance);
    }

    private static bool TryGetBounds(GameObject root, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        bool found = false;
        bounds = new Bounds();
        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled) continue;
            if (!found) { bounds = renderer.bounds; found = true; }
            else bounds.Encapsulate(renderer.bounds);
        }
        return found;
    }
}
#endif

