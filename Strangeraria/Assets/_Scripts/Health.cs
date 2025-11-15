using System;
using UnityEngine;


[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;


    [Header("Damage / Death")]
    public bool invincible = false;
    public float invincibilityTime = 0.5f;


    [Header("Refs")]
    public Animator anim;

    public event Action<int, int> OnHealthChanged;

    float invTimer = 0f;

    void Awake()
    {
        currentHealth = maxHealth;
        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Update()
    {
        if (invincible)
        {
            invTimer -= Time.deltaTime;
            if (invTimer <= 0f)
                invincible = false;
        }
    }

    public void TakeDamage(int amount)
    {
        if (invincible) return;

        currentHealth -= amount;
        invincible = true;
        invTimer = invincibilityTime;


        if (anim != null)
            anim.SetTrigger("3_Damaged");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        if (anim != null)
            anim.SetTrigger("4_Death");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}