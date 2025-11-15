using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class SlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public Image icon;
    public Sprite defaultSlotSprite;
    public TextMeshProUGUI countText;
    public int slotIndex;

    [Header("Bindings (optionnel)")]
    public Canvas parentCanvas;            // assigner depuis l'inspector si possible
    public InventoryModel inventoryModel;  // assigner depuis l'inspector ou auto-find


    private GameObject dragIcon;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        if (icon == null) icon = GetComponentInChildren<Image>();
        if (countText == null) countText = GetComponentInChildren<TextMeshProUGUI>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>() ?? Object.FindFirstObjectByType<Canvas>();

        if (inventoryModel == null)
            inventoryModel = Object.FindFirstObjectByType<InventoryModel>();

        // Diagnostics
        if (icon == null) Debug.LogWarning($"SlotUI (index {slotIndex}) : icon is null on {name}");
        if (parentCanvas == null) Debug.LogWarning($"SlotUI (index {slotIndex}) : parentCanvas not found in scene!");
        if (inventoryModel == null) Debug.LogWarning($"SlotUI (index {slotIndex}) : inventoryModel not found!");
    }

    public void Set(ItemDefinition item, int count)
    {
        if (icon == null || countText == null) return;

        if (item == null || count <= 0)
        {
            icon.sprite = null;
            icon.enabled = false;
            countText.text = "";
        }
        else
        {
            icon.enabled = true;
            icon.sprite = item.icon;
            countText.text = count > 1 ? count.ToString() : "";
        }
    }

    // --- Drag handlers ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Defensive: si pas d'icone ou pas d'image -> rien à drag
        if (icon == null || icon.sprite == null)
        {
            Debug.Log("OnBeginDrag: nothing to drag (icon null or sprite null)");
            return;
        }

        // Guarantee parentCanvas
        if (parentCanvas == null)
        {
            parentCanvas = GetComponentInParent<Canvas>() ?? Object.FindFirstObjectByType<Canvas>();
            if (parentCanvas == null)
            {
                Debug.LogError("OnBeginDrag: No Canvas found in scene - cannot create drag icon.");
                return;
            }
        }

        // Register source early so drops can see it even if something later fails
        DragAndDropState.CurrentSource = this;

        try
        {
            // Create drag visual under Canvas
            dragIcon = new GameObject("DragIcon");
            dragIcon.transform.SetParent(parentCanvas.transform, false);

            var img = dragIcon.AddComponent<Image>();
            img.raycastTarget = false;
            img.sprite = icon.sprite;

            // copy size from source icon
            var srcRect = icon.rectTransform;
            var dstRect = dragIcon.GetComponent<RectTransform>();
            dstRect.sizeDelta = srcRect.sizeDelta;
            dstRect.pivot = srcRect.pivot;

            // position
            dragIcon.transform.position = eventData.position;

            // allow raycasts to go through the source slot while dragging
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
            else Debug.LogWarning("OnBeginDrag: canvasGroup is null, cannot set blocksRaycasts=false");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"OnBeginDrag Exception: {ex}\nSource slotIndex {slotIndex}");
            // Ensure state cleaned up on error
            if (dragIcon != null) Destroy(dragIcon);
            DragAndDropState.CurrentSource = null;
            if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (dragIcon != null) Destroy(dragIcon);
        dragIcon = null;

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;

        DragAndDropState.CurrentSource = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var source = DragAndDropState.CurrentSource;
        if (source == null)
        {
            Debug.Log("OnDrop: no source registered");
            return;
        }
        if (source == this) return;

        if (inventoryModel == null)
            inventoryModel = Object.FindFirstObjectByType<InventoryModel>();

        if (inventoryModel == null)
        {
            Debug.LogError("OnDrop: inventoryModel missing, cannot swap.");
            return;
        }

        inventoryModel.SwapSlots(source.slotIndex, this.slotIndex);
    }
}

// simple global holder
public static class DragAndDropState
{
    public static SlotUI CurrentSource;
}
