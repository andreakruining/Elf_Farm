using UnityEngine;

// This component simply holds and manages a grid coordinate.
// It does NOT assume it's part of a Unity Tilemap system,
// nor does it rely on finding a Grid component.
// Its gridPosition must be set manually in the Inspector.
public class GridPosition : MonoBehaviour
{
    // Publicly accessible grid coordinate.
    // You MUST set this manually in the inspector for objects,
    // or update it via code when they move.
    public Vector3Int gridPosition; // If you need this for other mechanics, keep it. Otherwise, it can be removed.

    // If you don't need explicit grid coordinates for anything else,
    // you can likely delete this whole script.
}