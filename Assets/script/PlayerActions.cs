using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerActions : MonoBehaviour
{
    public List<StaffAction> availableActions; // Assign ALL StaffAction assets here.

    void Start()
    {
        if (FindObjectOfType<GenericActionExecutor>() == null)
        {
            Debug.LogError("PlayerActions requires a GenericActionExecutor to be present in the scene!");
        }
    }

    public void AttemptAction(Selectable targetSelectable)
    {
        if (targetSelectable == null)
        {
            Debug.LogWarning("AttemptAction called with null target.");
            return;
        }

        GameObject targetGameObject = targetSelectable.gameObject;
        string targetTag = targetGameObject.tag;
        SpriteRenderer targetSpriteRenderer = targetSelectable.GetComponent<SpriteRenderer>();
        Sprite currentTargetSprite = targetSpriteRenderer != null ? targetSpriteRenderer.sprite : null;

        // Find the best matching StaffAction asset.
        StaffAction matchingAction = FindBestMatchingAction(targetTag, currentTargetSprite);

        if (matchingAction != null)
        {
            GenericActionExecutor executor = FindObjectOfType<GenericActionExecutor>();
            if (executor != null)
            {
                // --- CORRECTED CALL ---
                // Pass the found StaffAction asset, the target GameObject, and the player GameObject.
                // The executor will now handle all the logic, including special cases like planting.
                executor.ExecuteAction(matchingAction, targetGameObject, this.gameObject);
            }
            else
            {
                Debug.LogError("GenericActionExecutor not found in scene when attempting to perform action!");
            }
        }
        else
        {
            Debug.Log($"No suitable StaffAction found in availableActions for target with tag: {targetTag}");
            if (SelectionManager.Instance != null)
            {
                SelectionManager.Instance.DeselectCurrentHoveredObject();
            }
        }
    }

    // Helper to find the best matching action from PlayerActions' availableActions list.
    private StaffAction FindBestMatchingAction(string targetTag, Sprite currentTargetSprite)
    {
        StaffAction bestMatch = null;
        bestMatch = availableActions.FirstOrDefault(action =>
            action != null && !string.IsNullOrEmpty(action.targetTag) && action.targetTag == targetTag &&
            action.targetSprite != null && currentTargetSprite != null && action.targetSprite == currentTargetSprite
        );
        if (bestMatch == null) bestMatch = availableActions.FirstOrDefault(action =>
            action != null && !string.IsNullOrEmpty(action.targetTag) && action.targetTag == targetTag && action.targetSprite == null
        );
        if (bestMatch == null && currentTargetSprite != null) bestMatch = availableActions.FirstOrDefault(action =>
            action != null && string.IsNullOrEmpty(action.targetTag) &&
            action.targetSprite != null && action.targetSprite == currentTargetSprite
        );
        return bestMatch;
    }
}