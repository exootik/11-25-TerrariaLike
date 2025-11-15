using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    [Header("Parents")]
    public Transform equipParent;
    public Transform previewParent;

    public ItemDefinition currentEquippedItem;
    private GameObject currentEquipGO;
    private GameObject currentBlockPreviewGO;
    public int equippedHotbarIndex = -1;

    [Header("Health")]
    public Health health;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<Health>();
    }

    public void Equip(ItemDefinition item)
    {
        if (item == null)
        {
            Unequip();
            return;
        }

        if (item.itemType != ItemType.Weapon)
        {
            Debug.LogWarning("Equip appelé avec un item non-weapon");
            return;
        }

        if (currentEquippedItem == item && currentEquipGO != null)
        {
            return;
        }

        ClearBlockPreview();

        if (currentEquipGO != null) Destroy(currentEquipGO);

        if (item.equipPrefab != null)
        {
            currentEquipGO = Instantiate(item.equipPrefab, equipParent, false);
            currentEquipGO.transform.localPosition = Vector3.zero;
            currentEquipGO.transform.localRotation = Quaternion.identity;
            currentEquipGO.transform.localScale = Vector3.one;
        }
        else
        {
            var go = new GameObject($"Equip_{item.displayName}");
            go.transform.SetParent(equipParent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = item.icon;
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 10;
            currentEquipGO = go;
        }

        currentEquippedItem = item;
    }

    public void EquipBlock(ItemDefinition blockItem)
    {
        if (blockItem == null || blockItem.itemType != ItemType.Block)
        {
            Unequip();
            return;
        }

        currentEquippedItem = blockItem;

        ClearBlockPreview();
    }

    public void Unequip()
    {
        if (currentEquipGO != null) Destroy(currentEquipGO);
        currentEquipGO = null;
        currentEquippedItem = null;
    }

    private void ClearBlockPreview()
    {
        if (currentBlockPreviewGO != null) Destroy(currentBlockPreviewGO);
        currentBlockPreviewGO = null;
    }

    public bool UseResource(ItemDefinition resourceItem)
    {
        if (resourceItem == null || resourceItem.itemType != ItemType.Resource)
            return false;

        if (resourceItem.id == "heal")
        {
            health.Heal(resourceItem.healthValue);
            return true;
        }
        else if (resourceItem.id == "test")
        {
            Debug.Log("test ressource used");
            return true;
        }

        Debug.Log($"UseResource: applied default effect for {resourceItem.displayName}");
        return true;
    }
}
