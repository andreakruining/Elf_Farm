using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance { get; private set; } // Singleton

    private List<Selectable> allSelectables = new List<Selectable>();
    private Selectable currentHoveredObject = null; // The object currently under the mouse.

    [Header("Highlight Settings")]
    public Material defaultHighlightMaterial; // Optional: fallback highlight material.

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterSelectable(Selectable selectable)
    {
        if (!allSelectables.Contains(selectable))
        {
            allSelectables.Add(selectable);
        }
    }

    public void UnregisterSelectable(Selectable selectable)
    {
        if (allSelectables.Contains(selectable))
        {
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
            potentialHoverObject = hit.collider.GetComponent<Selectable>();
            // --- REMOVED PROXIMITY CHECK HERE ---
            // If you want to re-introduce proximity later, we'll do it differently.
        }

        // If the hovered object has changed
        if (potentialHoverObject != currentHoveredObject)
        {
            // Deselect the previously hovered object
            if (currentHoveredObject != null)
            {
                currentHoveredObject.SetSelected(false);
                currentHoveredObject.OnSelectionEnd();
            }

            // Set the new hovered object
            currentHoveredObject = potentialHoverObject;

            // Select the new hovered object if it's not null
            if (currentHoveredObject != null)
            {
                currentHoveredObject.SetSelected(true);
                currentHoveredObject.OnSelectionStart();
            }
        }
    }

    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0) && currentHoveredObject != null)
        {
            // Trigger the action system on the selected object.
            PlayerActions playerActions = FindObjectOfType<PlayerActions>(); // Optimize this if possible!
            if (playerActions != null)
            {
                playerActions.AttemptAction(currentHoveredObject);
            }
            else
            {
                Debug.LogError("PlayerActions script not found in scene!");
            }
        }
    }

    public void DeselectCurrentHoveredObject()
    {
        if (currentHoveredObject != null)
        {
            currentHoveredObject.SetSelected(false);
            currentHoveredObject.OnSelectionEnd();
            currentHoveredObject = null;
        }
    }
}