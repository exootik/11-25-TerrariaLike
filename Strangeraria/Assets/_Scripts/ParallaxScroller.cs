using UnityEngine;

public class ParallaxAutoScroller : MonoBehaviour
{
    [System.Serializable]
    public class LayerConfig
    {
        public Transform sourceSprite; // Le sprite de base
        public float speed = 1f;
    }

    public LayerConfig[] layers;
    public Vector2 direction = Vector2.left;

    void Start()
    {
        foreach (var layer in layers)
        {
            SetupLayer(layer);
        }
    }

    void SetupLayer(LayerConfig config)
    {
        SpriteRenderer sr = config.sourceSprite.GetComponent<SpriteRenderer>();
        float width = sr.bounds.size.x;

        // Crée 2 clones
        for (int i = 1; i <= 2; i++)
        {
            Transform clone = Instantiate(config.sourceSprite, config.sourceSprite.parent);
            clone.name = config.sourceSprite.name + "_clone" + i;
            clone.position = config.sourceSprite.position + Vector3.right * width * i;
        }
    }

    void Update()
    {
        foreach (var layer in layers)
        {
            Transform[] children = new Transform[3];
            int index = 0;
            foreach (Transform child in layer.sourceSprite.parent)
            {
                if (child.name.StartsWith(layer.sourceSprite.name))
                    children[index++] = child;
            }

            foreach (Transform sprite in children)
            {
                sprite.position += (Vector3)(direction * layer.speed * Time.deltaTime);
            }

            float width = layer.sourceSprite.GetComponent<SpriteRenderer>().bounds.size.x;

            foreach (Transform sprite in children)
            {
                if (sprite.position.x <= -width)
                {
                    sprite.position += Vector3.right * width * 3;
                }
            }
        }
    }
}