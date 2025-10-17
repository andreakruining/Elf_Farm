using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))] // Removed GridPosition requirement
public class PlayerManager : MonoBehaviour
{
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Singleton pattern for easy access
    public static PlayerManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        rb = GetComponent<Rigidbody2D>();
        // gridPosition = GetComponent<GridPosition>(); // Removed

        if (rb == null) Debug.LogError("PlayerManager requires Rigidbody2D.");
        // if (gridPosition == null) Debug.LogError("PlayerManager requires GridPosition."); // Removed
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // No GridPosition updates needed here if GridPosition is removed for selection.
        // If you keep GridPosition for other reasons, you would update it here.
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    // Removed GetCurrentGridPosition if GridPosition is removed entirely.
}