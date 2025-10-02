using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Required for Linq operations like .FirstOrDefault()

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; } // Singleton pattern

    // This list will hold all active Selectable components in the scene.
    private List<Selectable> allSelectables = new List<Selectable>();

    private Selectable currentHoveredObject = null; // The object currently under the mouse

    [Header("Highlight Settings")]
    public Material defaultHighlightMaterial; // Assign your main highlight material here if all selectables use the same one.
                                              // Or, keep it on individual Selectable components if they can have different highlights.

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // This method is called by Selectable components when they are enabled or created.
    public void RegisterSelectable(Selectable selectable)
    {
        if (!allSelectables.Contains(selectable))
        {
            allSelectables.Add(selectable);
            // Optionally, if you have a default highlight material to apply here:
            // if (selectable.highlightMaterial == null && defaultHighlightMaterial != null)
            // {
            //     selectable.highlightMaterial = defaultHighlightMaterial;
            // }
        }
    }

    // This method is called by Selectable components when they are disabled or destroyed.
    public void UnregisterSelectable(Selectable selectable)
    {
        if (allSelectables.Contains(selectable))
        {
            // If the object being unregistered is the currently hovered one, deselect it.
            if (currentHoveredObject == selectable)
            {
                DeselectCurrentHoveredObject();
            }
            allSelectables.Remove(selectable);
        }
    }

    void Update()
    {
        HandleMouseHover();
        HandleMouseClick();
    }

    private void HandleMouseHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

        Selectable potentialHoverObject = null;

        if (hit.collider != null)
        {
            // Try to get the Selectable component from the hit object.
            potentialHoverObject = hit.collider.GetComponent<Selectable>();
        }

        // Check if the hovered object has changed
        if (potentialHoverObject != currentHoveredObject)
        {
            // Deselect the previously hovered object
            if (currentHoveredObject != null)
            {
                currentHoveredObject.SetSelected(false); // Call SetSelected to handle material change
                currentHoveredObject.OnSelectionEnd();   // Call custom end selection logic
            }

            // Set the new hovered object
            currentHoveredObject = potentialHoverObject;

            // Select the new hovered object if it's not null
            if (currentHoveredObject != null)
            {
                currentHoveredObject.SetSelected(true); // Call SetSelected to handle material change
                currentHoveredObject.OnSelectionStart(); // Call custom start selection logic
            }
        }
        // If we hit nothing or an object without Selectable, potentialHoverObject will be null,
        // and the above 'if (potentialHoverObject != currentHoveredObject)' logic handles deselecting.
    }

    private void HandleMouseClick()
    {
        // Check if the left mouse button was clicked and if we have a currently hovered object
        if (Input.GetMouseButtonDown(0) && currentHoveredObject != null)
        {
            // Trigger the action system on the selected object
            // We need to ensure the PlayerActions script is on the player and can access this.
            // For simplicity here, let's assume PlayerActions is accessible.
            PlayerActions playerActions = FindObjectOfType<PlayerActions>(); // You might want a more robust way to get the player
            if (playerActions != null)
            {
                playerActions.AttemptAction(currentHoveredObject);
            }
        }
    }

    // This method is called by PlayerActions to deselect if an action occurs.
    public void DeselectCurrentHoveredObject()
    {
        if (currentHoveredObject != null)
        {
            currentHoveredObject.SetSelected(false);
            currentHoveredObject.OnSelectionEnd();
            currentHoveredObject = null;
        }
    }

    // This method will be called by PlayerActions if you want to select *programmatically*
    // (e.g., from UI buttons, or when the player interacts with something directly)
    public void ProgrammaticallySelect(Selectable selectable)
    {
        if (selectable == currentHoveredObject) return; // Already selected/hovered

        // Deselect current if it exists
        if (currentHoveredObject != null)
        {
            currentHoveredObject.SetSelected(false);
            currentHoveredObject.OnSelectionEnd();
        }

        currentHoveredObject = selectable;
        currentHoveredObject.SetSelected(true);
        currentHoveredObject.OnSelectionStart();
    }
}