using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of player movement

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("PlayerManager requires a Rigidbody2D component on the GameObject.");
            enabled = false; // Disable the script if Rigidbody2D is missing
        }
    }

    void Update()
    {
        // Get input from WASD keys
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrow keys
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S or Up/Down arrow keys

        // Normalize the input to prevent faster diagonal movement
        moveInput.Normalize();
    }

    void FixedUpdate()
    {
        // Apply velocity to the Rigidbody2D
        rb.velocity = moveInput * moveSpeed;
    }
}