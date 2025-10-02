using UnityEngine;

// Add a requirement for Collider2D, as it's needed for raycasting.
[RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
public class Selectable : MonoBehaviour
{
    public bool IsSelected { get; private set; } = false;
    public Material highlightMaterial; // Assign your highlight material in the inspector

    private Material originalMaterial;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Store the initial material. It's best to use sharedMaterial for efficiency
            // if multiple objects use the same base material, but for simplicity
            // and allowing individual modification (like disabling a renderer), .material is okay.
            originalMaterial = spriteRenderer.material;
        }

        // If no highlight material is assigned, try to get the default from SelectionManager if it exists.
        if (highlightMaterial == null)
        {
            if (SelectionManager.Instance != null)
            {
                highlightMaterial = SelectionManager.Instance.defaultHighlightMaterial;
            }
            if (highlightMaterial == null)
            {
                Debug.LogWarning($"Highlight material not assigned for {gameObject.name} and no default found. Highlighting may not work.");
            }
        }

        // Register this selectable with the SelectionManager
        SelectionManager.Instance?.RegisterSelectable(this);
    }

    void OnEnable()
    {
        // Ensure registration happens if the object is enabled after being disabled.
        SelectionManager.Instance?.RegisterSelectable(this);
    }

    void OnDisable()
    {
        // Unregister when the object is disabled or destroyed.
        SelectionManager.Instance?.UnregisterSelectable(this);
    }

    void OnDestroy()
    {
        // Ensure unregistration happens when the object is destroyed.
        SelectionManager.Instance?.UnregisterSelectable(this);
    }

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
            // If not selected, revert to original material.
            spriteRenderer.material = originalMaterial;
        }
    }

    // Optional: methods called by SelectionManager when selection state changes
    public void OnSelectionStart()
    {
        // Debug.Log($"{gameObject.name} started selection");
    }
    public void OnSelectionEnd()
    {
        // Debug.Log($"{gameObject.name} ended selection");
    }
}