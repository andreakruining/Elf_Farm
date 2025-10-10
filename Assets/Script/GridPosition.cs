using UnityEngine;

// This component simply holds and manages a grid coordinate.
// It does NOT assume it's part of a Unity Tilemap system.
public class GridPosition : MonoBehaviour
{
    // Publicly accessible grid coordinate.
    // You'll need to set this manually in the inspector for objects,
    // or update it via code when they move.
    public Vector3Int gridPosition;

    // Helper to update position if the object is moving in world space.
    // Call this method from your movement script.
    public void UpdateGridPositionFromWorld(Vector3 worldPosition)
    {
        // For a simple rectangle grid, we can often just round the world position.
        // Adjust this rounding logic if your world-to-grid conversion is more complex.
        gridPosition.x = Mathf.RoundToInt(worldPosition.x);
        gridPosition.y = Mathf.RoundToInt(worldPosition.y);
        gridPosition.z = 0; // Assuming a 2D grid, Z is usually 0.
    }

    // Helper to check if two grid positions are adjacent (including diagonals)
    // This is a static helper, so it can be called from anywhere.
    public static bool IsAdjacent(Vector3Int pos1, Vector3Int pos2)
    {
        // Manhattan distance for horizontal/vertical adjacency
        int dx = Mathf.Abs(pos1.x - pos2.x);
        int dy = Mathf.Abs(pos1.y - pos2.y);

        // Checks if the distance is exactly 1 in either x or y direction, or both.
        // This covers horizontal, vertical, and diagonal neighbors.
        // It also ensures it's not the same position (dx=0, dy=0).
        return dx <= 1 && dy <= 1 && !(dx == 0 && dy == 0);
    }
}