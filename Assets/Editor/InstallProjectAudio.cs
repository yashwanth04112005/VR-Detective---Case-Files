#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Creates the persistent audio manager in the main menu and wires the generated sound library.</summary>
public static class InstallProjectAudio
{
    private const string MainMenuPath = "Assets/Scenes/00_MainMenu.unity";
    private const string AudioPath = "Assets/Audio/";

    [MenuItem("Tools/VR Detective/Install Project Audio")]
    public static void Install()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        AssetDatabase.Refresh();

        string[] names =
        {
            "ambient_office", "ambient_archive", "sfx_clue_found", "sfx_usb_found",
            "sfx_page_turn", "sting_case_solved", "sting_unresolved", "sfx_ui_click"
        };

        AudioClip[] clips = new AudioClip[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioPath + names[i] + ".wav");
            if (clips[i] == null)
            {
                Debug.LogError("Missing audio clip: " + AudioPath + names[i] + ".wav");
                return;
            }
        }

        Scene scene = EditorSceneManager.OpenScene(MainMenuPath, OpenSceneMode.Single);
        GameObject audioObject = GameObject.Find("AudioManager");
        if (audioObject == null)
        {
            audioObject = new GameObject("AudioManager");
        }

        AudioManager manager = audioObject.GetComponent<AudioManager>();
        if (manager == null)
        {
            manager = audioObject.AddComponent<AudioManager>();
        }

        manager.sounds = new[]
        {
            Entry(names[0], clips[0], 0.09f, true),
            Entry(names[1], clips[1], 0.09f, true),
            Entry(names[2], clips[2], 0.38f, false),
            Entry(names[3], clips[3], 0.42f, false),
            Entry(names[4], clips[4], 0.24f, false),
            Entry(names[5], clips[5], 0.42f, false),
            Entry(names[6], clips[6], 0.30f, false),
            Entry(names[7], clips[7], 0.32f, false)
        };

        GameObject stateObject = GameObject.Find("GameStateManager");
        if (stateObject == null)
        {
            stateObject = new GameObject("GameStateManager");
        }

        if (stateObject.GetComponent<GameStateManager>() == null)
        {
            stateObject.AddComponent<GameStateManager>();
        }

        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(stateObject);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, MainMenuPath);
        AssetDatabase.SaveAssets();
        Debug.Log("Project audio installed: office/archive ambience, clue sounds, and deduction stings.");
    }

    private static AudioManager.SoundEntry Entry(string name, AudioClip clip, float volume, bool loop)
    {
        return new AudioManager.SoundEntry
        {
            name = name,
            clip = clip,
            volume = volume,
            loop = loop
        };
    }
}
#endif