using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))] // Removed GridPosition requirement
public class Selectable : MonoBehaviour
{
    public bool IsSelected { get; private set; } = false;
    public Material highlightMaterial; // Assign your highlight material

    private Material originalMaterial;
    private SpriteRenderer spriteRenderer;
    // private GridPosition gridPosition; // Removed

    // --- Methods to manage selection state and visual highlight ---
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        UpdateHighlight();
    }

    private void UpdateHighlight()
    {
        if (spriteRenderer == null) return;

        if (IsSelected && highlightMaterial != null)
        {
            spriteRenderer.material = highlightMaterial;
        }
        else
        {
            spriteRenderer.material = originalMaterial; // Revert to original material
        }
    }

    // --- Unity Lifecycle & Registration ---
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // gridPosition = GetComponent<GridPosition>(); // Removed

        if (spriteRenderer == null) Debug.LogError($"Selectable requires a SpriteRenderer on {gameObject.name}.");
        // if (gridPosition == null) Debug.LogError($"Selectable requires a GridPosition on {gameObject.name}."); // Removed

        if (spriteRenderer != null)
        {
            originalMaterial = spriteRenderer.material; // Store initial material
        }

        // Attempt to get a highlight material, falling back to a default if available.
        if (highlightMaterial == null && SelectionManager.Instance != null)
        {
            highlightMaterial = SelectionManager.Instance.defaultHighlightMaterial;
        }
        if (highlightMaterial == null)
        {
            Debug.LogWarning($"Highlight material not assigned for {gameObject.name} and no default found.");
        }

        // Register with the SelectionManager when this object becomes active.
        SelectionManager.Instance?.RegisterSelectable(this);
    }

    void OnEnable()
    {
        SelectionManager.Instance?.RegisterSelectable(this);
    }

    void OnDisable()
    {
        SelectionManager.Instance?.UnregisterSelectable(this);
    }

    void OnDestroy()
    {
        SelectionManager.Instance?.UnregisterSelectable(this);
    }

    // --- Optional: Custom selection start/end logic ---
    public void OnSelectionStart() { /* Debug.Log($"{gameObject.name} started selection"); */ }
    public void OnSelectionEnd() { /* Debug.Log($"{gameObject.name} ended selection"); */ }
}