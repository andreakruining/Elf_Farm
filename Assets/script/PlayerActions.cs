
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerActions : MonoBehaviour
{
	public List<StaffAction> availableActions; // Drag ALL your StaffAction assets here.

	void Start()
	{
		// Check if GenericActionExecutor is present.
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

		// Find the StaffAction asset that best matches the target's tag and sprite.
		StaffAction matchingAction = FindBestMatchingAction(targetTag, targetSelectable.GetComponent<SpriteRenderer>()?.sprite);

		// --- CORRECTED CALL ---
		if (matchingAction != null)
		{
			GenericActionExecutor executor = FindObjectOfType<GenericActionExecutor>();
			if (executor != null)
			{
				// CALL THE CORRECT PUBLIC METHOD: ExecuteAction expects the 'Selectable'
				// The executor itself will then find the right StaffAction asset and apply effects.
				executor.ExecuteAction(targetSelectable);
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

	// Helper to find the best matching action from the PlayerActions' list.
	// This logic is similar to what was in GenericActionExecutor before,
	// but now it's here to provide the *correct* matching action to the executor.
	private StaffAction FindBestMatchingAction(string targetTag, Sprite currentTargetSprite)
	{
		StaffAction bestMatch = null;

		bestMatch = availableActions.FirstOrDefault(action =>
			action != null &&
			!string.IsNullOrEmpty(action.targetTag) && action.targetTag == targetTag &&
			action.targetSprite != null && currentTargetSprite != null && action.targetSprite == currentTargetSprite
		);

		if (bestMatch == null)
		{
			bestMatch = availableActions.FirstOrDefault(action =>
				action != null &&
				!string.IsNullOrEmpty(action.targetTag) && action.targetTag == targetTag &&
				action.targetSprite == null
			);
		}

		if (bestMatch == null && currentTargetSprite != null)
		{
			bestMatch = availableActions.FirstOrDefault(action =>
				action != null &&
				string.IsNullOrEmpty(action.targetTag) &&
				action.targetSprite != null && action.targetSprite == currentTargetSprite
			);
		}
		return bestMatch;
	}
}