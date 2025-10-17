using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class GenericActionExecutor : MonoBehaviour
{
    public List<StaffAction> allStaffActionAssets;

    void Awake()
    {
        if (allStaffActionAssets == null || allStaffActionAssets.Count == 0)
        {
            Debug.LogWarning("GenericActionExecutor: No StaffAction assets assigned in Inspector.");
        }
    }

    // Called by PlayerActions.AttemptAction.
    public void ExecuteAction(Selectable targetSelectable)
    {
        if (targetSelectable == null) return;

        GameObject targetGameObject = targetSelectable.gameObject;
        string targetTag = targetGameObject.tag;
        SpriteRenderer targetSpriteRenderer = targetGameObject.GetComponent<SpriteRenderer>();
        Sprite currentTargetSprite = targetSpriteRenderer != null ? targetSpriteRenderer.sprite : null;

        StaffAction bestMatchAction = FindBestMatchingAction(targetTag, currentTargetSprite);

        if (bestMatchAction != null)
        {
            // --- Check for InteractiveTile component ---
            InteractiveTile interactiveTile = targetGameObject.GetComponent<InteractiveTile>();

            // --- Trigger InteractiveTile's specific logic FIRST ---
            if (interactiveTile != null)
            {
                // Pass the action asset to the interactive tile so it knows what player action was performed.
                // The InteractiveTile will then manage its own stage transitions and state changes.
                interactiveTile.Interact(bestMatchAction);
            }

            // --- Apply general effects from the StaffAction asset ---
            // This handles things like animations, sounds, visual changes not tied to stage logic.
            ApplyEffects(bestMatchAction, targetGameObject, targetSelectable.gameObject);
        }
        else
        {
            Debug.Log($"GenericActionExecutor: No suitable StaffAction asset found for target '{targetGameObject.name}' (Tag: {targetTag}, Sprite: {currentTargetSprite?.name})");
            if (SelectionManager.Instance != null) SelectionManager.Instance.DeselectCurrentHoveredObject();
        }
    }

    // Helper to find the best matching StaffAction asset. (Same as before)
    private StaffAction FindBestMatchingAction(string targetTag, Sprite currentTargetSprite)
    {
        StaffAction bestMatch = null;
        bestMatch = allStaffActionAssets.FirstOrDefault(action =>
            action != null && !string.IsNullOrEmpty(action.targetTag) && action.targetTag == targetTag &&
            action.targetSprite != null && currentTargetSprite != null && action.targetSprite == currentTargetSprite
        );
        if (bestMatch == null) bestMatch = allStaffActionAssets.FirstOrDefault(action =>
            action != null && !string.IsNullOrEmpty(action.targetTag) && action.targetTag == targetTag && action.targetSprite == null
        );
        if (bestMatch == null && currentTargetSprite != null) bestMatch = allStaffActionAssets.FirstOrDefault(action =>
            action != null && string.IsNullOrEmpty(action.targetTag) &&
            action.targetSprite != null && action.targetSprite == currentTargetSprite
        );
        return bestMatch;
    }

    // Applies the effects from a StaffAction asset. (Same as before)
    private void ApplyEffects(StaffAction action, GameObject targetObject, GameObject playerObject)
    {
        Debug.Log($"GenericActionExecutor Applying Effects for: {action.actionName}");
        foreach (ActionEffect effect in action.effects)
        {
            switch (effect.type)
            {
                case ActionEffectType.ChangeSprite:
                    SpriteRenderer sr = targetObject.GetComponent<SpriteRenderer>();
                    if (sr != null) { if (effect.resultSprite != null) sr.sprite = effect.resultSprite; else Debug.LogWarning($"Action '{action.actionName}' has ChangeSprite effect but no resultSprite."); }
                    break;
                case ActionEffectType.ChangeTag:
                    if (!string.IsNullOrEmpty(effect.resultTag)) targetObject.tag = effect.resultTag;
                    else Debug.LogWarning($"Action '{action.actionName}' has ChangeTag effect but no resultTag.");
                    break;
                case ActionEffectType.PlayPlayerAnimation:
                    if (effect.playerAnimation != null) { Animator pa = playerObject.GetComponent<Animator>(); if (pa != null) pa.Play(effect.playerAnimation.name); else Debug.LogWarning($"Player object '{playerObject.name}' missing Animator."); }
                    break;
                case ActionEffectType.PlayTargetAnimation:
                    if (effect.targetAnimation != null) { Animator ta = targetObject.GetComponent<Animator>(); if (ta != null) ta.Play(effect.targetAnimation.name); else Debug.LogWarning($"Target object '{targetObject.name}' missing Animator."); }
                    break;
                case ActionEffectType.PlaySound:
                    if (effect.sound != null) AudioSource.PlayClipAtPoint(effect.sound, playerObject.transform.position);
                    break;
                case ActionEffectType.ApplyStatusEffect: Debug.LogWarning("ApplyStatusEffect type is not yet implemented."); break;
                case ActionEffectType.CallComponentMethod: /* ... same as before ... */ break;
            }
        }
        if (SelectionManager.Instance != null) SelectionManager.Instance.DeselectCurrentHoveredObject();
    }
}