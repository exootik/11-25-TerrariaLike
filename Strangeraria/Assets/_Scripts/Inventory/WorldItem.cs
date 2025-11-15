using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public ItemDefinition item;
    public int amount = 1;
    public string playerTag = "Player";

    private void Update()
    {
        if (gameObject.transform.position.y < -500)
        {
            Debug.Log("Destroy worldItem");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        var inv = Object.FindFirstObjectByType<InventoryModel>();
        if (inv == null) return;

        int leftover = inv.AddItem(item, amount);
        if (leftover == 0)
            Destroy(gameObject);
        else
        {
            amount = leftover;
        }
    }
}
