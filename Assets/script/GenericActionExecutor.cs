using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class GenericActionExecutor : MonoBehaviour
{
    // --- CRUCIAL FIX ---
    // Ensure this list is public so it's visible in the Inspector.
    // It's already declared as public, so this might just be a re-confirmation or checking if it was accidentally changed.
    public List<StaffAction> allStaffActionAssets;

    void Awake()
    {
        // Validation check
        if (FindObjectsOfType<StaffAction>().Length == 0)
        {
            Debug.LogWarning("GenericActionExecutor: No StaffAction assets found in the project. Ensure they are created correctly.");
        }
    }

    // Method called by PlayerActions.AttemptAction.
    // It receives the already matched StaffAction asset, the target GameObject, and the player GameObject.
    public void ExecuteAction(StaffAction actionToExecute, GameObject targetGameObject, GameObject playerObject)
    {
        if (actionToExecute == null)
        {
            Debug.LogWarning("ExecuteAction called with a null StaffAction asset.");
            return;
        }

        string targetTag = targetGameObject.tag;
        SpriteRenderer targetSpriteRenderer = targetGameObject.GetComponent<SpriteRenderer>();
        Sprite currentTargetSprite = targetSpriteRenderer != null ? targetSpriteRenderer.sprite : null;

        // Now, instead of finding the action here, we process the one that was passed in.
        // We still need to find the *correct* StaffAction asset IF the passed 'actionToExecute'
        // doesn't contain all the necessary info for `ApplyEffects` directly.
        // However, the PlayerActions is already finding the best match.
        // So, we should ideally just USE the 'actionToExecute' that was passed.

        // Let's assume PlayerActions already found the best match.
        // The GenericActionExecutor's job is to execute it.
        // If actionToExecute is the StaffAction asset, it already HAS all the effect data.

        // The issue might be in how we're handling the action name vs. its effects.
        // The Executor's ApplyEffects method directly uses the passed actionToExecute.

        // Re-evaluating the flow:
        // PlayerActions finds the best StaffAction asset.
        // PlayerActions passes that FOUND asset to GenericActionExecutor.ExecuteAction.
        // GenericActionExecutor then uses the passed StaffAction asset to:
        // 1. Check if it's targeting an InteractiveTile and call its methods.
        // 2. Apply the effects defined IN THAT StaffAction asset.

        // The 'allStaffActionAssets' list in GenericActionExecutor might be redundant if PlayerActions always finds the action.
        // Let's simplify and rely on PlayerActions passing the correct asset.

        // --- CORRECTED FLOW WITHIN ExecuteAction ---
        // The parameters are correct: actionToExecute, targetGameObject, playerObject.
        // The issue might be in how 'actionToExecute' is used downstream or if it's null.

        // Let's ensure the correct matching logic is called if needed, but primarily
        // we should rely on the action passed from PlayerActions.

        // The previous logic in GenericActionExecutor to FIND the action might have been unnecessary
        // if PlayerActions ALREADY found the best one.
        // Let's test with the assumption that PlayerActions provides a valid, found action.

        Debug.Log($"GenericActionExecutor executing action: {actionToExecute.actionName} on {targetGameObject.name}");

        InteractiveTile interactiveTile = targetGameObject.GetComponent<InteractiveTile>();

        // --- Handle specific actions that directly affect InteractiveTile ---
        // The 'actionToExecute' is the StaffAction asset found by PlayerActions.
        if (interactiveTile != null)
        {
            // If the action is "Plant Seed", and we have seed data in the StaffAction asset.
            if (actionToExecute.actionName == "Plant Seed" && actionToExecute.seedToPlantData != null)
            {
                interactiveTile.PlantSpecificSeed(actionToExecute.seedToPlantData);
            }
            else
            {
                // For all other actions targeting an InteractiveTile, pass the action directly.
                interactiveTile.Interact(actionToExecute);
            }
        }

        // --- Apply general effects from the StaffAction asset ---
        // This should apply effects defined within the 'actionToExecute' asset.
        ApplyEffects(actionToExecute, targetGameObject, playerObject);
    }

    // The ApplyEffects method remains the same, it works with the provided action asset.
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
                case ActionEffectType.CallComponentMethod:
                    if (!string.IsNullOrEmpty(effect.componentMethodCall))
                    {
                        string[] parts = effect.componentMethodCall.Split('.');
                        if (parts.Length == 2)
                        {
                            string componentName = parts[0]; string methodName = parts[1];
                            Component component = targetObject.GetComponent(componentName);
                            if (component != null)
                            {
                                MethodInfo method = component.GetType().GetMethod(methodName);
                                if (method != null) method.Invoke(component, null);
                                else Debug.LogWarning($"Method '{methodName}' not found on component '{componentName}' on object '{targetObject.name}'.");
                            }
                            else Debug.LogWarning($"Component '{componentName}' not found on object '{targetObject.name}' for method call.");
                        }
                        else Debug.LogWarning($"Invalid componentMethodCall format: '{effect.componentMethodCall}'. Expected 'ComponentName.MethodName'.");
                    }
                    break;
            }
        }
        if (SelectionManager.Instance != null)
        {
            SelectionManager.Instance.DeselectCurrentHoveredObject();
        }
    }
}