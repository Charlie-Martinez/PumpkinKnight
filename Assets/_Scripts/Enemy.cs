using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float chaseRange = 4f;
    public float attackRange = 1.5f;
    public int health = 6;
    public int damageAmount = 1;
    public float attackCooldown = 1.5f;
    public Collider2D attackHitbox;
    public Transform player;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector3 originalScale;

    private bool isDead = false;
    private bool isAttacking = false;
    private float lastAttackTime = -999f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (attackHitbox != null) attackHitbox.enabled = false;
    }

    void Update()
    {
        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Attack if in range and cooldown is ready
        if (distance < attackRange && Time.time - lastAttackTime > attackCooldown && !isAttacking && !isDead)
        {
            StartCoroutine(PerformAttack());
            return;
        }

        // Chase if in range and not attacking
        if (distance < chaseRange && !isAttacking && !isDead)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);

            if (direction.x != 0)
            {
                transform.localScale = new Vector3(
                    Mathf.Sign(direction.x) * Mathf.Abs(originalScale.x),
                    originalScale.y,
                    originalScale.z
                );
            }

            if (anim != null) anim.SetBool("IsChasing", true);
        }
        else if (!isAttacking && !isDead)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (anim != null) anim.SetBool("IsChasing", false);
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetTrigger("Attack");

        // Wind-up delay before hitbox activates
        float hitboxDelay = 0.2f;
        float elapsed = 0f;
        while (elapsed < hitboxDelay && isAttacking && !isDead)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (isDead || !isAttacking)
        {
            if (attackHitbox != null) attackHitbox.enabled = false;
            isAttacking = false;
            yield break;
        }

        // Enable hitbox
        if (attackHitbox != null) attackHitbox.enabled = true;

        // Hitbox active duration
        float hitboxActiveTime = 0.3f;
        elapsed = 0f;
        while (elapsed < hitboxActiveTime && isAttacking && !isDead)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Disable hitbox
        if (attackHitbox != null) attackHitbox.enabled = false;

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Interrupt attack
        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            if (attackHitbox != null) attackHitbox.enabled = false;
        }

        // Apply damage
        health -= damage;
        if (anim != null) anim.SetTrigger("Hit");

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        isAttacking = false;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        if (anim != null) anim.SetTrigger("Death");
        if (attackHitbox != null) attackHitbox.enabled = false;
        this.enabled = false;
        Destroy(gameObject, 1.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;
        if (other.CompareTag("AttackHitbox"))
        {
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            if (player != null)
            {
                TakeDamage(player.attackDamage);
            }
        }
        if (other.CompareTag("Player") && attackHitbox != null && attackHitbox.enabled)
        {
            PlayerMovement playerScript = other.GetComponent<PlayerMovement>();
            if (playerScript != null)
            {
                playerScript.TakeDamage(damageAmount);
            }
        }
    }
}