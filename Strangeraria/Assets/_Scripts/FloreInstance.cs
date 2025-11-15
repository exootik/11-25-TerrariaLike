using UnityEngine;
public interface ISeededFlora
{
    void ApplySeed(int seed);
}

/// <summary>
/// Script à mettre sur TOUT prefab de flore (arbre, herbe, fleur...).
/// Il s’occupe d’appliquer un seed, de gérer le comportement et la génération.
/// </summary>
public class FloraInstance : MonoBehaviour, ISeededFlora
{
    [Header("Paramètres généraux")]
    public string floraName;
    public bool randomizeAppearance = true;
    public int localSeed;

    [Header("Composants liés")]
    public FloraBehavior behavior; // optionnel : comportement dynamique (croissance, mouvement, etc.)
    public SpriteRenderer spriteRenderer;

    private bool initialized = false;

    void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (behavior == null)
            behavior = GetComponent<FloraBehavior>();
    }

    void Start()
    {
        if (!initialized)
        {
            ApplySeed(localSeed);
            initialized = true;
        }
    }

    /// <summary>
    /// Applique un seed pour obtenir une apparence ou taille déterministe.
    /// </summary>
    public void ApplySeed(int seed)
    {
        localSeed = seed;
        Random.InitState(seed);

        if (randomizeAppearance && spriteRenderer != null)
        {
            // Exemple : variation subtile de taille, rotation, teinte, etc.
            float scale = Random.Range(0.9f, 1.1f);
            transform.localScale = new Vector3(scale, scale, 1);

            float hueShift = Random.Range(-0.05f, 0.05f);
            spriteRenderer.color = Color.HSVToRGB(0.33f + hueShift, 0.7f, 0.9f);
        }

        if (behavior != null)
        {
            behavior.InitializeBehavior(seed);
        }
    }
}
