using Assets.Scripts.Core;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    [RequireComponent(typeof(UIDocument))]
    public class ResultsController : MonoBehaviour
    {
        private Button _retryButton;
        private Button _mainMenuButton;
        private Button _retryScenarioButton;

        private VisualElement _root;
        private readonly List<AsyncOperationHandle<Sprite>> _spriteHandles = new();

        /// <summary>
        /// Initialize the results UI when this component becomes active.
        ///
        /// This method caches references to UI controls and registers click handlers for the retry, main menu,
        /// and new scenario buttons. It then loads avatar and clothing sprites and runs scoring for the results summary.
        /// </summary>
        /// <remarks>
        /// Called by Unity when the GameObject is enabled. Assumes a <see cref="UIDocument"/> component is 
        /// present on the same GameObject and that global state objects (ScenarioState, WardrobeState) are available.
        /// </remarks>
        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _retryButton = _root.Q<Button>("btnRetry");
            _retryButton?.RegisterCallback<ClickEvent>(GoToWardrobe);

            _mainMenuButton = _root.Q<Button>("btnMainMenu");
            _mainMenuButton?.RegisterCallback<ClickEvent>(GoToMainMenu);

            _retryScenarioButton = _root.Q<Button>("btnNewScenario");
            _retryScenarioButton?.RegisterCallback<ClickEvent>(NewScenario);

            Scenario activeScenario = ScenarioState.Instance.ActiveScenario;
            if (activeScenario != null)
            {
                UpdateBackground(activeScenario.backgroundImage);
                UpdateIdealAvatar(activeScenario.avatarImage, activeScenario.idealOutfit);
            }

            LoadAvatarImages();
            ReadScenario();
        }

        /// <summary>
        /// Tear down UI bindings and release runtime resources when the component is disabled.
        /// This unregisters click callbacks registered in <see cref="OnEnable"/> and
        /// releases any Addressables sprite handles that were loaded so they do not leak
        /// memory.
        /// </summary>
        private void OnDisable()
        {
            _retryButton?.UnregisterCallback<ClickEvent>(GoToWardrobe);

            _mainMenuButton?.UnregisterCallback<ClickEvent>(GoToMainMenu);

            _retryScenarioButton?.UnregisterCallback<ClickEvent>(NewScenario);

            foreach (AsyncOperationHandle<Sprite> handle in _spriteHandles)
            {
                Addressables.Release(handle);
            }
            _spriteHandles.Clear();
        }

        /// <summary>
        /// Navigate to the wardrobe scene so the player can try a different outfit.
        /// </summary>
        /// <param name="e">The UI click event that triggered navigation.</param>
        private void GoToWardrobe(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.WardrobeScene);
        }

        /// <summary>
        /// Navigate back to the main menu scene.
        /// </summary>
        /// <param name="e">The UI click event that triggered navigation.</param>
        private void GoToMainMenu(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.MainMenuScene);
        }

        /// <summary>
        /// Load a new scenario prompt (task) so the user can attempt a different challenge.
        /// </summary>
        /// <param name="e">The UI click event that triggered loading a new scenario.</param>
        private void NewScenario(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
        }

        /// <summary>
        /// Populate the stacked avatar and clothing image elements with sprites from Addressables.
        /// This uses <see cref="SetSlotSprite"/> for each of the avatar and equipped clothing
        /// slots. If a sprite key is missing the corresponding image will be cleared.
        /// </summary>
        private void LoadAvatarImages()
        {
            SetSlotSprite("activeAvatar", ScenarioState.Instance.ActiveScenario?.avatarImage);
            SetSlotSprite("activeJacket", WardrobeState.Instance.CurrentItemJacket?.sprite);
            SetSlotSprite("activeTop", WardrobeState.Instance.CurrentItemTop?.sprite);
            SetSlotSprite("activeBottoms", WardrobeState.Instance.CurrentItemBottom?.sprite);
            SetSlotSprite("activeShoes", WardrobeState.Instance.CurrentItemShoe?.sprite);
        }

        /// <summary>
        /// Load a Sprite asset from Addressables and assign it to a UI Image element.
        /// </summary>
        /// <param name="elementName">The UXML name of the Image element to update.</param>
        /// <param name="spriteKey">The Addressables key for the Sprite to load. If null or empty
        /// the target Image will be cleared.</param>
        /// <remarks>
        /// Performs a resource-location pre-check before requesting the asset and uses
        /// <see cref="LoadSprite"/> to perform the asynchronous load. Loaded handles are
        /// tracked for later release in <see cref="OnDisable"/>.
        /// </remarks>
        private void SetSlotSprite(string elementName, string spriteKey)
        {
            Image slotImage = _root.Q<Image>(elementName);
            if (slotImage == null)
            {
                Debug.LogError("ResultsController: Image element not found: " + elementName);
                return;
            }

            if (string.IsNullOrEmpty(spriteKey))
            {
                slotImage.sprite = null;
                return;
            }

            LoadSprite(spriteKey, loadedSprite =>
            {
                slotImage.sprite = loadedSprite;
            });
        }

        /// <summary>
        /// Asynchronously load a Sprite from Addressables after verifying there is a resource location.
        /// </summary>
        /// <param name="key">Addressables key for the Sprite asset.</param>
        /// <param name="onSuccess">Callback invoked with the loaded Sprite when the operation succeeds.</param>
        /// <remarks>
        /// This method first queries resource locations for the given key to avoid unnecessary
        /// load attempts. If successful it starts an asset load and adds the returned handle
        /// to <see cref="_spriteHandles"/> so the handle can be released later.
        /// </remarks>
        private void LoadSprite(string key, Action<Sprite> onSuccess)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locHandle =
                Addressables.LoadResourceLocationsAsync(key, typeof(Sprite));

            locHandle.Completed += locOp =>
            {
                if (locOp.Status != AsyncOperationStatus.Succeeded || locOp.Result.Count == 0)
                {
                    Debug.LogWarning("ResultsController: no Addressable location found for key: " + key);
                    Addressables.Release(locHandle);
                    return;
                }

                Addressables.Release(locHandle);

                AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(key);
                _spriteHandles.Add(handle);

                handle.Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        onSuccess(op.Result);
                    }
                    else
                    {
                        Debug.LogWarning("ResultsController: failed to load sprite with key: " + key);
                    }
                };
            };
        }



        /// <summary>
        /// Runs scoring so the results summary label reflects the latest outfit versus the active scenario.
        /// </summary>
        /// <remarks>
        /// Invokes <see cref="Scoring"/> after sprites are loaded in <see cref="OnEnable"/>.
        /// </remarks>
        public void ReadScenario()
        {
            Scoring();
        }


        /// <summary>
        /// Compute and display per-slot scores and the overall total score for the active scenario.
        /// </summary>
        /// <remarks>
        /// This method reads the currently equipped wardrobe items and the active scenario's
        /// ideal outfit, uses <see cref="ScoreItem(WardrobeItem, IdealOutfitItem, List{ScoredItem})"/>
        /// for each slot, then updates the total summary label with a count of fully correct slots (partial credit excluded).
        /// </remarks>
        public void Scoring()
        {
            Scenario activeScenario = ScenarioState.Instance.ActiveScenario;
            if (activeScenario == null)
            {
                Debug.LogWarning("ResultsController: Scoring requires scenario.");
                return;
            }

            IdealOutfit idealOutfit = activeScenario.idealOutfit;
            List<ScoredItem> scoredItems = activeScenario.scoredItems;
            

            float jacketScore = ScoreItem(WardrobeState.Instance.CurrentItemJacket, idealOutfit?.jacket, scoredItems);            
          
            float topScore = ScoreItem(WardrobeState.Instance.CurrentItemTop, idealOutfit?.top, scoredItems);
           
            float bottomScore = ScoreItem(WardrobeState.Instance.CurrentItemBottom, idealOutfit?.bottom, scoredItems);
            
            float shoesScore = ScoreItem(WardrobeState.Instance.CurrentItemShoe, idealOutfit?.shoes, scoredItems);

            Label totalScoreDisplay = _root.Q<Label>("lblTotalScore");
            if (totalScoreDisplay != null)
            {
                int correctItems = 0;
                if (Mathf.Approximately(jacketScore, 1f))
                {
                    correctItems++;
                }

                if (Mathf.Approximately(topScore, 1f))
                {
                    correctItems++;
                }

                if (Mathf.Approximately(bottomScore, 1f))
                {
                    correctItems++;
                }

                if (Mathf.Approximately(shoesScore, 1f))
                {
                    correctItems++;
                }

                totalScoreDisplay.text =
                    " Let's see... you got " + correctItems.ToString() + " out of 4 items correct.";
            }
        }

        /// <summary>
        /// Evaluate the score for a single wardrobe slot.
        /// </summary>
        /// <param name="selectedItem">The equipped <see cref="WardrobeItem"/> for the slot. May be null.</param>
        /// <param name="idealItem">The scenario's <see cref="IdealOutfitItem"/> for this slot. May be null.</param>
        /// <param name="scoredItems">Optional list of <see cref="ScoredItem"/> rows used for partial credit when the selection does not match the ideal id.</param>
        /// <returns>Floating-point score for the slot in the range [0.0, 1.0].</returns>
        private float ScoreItem(
            WardrobeItem selectedItem, IdealOutfitItem idealItem, List<ScoredItem> scoredItems)
        {
            string selectedId = selectedItem?.id;
            // Treat "nothing_*" selection items as an empty slot
            if (selectedId != null && selectedId.StartsWith("nothing_"))
                selectedId = null;

            string idealId = idealItem?.itemId;

            // Full point: both empty/null, or both match the same non-empty id
            if ((string.IsNullOrEmpty(idealId) && string.IsNullOrEmpty(selectedId)) ||
                (!string.IsNullOrEmpty(idealId) && selectedId == idealId))
            {                
                return 1f;
            }

            // Partial credit from scoredItems
            if (!string.IsNullOrEmpty(selectedId) && scoredItems != null)
            {
                foreach (ScoredItem scoredRow in scoredItems)
                {
                    if (scoredRow == null)
                    {
                        continue;
                    }

                    if (scoredRow.itemId == selectedId)
                    {                        
                        return scoredRow.score;
                    }
                }
            }

            // No match
           
            return 0f;
        }

        /// <summary>
        /// Loads the results screen background sprite from Addressables and applies it to the root background image.
        /// </summary>
        /// <param name="backgroundSpriteKey">Addressables key from the active scenario, or null to use the default dressing-room key.</param>
        private void UpdateBackground(string backgroundSpriteKey)
        {
            Image backgroundElement = _root.Q<Image>("imgBackground");
            if (backgroundElement == null)
            {
                Debug.LogError("ResultsController: imgBackground Image not found in UXML.");
                return;
            }

            string resolvedKey;
            if (string.IsNullOrEmpty(backgroundSpriteKey))
            {
                resolvedKey = "Scenarios/DressingRoom.png";
            }
            else
            {
                resolvedKey = backgroundSpriteKey;
            }

            LoadSprite(resolvedKey, loadedSprite =>
            {
                backgroundElement.style.backgroundImage = new StyleBackground(loadedSprite);
            });
        }

        /// <summary>
        /// Loads the ideal-outfit avatar preview and populates ideal slot images from wardrobe data.
        /// </summary>
        /// <param name="avatarSpriteKey">Addressables key for the scenario avatar sprite shown behind ideal clothing.</param>
        /// <param name="idealOutfit">Ideal outfit rows from the active scenario; may be null.</param>
        private void UpdateIdealAvatar(string avatarSpriteKey, IdealOutfit idealOutfit)
        {
            const string idealAvatarElementName = "idealAvatar";

            Image idealAvatarElement = _root.Q<Image>(idealAvatarElementName);
            if (idealAvatarElement == null)
            {
                Debug.LogError("ResultsController: idealAvatar Image not found in UXML.");
            }
            else if (!string.IsNullOrEmpty(avatarSpriteKey))
            {
                LoadSprite(avatarSpriteKey, loadedSprite =>
                {
                    idealAvatarElement.style.backgroundImage = new StyleBackground(loadedSprite);
                });
            }

            if (idealOutfit == null)
            {
                return;
            }

            SetSlotSprite("idealTop", GetItemById(idealOutfit.top?.itemId)?.sprite);
            SetSlotSprite("idealJacket", GetItemById(idealOutfit.jacket?.itemId)?.sprite);
            SetSlotSprite("idealBottoms", GetItemById(idealOutfit.bottom?.itemId)?.sprite);
            SetSlotSprite("idealShoes", GetItemById(idealOutfit.shoes?.itemId)?.sprite);
        }

        /// <summary>
        /// Returns the first wardrobe item matching the given clothing item id.
        /// </summary>
        /// <param name="itemId">Clothing item id from scenario JSON, or null.</param>
        /// <returns>The matching item, or null if not found or id is null.</returns>
        private WardrobeItem GetItemById(string itemId)
        {
            if (itemId == null)
            {
                return null;
            }

            foreach (WardrobeItem item in WardrobeState.Instance.AllWardrobeItems)
            {
                if (item.id == itemId)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
