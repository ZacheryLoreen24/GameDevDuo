using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerInventory : MonoBehaviour
{

    private bool isInventoryOpen = false;
    public string[] inventory = new string[10]; // Example inventory with 10 slots
    public GameObject inventoryObject;
    private bool isInventoryFull = false;
    public MouseMovement mouseMovment;
    private float sensitivity;
    private GameObject slotObjectRef;
    private string cursorItem;
    public GameObject CursorImage;
    private FollowCursor cursorImageScriptRef;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryObject.SetActive(false); // Ensure inventory UI is hidden at start
        sensitivity = mouseMovment.mouseSensitivity;
        cursorImageScriptRef = CursorImage.GetComponent<FollowCursor>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check for key press to open inventory
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Toggle inventory UI visibility
            ToggleInventoryUI();
        }

    }

    private void ToggleInventoryUI()
    {
        // Logic to show or hide the inventory UI
        isInventoryOpen = !isInventoryOpen;
        inventoryObject.SetActive(isInventoryOpen);
        Debug.Log("Inventory toggled: " + !isInventoryOpen + " --> " + isInventoryOpen);

        // enable or disable the cursor based on inventory state
        if (isInventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None; // Unlock cursor when inventory is open
            Cursor.visible = true; // Make cursor visible
            mouseMovment.mouseSensitivity = 0; // Disable mouse movement when inventory is open
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Lock cursor when inventory is closed
            Cursor.visible = false; // Hide cursor
            mouseMovment.mouseSensitivity = sensitivity; // Restore mouse movement sensitivity
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        // Example logic to add an item to the inventory when colliding with a collectible
        if (other.CompareTag("Item"))
        {
            if (isInventoryFull)
            {
                Debug.Log("Inventory is full. Cannot add more items.");
                return; // Exit if inventory is full
            }
            for (int i = 0; i < inventory.Length; i++)
            {
                if (string.IsNullOrEmpty(inventory[i]))
                {
                    inventory[i] = other.gameObject.name; // Add item to the first empty slot
                    Debug.Log("Added " + other.gameObject.name + " to inventory slot " + i);
                    slotObjectRef = inventoryObject.transform.Find("InventoryBase").gameObject.transform.Find("InventorySlot_" + i).gameObject;
                    InventorySlot slotScriptRef = slotObjectRef.GetComponent<InventorySlot>();
                    slotScriptRef.SetItem(inventory[i]);
                    Debug.Log("LOOK AT THIS SHIT: " + slotScriptRef.itemName);
                    break;
                }
            }
        }
    }

    public void setCursorItem(string newItem)
    {
        cursorItem = newItem;
        cursorImageScriptRef.setCursorItem(newItem);
    }

    public string getCursorItem()
    {
        return cursorItem;
    }
}
