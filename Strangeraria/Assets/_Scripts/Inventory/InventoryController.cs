using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [Header("Références")]
    public InventoryUI inventoryUI;
    public InventoryModel inventory;
    public PlayerEquip playerEquip;
    public GameObject inventoryUIPanel;

    [Header("Input Actions")]
    public InputActionReference toggleInventoryAction;
    public InputActionReference[] hotbarActions = new InputActionReference[8];

    [Header("Default setup")]
    public List<ItemDefinition> defaultItems = new List<ItemDefinition>();

    private void OnEnable()
    {
        toggleInventoryAction.action.performed += OnToggleInventory;
        foreach (var a in hotbarActions) if (a != null) a.action.performed += OnHotbarPressed;
        toggleInventoryAction.action.Enable();
        foreach (var a in hotbarActions) if (a != null) a.action.Enable();
    }

    private void OnDisable()
    {
        toggleInventoryAction.action.performed -= OnToggleInventory;
        foreach (var a in hotbarActions) if (a != null) a.action.performed -= OnHotbarPressed;
        toggleInventoryAction.action.Disable();
        foreach (var a in hotbarActions) if (a != null) a.action.Disable();
    }

    private void Start()
    {
        EquipDefaultWeapon();
    }

    private void EquipDefaultWeapon()
    {
        if (inventory == null)
        {
            Debug.LogWarning("[InventoryController] inventory not assigned.");
            return;
        }


        if (playerEquip == null)
        {
            Debug.LogWarning("[InventoryController] playerEquip not assigned.");
        }

        if (defaultItems == null || defaultItems.Count == 0)
            return;

        foreach (var item in defaultItems)
        {
            if (item == null) continue;
            inventory.AddItem(item);
        }

        playerEquip.Equip(defaultItems[0]);
    }


    private void OnToggleInventory(InputAction.CallbackContext ctx)
    {
        if (inventoryUIPanel != null)
        {
            bool newState = !inventoryUIPanel.activeSelf;
            inventoryUIPanel.SetActive(newState);

            if (newState && inventoryUI != null)
            {
                inventoryUI.BuildGridIfNeeded();
            }
        }
    }


    private void OnHotbarPressed(InputAction.CallbackContext ctx)
    {
        for (int i = 0; i < hotbarActions.Length; i++)
        {
            if (hotbarActions[i] == null) continue;
            if (hotbarActions[i].action == ctx.action) 
            {
                Debug.Log($"Action n: {i} enclenché");
                EquipHotbarIndex(i);
                return;
            }
        }
    }

    public void EquipHotbarIndex(int hotbarIndex)
    {
        if (hotbarIndex < 0 || hotbarIndex >= inventory.hotbarSize) return;
        var slot = inventory.GetSlot(hotbarIndex);
        if (slot.IsEmpty)
        {
            playerEquip.Unequip();
            return;
        }

        var item = slot.item;
        if (item == null) return;

        switch (item.itemType)
        {
            case ItemType.Weapon:
                playerEquip.Equip(item);
                playerEquip.equippedHotbarIndex = hotbarIndex;
                break;

            case ItemType.Block:
                playerEquip.EquipBlock(item);
                playerEquip.equippedHotbarIndex = hotbarIndex;
                break;

            case ItemType.Resource:
                bool consumed = playerEquip.UseResource(item);
                if (consumed)
                {
                    inventory.RemoveOneAt(hotbarIndex);
                }
                break;

            default:
                Debug.Log($"Unhandled itemType {item.itemType}");
                break;
        }
    }
}
