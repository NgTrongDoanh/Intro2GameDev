using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Điểm di chuyển")]
    public Transform pointA; // Điểm bắt đầu
    public Transform pointB; // Điểm kết thúc

    [Header("Thông số")]
    [Tooltip("Thời gian (giây) để di chuyển từ A đến B")]
    public float travelTime = 3f;

    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;

    void Start()
    {
        // Lưu lại vị trí các điểm mốc CỐ ĐỊNH
        startPosition = pointA.position;
        endPosition = pointB.position;
    }

    void Update()
    {
        if (!isMoving)
        {
            return;
        }
        
        // Tính toán vị trí dựa trên thời gian để tạo ra chuyển động ping-pong mượt mà
        float time = Mathf.PingPong(Time.time / travelTime, 1);
        transform.position = Vector3.Lerp(startPosition, endPosition, time);
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f); // Đảm bảo z = 0
        Debug.Log("Nền tảng đang di chuyển. Vị trí hiện tại: " + transform.position);
    }

    // Hàm công khai để Cần gạt gọi
    public void ToggleMovement()
    {
        isMoving = !isMoving;
    }
}