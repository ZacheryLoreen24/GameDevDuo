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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        itemName = playerInventory.inventory[0];
        imageObject = gameObject.transform.GetChild(0).gameObject;
        if (string.IsNullOrEmpty(itemName))
        {
            imageObject.GetComponent<Image>().sprite = null;
        }
        else
        {
            //imageObject.GetComponent<Image>().sprite = playerInventory.inventory[slotIndex];
        }

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
        // You can check which mouse button too:
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            Debug.Log("Left Click!");
            // Check if gameObject has children and log their names
            if (gameObject.transform.childCount > 0)
            {
                foreach (Transform child in gameObject.transform)
                {
                    Debug.Log("Child: " + child.name);
                }
            }
            else
            {
                Debug.Log("No children found in " + gameObject.name);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log("Right Click!");
        }
    }

}
