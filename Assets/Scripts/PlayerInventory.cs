using UnityEngine;
using TMPro;

public class PlayerInventory : MonoBehaviour
{

    private bool isInventoryOpen = false;
    private string[] inventory = new string[10]; // Example inventory with 10 slots
    public GameObject inventoryObject;
    private TextMeshProUGUI inventoryText;
    private bool isInventoryEmpty = true;
    private bool isInventoryFull = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inventoryObject.SetActive(false); // Ensure inventory UI is hidden at start
        inventoryText = inventoryObject.GetComponentInChildren<TextMeshProUGUI>();
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

        // If inventory is empty, set inventoryObject text to "Inventory Empty"
        if (isInventoryEmpty)
        {
            inventoryText.text = "Inventory Empty";
        }
        else
        {
            // If inventory is not empty, display the items in the inventory
            string inventoryDisplay = "Inventory:\n";
            for (int i = 0; i < inventory.Length; i++)
            {
                if (!string.IsNullOrEmpty(inventory[i]))
                {
                    inventoryDisplay += $"{i + 1}: {inventory[i]}\n"; // Display item with its slot number
                }
            }
            inventoryText.text = inventoryDisplay;
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
                    isInventoryEmpty = false; // Inventory is no longer empty
                    break;
                }
            }
        }
    }
}
