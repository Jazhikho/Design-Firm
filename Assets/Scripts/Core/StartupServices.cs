using Assets.Scripts.Data;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Core
{
    internal class StartupServices
    {
        public static bool ScenariosLoaded { get; private set; }
        public static bool WardrobeLoaded { get; private set; }

        public static bool ScenariosFailed { get; private set; }
        public static bool WardrobeFailed { get; private set; }

        public static bool HasCriticalFailure => ScenariosFailed || WardrobeFailed;
        public static bool IsFullyReady => ScenariosLoaded && WardrobeLoaded && !HasCriticalFailure;

        public static string LastCriticalError { get; private set; } = string.Empty;

        /// <summary>
        /// Registers a method to load scenario and clothing item data from Addressables before the first scene loads.
        /// This ensures that all necessary data is available in the GameState before any gameplay begins.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            ResetStartupState();

            Debug.Log("[StartupServices] Beginning startup data load.");

            Debug.Log("[StartupServices] Requesting Addressables key: scenarios");
            AsyncOperationHandle<TextAsset> scenariosHandle =
                Addressables.LoadAssetAsync<TextAsset>("scenarios");
            scenariosHandle.Completed += OnScenariosLoaded;

            Debug.Log("[StartupServices] Requesting Addressables key: clothing_items");
            AsyncOperationHandle<TextAsset> clothingHandle =
                Addressables.LoadAssetAsync<TextAsset>("clothing_items");
            clothingHandle.Completed += OnClothingItemsLoaded;
        }
        private static void ResetStartupState()
        {
            ScenariosLoaded = false;
            WardrobeLoaded = false;
            ScenariosFailed = false;
            WardrobeFailed = false;
            LastCriticalError = string.Empty;
        }

        /// <summary>
        /// Handles the completion of the asynchronous operation to load the scenarios asset and updates the scenario
        /// state accordingly.
        /// </summary>
        /// <remarks>If the operation fails, an error is logged and no state is updated. On success, the
        /// loaded scenarios are parsed and set in the scenario state, and listeners are notified. The handle is
        /// released after processing.</remarks>
        /// <param name="handle">The handle representing the asynchronous operation for loading the scenarios asset. Must have a status of
        /// Succeeded and contain a valid TextAsset result.</param>
        private static void OnScenariosLoaded(AsyncOperationHandle<TextAsset> handle)
        {
            try
            {
                Debug.Log(
                    $"[StartupServices] scenarios load completed. " +
                    $"Status={handle.Status}, IsDone={handle.IsDone}, " +
                    $"PercentComplete={handle.PercentComplete:0.00}");

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    string details = BuildAddressablesFailureMessage("scenarios", handle);
                    FailScenarios(details);
                    return;
                }

                Debug.Log(
                    $"[StartupServices] scenarios loaded successfully. " +
                    $"Text length={handle.Result.text?.Length ?? 0}");

                ScenarioState.Instance.Scenarios = DataLoader.LoadScenarios(handle.Result.text);

                int scenarioCount = ScenarioState.Instance.Scenarios?.Count ?? 0;
                Debug.Log($"[StartupServices] Parsed scenarios count={scenarioCount}");

                if (scenarioCount == 0)
                {
                    FailScenarios(
                        "[StartupServices] scenarios loaded from Addressables, " +
                        "but zero scenarios were parsed. This is treated as a startup failure.");
                    return;
                }

                ScenariosLoaded = true;
                ScenarioState.Instance.NotifyScenariosLoaded();
                Debug.Log("[StartupServices] ScenarioState updated and listeners notified.");
            }
            catch (Exception ex)
            {
                FailScenarios(
                    "[StartupServices] Exception while processing scenarios: " + ex);
            }
            finally
            {
                SafeRelease(handle, "scenarios");
                LogOverallStartupState();
            }
        }

        /// <summary>
        /// Handles the completion of the asynchronous operation to load clothing item data from an addressable asset.
        /// </summary>
        /// <remarks>If the operation fails, an error is logged and no further processing occurs. On
        /// success, the clothing item data is loaded, the asset is released, and subscribers are notified that wardrobe
        /// items are available.</remarks>
        /// <param name="handle">The handle representing the asynchronous operation for loading the clothing items asset. Must contain a
        /// valid and successfully loaded TextAsset.</param>
        private static void OnClothingItemsLoaded(AsyncOperationHandle<TextAsset> handle)
        {
            try
            {
                Debug.Log(
                    $"[StartupServices] clothing_items load completed. " +
                    $"Status={handle.Status}, IsDone={handle.IsDone}, " +
                    $"PercentComplete={handle.PercentComplete:0.00}");

                if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                {
                    string details = BuildAddressablesFailureMessage("clothing_items", handle);
                    FailWardrobe(details);
                    return;
                }

                Debug.Log(
                    $"[StartupServices] clothing_items loaded successfully. " +
                    $"Text length={handle.Result.text?.Length ?? 0}");

                int addedCount = DataLoader.LoadClothingItems(handle.Result.text);

                Debug.Log($"[StartupServices] Parsed clothing item count={addedCount}");

                if (addedCount == 0)
                {
                    FailWardrobe(
                        "[StartupServices] clothing_items loaded from Addressables, " +
                        "but zero wardrobe items were parsed. This is treated as a startup failure.");
                    return;
                }

                WardrobeLoaded = true;
                WardrobeState.Instance.NotifyWardrobeItemsLoaded();
                Debug.Log("[StartupServices] WardrobeState updated and listeners notified.");
            }
            catch (Exception ex)
            {
                FailWardrobe(
                    "[StartupServices] Exception while processing clothing_items: " + ex);
            }
            finally
            {
                SafeRelease(handle, "clothing_items");
                LogOverallStartupState();
            }
        }

        private static string BuildAddressablesFailureMessage(
            string key,
            AsyncOperationHandle<TextAsset> handle)
        {
            string exceptionText = handle.OperationException != null
                ? handle.OperationException.ToString()
                : "<null>";

            return
                $"[StartupServices] Failed to load Addressables key '{key}'. " +
                $"Status={handle.Status}, IsDone={handle.IsDone}, " +
                $"PercentComplete={handle.PercentComplete:0.00}, " +
                $"OperationException={exceptionText}. " +
                $"Ensure the asset exists, is marked Addressable, and is reachable in the current catalog.";
        }

        private static void FailScenarios(string message)
        {
            ScenariosFailed = true;
            LastCriticalError = message;
            Debug.LogError(message);
        }

        private static void FailWardrobe(string message)
        {
            WardrobeFailed = true;
            LastCriticalError = message;
            Debug.LogError(message);
        }

        private static void SafeRelease(AsyncOperationHandle<TextAsset> handle, string key)
        {
            try
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                    Debug.Log($"[StartupServices] Released Addressables handle for '{key}'.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(
                    $"[StartupServices] Failed to release handle for '{key}'. Exception: {ex}");
            }
        }

        private static void LogOverallStartupState()
        {
            Debug.Log(
                $"[StartupServices] State => " +
                $"ScenariosLoaded={ScenariosLoaded}, WardrobeLoaded={WardrobeLoaded}, " +
                $"ScenariosFailed={ScenariosFailed}, WardrobeFailed={WardrobeFailed}, " +
                $"IsFullyReady={IsFullyReady}");

            if (HasCriticalFailure)
            {
                Debug.LogError(
                    $"[StartupServices] Critical startup failure detected. " +
                    $"LastCriticalError={LastCriticalError}");
            }
        }
    }
}
