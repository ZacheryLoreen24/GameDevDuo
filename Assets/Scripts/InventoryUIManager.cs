using UnityEngine;
using UnityEngine.UI;

public class InventoryUIManager : MonoBehaviour
{
    public GameObject playerRef;
    private PlayerInventory playerInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInventory = playerRef.GetComponent<PlayerInventory>();
    }

    // Update is called once per frame
    void Update()
    {
        // if tab pressed, update UI
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Logic to update the inventory UI
            UpdateInventoryUI();
        }
    }

    void UpdateInventoryUI()
    {
        // Logic to update the inventory UI
        // This could involve refreshing the displayed items, updating counts, etc.
        Debug.Log("Inventory UI updated");

        // Update the each child named "InventorySlot_i" where i is the index of the array in the PlayerInventory script, which contains the name of the item to be updated for the slot
        for (int i = 0; i < playerInventory.inventory.Length; i++)
        {
            string itemName = playerInventory.inventory[i];
            GameObject slot = GameObject.Find("InventorySlot_" + i);
            if (slot != null)
            {
                Image imageComponent = slot.GetComponentInChildren<Image>();
                if (imageComponent != null)
                {
                    if (string.IsNullOrEmpty(itemName))
                    {
                        imageComponent.sprite = null; // Clear the sprite if no item
                    }
                    else
                    {
                        imageComponent.sprite = Resources.Load<Sprite>("Images/" + itemName + "Image");
                        if (imageComponent.sprite == null)
                        {
                            Debug.LogWarning("Sprite not found for item: " + itemName);
                        }
                    }
                }
            }
        }
    }
}
