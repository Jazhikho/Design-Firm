using Assets.Scripts.Core;
using Assets.Scripts.Core.Scoring;
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
        /// This method caches references to UI controls and registers click handlers for the retry, main menu 
        /// and new scenario buttons. It then loads avatar and clothing sprites, populates the equipped item 
        /// labels, and runs the scenario result pass (scoring + ideal-item hints).
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

            LoadAvatarImages();
            ReadWardrobeItems();
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
        /// Populate the UI labels that list the player's currently equipped wardrobe items.
        /// </summary>
        /// <remarks>
        /// Each label is updated via <see cref="ReadItem"/>. Missing UI elements or
        /// missing equipped items are logged as errors.
        /// </remarks>
        public void ReadWardrobeItems()
        {
            ReadItem("lblYourJacketName", "Jacket", WardrobeState.Instance.CurrentItemJacket);
            ReadItem("lblYourTopName", "Top", WardrobeState.Instance.CurrentItemTop);
            ReadItem("lblYourBottomsName", "Bottoms", WardrobeState.Instance.CurrentItemBottom);
            ReadItem("lblYourShoesName", "Shoes", WardrobeState.Instance.CurrentItemShoe);
        }

        /// <summary>
        /// Update a single UI label with the provided wardrobe item information.
        /// </summary>
        /// <param name="slotLabel">UXML name of the label element to update.</param>
        /// <param name="slotStr">Human-readable slot name used in the displayed text (e.g. "Jacket").</param>
        /// <param name="itemSlot">The equipped <see cref="WardrobeItem"/> for the slot. If null
        /// the method logs an error and does not update the label.</param>
        public void ReadItem(string slotLabel, string slotStr, WardrobeItem itemSlot)
        {
            Label itemDisplay = _root.Q<Label>(slotLabel);
            if (itemDisplay == null)
            {
                Debug.LogError("ResultsController: Could not find itemDisplay for ReadItem " + slotStr);
                return;
            }

            if (itemSlot == null)
            {
                Debug.LogError("ResultsController: Could not find the item for ReadItem " + slotStr);
                return;
            }

            itemDisplay.text = "Your " + slotStr + ": " + itemSlot.name;
        }

        /// <summary>
        /// Execute the scenario result pass: compute scores and populate ideal-item hints.
        /// </summary>
        /// <remarks>
        /// This is a convenience wrapper that invokes <see cref="Scoring"/> followed by
        /// <see cref="IdealItemsHint"/> so both score labels and hint labels are updated.
        /// </remarks>
        public void ReadScenario()
        {
            Scoring();
            IdealItemsHint();
        }

        /// <summary>
        /// Populate the ideal-item hint labels using the active scenario's <see cref="IdealOutfit"/>.
        /// </summary>
        /// <remarks>
        /// If there is no active scenario or the scenario does not define an ideal outfit,
        /// a diagnostic error is logged and no UI is updated.
        /// </remarks>
        public void IdealItemsHint()
        {
            if (ScenarioState.Instance.ActiveScenario == null || ScenarioState.Instance.ActiveScenario.idealOutfit == null)
            {
                Debug.LogError("ResultsController: IdealItemsHint missing scenario or idealOutfit.");
                return;
            }

            IdealOutfit outfit = ScenarioState.Instance.ActiveScenario.idealOutfit;
            IdealItemHint("lblHintIdealJacket", outfit.jacket);
            IdealItemHint("lblHintIdealTop", outfit.top);
            IdealItemHint("lblHintIdealBottoms", outfit.bottoms);
            IdealItemHint("lblHintIdealShoes", outfit.shoes);
        }

        /// <summary>
        /// Set the hint text for a single ideal-item label.
        /// </summary>
        /// <param name="label">UXML name of the label element to update.</param>
        /// <param name="ideal">The <see cref="IdealOutfitItem"/> that supplies the hint text.
        /// If null or if <see cref="IdealOutfitItem.commentary"/> is empty the label is set to
        /// a "no hints" fallback string.</param>
        public void IdealItemHint(string label, IdealOutfitItem ideal)
        {
            Label hintLabel = _root.Q<Label>(label);
            if (hintLabel == null)
            {
                Debug.LogError("ResultsController: Could not find label for IdealItemHint.");
                return;
            }

            if (ideal == null || string.IsNullOrEmpty(ideal.commentary))
            {
                hintLabel.text = "Hint: no hints";
                return;
            }

            hintLabel.text = "Hint: " + ideal.commentary;
        }

        /// <summary>
        /// Update a feedback label with the given commentary text.
        /// </summary>
        /// <param name="label">UXML name of the label element to update.</param>
        /// <param name="commentary">The text to display. If null the method treats it as an empty string.</param>
        public void FeedBackItem(string label, string commentary)
        {
            Label feedbackLabel = _root.Q<Label>(label);
            if (feedbackLabel == null) return;

            if (commentary == null)
            {
                commentary = string.Empty;
            }

            feedbackLabel.text = commentary;
        }

        /// <summary>
        /// Compute and display per-slot scores and the overall total score for the active scenario.
        /// </summary>
        /// <remarks>
        /// This method reads the currently equipped wardrobe items and the active scenario's
        /// ideal outfit, uses <see cref="ScoreItem(WardrobeItem, IdealOutfitItem, List{ScoredItem}, string, string)"/>
        /// to compute each slot's score, updates the UI labels for each slot, and computes
        /// the displayed total.
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
            IncorrectSlotFeedback fb = activeScenario.incorrectSlotFeedback;

            float jacketScore = ScoreItem(WardrobeState.Instance.CurrentItemJacket, idealOutfit?.jacket, scoredItems, "lblFeedbackJacket", fb.jacket);
            Label lblJacketScore = _root.Q<Label>("lblJacketScore");
            if (lblJacketScore != null)
                lblJacketScore.text = jacketScore.ToString("F1");

            float topScore = ScoreItem(WardrobeState.Instance.CurrentItemTop, idealOutfit?.top, scoredItems, "lblFeedbackTop", fb.top);
            Label lblTopScore = _root.Q<Label>("lblTopScore");
            if (lblTopScore != null)
                lblTopScore.text = topScore.ToString("F1");

            float bottomScore = ScoreItem(WardrobeState.Instance.CurrentItemBottom, idealOutfit?.bottoms, scoredItems, "lblFeedbackBottoms", fb.bottoms);
            Label lblBottomsScore = _root.Q<Label>("lblBottomsScore");
            if (lblBottomsScore != null)
                lblBottomsScore.text = bottomScore.ToString("F1");

            float shoesScore = ScoreItem(WardrobeState.Instance.CurrentItemShoe, idealOutfit?.shoes, scoredItems, "lblFeedbackShoes", fb.shoes);
            Label lblShoesScore = _root.Q<Label>("lblShoesScore");
            if (lblShoesScore != null)
                lblShoesScore.text = shoesScore.ToString("F1");

            Label totalScoreDisplay = _root.Q<Label>("lblTotalScore");
            if (totalScoreDisplay != null)
            {
                float totalScore = jacketScore + topScore + bottomScore + shoesScore;
                totalScoreDisplay.text = totalScore.ToString("F1") + "/4.0";
            }
        }

        /// <summary>
        /// Evaluate the score for a single wardrobe slot.
        /// </summary>
        /// <param name="selectedItem">The equipped <see cref="WardrobeItem"/> for the slot. May be null.</param>
        /// <param name="idealItem">The scenario's <see cref="IdealOutfitItem"/> for this slot. May be null.</param>
        /// <param name="scoredItems">A list of <see cref="ScoredItem"/> entries used to provide partial credit.</param>
        /// <param name="commentaryLabel">UXML name of the label element where feedback/commentary should be written.</param>
        /// <param name="slotFeedback">Fallback feedback string to use when no match is found.</param>
        /// <returns>
        /// A floating point score for the slot in the range [0.0, 1.0]. The method also updates the feedback label with the 
        /// appropriate commentary for full, partial, or no credit cases.
        /// </returns>
        private float ScoreItem(
            WardrobeItem selectedItem, IdealOutfitItem idealItem, List<ScoredItem> scoredItems, string commentaryLabel, string slotFeedback)
        {
            var result = WardrobeScoringService.ScoreItem(
                selectedItem?.id,
                idealItem?.itemId,
                idealItem?.commentary,
                scoredItems,
                slotFeedback);

            FeedBackItem(commentaryLabel, result.Commentary);
            return result.Score;
        }
    }
}
