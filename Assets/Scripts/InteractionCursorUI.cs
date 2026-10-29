using UnityEngine;
using UnityEngine.UI;

public class InteractionCursorUI : MonoBehaviour
{
    [Header("Cursor Settings")]
    public Image cursorImage;
    public Color normalColor = Color.white;
    public Color interactColor = Color.yellow;

    public void SetInteractable(bool canInteract)
    {
        if (cursorImage == null)
        {
            return;
        }

        cursorImage.color = canInteract ? interactColor : normalColor;
    }
}