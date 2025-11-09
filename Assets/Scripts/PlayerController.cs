using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 12f;
    public float climbSpeed = 3f;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public LayerMask whatIsGround;
    public float groundCheckRadius = 0.2f;

    // Components & State
    private Rigidbody2D rb;
    private Animator animator;
    private float moveInput;
    private float verticalInput;
    private bool isGrounded;
    private bool isOnLadder;
    private bool isFacingRight = true;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
            rb.gravityScale = 3f;
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }
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
        // Logic cho chạy/đứng yên
        if (!isOnLadder)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput));
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        // Logic cho nhảy
        animator.SetBool("IsJumping", !isGrounded && !isOnLadder);

        // --- LOGIC MỚI CHO LEO THANG ---
        // Báo cho Animator biết có đang trong trạng thái leo hay không
        animator.SetBool("IsClimbing", isOnLadder);

        // Báo cho Animator biết tốc độ leo (để phân biệt giữa leo và bám)
        if (isOnLadder)
        {
            animator.SetFloat("ClimbSpeed", Mathf.Abs(verticalInput));
        }
    }
    
    private bool canClimb = false; // Biến mới để kiểm tra xem có "chạm" thang hay không

    void Update()
    {
        if (isDead) return;

        moveInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // --- LOGIC LEO THANG MỚI ---
        // Nếu đang chạm vào thang và nhấn Lên/Xuống
        if (canClimb && Mathf.Abs(verticalInput) > 0f)
        {
            isOnLadder = true; // Bắt đầu trạng thái leo
        }
        
        // --- LOGIC NHẢY ---
        // Cho phép nhảy khỏi thang
        if (Input.GetButtonDown("Jump"))
        {
            if (isOnLadder) // Nếu đang leo thang mà nhảy
            {
                isOnLadder = false; // Thoát khỏi trạng thái leo
                rb.velocity = new Vector2(rb.velocity.x, jumpForce); // Tác động lực nhảy
            }
            else if (isGrounded) // Nếu đang trên mặt đất
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        if (!isOnLadder)
        {
            Flip();
        }
        
        UpdateAnimationStates();
    }

    // Hàm này sẽ chỉ còn nhiệm vụ phát hiện xem có "đang chạm" thang hay không
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            canClimb = true;
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
            isOnLadder = false; // Khi rời khỏi vùng thang, chắc chắn không còn leo nữa
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