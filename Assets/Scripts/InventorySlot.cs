using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private GameObject imageObject;
    public string itemName;
    public PlayerInventory playerInventory;
    public bool isHovered = false;
    public int slotIndex = 0;
    public Color color1 = new Color(255f, 255f, 255f, 150f);
    public Color color2 = new Color(255f, 255f, 255f, 150f);

    private Image slotImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemName = playerInventory.inventory[0];
        imageObject = gameObject.transform.GetChild(0).gameObject;
        slotImage = gameObject.transform.GetChild(0).gameObject.GetComponent<Image>();
        slotImage.sprite = Resources.Load<Sprite>("Images/" + itemName + "Image");

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        gameObject.GetComponent<UnityEngine.UI.Image>().color = color1;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        gameObject.GetComponent<UnityEngine.UI.Image>().color = color2;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("CLICKED on " + gameObject.name);

        string tempCursorItem = playerInventory.getCursorItem();

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Left Click!");

            
            if(string.IsNullOrEmpty(itemName))
            {
                if(string.IsNullOrEmpty(tempCursorItem))
                {
                    Debug.Log("No item to place in " + gameObject.name);
                }
                else
                {
                    // Place the cursor item into this slot
                    itemName = tempCursorItem;
                    playerInventory.setCursorItem(null); // Clear the cursor item
                    slotImage.sprite = Resources.Load<Sprite>("Images/" + itemName + "Image");
                    slotImage.color = Color.white; // Reset color to white
                    Debug.Log("Placed " + itemName + " in slot " + slotIndex);
                }
            }
            else
            {
                if(string.IsNullOrEmpty(tempCursorItem))
                {
                    // Move item from this slot to cursor
                    playerInventory.setCursorItem(itemName);
                    itemName = null; // Clear this slot
                    slotImage.sprite = null; // Clear the image
                    slotImage.color = Color.clear; // Set color to transparent
                    Debug.Log("Moved " + itemName + " to cursor from slot " + slotIndex);
                }
                else
                {
                    // Swap items between cursor and this slot
                    playerInventory.setCursorItem(itemName);
                    itemName = tempCursorItem;
                    slotImage.sprite = Resources.Load<Sprite>("Images/" + itemName + "Image");
                    slotImage.color = Color.white; // Reset color to white
                    Debug.Log("Swapped " + itemName + " with cursor item in slot " + slotIndex);
                }
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right Click!");
        }
    }

    public void SetItem(string itemName)
    {
        this.itemName = itemName;
        slotImage.sprite = Resources.Load<Sprite>("Images/" + itemName + "Image");
        slotImage.color = Color.white; // Reset color to white
    }

}
