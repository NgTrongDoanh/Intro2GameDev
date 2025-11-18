using UnityEngine;

public class Lever : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite spriteOn;  // tilemap_64
    public Sprite spriteOff; // tilemap_66

    [Header("Logic")]
    public MovingPlatform platformToControl;

    private bool is_On = false;
    private bool playerInRange = false;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = spriteOff;
    }

    void Update()
    {
        // Kiểm tra nếu người chơi ở trong tầm và nhấn phím Xuống
        // Input.GetAxisRaw("Vertical") sẽ trả về -1 khi nhấn S hoặc mũi tên xuống
        if (playerInRange && Input.GetAxisRaw("Vertical") < 0f)
        {
            // Thêm một điều kiện nhỏ để tránh kích hoạt liên tục
            // Chúng ta sẽ dùng GetKeyDown để chỉ kích hoạt 1 lần
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                ToggleLever();
            }
        }
    }

    void ToggleLever()
    {
        is_On = !is_On;
        spriteRenderer.sprite = is_On ? spriteOn : spriteOff;

        if (platformToControl != null)
        {
            platformToControl.ToggleMovement();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}