using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for OrderBy

public class PlayerActions : MonoBehaviour
{
    // List of all possible actions the player can perform with their staff.
    // These should be ScriptableObject assets you create.
    public List<StaffAction> availableActions;

    void Start()
    {
        // Optional: Ensure actions are sorted by priority if needed, or by target tag.
        // For example, if you want "Cut Grass" to be tried before "Water Soil" on a grass object
        // that could potentially be a seed later, you might sort.
        // For now, we'll just iterate.
    }

    // This method is called by SelectionManager when the mouse clicks on a selected object.
    public void AttemptAction(Selectable targetSelectable)
    {
        if (targetSelectable == null)
        {
            Debug.LogWarning("AttemptAction called with null target.");
            return;
        }

        GameObject targetGameObject = targetSelectable.gameObject;
        string targetTag = targetGameObject.tag;

        // Sort actions to prioritize certain ones if they have the same targetTag
        // For example, if you have a "Grass" tag, and you want "CutGrassAction" to be
        // prioritized over a "PlantSeedAction" if somehow both were applicable.
        // For now, a simple iteration is fine if your tags are distinct enough.

        // Find the FIRST action in the list that matches the target's tag.
        StaffAction matchingAction = availableActions.FirstOrDefault(action => action.targetTag == targetTag);

        if (matchingAction != null)
        {
            // Perform the action
            matchingAction.PerformAction(targetGameObject, this.gameObject); // Pass target and player GameObject

            // After performing an action, you might want to deselect the object.
            // This depends on your game's flow. For example, if harvesting an item, you might keep it selected
            // to potentially pick it up. If tilling, you might deselect.
            SelectionManager.Instance.DeselectCurrentHoveredObject();
        }
        else
        {
            Debug.Log($"No suitable action found in availableActions for target with tag: {targetTag}");
            // Optionally, if no action is found, you might deselect or perform a default action.
            SelectionManager.Instance.DeselectCurrentHoveredObject();
        }
    }
}