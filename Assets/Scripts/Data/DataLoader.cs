using Assets.Scripts.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// This class provides methods to load and deserialize scenario data from JSON strings.
    /// It is used by StartupServices to populate the GameState with scenario data after loading from Addressables.
    /// </summary>
    internal static class DataLoader
    {
        /// <summary>
        /// Deserializes a JSON string containing a "scenarios" array into a list of <see cref="Scenario"/> objects.
        /// </summary>
        internal static List<Scenario> LoadScenarios(string json)
        {
            ScenariosWrapper wrapper = JsonUtility.FromJson<ScenariosWrapper>(json);
            if (wrapper == null || wrapper.scenarios == null)
            {
                Debug.LogError("DataLoader: failed to deserialize scenarios JSON.");
                return new List<Scenario>();
            }

            List<Scenario> scenarios = new();
            foreach (Scenario raw in wrapper.scenarios)
            {
                scenarios.Add(raw);
            }
            return scenarios;
        }

        /// <summary>
        /// Deserializes a JSON string containing a "clothingItems" array and populates <see cref="WardrobeState"/>.
        /// </summary>
        internal static void LoadClothingItems(string json)
        {
            WardrobeItemsWrapper itemsInFile = JsonUtility.FromJson<WardrobeItemsWrapper>(json);
            if (itemsInFile == null || itemsInFile.clothingItems == null)
            {
                Debug.LogError("DataLoader: JSON did not deserialize to SerialItems with a non-null clothingItems array.");
                return;
            }

            foreach (WardrobeItem newItem in itemsInFile.clothingItems)
            {
                WardrobeState.Instance.AddItem(
                    newItem.id,
                    newItem.slot,
                    newItem.name,
                    newItem.description,
                    newItem.sprite,
                    newItem.coversBottoms);
            }
        }
    }
}
