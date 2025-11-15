using UnityEngine;
using UnityEngine.Tilemaps;

public enum ItemType { Block, Weapon, Resource }

[CreateAssetMenu(fileName = "ItemDefinition", menuName = "Scriptable Objects/ItemDefinition")]
public class ItemDefinition : ScriptableObject
{
    public string id;
    public string displayName;
    public ItemType itemType;
    public Sprite icon;
    public bool stackable = true;
    public int maxStack = 99;

    public TileBase placeTile;
    public GameObject equipPrefab;

    // Pour les armes
    public int damage;
    public float attackCooldown = 1.0f;
    public float attackRadius = 0.8f;

    public float mineSpeedMultiplier = 1f;

    public int healthValue;
}
