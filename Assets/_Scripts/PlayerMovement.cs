using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    // Movement and jump settings
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float groundCheckRadius = 0.2f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    private bool IsGrounded;
    private float lastJumpTime = -999f;
    public float jumpCooldown = 0.2f;

    //Attack settings
    public Collider2D attackHitbox;
    public float attackDuration = 0.2f;
    private bool isAttacking = false;
    public int attackDamage = 1;

    // Health settings
    public int maxHealth = 5;
    public int currentHealth;
    public float invincibilityDuration = 1f;
    private bool isInvincible = false;
    private bool isDead = false;

    private Rigidbody2D rb;
    private Animator anim;
    private Vector2 moveInput;
    private InputSystem_Actions inputActions;
    private AudioSource audioSource;
    public AudioClip attackSound;
    public AudioClip hitSound;


    void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Jump.performed += OnJump;
        inputActions.Player.Attack.performed += OnAttack;
    }

    void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Jump.performed -= OnJump;
        inputActions.Player.Attack.performed -= OnAttack;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead)
        {
            return;
        }
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        // Jump mechanics checking if the player is grounded and able to jump with raycast
        bool wasGrounded = IsGrounded;
        IsGrounded = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, 0.5f, groundLayer).collider != null;
        anim.SetBool("IsGrounded", IsGrounded);

        // Movement mechanics for the player
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        anim.SetFloat("Speed", Mathf.Abs(moveInput.x));
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        // Flip sprite based on movement direction
        if (moveInput.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(moveInput.x);
            transform.localScale = scale;
        }

    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (isDead)
        {
            return;
        }
        if (Time.time - lastJumpTime >= jumpCooldown)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("Jump");
            lastJumpTime = Time.time;
        }
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (isDead)
        {
            return;
        }
        if (context.performed && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        anim.SetTrigger("Attack");
        audioSource.PlayOneShot(attackSound);
        if (attackHitbox != null)
            attackHitbox.enabled = true;
        yield return new WaitForSeconds(attackDuration);
        if (attackHitbox != null)
            attackHitbox.enabled = false;
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || isInvincible) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            isDead = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
            GetComponent<Collider2D>().enabled = false;
            anim.Play("Player_Death");
            this.enabled = false;
            Destroy(gameObject, 5f);
            return;
        }
        else
        {
            audioSource.PlayOneShot(hitSound);
            anim.SetTrigger("Hit");
            isInvincible = true;
            StartCoroutine(ResetInvincibility());
        }
    }

    private IEnumerator ResetInvincibility()
    {
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}