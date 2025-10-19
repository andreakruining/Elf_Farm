using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System; // Ensure this is present

public class InteractiveTile : MonoBehaviour
{
    // public PlantData plantDefinition; // We will assign this when a seed is planted.

    [Header("Plant Definition (assigned when seed is planted)")]
    public PlantData plantDefinition; // This will be set by the seed planting action.

    [Header("Current State")]
    public PlantStage currentStage = PlantStage.Grass; // Default starting state.
    public bool isWatered = false;
    public float timeInCurrentStage = 0f;
    public float timeSinceLastWatering = Mathf.Infinity;

    private SpriteRenderer spriteRenderer;
    private PlantStageData currentStageData;
    private Coroutine growthCoroutine;
    private GameObject playerObject;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError($"InteractiveTile requires a SpriteRenderer on {gameObject.name}.");

        playerObject = PlayerManager.Instance?.gameObject;
        if (playerObject == null) Debug.LogWarning("InteractiveTile: PlayerManager.Instance not found. Some interactions might fail.");

        // Initialize based on starting stage.
        UpdateVisualsAndStageData(currentStage); // Load data for the initial stage (e.g., Grass or Soil_Empty)
        UpdateVisualsSprite();
    }

    // ... (Update and GetCurrentStageData methods remain the same) ...

    // --- Interaction Handler ---
    public void Interact(StaffAction action)
    {
        // Fetch latest stage data in case it changed recently.
        // Important to call this BEFORE checking stages, as it might be a new stage.
        UpdateVisualsAndStageData(currentStage);

        bool stateChangedOrTransitioned = false; // Flag to know if visuals need updating.

        // --- Player Action Logic based on current stage and action name ---
        switch (currentStage)
        {
            case PlantStage.Grass:
                if (action.actionName == currentStageData.actionToTillSoil)
                {
                    TransitionToStage(PlantStage.Soil_Empty, false);
                    stateChangedOrTransitioned = true;
                }
                break;

            case PlantStage.Soil_Empty:
                // --- NEW: Handle Planting a Seed ---
                // Check if the action is "Plant Seed" AND if the action carries seed data.
                if (action.actionName == currentStageData.actionToPlantSeed && action.seedToPlantData != null)
                {
                    PlantSeed(action.seedToPlantData); // Use the PlantData from the action asset.
                    stateChangedOrTransitioned = true; // Visuals will be updated inside PlantSeed.
                }
                else if (action.actionName == currentStageData.actionToTillSoil) // Allow tilling again
                {
                    TransitionToStage(PlantStage.Soil_Empty, false);
                    stateChangedOrTransitioned = true;
                }
                break;

            case PlantStage.Soil_Seeded: // Now this is the actual "planted seed" stage.
                if (action.actionName == currentStageData.actionToWater && currentStageData.requiresWatering)
                {
                    if (timeSinceLastWatering >= currentStageData.wateringCooldown)
                    {
                        isWatered = true;
                        timeSinceLastWatering = 0f;
                        stateChangedOrTransitioned = true; // Visuals will update.
                    }
                    else { Debug.Log("Cannot water yet, on cooldown."); }
                }
                break;

            // --- Rose Stages --- (These remain similar, but use 'currentStageData' correctly)
            case PlantStage.Rose_Sprout:
                HandleWateringInteraction(action, currentStageData.actionToWater, currentStageData.requiresWatering, currentStageData.wateringCooldown);
                break;
            case PlantStage.Rose_SproutWatered:
                HandleWateringInteraction(action, currentStageData.actionToWater, currentStageData.requiresWatering, currentStageData.wateringCooldown);
                break;
            case PlantStage.Rose_Budding:
                HandleWateringInteraction(action, currentStageData.actionToWater, currentStageData.requiresWatering, currentStageData.wateringCooldown);
                break;
            case PlantStage.Rose_BuddingWatered:
                HandleWateringInteraction(action, currentStageData.actionToWater, currentStageData.requiresWatering, currentStageData.wateringCooldown);
                break;
            case PlantStage.Rose_Ripe:
                if (action.actionName == currentStageData.actionToHarvest)
                {
                    TransitionToStage(PlantStage.Soil_Empty, false);
                    stateChangedOrTransitioned = true; // Transition handled, visuals will update.
                    Debug.Log($"Harvested {gameObject.name}!");
                    // You would also trigger inventory/item drop here.
                }
                break;
        }

        // Update visuals if any state changed.
        if (stateChangedOrTransitioned)
        {
            UpdateVisualsSprite(); // Update sprite based on new state (stage, watered status).
            // If the stage actually changed, we need to get the new stage data for subsequent checks.
            if (currentStage != PlantStage.Soil_Seeded && // Avoid re-getting data if it's just a watered status change
                currentStage != PlantStage.Rose_SproutWatered && // unless the current state requires watering
                currentStage != PlantStage.Rose_BuddingWatered)
            {
                UpdateVisualsAndStageData(currentStage);
            }
        }
    }

    private void PlantSeed(PlantData seedToPlantData)
    {
        throw new NotImplementedException();
    }

    // --- Helper for Watering Interactions ---
    // This simplifies the common watering logic.
    private void HandleWateringInteraction(StaffAction action, string expectedActionName, bool needsWatering, float cooldown)
    {
        if (action.actionName == expectedActionName && needsWatering)
        {
            if (timeSinceLastWatering >= cooldown)
            {
                isWatered = true;
                timeSinceLastWatering = 0f;
                // Signal that visuals need updating.
                // The main Interact method will call UpdateVisualsSprite if stageChanged is true.
                // For just watering, we need to make sure visuals are updated if isWatered changes.
                UpdateVisualsSprite(); // Update sprite immediately for watering feedback.
                Debug.Log($"Watered {gameObject.name}.");
            }
            else { Debug.Log("Cannot water yet, on cooldown."); }
        }
    }

    // --- Stage Transition ---
    public void TransitionToStage(PlantStage newStage, bool isAutomatic)
    {
        currentStage = newStage;
        timeInCurrentStage = 0f;
        isWatered = false; // Reset watering status for new stages.
        timeSinceLastWatering = Mathf.Infinity; // Reset watering cooldown.

        // Load data for the new stage.
        UpdateVisualsAndStageData(currentStage);

        UpdateVisualsSprite(); // Update sprite for the new stage.
    }

    private void UpdateVisualsAndStageData(PlantStage stage)
    {
        currentStageData = plantDefinition.GetStageData(stage);
        if (currentStageData.stage != stage)
        {
            Debug.LogWarning($"PlantData '{plantDefinition?.plantName ?? "Null"}' does not contain data for stage '{stage}'.");
            // Optionally assign default data here to prevent null errors.
            currentStageData = new PlantStageData() { stage = stage, stageDuration = 0f, requiresWatering = false, nextStageAuto = stage, nextStageOnWater = stage, nextStageOnPlayerAction = stage, wateringCooldown = 0f };
        }
    }

    // Updates sprite based on stage, watered status, and definition.
    private void UpdateVisualsSprite()
    {
        if (spriteRenderer == null || plantDefinition == null) return;

        Sprite spriteToDisplay = null;
        if (currentStageData.stage == currentStage) // If stage data is valid
        {
            if (currentStageData.requiresWatering)
            {
                spriteToDisplay = isWatered ? currentStageData.wateredSprite : currentStageData.stageSprite;
            }
            else
            {
                spriteToDisplay = currentStageData.stageSprite;
            }
        }
        else if (currentStage == PlantStage.Soil_Empty)
        {
            // Fallback for empty soil if no specific data or it's not found.
            // You might need a default "empty soil" sprite, or this will be handled by the "Till Soil" action's result sprite.
            spriteToDisplay = null; // Or perhaps a default empty soil sprite if you add one.
        }
        else if (currentStage == PlantStage.Soil_Seeded)
        {
            spriteToDisplay = plantDefinition.seedSprite; // Sprite for the planted seed.
        }

        if (spriteToDisplay != null) spriteRenderer.sprite = spriteToDisplay;
        else
        {
            // If no sprite is determined, try to use the default grass sprite or just log.
            // This might happen if a stage is missing its sprite data.
            Debug.LogWarning($"No sprite determined for stage {currentStage} on {gameObject.name}. Leaving sprite as is or will be set by a default.");
        }
    }

    // --- Seed Planting Method ---
    // This method is called by a StaffAction's effect when planting a specific seed.
    public void PlantSpecificSeed(PlantData seedPlantData)
    {
        if (currentStage == PlantStage.Soil_Empty && seedPlantData != null)
        {
            this.plantDefinition = seedPlantData; // Assign the data for THIS specific plant.
            TransitionToStage(PlantStage.Soil_Seeded, false); // Transition to the seeded stage.
            Debug.Log($"Planted {seedPlantData.plantName} in {gameObject.name}.");
        }
        else
        {
            Debug.LogWarning($"Cannot plant here. Current stage: {currentStage}, or seed data is missing/invalid.");
        }
    }
}