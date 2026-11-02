using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent singleton audio manager.
/// Attach to a GameObject in your first scene; it will survive all scene loads.
///
/// SETUP IN INSPECTOR:
///   Add SoundEntry rows for each named sound used by the game:
///     "ambient_office"    – loop: true
///     "sfx_clue_found"    – loop: false
///     "sfx_usb_found"     – loop: false
///     "sfx_page_turn"     – loop: false
///     "sting_case_solved" – loop: false
///     "sting_unresolved"  – loop: false
///     "ambient_terminal"  – loop: true
/// </summary>
public class AudioManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inner data class
    // -------------------------------------------------------------------------

    [System.Serializable]
    public class SoundEntry
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        public bool loop = false;
    }

    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    /// <summary>Global singleton reference.</summary>
    public static AudioManager Instance { get; private set; }

    [Header("Sound Library")]
    [Tooltip("Define every named sound the game uses. Loop flag and volume are set per-entry.")]
    public SoundEntry[] sounds;

    /// <summary>Runtime lookup: sound name → its dedicated AudioSource.</summary>
    private Dictionary<string, AudioSource> audioSources;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        // --- Singleton enforcement ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // --- Build one AudioSource per SoundEntry ---
        audioSources = new Dictionary<string, AudioSource>();

        if (sounds == null)
        {
            return;
        }

        foreach (SoundEntry entry in sounds)
        {
            if (string.IsNullOrWhiteSpace(entry.name))
            {
                Debug.LogWarning("[AudioManager] A SoundEntry has an empty name and will be skipped.");
                continue;
            }

            if (audioSources.ContainsKey(entry.name))
            {
                Debug.LogWarning($"[AudioManager] Duplicate sound name '{entry.name}' – skipping second entry.");
                continue;
            }

            // Create a dedicated child GameObject so each sound is independently addressable.
            GameObject child = new GameObject($"AudioSource_{entry.name}");
            child.transform.SetParent(transform);

            AudioSource src = child.AddComponent<AudioSource>();
            src.clip         = entry.clip;
            src.volume       = entry.volume;
            src.loop         = entry.loop;
            src.playOnAwake  = false;

            audioSources[entry.name] = src;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Stop("ambient_office");
        Stop("ambient_archive");
        Stop("suspense_bgm");

        if (scene.name == "01_CaseRoom")
        {
            Play("ambient_office");
            Play("suspense_bgm");
        }
        else if (scene.name == "02_ResearchArchive")
        {
            Play("ambient_archive");
            Play("suspense_bgm");
        }
        else if (scene.name == "03_DetectiveBoard" || scene.name == "04_CaseConclusion")
        {
            Play("suspense_bgm");
        }
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    /// <summary>
    /// Plays the named sound.
    /// If the sound is a looping track that is already playing, the call is ignored
    /// to prevent restarting mid-playback.
    /// </summary>
    public void Play(string soundName)
    {
        if (!TryGetSource(soundName, out AudioSource src))
        {
            return;
        }

        // Don't restart a looping track that is already running.
        if (src.loop && src.isPlaying)
        {
            return;
        }

        src.Play();
    }

    /// <summary>
    /// Stops the named sound immediately.
    /// </summary>
    public void Stop(string soundName)
    {
        if (!TryGetSource(soundName, out AudioSource src))
        {
            return;
        }

        src.Stop();
    }

    /// <summary>
    /// Plays the named sound as a one-shot (does not interrupt loops or other one-shots).
    /// Useful for repeated SFX that can overlap.
    /// </summary>
    public void PlayOneShot(string soundName)
    {
        if (!TryGetSource(soundName, out AudioSource src))
        {
            return;
        }

        if (src.clip == null)
        {
            Debug.LogWarning($"[AudioManager] PlayOneShot: '{soundName}' has no AudioClip assigned.");
            return;
        }

        src.PlayOneShot(src.clip);
    }

    /// <summary>
    /// Stops every registered AudioSource immediately.
    /// </summary>
    public void StopAll()
    {
        foreach (AudioSource src in audioSources.Values)
        {
            if (src != null && src.isPlaying)
            {
                src.Stop();
            }
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Looks up the AudioSource for <paramref name="soundName"/>.
    /// Logs a warning and returns <c>false</c> when not found.
    /// </summary>
    private bool TryGetSource(string soundName, out AudioSource src)
    {
        if (audioSources != null && audioSources.TryGetValue(soundName, out src))
        {
            return true;
        }

        Debug.LogWarning($"[AudioManager] Sound '{soundName}' not found in the sound library.");
        src = null;
        return false;
    }
}
