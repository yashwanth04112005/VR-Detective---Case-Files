using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildEvidenceBoardScene
{
    private const string SceneFolder = "Assets/Scenes/";
    private const string BoardPath = SceneFolder + "03_DetectiveBoard.unity";
    private const string OldConclusionPath = SceneFolder + "03_CaseConclusion.unity";
    private const string ConclusionPath = SceneFolder + "04_CaseConclusion.unity";
    private const string ArchivePath = SceneFolder + "02_ResearchArchive.unity";

    [MenuItem("Tools/VR Detective/Build Detective Board Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(OldConclusionPath) != null &&
            AssetDatabase.LoadAssetAtPath<SceneAsset>(ConclusionPath) == null)
        {
            string moveError = AssetDatabase.MoveAsset(OldConclusionPath, ConclusionPath);
            if (!string.IsNullOrEmpty(moveError))
            {
                EditorUtility.DisplayDialog("Scene Rename Failed", moveError, "OK");
                return;
            }
        }

        Scene boardScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject flow = new GameObject("Detective Board Flow");
        flow.AddComponent<DetectiveBoardController>();
        GameObject cameraObject = new GameObject("Board UI Camera", typeof(Camera));
        Camera boardCamera = cameraObject.GetComponent<Camera>();
        boardCamera.tag = "MainCamera";
        boardCamera.clearFlags = CameraClearFlags.SolidColor;
        boardCamera.backgroundColor = new Color(0.035f, 0.052f, 0.073f);
        boardCamera.cullingMask = 0;
        boardCamera.nearClipPlane = 0.1f;
        boardCamera.farClipPlane = 100f;
        EditorSceneManager.SaveScene(boardScene, BoardPath);

        UpdateArchiveDestination();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Detective Board Ready",
            "The evidence board scene is built and the game flow is now Case Room → Research Archive → Detective Board → Case Conclusion.", "OK");
    }

    private static void UpdateArchiveDestination()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ArchivePath) == null) return;
        Scene archiveScene = EditorSceneManager.OpenScene(ArchivePath, OpenSceneMode.Single);
        foreach (GameObject root in archiveScene.GetRootGameObjects())
        {
            foreach (ArchiveSceneController controller in root.GetComponentsInChildren<ArchiveSceneController>(true))
            {
                controller.conclusionScene = "03_DetectiveBoard";
                EditorUtility.SetDirty(controller);
            }
        }
        EditorSceneManager.SaveScene(archiveScene);
        EditorSceneManager.OpenScene(BoardPath, OpenSceneMode.Single);
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            string path = scene.path == OldConclusionPath ? ConclusionPath : scene.path;
            if (path == BoardPath) continue;
            scenes.Add(new EditorBuildSettingsScene(path, scene.enabled));
        }

        int archiveIndex = scenes.FindIndex(scene => scene.path == ArchivePath);
        int insertionIndex = archiveIndex >= 0 ? archiveIndex + 1 : scenes.Count;
        scenes.Insert(insertionIndex, new EditorBuildSettingsScene(BoardPath, true));
        if (!scenes.Exists(scene => scene.path == ConclusionPath))
            scenes.Add(new EditorBuildSettingsScene(ConclusionPath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
