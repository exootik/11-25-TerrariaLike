using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ItemDropOnSpawn : MonoBehaviour
{
    //public float dropHorizontalForce = 3.0f;
    //public float dropUpForce = 3.0f;
    //public float dropAngleVariance = 15f;

    public float minHorizontalSpeed = 2f;
    public float maxHorizontalSpeed = 4f;
    public float minUpSpeed = 2f;
    public float maxUpSpeed = 4f;

    void Start()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float dir = (Random.value < 0.5f) ? -1f : 1f;
            float hx = Random.Range(minHorizontalSpeed, maxHorizontalSpeed) * dir;
            float vy = Random.Range(minUpSpeed, maxUpSpeed);
            rb.AddForce(new Vector2(hx, vy), ForceMode2D.Impulse);
        }
    }
}
