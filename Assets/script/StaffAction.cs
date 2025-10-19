using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Included here in case it's needed by StaffAction itself later, though not strictly for its definition.

// Enum to define different types of effects an action can have.
// Make this public so other scripts can use it.
[System.Serializable] // Essential for structs used in lists that appear in the inspector
public enum ActionEffectType
{
    None,
    ChangeSprite,
    ChangeTag,
    PlayPlayerAnimation,
    PlayTargetAnimation,
    PlaySound,
    ApplyStatusEffect,
    CallComponentMethod,
    TriggerInteractiveTileAction // This effect type is used to signal a direct call to InteractiveTile's methods.
}

// Serializable class to hold the data for a single effect within an action.
// Make this public and serializable so it can be accessed and displayed.
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
    [Tooltip("Specify the action name to pass to InteractiveTile.Interact() for this specific effect.")]
    public string interactiveActionName; // e.g., "Water", "Harvest", "Till Soil", "Plant Seed"

    // *** Important Note on Passing PlantData for Planting ***
    // If an effect needs to pass specific data like PlantData,
    // you'd need to add a field here, e.g., public PlantData plantDataToPass;
    // For now, we'll have PlayerActions detect the "Plant Seed" action and pass it.
}

// ScriptableObject defining a single staff action.
[CreateAssetMenu(fileName = "NewStaffAction", menuName = "YourGame/StaffActions/GenericAction")]
public class StaffAction : ScriptableObject
{
    [Header("General Action Info")]
    public string actionName = "New Action";
    public Sprite actionIcon;

    [Header("Targeting")]
    public string targetTag;
    public Sprite targetSprite;

    [Header("Seed Planting Specific")]
    [Tooltip("If this action is 'Plant Seed', assign the PlantData asset for the seed being planted.")]
    public PlantData seedToPlantData; // This is used by PlayerActions to pass data.

    [Header("Effects List")]
    public List<ActionEffect> effects = new List<ActionEffect>();
}