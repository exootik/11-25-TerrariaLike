using System;
using UnityEngine;

[System.Serializable]
public struct InventorySlot
{
    public ItemDefinition item;
    public int count;

    public bool IsEmpty => item == null || count <= 0;

    public void Clear()
    {
        item = null;
        count = 0;
    }
}

public class InventoryModel : MonoBehaviour
{
    public int totalSlots = 30;     // total slots visible dans l'inventaire (incl hotbar)
    public int hotbarSize = 5;      // les N premiers slots sont la hotbar
    public InventorySlot[] slots;

    public event Action OnInventoryChanged;

    private void Awake()
    {
        slots = new InventorySlot[totalSlots];
        for (int i = 0; i < slots.Length; i++) slots[i].Clear();
    }

    // HELPERS :
    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Length) throw new IndexOutOfRangeException();
        return slots[index];
    }

    public bool IsHotbarIndex(int index) => index >= 0 && index < hotbarSize;

    // AJOUT ITEM
    public int AddItem(ItemDefinition itemDef, int amount = 1)
    {
        if (itemDef == null || amount <= 0) return amount;

        if (itemDef.stackable)
        {
            // si ya un item deja present et qu'on peut le stack
            for (int i = 0; i < slots.Length && amount > 0; i++)
            {
                if (!slots[i].IsEmpty && slots[i].item == itemDef && slots[i].count < itemDef.maxStack)
                {
                    int space = itemDef.maxStack - slots[i].count;
                    int put = Mathf.Min(space, amount);
                    slots[i].count += put;
                    amount -= put;
                }
            }
        }

        // sinon on place dans un slot vide
        for (int i = 0; i < slots.Length && amount > 0; i++)
        {
            if (slots[i].IsEmpty)
            {
                if (itemDef.stackable)
                {
                    int put = Mathf.Min(itemDef.maxStack, amount);
                    slots[i].item = itemDef;
                    slots[i].count = put;
                    amount -= put;
                }
                else
                {
                    slots[i].item = itemDef;
                    slots[i].count = 1;
                    amount -= 1;
                }
            }
        }

        OnInventoryChanged?.Invoke();
        return amount;
    }

    // SUPP ITEM
    public bool RemoveItem(ItemDefinition itemDef, int amount = 1)
    {
        if (itemDef == null || amount <= 0) return false;
        int needed = amount;

        for (int i = 0; i < slots.Length && needed > 0; i++)
        {
            if (!slots[i].IsEmpty && slots[i].item == itemDef)
            {
                int take = Mathf.Min(slots[i].count, needed);
                slots[i].count -= take;
                needed -= take;
                if (slots[i].count <= 0) slots[i].Clear();
            }
        }

        OnInventoryChanged?.Invoke();
        return needed == 0;
    }

    // décrémente 1 item dans le slot indiqué
    public bool RemoveOneAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return false;
        var slot = slots[slotIndex];
        if (slot.IsEmpty) return false;

        slot.count = Mathf.Max(0, slot.count - 1);
        if (slot.count == 0)
        {
            slot.item = null;
        }

        slots[slotIndex] = slot;
        OnInventoryChanged?.Invoke();
        return true;
    }

    // Drag & drop
    public void SwapSlots(int a, int b)
    {
        if (a == b) return;
        if (a < 0 || b < 0 || a >= slots.Length || b >= slots.Length) return;

        var tmp = slots[a];
        slots[a] = slots[b];
        slots[b] = tmp;

        OnInventoryChanged?.Invoke();
    }

    // Move partial count from slot src to dest (handles stacking)
    public void MoveCount(int src, int dest, int count)
    {
        if (src == dest) return;
        if (src < 0 || src >= slots.Length || dest < 0 || dest >= slots.Length) return;
        if (slots[src].IsEmpty) return;

        ItemDefinition item = slots[src].item;
        if (slots[dest].IsEmpty)
        {
            int move = Mathf.Min(count, slots[src].count);
            slots[dest].item = item;
            slots[dest].count = move;
            slots[src].count -= move;
            if (slots[src].count <= 0) slots[src].Clear();
        }
        else if (slots[dest].item == item && item.stackable)
        {
            int space = item.maxStack - slots[dest].count;
            int move = Mathf.Min(space, count, slots[src].count);
            slots[dest].count += move;
            slots[src].count -= move;
            if (slots[src].count <= 0) slots[src].Clear();
        }

        OnInventoryChanged?.Invoke();
    }

    // Return l'index du 1er item trouve dans l'inventaire
    public int FindSlotWithItem(ItemDefinition item)
    {
        for (int i = 0; i < slots.Length; i++)
            if (!slots[i].IsEmpty && slots[i].item == item) return i;
        return -1;
    }
}
