using UnityEngine;

/// <summary>
/// Plays randomised footstep sounds while the player is grounded and moving.
///
/// SETUP:
///   1. Attach to the same GameObject that has a CharacterController (i.e. the Player root).
///   2. Populate the 'Footstep Clips' array with 2–4 footstep audio variations.
///   3. Tune 'Step Interval' and 'Footstep Volume' as needed.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepAudio : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Audio Clips")]
    [Tooltip("Pool of footstep sound variations; one is chosen at random each step.")]
    public AudioClip[] footstepClips;

    [Tooltip("Separate, heavier clips used while sprinting. Falls back to Footstep Clips if empty.")]
    public AudioClip[] sprintFootstepClips;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    [Tooltip("Playback volume for every footstep.")]
    public float footstepVolume = 0.4f;

    [Tooltip("Minimum time in seconds between consecutive footstep sounds.")]
    public float stepInterval = 0.44f;

    [Tooltip("Time between steps while sprinting.")]
    public float sprintStepInterval = 0.26f;

    [Tooltip("Slight pitch increase for faster running steps.")]
    [Range(0.8f, 1.5f)]
    public float sprintPitch = 1.12f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    /// <summary>Reference to the CharacterController used to detect grounded state.</summary>
    private CharacterController controller;

    /// <summary>Counts down to the next permitted footstep.</summary>
    private float stepTimer;

    /// <summary>Dedicated AudioSource for footstep playback (2-D, non-spatial).</summary>
    private AudioSource audioSource;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        controller = GetComponent<CharacterController>();

        // Use an existing AudioSource on this GameObject if one is already present,
        // otherwise add a new one so footstep audio doesn't conflict with other sources.
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Full 2-D sound – footsteps should feel equally present regardless of camera angle.
        audioSource.spatialBlend = 0f;
        audioSource.volume       = 1f;
        audioSource.playOnAwake  = false;
        audioSource.loop         = false;

        // Start the timer so the first step fires promptly after the player begins walking.
        stepTimer = stepInterval;
    }

    private void Update()
    {
        // Guard: no clips configured → nothing to play.
        if (footstepClips == null || footstepClips.Length == 0)
        {
            return;
        }

        // Determine horizontal movement from raw Input axes (same axes used by most FPS setups).
        float horizontal = Input.GetAxis("Horizontal");
        float vertical   = Input.GetAxis("Vertical");
        Vector2 inputVector = new Vector2(horizontal, vertical);
        bool isMoving = inputVector.magnitude > 0.1f;

        bool sprinting = isMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        float activeInterval = sprinting ? sprintStepInterval : stepInterval;

        if (controller.isGrounded && isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayRandomFootstep(sprinting);
                stepTimer = activeInterval;
            }
        }
        else
        {
            // Reset timer while idle so the next step fires immediately when movement resumes.
            stepTimer = stepInterval;
        }
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Picks a random clip from <see cref="footstepClips"/> and plays it as a one-shot
    /// so that rapid steps can overlap without cutting each other off.
    /// </summary>
    private void PlayRandomFootstep(bool sprinting)
    {
        AudioClip[] clipPool = sprinting && sprintFootstepClips != null && sprintFootstepClips.Length > 0
            ? sprintFootstepClips : footstepClips;
        if (clipPool == null || clipPool.Length == 0) return;

        AudioClip clip = null;
        int attempts = 0;
        while (clip == null && attempts < clipPool.Length * 2)
        {
            clip = clipPool[Random.Range(0, clipPool.Length)];
            attempts++;
        }

        if (clip == null) return;
        audioSource.pitch = sprinting ? sprintPitch : 1f;
        audioSource.PlayOneShot(clip, footstepVolume);
    }
}
