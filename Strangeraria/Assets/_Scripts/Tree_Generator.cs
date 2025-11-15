using UnityEngine;

public class Tree_Generator : MonoBehaviour
{
    [Header("Taille")]
    public int TreeSizeMin = 3;
    public int TreeSizeMax = 6;
    private int treeSize;

    [Header("Croissance")]
    public int TimeToGrow = 0;
    public bool IsGrow = false;

    [Header("Sprites")]
    public Sprite Log;
    public Sprite Leaf;

    private bool generated = false;

    void Start()
    {
        if (!generated)
        {
            GenerateTree();
            generated = true;
        }
    }

    void GenerateTree()
    {
        treeSize = Random.Range(TreeSizeMin, TreeSizeMax);

        // Troncs
        for (int i = 0; i < treeSize; i++)
        {
            CreatePart(Log, new Vector2(0, i));
        }

        // Feuillage
        CreatePart(Leaf, new Vector2(0, treeSize));
        CreatePart(Leaf, new Vector2(0, treeSize + 1));
        CreatePart(Leaf, new Vector2(1, treeSize));
        CreatePart(Leaf, new Vector2(-1, treeSize));
    }

    void CreatePart(Sprite sprite, Vector2 position)
    {
        GameObject part = new GameObject(sprite.name);
        part.transform.SetParent(this.transform, false);
        part.transform.localPosition = position;

        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;

        // Pour éviter les overlaps visuels
        renderer.sortingOrder = 1;
    }
}