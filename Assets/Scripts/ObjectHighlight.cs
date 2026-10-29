using UnityEngine;

public class ObjectHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    public Color highlightColor = Color.yellow;

    private Renderer[] objectRenderers;
    private Color[] originalColors;
    private bool isHighlighted = false;

    private void Awake()
    {
        objectRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[objectRenderers.Length];

        for (int i = 0; i < objectRenderers.Length; i++)
        {
            originalColors[i] = objectRenderers[i].material.color;
        }
    }

    public void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted)
        {
            return;
        }

        isHighlighted = highlighted;

        for (int i = 0; i < objectRenderers.Length; i++)
        {
            if (highlighted)
            {
                objectRenderers[i].material.color = highlightColor;
            }
            else
            {
                objectRenderers[i].material.color = originalColors[i];
            }
        }
    }
}