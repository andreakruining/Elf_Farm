using UnityEngine;
using System.Collections.Generic;

public enum ActionEffectType
{
    None,
    ChangeSprite,
    ChangeTag,
    PlayPlayerAnimation,
    PlayTargetAnimation,
    PlaySound,
    ApplyStatusEffect,
    CallComponentMethod, // Useful for complex custom logic NOT handled directly here.
    // NEW: Player Action Trigger - this effect signals the InteractiveTile to use a specific action.
    TriggerInteractiveTileAction
}

[System.Serializable]
public class ActionEffect
{
    public ActionEffectType type = ActionEffectType.None;

    [Header("Sprite/Tag Change")]
    public Sprite resultSprite;
    public string resultTag;

    [Header("Animations")]
    public AnimationClip playerAnimation;
    public AnimationClip targetAnimation;

    [Header("Audio")]
    public AudioClip sound;

    [Header("Custom Logic")]
    public string componentMethodCall; // e.g., "SeedComponent.StartGrowth"

    [Header("Interactive Tile Action Trigger")]
    [Tooltip("Specify the player action name (e.g., 'Water', 'Plant Seed') to trigger on an InteractiveTile.")]
    public string interactiveActionName; // Name of the action to pass to InteractiveTile.Interact()
}

[CreateAssetMenu(fileName = "NewStaffAction", menuName = "YourGame/StaffActions/GenericAction")]
public class StaffAction : ScriptableObject
{
    [Header("General Action Info")]
    public string actionName = "New Action"; // e.g., "Cut Grass", "Water", "Plant Seed"
    public Sprite actionIcon;

    [Header("Targeting")]
    public string targetTag;
    public Sprite targetSprite;

    [Header("Effects List")]
    public List<ActionEffect> effects = new List<ActionEffect>();

    // Add common properties like mana cost, cooldown, etc., if needed.
}