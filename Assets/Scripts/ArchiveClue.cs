using UnityEngine;

/// <summary>
/// Narrative evidence found in the Research Archive scene.
/// </summary>
public class ArchiveClue : MonoBehaviour
{
    [Header("Archive Record")]
    public string clueId;
    public string clueTitle;

    [TextArea(3, 8)]
    public string description;
}