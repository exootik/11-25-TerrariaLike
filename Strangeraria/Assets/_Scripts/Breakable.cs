using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Tiles/BreakableTile")]
public class BreakableTile : Tile
{
    public bool isBreakable = true;
    public float breakTime = 1f;
    public bool HasGravity = false;
    public enum ToolType { None, Pickaxe, Axe, Hammer }
    public ToolType requiredTool = ToolType.None;

    public GameObject dropPrefab;
    public int dropAmount = 1;
}