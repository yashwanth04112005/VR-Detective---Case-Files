using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// Persistent HUD badge displayed in the top-right corner.
/// Shows live evidence progress in the format "🔍 {collected} / {total}".
///
/// SETUP:
///   1. Place this script on the badge UI GameObject (Canvas → Badge panel).
///   2. Assign a TMP_Text component to 'Badge Text' in the Inspector.
///   3. Ensure GameStateManager is present in the scene so its events can be subscribed to.
/// </summary>
public class HUDBadge : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("UI Reference")]
    [Tooltip("TMP_Text that displays the evidence counter, e.g. '🔍 2 / 5'.")]
    public TMP_Text badgeText;

    [Header("Pulse Animation")]
    [Tooltip("Total duration of one pulse animation in seconds.")]
    public float pulseDuration = 0.2f;

    [Tooltip("Peak uniform scale reached at the midpoint of the pulse.")]
    public float pulseScale = 1.3f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    /// <summary>Tracked so a new pulse can cancel an in-progress one cleanly.</summary>
    private Coroutine pulseCoroutine;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Start()
    {
        // Subscribe to evidence change events from GameStateManager.
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnEvidenceUpdated += OnEvidenceChanged;
        }
        else
        {
            Debug.LogWarning("[HUDBadge] GameStateManager.Instance is null in Start(). " +
                             "Badge will not receive live updates.");
        }

        // Populate the text immediately with the current state.
        OnEvidenceChanged();
    }

    private void OnDestroy()
    {
        // Null-safe unsubscription to avoid dangling delegate references.
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.OnEvidenceUpdated -= OnEvidenceChanged;
        }
    }

    // -------------------------------------------------------------------------
    // Event handler
    // -------------------------------------------------------------------------

    /// <summary>
    /// Called whenever the evidence count changes.
    /// Updates the badge text and triggers the pulse animation.
    /// </summary>
    private void OnEvidenceChanged()
    {
        if (GameStateManager.Instance == null)
        {
            return;
        }

        int count = GameStateManager.Instance.EvidenceCollected;
        int total = GameStateManager.Instance.TotalEvidenceRequired;

        if (badgeText != null)
        {
            badgeText.text = $"🔍 {count} / {total}";
        }

        // Trigger pulse; cancel any pulse already running to avoid overlapping coroutines.
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            // Snap back to neutral scale before restarting so animation never hangs mid-way.
            transform.localScale = Vector3.one;
        }

        pulseCoroutine = StartCoroutine(PulseEffect());
    }

    // -------------------------------------------------------------------------
    // Animation coroutine
    // -------------------------------------------------------------------------

    /// <summary>
    /// Scales the badge up to <see cref="pulseScale"/> then back to 1 over
    /// <see cref="pulseDuration"/> seconds using linear interpolation.
    /// </summary>
    private IEnumerator PulseEffect()
    {
        float halfDuration = pulseDuration / 2f;

        Vector3 normalScale = Vector3.one;
        Vector3 bigScale    = new Vector3(pulseScale, pulseScale, 1f);

        // Phase 1 – scale up.
        float elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            transform.localScale = Vector3.Lerp(normalScale, bigScale, t);
            yield return null;
        }

        // Ensure we reach the target exactly before beginning phase 2.
        transform.localScale = bigScale;

        // Phase 2 – scale back down.
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / halfDuration);
            transform.localScale = Vector3.Lerp(bigScale, normalScale, t);
            yield return null;
        }

        // Guarantee clean final state.
        transform.localScale = normalScale;
        pulseCoroutine = null;
    }
}
