using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Attack")]
    public Transform attackPoint;
    public float defaultAttackRadius = 0.8f;
    public LayerMask enemyLayer;

    [Header("References")]
    public Animator anim;
    public PlayerEquip playerEquip;
    public InfiniteWorld infiniteWorld;

    [Header("Health")]
    public Health health;

    [Header("Death")]
    public float deathDelay = 0.6f;

    [Header("UI menu")]
    public GameObject deathPanel;
    public GameObject pausePanel;
    public string menuSceneName = "Menu";

    Rigidbody2D rb;
    bool facingRight = true;
    bool isGrounded;

    public PlayerInput playerInput;
    InputAction moveAction;
    InputAction jumpAction;
    InputAction attackAction;
    InputAction escapeAction;

    float lastAttackTime = -9f;

    bool deathStarted = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        attackAction = playerInput.actions["Attack"];
        escapeAction = playerInput.actions["Escape"];

        if (Game_Manager.Instance != null)
        {
            Game_Manager.Instance.RegisterPlayer(gameObject);
        }
        else
        {
            Debug.LogWarning("GameManager not found when trying to register Player.");
        }

        if (anim == null)
            anim = GetComponentInChildren<Animator>();


        if (playerEquip == null)
            playerEquip = GetComponentInChildren<PlayerEquip>();


        if (health == null)
            health = GetComponent<Health>();

        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    void Update()
    {
        if (deathStarted) return;

        float h = moveAction.ReadValue<Vector2>().x;

        Vector2 vel = rb.linearVelocity;
        vel.x = h * moveSpeed;
        rb.linearVelocity = vel;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        if (jumpAction.triggered && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (h > 0 && facingRight) Flip();
        else if (h < 0 && !facingRight) Flip();

        if (anim != null)
        {
            anim.SetBool("1_Move", Mathf.Abs(h) > 0.01f);
        }

        if (attackAction.triggered)
        {
            TryAttack();
        }
        
        if (IsDead())
        {
            StartCoroutine(DieCoroutine());
        }

        if (transform.position.y < -85)
        {
            Vector3 pos = transform.position;
            pos.y = 50;
            transform.position = pos;

            StartCoroutine(IncreaseChangeMapCoroutine());
        }

        if (escapeAction.triggered)
        {
            if (pausePanel.activeSelf)
            {
                pausePanel.SetActive(false);
            }
            else
            {
                pausePanel.SetActive(true);
            }
            
        }
    }
    void FixedUpdate()
    {
        float maxFallSpeed = -10f;

        if (rb.linearVelocity.y < maxFallSpeed)
        {
            Vector3 v = rb.linearVelocity;
            v.y = maxFallSpeed;
            rb.linearVelocity = v;

            
        }
    }

    void TryAttack()
    {
        ItemDefinition item = (playerEquip != null) ? playerEquip.currentEquippedItem : null;
        int dmg = (item != null && item.itemType == ItemType.Weapon) ? item.damage : 20;
        float radius = (item != null && item.itemType == ItemType.Weapon) ? item.attackRadius : defaultAttackRadius;
        float cooldown = (item != null && item.itemType == ItemType.Weapon) ? item.attackCooldown : 1.0f;

        if (Time.time - lastAttackTime < cooldown) return;

        lastAttackTime = Time.time;

        if (anim != null)
            anim.SetTrigger("2_Attack");

        if (attackPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, radius, enemyLayer);
            foreach (var c in hits)
            {
                var enemyHealth = c.GetComponent<Health>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(dmg);
                }
            }
        }
    }

    bool IsDead()
    {
        if (health != null)
        {
            if (health.currentHealth <= 0)
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator DieCoroutine()
    {
        if (deathStarted) yield break;
        deathStarted = true;

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(deathDelay);

        ShowDeathPanel();
        //Destroy(gameObject);
    }

    IEnumerator IncreaseChangeMapCoroutine()
    {
        yield return new WaitForSeconds(4f);
        infiniteWorld.IncreaseDifficultyOfMap();
    }

    void ShowDeathPanel()
    {
        if (deathPanel != null)
            deathPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void Respawn()
    {
        if (deathPanel != null)
            deathPanel.SetActive(false);

        Time.timeScale = 1f;

        if (anim != null)
            anim.updateMode = AnimatorUpdateMode.Normal;

        if (health != null)
            health.currentHealth = health.maxHealth;

        Vector3 p = transform.position;
        transform.position = new Vector3(p.x, 25f, p.z);

        if (playerInput != null) playerInput.enabled = true;
        this.enabled = true;

        deathStarted = false;
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(menuSceneName);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }
    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            float radius = (playerEquip != null && playerEquip.currentEquippedItem != null) ? playerEquip.currentEquippedItem.attackRadius : defaultAttackRadius;
            Gizmos.DrawWireSphere(attackPoint.position, radius);
        }
    }
}
