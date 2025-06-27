using UnityEngine;
using UnityEngine.UI;
public class FollowCursor : MonoBehaviour
{
    private RectTransform rectTransform;
    private Image cursorImage;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cursorImage = GetComponent<Image>();
        cursorImage.color = Color.clear;
    }

    void Update()
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            Input.mousePosition,
            null, // null for Screen Space - Overlay; otherwise, pass the camera
            out pos
        );

        rectTransform.anchoredPosition = pos;
    }

    public void setCursorItem(string newItem)
    {
        if (string.IsNullOrEmpty(newItem))
        {
            cursorImage.color = Color.clear; // Hide cursor image if no item is set
        }
        else
        {
            Debug.Log("Setting cursor item to: " + newItem);
            cursorImage.sprite = Resources.Load<Sprite>("Images/" + newItem + "Image");
            cursorImage.color = Color.white;
        }
    }


}
