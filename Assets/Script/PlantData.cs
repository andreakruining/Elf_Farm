using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Ensure this is present for ?? operator and Find

// Enum for plant stages.
public enum PlantStage
{
    // Basic states
    Grass,
    Soil_Empty,
    Soil_Seeded,

    // Rose specific example stages
    Rose_Sprout,
    Rose_SproutWatered,
    Rose_Budding,
    Rose_BuddingWatered,
    Rose_Ripe,
}

// Defines the data for a single stage of a plant.
[System.Serializable]
public struct PlantStageData
{
    public PlantStage stage;
    public Sprite stageSprite;
    public Sprite wateredSprite;
    public bool requiresWatering;
    public float stageDuration;
    public PlantStage nextStageAuto;
    public PlantStage nextStageOnWater;
    public PlantStage nextStageOnPlayerAction;

    // Action names that trigger transitions from this stage.
    public string actionToPlantSeed;
    public string actionToWater;
    public string actionToHarvest;
    public string actionToTillSoil;

    // *** ADDED waterCooldown HERE, as suggested by Visual Studio ***
    // This is where it belongs, so InteractiveTile can access it.
    public float wateringCooldown;
}

// ScriptableObject defining a single plant's data.
[CreateAssetMenu(fileName = "NewPlantData", menuName = "YourGame/PlantData")]
public class PlantData : ScriptableObject
{
    public string plantName = "Unnamed Plant";
    public Sprite seedSprite;

    public List<PlantStageData> stages;

    public PlantStageData GetStageData(PlantStage stage)
    {
        // --- The ?? operator with Find ---
        // The error here is likely because 'stages' might be empty, or Find is called
        // when the list isn't properly initialized, or the stage isn't in the list.
        // 'stages.Find(match: gs => gs.stage == stage)' is the correct syntax for the lambda.
        // The 'match:' keyword for lambda parameters is optional but can improve readability.
        // The '?? defaultData' is a null-coalescing operator. It works if Find returns null.

        // Let's make sure 'Find' can return null if the stage isn't found.
        // And ensure 'defaultData' is a valid PlantStageData struct.

        PlantStageData foundStage = stages.Find(gs => gs.stage == stage); // Removed 'match:' for broader compatibility.

        if (foundStage.stage != stage) // Check if Find returned a valid stage or the default (if stage was not found)
        {
            // If the found stage's 'stage' field isn't the one we looked for, it means Find returned
            // the default struct (or the first element if list was empty), not a valid match.
            // Log a warning but return a default to prevent further errors.

            Debug.LogWarning($"PlantData '{this.name}' does not contain data for stage '{stage}'. Returning default stage data.");

            // Return a default, correctly initialized PlantStageData.
            return new PlantStageData()
            {
                stage = stage, // Set the stage to what was requested, so it's identifiable.
                stageSprite = null, // Default to null
                wateredSprite = null,
                requiresWatering = false,
                stageDuration = 0f,
                nextStageAuto = stage, // Stay in this stage.
                nextStageOnWater = stage,
                nextStageOnPlayerAction = stage,
                actionToPlantSeed = "",
                actionToWater = "",
                actionToHarvest = "",
                actionToTillSoil = "",
                wateringCooldown = 0f // Default cooldown
            };
        }

        return foundStage; // Return the found stage data if it was a valid match.
    }
}