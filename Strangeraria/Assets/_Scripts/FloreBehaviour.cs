using UnityEngine;

/// <summary>
/// Gère le comportement de croissance, animation ou interaction de la flore.
/// Peut être ajouté sur n’importe quel prefab de flore.
/// </summary>
public class FloraBehavior : MonoBehaviour
{
    [Header("Croissance")]
    public bool growsOverTime = false;
    public float growDuration = 5f;
    public Vector3 finalScale = Vector3.one;

    [Header("Animation")]
    public bool swayWithWind = false;
    public float swayAmplitude = 2f;
    public float swaySpeed = 1f;

    private Vector3 initialScale;
    private float growthTimer = 0f;
    private float randomOffset;

    public void InitializeBehavior(int seed)
    {
        Random.InitState(seed);
        randomOffset = Random.Range(0f, 1000f);
        initialScale = transform.localScale;

        if (growsOverTime)
            transform.localScale = Vector3.zero;
    }

    void Update()
    {
        // Croissance progressive
        if (growsOverTime && growthTimer < growDuration)
        {
            growthTimer += Time.deltaTime;
            float t = Mathf.Clamp01(growthTimer / growDuration);
            transform.localScale = Vector3.Lerp(Vector3.zero, finalScale, t);
        }

        // Animation au vent
        if (swayWithWind)
        {
            float sway = Mathf.Sin(Time.time * swaySpeed + randomOffset) * swayAmplitude * 0.01f;
            transform.rotation = Quaternion.Euler(0, 0, sway);
        }
    }
    void OnDestroy()
    {
        if (Object.FindFirstObjectByType<InfiniteWorld>() is InfiniteWorld world)
        {
            world.DestroyFlora(transform.position);
        }
    }
}
