using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // The player's transform to follow
    public float smoothSpeed = 0.125f; // How smoothly the camera catches up to the player
    public Vector3 offset; // The desired offset from the player

    [Header("Bounds")]
    public bool enableBounds = true; // Should bounds be applied?
    public float minX, maxX, minY, maxY; // The boundaries of the playable area

    private Camera mainCamera;
    private float cameraHalfWidth;
    private float cameraHalfHeight;

    void Awake()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraFollow script requires a Camera component on the same GameObject.");
            enabled = false; // Disable the script if no camera is found
            return;
        }

        // Calculate camera view dimensions based on its orthographic size
        // For orthographic cameras:
        cameraHalfHeight = mainCamera.orthographicSize;
        cameraHalfWidth = cameraHalfHeight * mainCamera.aspect; // Aspect ratio is width / height
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target (player) is not assigned.");
            return;
        }

        // --- Following Logic ---
        // Calculate the desired position for the camera
        Vector3 desiredPosition = target.position + offset;

        // Smoothly interpolate between the camera's current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // --- Bounding Logic ---
        if (enableBounds)
        {
            ApplyBounds();
        }
    }

    void ApplyBounds()
    {
        // Get the current camera's position
        Vector3 currentCameraPosition = transform.position;

        // Clamp the camera's X position within the minX and maxX bounds
        // We need to consider the camera's own width so it doesn't go beyond the bounds itself.
        float clampedX = Mathf.Clamp(currentCameraPosition.x, minX + cameraHalfWidth, maxX - cameraHalfWidth);

        // Clamp the camera's Y position within the minY and maxY bounds
        // We need to consider the camera's own height.
        float clampedY = Mathf.Clamp(currentCameraPosition.y, minY + cameraHalfHeight, maxY - cameraHalfHeight);

        // Update the camera's position with the clamped values
        transform.position = new Vector3(clampedX, clampedY, currentCameraPosition.z);
    }

    // Optional: Draw gizmos in the scene view to visualize bounds
    void OnDrawGizmosSelected()
    {
        if (!enableBounds) return;

        Gizmos.color = Color.yellow;
        // Draw a rectangle representing the playable area limits
        Vector3 center = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
        Vector3 size = new Vector3(maxX - minX, maxY - minY, 0);
        Gizmos.DrawWireCube(center, size);

        // Visualize the camera's restricted area within these bounds
        if (mainCamera != null)
        {
            float camHeight = mainCamera.orthographicSize;
            float camWidth = camHeight * mainCamera.aspect;

            Gizmos.color = Color.red;
            // Center of the visible area
            Vector3 visibleCenter = new Vector3((minX + maxX) / 2f, (minY + maxY) / 2f, transform.position.z);
            // Size of the area the camera *cannot* go outside of
            Vector3 visibleSize = new Vector3(maxX - minX - 2 * camWidth, maxY - minY - 2 * camHeight, 0);

            // If the visible area is valid (i.e., camera is smaller than bounds)
            if (visibleSize.x > 0 && visibleSize.y > 0)
            {
                Gizmos.DrawWireCube(visibleCenter, visibleSize);
            }
        }
    }
}