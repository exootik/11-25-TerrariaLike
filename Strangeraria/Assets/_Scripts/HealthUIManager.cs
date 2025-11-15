// HealthUIManagerManual.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUIManager : MonoBehaviour
{
    [Header("Source de vie")]
    public Health health; 

    [Header("Sprites")]
    public Sprite heartBackgroundSprite;
    public Sprite heartHalfSprite;
    public Sprite heartFullSprite;

    [Header("Hearts (préfabriqués)")]
    public List<Image> heartBackgrounds = new List<Image>();
    public List<Image> heartForegrounds = new List<Image>();

    [Header("Paramètres")]
    [Tooltip("Combien de HP correspond à un coeur plein")]
    public int hpPerFullHeart = 10;

    void Start()
    {
        if (health == null)
        {
            Debug.LogWarning("HealthUIManagerManual: health non assigné.");
            return;
        }

        health.OnHealthChanged += OnHealthChanged;

        OnHealthChanged(health.currentHealth, health.maxHealth);
    }

    void OnDestroy()
    {
        if (health != null)
            health.OnHealthChanged -= OnHealthChanged;
    }

    void OnHealthChanged(int current, int max)
    {
        if (heartForegrounds == null || heartForegrounds.Count == 0) return;

        int heartCount = heartForegrounds.Count;
        int hp = current;

        for (int i = 0; i < heartCount; i++)
        {
            int coveredHp = Mathf.Clamp(hp - i * hpPerFullHeart, 0, hpPerFullHeart);

            Image fg = heartForegrounds[i];
            Image bg = (heartBackgrounds != null && heartBackgrounds.Count > i) ? heartBackgrounds[i] : null;


            if (coveredHp >= hpPerFullHeart)
            {
                fg.gameObject.SetActive(true);
                fg.sprite = heartFullSprite;
            }
            else if (coveredHp > 0)
            {
                fg.gameObject.SetActive(true);
                fg.sprite = heartHalfSprite;
            }
            else
            {
                fg.gameObject.SetActive(false);
            }
        }
    }
}
