using UnityEngine;

public class ParallaxFollowPlayer : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform sourceSprite;
        [Range(0f, 1f)] public float parallaxX = 0.5f; // force parallax horizontal
        [Range(0f, 1f)] public float parallaxY = 0.5f; // force parallax vertical
        //public float verticalSmooth = 6f;

        [HideInInspector] public float initialY;
        [HideInInspector] public float width;
        [HideInInspector] public Transform[] tiles; // 3 tuiles : gauche, centre, droite
    }

    public Layer[] layers = new Layer[5];
    public Transform cameraTransform;

    Vector3 previousCamPos;
    Vector3 initialCamPos;
    const int tilesNumber = 3;

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("cameraTransform null");
            enabled = false;
            return;
        }

        initialCamPos = cameraTransform.position;
        previousCamPos = cameraTransform.position;

        foreach (var layer in layers)
        {
            if (layer == null || layer.sourceSprite == null) continue;

            SpriteRenderer sr = layer.sourceSprite.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogWarning($"le gameObject '{layer.sourceSprite.name}' n'a pas de SpriteRenderer");
                continue;
            }

            layer.width = sr.bounds.size.x;
            layer.initialY = layer.sourceSprite.position.y;

            // Creation tuiles
            layer.tiles = new Transform[tilesNumber];
            layer.tiles[1] = layer.sourceSprite; 

            string baseName = layer.sourceSprite.name;

            // left
            Transform left = layer.sourceSprite.parent != null ? layer.sourceSprite.parent.Find(baseName + "_tile_left") : null;
            if (left == null)
            {
                left = Instantiate(layer.sourceSprite, layer.sourceSprite.parent);
                left.name = baseName + "_tile_left";
            }
            left.position = new Vector3(layer.sourceSprite.position.x - layer.width, layer.initialY, layer.sourceSprite.position.z);
            layer.tiles[0] = left;

            // right
            Transform right = layer.sourceSprite.parent != null ? layer.sourceSprite.parent.Find(baseName + "_tile_right") : null;
            if (right == null)
            {
                right = Instantiate(layer.sourceSprite, layer.sourceSprite.parent);
                right.name = baseName + "_tile_right";
            }
            right.position = new Vector3(layer.sourceSprite.position.x + layer.width, layer.initialY, layer.sourceSprite.position.z);
            layer.tiles[2] = right;
        }
    }

    void LateUpdate()
    {
        Vector3 camPos = cameraTransform.position;
        Vector3 deltaCam = camPos - previousCamPos;

        foreach (var layer in layers)
        {
            if (layer == null || layer.tiles == null) continue;

            float camDeltaY = camPos.y - initialCamPos.y;
            float targetY = layer.initialY + camDeltaY * layer.parallaxY;

            float moveX = deltaCam.x * layer.parallaxX;

            for (int i = 0; i < layer.tiles.Length; i++)
            {
                Transform tile = layer.tiles[i];
                if (tile == null) continue;

                Vector3 pos = tile.position;
                pos.x += moveX;
                pos.y = targetY;
                tile.position = pos;

                // SCROLL INFINI :
                float diff = cameraTransform.position.x - tile.position.x;

                // si une tuile trop a droite, on la décale
                if (diff > layer.width)
                {
                    tile.position += Vector3.right * layer.width * tilesNumber;
                }
                // si une tuile trop a gauche, on la décale
                else if (diff < -layer.width)
                {
                    tile.position -= Vector3.right * layer.width * tilesNumber;
                }
            }
        }

        previousCamPos = camPos;
    }
}
