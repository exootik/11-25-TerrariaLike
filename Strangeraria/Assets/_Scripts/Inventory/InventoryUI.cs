using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public InventoryModel inventoryModel;
    public Transform hotbarParent;   // toujours actif
    public Transform gridParent;     // enfant de InventoryPanel (qui peut être inactif)
    public GameObject slotPrefab;
    [Header("Options")]
    public bool createSlotsAtStart = true;

    private List<SlotUI> slotUIs = new List<SlotUI>();
    private bool builtHotbar = false;
    private bool builtGrid = false;

    private void Awake()
    {
        if (inventoryModel != null)
            inventoryModel.OnInventoryChanged += RefreshUI;
    }

    private void Start()
    {
        if (createSlotsAtStart)
        {
            BuildHotbarOnly();
        }
        RefreshUI();
    }

    private void OnDestroy()
    {
        if (inventoryModel != null)
            inventoryModel.OnInventoryChanged -= RefreshUI;
    }

    // Build only the hotbar slots (indices 0..hotbarSize-1)
    public void BuildHotbarOnly()
    {
        if (builtHotbar) return;
        if (inventoryModel == null || hotbarParent == null || slotPrefab == null)
        {
            Debug.LogError("InventoryUI: missing references for BuildHotbarOnly", this);
            return;
        }

        // clear existing hotbar children if any
        slotUIs.Clear();
        foreach (Transform t in hotbarParent) Destroy(t.gameObject);

        for (int i = 0; i < inventoryModel.hotbarSize; i++)
        {
            var go = Instantiate(slotPrefab, hotbarParent, false);
            Debug.Log($"Instantiated slot #{i}: activeSelf={go.activeSelf}, activeInHierarchy={go.activeInHierarchy}, parent={hotbarParent.name}, parentActiveInHierarchy={hotbarParent.gameObject.activeInHierarchy}");
            if (!go.activeSelf) go.SetActive(true);
            var ui = go.GetComponent<SlotUI>();
            if (ui == null) { Debug.LogError("SlotPrefab needs a SlotUI component", go); continue; }
            ui.slotIndex = i;
            slotUIs.Add(ui);
        }

        builtHotbar = true;
        Debug.Log($"InventoryUI: Built hotbar ({inventoryModel.hotbarSize} slots)");
    }

    // Build only the grid slots (indices hotbarSize .. totalSlots-1). Call when inventory panel is active.
    public void BuildGridIfNeeded()
    {
        if (builtGrid) return;
        if (inventoryModel == null || gridParent == null || slotPrefab == null)
        {
            Debug.LogError("InventoryUI: missing references for BuildGridIfNeeded", this);
            return;
        }

        // If hotbar not built, build it first to keep ordering consistent
        if (!builtHotbar) BuildHotbarOnly();

        for (int i = inventoryModel.hotbarSize; i < inventoryModel.totalSlots; i++)
        {
            var go = Instantiate(slotPrefab, gridParent, false);
            if (!go.activeSelf) go.SetActive(true); // safe: sets activeSelf true (activeInHierarchy still false if parent inactive)
            var ui = go.GetComponent<SlotUI>();
            if (ui == null) { Debug.LogError("SlotPrefab needs a SlotUI component", go); continue; }
            ui.slotIndex = i;
            slotUIs.Add(ui);
        }

        builtGrid = true;
        Debug.Log($"InventoryUI: Built grid ({inventoryModel.totalSlots - inventoryModel.hotbarSize} slots)");
        // Update visuals after build
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (inventoryModel == null) return;
        // if grid isn't built, only refresh built slots (hotbar)
        int toUpdate = Mathf.Min(slotUIs.Count, inventoryModel.totalSlots);
        for (int i = 0; i < toUpdate; i++)
        {
            var slot = inventoryModel.GetSlot(i);
            if (slotUIs[i] != null) slotUIs[i].Set(slot.item, slot.count);
        }
    }

    public void OnSlotClicked(int index)
    {
        Debug.Log($"Slot clicked {index}");
    }
}
