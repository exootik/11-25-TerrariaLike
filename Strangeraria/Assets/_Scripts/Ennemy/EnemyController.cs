using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTools.RandomService;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    public enum State { Wander, Chase, Attack, Die }

    [Header("Movement")]
    public float wanderSpeed = 3.5f;
    public float chaseSpeed = 6f;
    public float jumpForce = 10f;

    [Header("Wander")]
    public float wanderChangeMin = 1.0f;
    public float wanderChangeMax = 3.0f;

    [Header("Detection / Combat")]
    public float detectionRadius = 8f;
    public float attackRange = 1.25f;
    public float attackCooldown = 1.0f;
    public LayerMask obstacleMask;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Attack Hitbox")]
    public Transform attackPoint;
    public float attackRadius = 0.6f;
    public int attackDamage = 5;
    public LayerMask playerLayer;

    [Header("Animations")]
    public Animator anim;

    [Header("Health")]
    public Health health;

    [Header("Death")]
    public float deathDelayHalved = 0.6f;

    [Header("Drops items")]
    public List<GameObject> dropPrefabs = new List<GameObject>();
    public int dropCount = 2;

    Rigidbody2D rb;
    GameObject player;
    State currentState = State.Wander;

    bool facingRight = true;
    bool isGrounded;

    float wanderTimer;
    int wanderDir = 1;

    float lastAttackTime = -9f;

    bool deathStarted = false;

    [Header("Random Jump")]
    [Tooltip("Probabilite par seconde qu'il saute (si au sol)")]
    [Range(0f, 3f)] public float jumpChancePerSecond = 0.15f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player");

        // Init wander
        RandomWanderTimerChange();

        if (anim == null)
            anim = GetComponentInChildren<Animator>();

        if (health == null)
            health = GetComponent<Health>();
    }

    void Update()
    {
        if (groundCheck != null)
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

        // State Machine :
        switch (currentState)
        {
            case State.Wander:
                DoWander();

                if (PlayerVisible())
                    currentState = State.Chase;

                if (IsDead())
                    currentState = State.Die;
                break;

            case State.Chase:
                DoChase();
                if (PlayerInAttackRange())
                    currentState = State.Attack;

                if (!PlayerVisible()) 
                    currentState = State.Wander;

                if (IsDead())
                    currentState = State.Die;
                break;

            case State.Attack:
                DoAttack();

                if (!PlayerInAttackRange())
                    currentState = State.Chase;

                if (IsDead())
                    currentState = State.Die;
                break;

            case State.Die:
                DoDeath();
                break;
        }

        // anim de mouvement
        if (anim != null)
            anim.SetBool("1_Move", Mathf.Abs(rb.linearVelocity.x) > 0.05f);

        if (gameObject.transform.position.y < -500)
        {
            Debug.Log("Destroy");
            Destroy(gameObject);
        }
    }

    void DoWander()
    {
        // changer de direction quand timer expire
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderDir = (Random.value < 0.5f) ? -1 : 1;
            RandomWanderTimerChange();
        }

        // deplacement
        Vector2 vel = rb.linearVelocity;
        vel.x = wanderDir * wanderSpeed;
        rb.linearVelocity = vel;

        // Random jump 
        if (isGrounded && Random.value < jumpChancePerSecond * Time.deltaTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // flip
        if (vel.x > 0.01f && facingRight) Flip();
        else if (vel.x < -0.01f && !facingRight) Flip();
    }

    void DoChase()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.transform.position.x - transform.position.x);
        Vector2 vel = rb.linearVelocity;
        vel.x = dir * chaseSpeed;
        rb.linearVelocity = vel;

        if (dir > 0 && facingRight) Flip();
        else if (dir < 0 && !facingRight) Flip();

        // Random jump 
        if (isGrounded && Random.value < jumpChancePerSecond * Time.deltaTime)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    void DoAttack()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // cooldown
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            if (anim != null)
                anim.SetTrigger("2_Attack");

            DealAttackDamage();
        }
    }

    void DoDeath()
    {
        if (!deathStarted)
        {
            deathStarted = true;
            StartCoroutine(DieCoroutine());
        }
    }

    bool PlayerVisible()
    {
        if (player == null) return false;

        Vector2 origin = (Vector2)transform.position + new Vector2(0f, 0.5f);
        Vector2 target = (Vector2)player.transform.position + new Vector2(0f, 0.5f);
        float dist = Vector2.Distance(origin, target);
        if (dist > detectionRadius) return false;

        if (Physics2D.Linecast(origin, target, obstacleMask))
        {
            Debug.DrawLine(origin, target, Color.red);
            return false;
        }

        Debug.DrawLine(origin, target, Color.green);
        return true;
    }

    bool PlayerInAttackRange()
    {
        if (player == null) return false;
        float dx = Mathf.Abs(player.transform.position.x - transform.position.x);
        float dy = Mathf.Abs(player.transform.position.y - transform.position.y);

        return (dx <= attackRange && dy <= 1.5f);
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

    void DealAttackDamage()
    {
        if (attackPoint == null) return;
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, playerLayer);
        foreach (Collider2D c in hits)
        {
            var playerHealth = c.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    IEnumerator DieCoroutine()
    {
        rb.linearVelocity = Vector2.zero;

        this.enabled = false;

        yield return new WaitForSeconds(deathDelayHalved);
        SpawnDrops();
        yield return new WaitForSeconds(deathDelayHalved);

        Destroy(gameObject);
    }

    void SpawnDrops()
    {
        if (dropPrefabs == null || dropPrefabs.Count == 0) return;

        for (int i = 0; i < dropCount; i++)
        {
            GameObject prefab = dropPrefabs[Random.Range(0, dropPrefabs.Count)];
            if (prefab == null) continue;

            Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

            GameObject inst = Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }

    void RandomWanderTimerChange()
    {
        wanderTimer = Random.Range(wanderChangeMin, wanderChangeMax);
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
        Gizmos.color = Color.yellow;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + new Vector3(0f, 0.5f, 0f), detectionRadius);

        Gizmos.color = Color.red;
        if (attackPoint != null)
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
