using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Ensure this is here if not already

public class InteractiveTile : MonoBehaviour
{
    [Header("Plant Definition")]
    public PlantData plantDefinition;

    [Header("Current State")]
    public PlantStage currentStage = PlantStage.Grass;
    public bool isWatered = false;
    public float timeInCurrentStage = 0f;
    public float timeSinceLastWatering = Mathf.Infinity;

    private SpriteRenderer spriteRenderer;
    private PlantStageData currentStageData; // This should be populated by UpdateVisualsAndStageData
    private Coroutine growthCoroutine;
    private GameObject playerObject;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) Debug.LogError($"InteractiveTile requires a SpriteRenderer on {gameObject.name}.");

        playerObject = PlayerManager.Instance?.gameObject;
        if (playerObject == null) Debug.LogWarning("InteractiveTile: PlayerManager.Instance not found. Some interactions might fail.");

        // Initialize based on starting stage. This ensures currentStageData is populated.
        UpdateVisualsAndStageData(currentStage);
        UpdateVisualsSprite();
    }

    void Update()
    {
        timeInCurrentStage += Time.deltaTime;
        timeSinceLastWatering += Time.deltaTime;

        // --- Auto-Progression Logic ---
        if (currentStageData.stageDuration > 0 && timeInCurrentStage >= currentStageData.stageDuration)
        {
            bool canAutoProgress = !currentStageData.requiresWatering || isWatered;
            if (canAutoProgress)
            {
                TransitionToStage(currentStageData.nextStageAuto, false);
            }
        }
        // --- Deterioration Logic ---
        // *** THIS IS THE LINE THAT LIKELY CAUSED THE ERROR IF 'currentStageData' WAS NULL ***
        // Make sure currentStageData is valid before accessing its members.
        else if (currentStageData.requiresWatering && timeSinceLastWatering >= currentStageData.wateringCooldown * 1.5f)
        {
            if (isWatered)
            {
                isWatered = false;
                UpdateVisualsSprite();
                Debug.Log($"{gameObject.name} is no longer watered.");
            }
        }
    }

    // --- Interaction Handler ---
    public void Interact(StaffAction action)
    {
        if (plantDefinition == null) { Debug.LogError($"InteractiveTile on {gameObject.name} has no PlantData assigned!"); return; }
        UpdateVisualsAndStageData(currentStage); // Ensure we have data for the current stage.

        bool stageChanged = false;

        switch (currentStage)
        {
            case PlantStage.Grass:
                if (action.actionName == currentStageData.actionToTillSoil) { TransitionToStage(PlantStage.Soil_Empty, false); stageChanged = true; }
                break;
            case PlantStage.Soil_Empty:
                if (action.actionName == currentStageData.actionToPlantSeed && plantDefinition.seedSprite != null)
                {
                    currentStage = PlantStage.Soil_Seeded; isWatered = false; timeInCurrentStage = 0f; timeSinceLastWatering = Mathf.Infinity;
                    stageChanged = true;
                }
                break;
            case PlantStage.Soil_Seeded:
                if (action.actionName == currentStageData.actionToWater && currentStageData.requiresWatering)
                {
                    if (timeSinceLastWatering >= currentStageData.wateringCooldown) // *** CORRECT ACCESS: Via currentStageData ***
                    {
                        isWatered = true; timeSinceLastWatering = 0f; stageChanged = true;
                    }
                    else { Debug.Log("Cannot water yet, on cooldown."); }
                }
                break;
            case PlantStage.Rose_Sprout:
                if (action.actionName == currentStageData.actionToWater && currentStageData.requiresWatering)
                {
                    if (timeSinceLastWatering >= currentStageData.wateringCooldown) // *** CORRECT ACCESS: Via currentStageData ***
                    {
                        isWatered = true; timeSinceLastWatering = 0f; stageChanged = true;
                    }
                    else { Debug.Log("Cannot water yet, on cooldown."); }
                }
                break;
            case PlantStage.Rose_SproutWatered:
                if (action.actionName == currentStageData.actionToWater)
                {
                    if (timeSinceLastWatering >= currentStageData.wateringCooldown) // *** CORRECT ACCESS: Via currentStageData ***
                    {
                        isWatered = true; timeSinceLastWatering = 0f; stageChanged = true;
                    }
                    else { Debug.Log("Cannot water yet, on cooldown."); }
                }
                break;
            case PlantStage.Rose_Budding:
                if (action.actionName == currentStageData.actionToWater && currentStageData.requiresWatering)
                {
                    if (timeSinceLastWatering >= currentStageData.wateringCooldown) // *** CORRECT ACCESS: Via currentStageData ***
                    {
                        isWatered = true; timeSinceLastWatering = 0f; stageChanged = true;
                    }
                    else { Debug.Log("Cannot water yet, on cooldown."); }
                }
                break;
            case PlantStage.Rose_BuddingWatered:
                if (action.actionName == currentStageData.actionToWater)
                {
                    if (timeSinceLastWatering >= currentStageData.wateringCooldown) // *** CORRECT ACCESS: Via currentStageData ***
                    {
                        isWatered = true; timeSinceLastWatering = 0f; stageChanged = true;
                    }
                    else { Debug.Log("Cannot water yet, on cooldown."); }
                }
                break;
            case PlantStage.Rose_Ripe:
                if (action.actionName == currentStageData.actionToHarvest)
                {
                    TransitionToStage(PlantStage.Soil_Empty, false);
                    stageChanged = true;
                    Debug.Log($"Harvested {gameObject.name}!");
                }
                break;
        }

        if (stageChanged)
        {
            UpdateVisualsSprite();
            // If the stage itself changed, we need to get the new stage data.
            UpdateVisualsAndStageData(currentStage);
        }
    }

    public void TransitionToStage(PlantStage newStage, bool isAutomatic)
    {
        currentStage = newStage;
        timeInCurrentStage = 0f;
        isWatered = false;
        timeSinceLastWatering = Mathf.Infinity;
        UpdateVisualsAndStageData(currentStage);
        UpdateVisualsSprite();
        Debug.Log($"{gameObject.name} transitioned to stage: {currentStage}");
    }

    private void UpdateVisualsAndStageData(PlantStage stage)
    {
        // This is critical: it ensures currentStageData is valid before Update() reads it.
        currentStageData = plantDefinition.GetStageData(stage);
        if (currentStageData.stage != stage)
        {
            Debug.LogWarning($"PlantData '{plantDefinition?.plantName ?? "Null"}' does not contain data for stage '{stage}'.");
            // Assign a default or safe stage data if not found.
            // For robustness, you might want to have a "MissingStageData" in your enum and default data.
        }
    }

    private void UpdateVisualsSprite()
    {
        if (spriteRenderer == null || plantDefinition == null) return;

        Sprite spriteToDisplay = null;
        if (currentStageData.stage == currentStage) // Check if data was successfully loaded for this stage.
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
            // Fallback for Soil_Empty if stage data wasn't found or if it has a specific sprite.
            // You might need to add a 'soilEmptySprite' to PlantData for this.
            spriteToDisplay = plantDefinition.seedSprite; // Using seedSprite as a placeholder.
        }
        else if (currentStage == PlantStage.Soil_Seeded)
        {
            spriteToDisplay = plantDefinition.seedSprite; // Sprite for the planted seed.
        }

        if (spriteToDisplay != null) spriteRenderer.sprite = spriteToDisplay;
        else Debug.LogWarning($"No appropriate sprite found for stage {currentStage} on {gameObject.name}.");
    }

    public PlantStageData GetCurrentStageData()
    {
        return currentStageData;
    }

    public void PlantSeed(PlantData seedToPlantData)
    {
        if (currentStage == PlantStage.Soil_Empty && seedToPlantData != null)
        {
            this.plantDefinition = seedToPlantData;
            TransitionToStage(PlantStage.Soil_Seeded, false); // Transition to seeded stage
            Debug.Log($"Planted {seedToPlantData.plantName} in {gameObject.name}.");
        }
        else { Debug.LogWarning($"Cannot plant here. Current stage: {currentStage}, or seed data is missing."); }
    }
}