using Assets.Scripts.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Assets.Scripts.Core
{
    internal class StartupServices
    {
        /// <summary>
        /// Registers a method to load scenario and clothing item data from Addressables before the first scene loads.
        /// This ensures that all necessary data is available in the GameState before any gameplay begins.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            AsyncOperationHandle<TextAsset> scenariosHandle = Addressables.LoadAssetAsync<TextAsset>("scenarios");
            scenariosHandle.Completed += OnScenariosLoaded;

            AsyncOperationHandle<TextAsset> clothingHandle = Addressables.LoadAssetAsync<TextAsset>("clothing_items");
            clothingHandle.Completed += OnClothingItemsLoaded;
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
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("StartupServices: failed to load scenarios.json from Addressables. " +
                    "Ensure the asset is marked as Addressable with the key \"scenarios\".");
                return;
            }

            ScenarioState.Instance.Scenarios = DataLoader.LoadScenarios(handle.Result.text);
            Addressables.Release(handle);
            ScenarioState.Instance.NotifyScenariosLoaded();
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
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("StartupServices: failed to load clothing_items.json from Addressables. " +
                    "Ensure the asset is marked as Addressable with the key \"clothing_items\".");
                return;
            }

            DataLoader.LoadClothingItems(handle.Result.text);
            Addressables.Release(handle);
            WardrobeState.Instance.NotifyWardrobeItemsLoaded();
        }
    }
}
