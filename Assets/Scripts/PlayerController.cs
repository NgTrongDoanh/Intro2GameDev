using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps; // Giữ lại để tương thích chung

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float climbSpeed = 3f;

    [Header("Ground Check Settings")]
    public Transform groundCheckPoint;
    public LayerMask whatIsGround;
    public float groundCheckRadius = 0.2f;

    // --- Component References ---
    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D playerCollider;

    // --- State Variables ---
    private float moveInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isFacingRight = true;
    private bool isDead = false;

    // --- Ladder Logic Variables (Sử dụng Trigger riêng biệt) ---
    private bool canClimb = false;      // True khi player đang "chạm" vào vùng trigger của một thang
    private bool isOnLadder = false;    // True khi player đang "thực sự leo"
    private Collider2D currentLadderCollider; // Lưu trữ collider của thang đang chạm vào để kiểm tra bounds

    // ===================================================================
    // UNITY LIFECYCLE METHODS
    // ===================================================================

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        // Bắt đầu leo khi: đang chạm thang, nhấn nút lên/xuống, VÀ toàn bộ cơ thể đã ở trong thang
    if (canClimb)
    {
        Debug.Log("Có thể leo. Đang kiểm tra bounds..."); // <--- THÊM DÒNG NÀY
    }

        if (canClimb && Mathf.Abs(verticalInput) > 0f)
        {
            if (IsFullyInsideCurrentLadder())
            {
                Debug.Log("Đã ở trong thang! Bắt đầu leo!"); // <--- THÊM DÒNG NÀY

                isOnLadder = true;
            }
        }
        
        // Nhảy (có thể nhảy ra khỏi thang)
        if (Input.GetButtonDown("Jump"))
        {
            if (isOnLadder)
            {
                isOnLadder = false;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            else if (isGrounded)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // Chỉ lật nhân vật khi không ở trên thang
        if (!isOnLadder)
        {
            Flip();
        }
        
        UpdateAnimationStates();
    }
    
    void FixedUpdate()
    {
        if (isDead) return;

        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, whatIsGround);

        if (isOnLadder)
        {
            rb.gravityScale = 0f;
            rb.velocity = new Vector2(0, verticalInput * climbSpeed); 
        }
        else
        {
            rb.gravityScale = 3f; // Bạn có thể thay đổi giá trị này
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
    }

    // ===================================================================
    // TRIGGER DETECTION (Hoạt động với các BoxCollider2D riêng lẻ)
    // ===================================================================
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Đã chạm vào trigger có tag: " + other.tag); // <--- THÊM DÒNG NÀY

        if (other.CompareTag("Ladder"))
        {
            Debug.Log("Đã xác nhận là Thang! canClimb = true"); // <--- THÊM DÒNG NÀY

            canClimb = true;
            currentLadderCollider = other; // Lưu lại collider của thang vừa chạm vào
        }

        if (other.CompareTag("Trap"))
        {
            Die();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = false;
            isOnLadder = false;
            currentLadderCollider = null; // Quên đi collider của thang vừa rời khỏi
        }
    }

    // ===================================================================
    // CUSTOM METHODS
    // ===================================================================

    // Kiểm tra ranh giới với collider của thang hiện tại
    bool IsFullyInsideCurrentLadder()
    {
        if (currentLadderCollider == null || playerCollider == null) return false;

        Bounds playerBounds = playerCollider.bounds;
        Bounds ladderBounds = currentLadderCollider.bounds;
        Debug.Log("Player Bounds: " + playerBounds);
        Debug.Log("Ladder Bounds: " + ladderBounds);

        return ladderBounds.Contains(playerBounds.min) && ladderBounds.Contains(playerBounds.max);
    }
    
    void Flip()
    {
        if ((isFacingRight && moveInput < 0f) || (!isFacingRight && moveInput > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    void UpdateAnimationStates()
    {
        if (!isOnLadder)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        animator.SetBool("IsJumping", !isGrounded && !isOnLadder);

        animator.SetBool("IsClimbing", isOnLadder);
        if (isOnLadder)
        {
            animator.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));
        }
    }

    void Die()
    {
        isDead = true;
        animator.SetBool("IsDead", true);
        rb.velocity = Vector2.zero;
        rb.isKinematic = true;
        GetComponent<Collider2D>().enabled = false;
        animator.SetTrigger("Die");
        
        Invoke("ReloadScene", 2f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}