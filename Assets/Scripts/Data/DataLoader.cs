using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// This class provides methods to load and deserialize scenario data from JSON strings.
    /// It is used by StartupServices to populate the GameState with scenario data after loading from Addressables.
    /// </summary>
    public static class DataLoader
    {
        /// <summary>
        /// Deserializes a JSON string containing a "scenarios" array into a list of <see cref="Scenario"/> objects.
        /// </summary>
        public static List<Scenario> LoadScenarios(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("[DataLoader] scenarios JSON is null, empty, or whitespace.");
                return new List<Scenario>();
            }

            try
            {
                Debug.Log($"[DataLoader] Deserializing scenarios JSON. Length={json.Length}");

                ScenariosWrapper wrapper = JsonUtility.FromJson<ScenariosWrapper>(json);

                if (wrapper == null)
                {
                    Debug.LogError(
                        "[DataLoader] JsonUtility returned null for scenarios JSON. " +
                        $"Preview: {Preview(json)}");
                    return new List<Scenario>();
                }

                if (wrapper.scenarios == null)
                {
                    Debug.LogError(
                        "[DataLoader] scenarios wrapper deserialized, but wrapper.scenarios is null. " +
                        $"Preview: {Preview(json)}");
                    return new List<Scenario>();
                }

                List<Scenario> scenarios = new(wrapper.scenarios.Count);
                foreach (Scenario raw in wrapper.scenarios)
                {
                    scenarios.Add(raw);
                }

                Debug.Log($"[DataLoader] Parsed scenarios successfully. Count={scenarios.Count}");
                return scenarios;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[DataLoader] Exception while deserializing scenarios JSON: " + ex +
                    Environment.NewLine +
                    $"Preview: {Preview(json)}");
                return new List<Scenario>();
            }
        }

        /// <summary>
        /// Deserializes a JSON string containing a "clothingItems" array and populates <see cref="WardrobeState"/>.
        /// </summary>
        /// <param name="json">The JSON string containing the clothing items data.</param>
        /// <returns>The number of clothing items successfully loaded.</returns>
        public static List<WardrobeItem> LoadClothingItems(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.LogError("[DataLoader] clothing_items JSON is null, empty, or whitespace.");
                return new List<WardrobeItem>();
            }

            try
            {
                Debug.Log($"[DataLoader] Deserializing clothing_items JSON. Length={json.Length}");

                WardrobeItemsWrapper itemsInFile = JsonUtility.FromJson<WardrobeItemsWrapper>(json);

                if (itemsInFile == null)
                {
                    Debug.LogError(
                        "[DataLoader] JsonUtility returned null for clothing_items JSON. " +
                        $"Preview: {Preview(json)}");
                    return new List<WardrobeItem>();
                }

                if (itemsInFile.clothingItems == null)
                {
                    Debug.LogError(
                        "[DataLoader] clothing_items wrapper deserialized, but clothingItems is null. " +
                        $"Preview: {Preview(json)}");
                    return new List<WardrobeItem>();
                }

                int addedCount = itemsInFile.clothingItems.Count();
                Debug.Log($"[DataLoader] Parsed clothing items successfully. Count={addedCount}");

                return itemsInFile.clothingItems.ToList();
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    "[DataLoader] Exception while deserializing clothing_items JSON: " + ex +
                    Environment.NewLine +
                    $"Preview: {Preview(json)}");
                return new List<WardrobeItem>();
            }
        }

        private static string Preview(string json, int maxLength = 300)
        {
            if (string.IsNullOrEmpty(json)) return "<empty>";

            string sanitized = json.Replace("\r", "\\r").Replace("\n", "\\n");
            return sanitized.Length <= maxLength
                ? sanitized
                : sanitized.Substring(0, maxLength) + "...";
        }
    }
}
