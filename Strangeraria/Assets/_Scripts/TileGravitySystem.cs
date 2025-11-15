using UnityEngine;
using UnityEngine.Tilemaps;

public class FallingTileBehavior : MonoBehaviour
{
    private Tilemap tilemap;
    private BreakableTile tileData;
    private Vector3Int currentCell;

    private const float FallInterval = 0.1f;
    private float timer;

    public void Initialize(Tilemap tm, BreakableTile data, Vector3Int startCell, Sprite sprite, int sortingOrder = 0)
    {
        if (tm == null)
        {
            Debug.LogError("FallingTileBehavior.Initialize: Tilemap is null!");
            Destroy(gameObject);
            return;
        }

        if (data == null)
        {
            Debug.LogError("FallingTileBehavior.Initialize: Tile data is null!");
            Destroy(gameObject);
            return;
        }

        tilemap = tm;
        tileData = data;
        currentCell = startCell;

        this.GetComponent<SpriteRenderer>().sprite = sprite;

        transform.position = tilemap.CellToWorld(currentCell) + tilemap.tileAnchor;

        timer = 0f;
    }


    private void Update()
    {
        // Empêche un Update prématuré
        if (tilemap == null || tileData == null)
            return;

        timer += Time.deltaTime;
        if (timer < FallInterval) return;
        timer = 0f;

        Vector3Int below = currentCell + Vector3Int.down;

        if (!tilemap.HasTile(below))
        {
            currentCell = below;
            transform.position = tilemap.CellToWorld(currentCell) + tilemap.tileAnchor;
        }
        else
        {
            tilemap.SetTile(currentCell, tileData);
            Destroy(gameObject);
        }
    }

}
