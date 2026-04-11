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
        private Scenario _currentScenario;
        private float _totalScore;
        private readonly List<AsyncOperationHandle<Sprite>> _spriteHandles = new();

        /// <summary>
        /// Caches result buttons, runs TEMP scenario fill, then binds wardrobe and scenario text to the UI.
        /// </summary>
        private void OnEnable()
        {
            _root = GetComponent<UIDocument>().rootVisualElement;

            _retryButton = _root.Q<Button>("btnRetry");
            if (_retryButton == null)
            {
                Debug.LogError("ResultsController: btnRetry not found in UXML.");
                return;
            }
            _retryButton.RegisterCallback<ClickEvent>(GoToWardrobe);

            _mainMenuButton = _root.Q<Button>("btnMainMenu");
            if (_mainMenuButton == null)
            {
                Debug.LogError("ResultsController: btnMainMenu not found in UXML.");
                return;
            }
            _mainMenuButton.RegisterCallback<ClickEvent>(GoToMainMenu);

            _retryScenarioButton = _root.Q<Button>("btnNewScenario");
            if (_retryScenarioButton == null)
            {
                Debug.LogError("ResultsController: btnNewScenario not found in UXML.");
                return;
            }
            _retryScenarioButton.RegisterCallback<ClickEvent>(NewScenario);

            _totalScore = 0f;
            _currentScenario = ScenarioState.Instance.ActiveScenario;
            if (_currentScenario == null)
            {
                Debug.LogWarning("ResultsController: There is no current scenario.");
            }

            LoadAvatarImages();
            ReadWardrobeItems();
            ReadScenario();
        }

        /// <summary>
        /// Unbinds result screen button handlers.
        /// </summary>
        private void OnDisable()
        {
            if (_retryButton != null)
            {
                _retryButton.UnregisterCallback<ClickEvent>(GoToWardrobe);
            }

            if (_mainMenuButton != null)
            {
                _mainMenuButton.UnregisterCallback<ClickEvent>(GoToMainMenu);
            }

            if (_retryScenarioButton != null)
            {
                _retryScenarioButton.UnregisterCallback<ClickEvent>(NewScenario);
            }

            foreach (AsyncOperationHandle<Sprite> handle in _spriteHandles)
            {
                Addressables.Release(handle);
            }
            _spriteHandles.Clear();
        }

        /// <summary>
        /// Loads the wardrobe scene for another outfit attempt.
        /// </summary>
        private void GoToWardrobe(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.WardrobeScene);
        }

        /// <summary>
        /// Returns to the main menu scene.
        /// </summary>
        private void GoToMainMenu(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.MainMenuScene);
        }

        /// <summary>
        /// Loads another scenario prompt.
        /// </summary>
        private void NewScenario(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
        }

        /// <summary>
        /// Loads avatar and clothing sprites from Addressables into the stacked result images.
        /// </summary>
        private void LoadAvatarImages()
        {
            SetSlotSprite("activeAvatar", _currentScenario?.avatarImage);
            SetSlotSprite("activeJacket", WardrobeState.Instance.CurrentItemJacket?.sprite);
            SetSlotSprite("activeTop", WardrobeState.Instance.CurrentItemTop?.sprite);
            SetSlotSprite("activeBottoms", WardrobeState.Instance.CurrentItemBottom?.sprite);
            SetSlotSprite("activeShoes", WardrobeState.Instance.CurrentItemShoe?.sprite);
        }

        /// <summary>
        /// Loads a sprite from Addressables by key and sets it on the named Image element.
        /// Clears the sprite if the key is null or empty.
        /// </summary>
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
        /// Loads a sprite from Addressables with a location pre-check, tracks the handle for cleanup.
        /// </summary>
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
        /// Sets label text for each equipped wardrobe slot.
        /// </summary>
        public void ReadWardrobeItems()
        {
            ReadItem("lblYourJacketName", "Jacket", WardrobeState.Instance.CurrentItemJacket);
            ReadItem("lblYourTopName", "Top", WardrobeState.Instance.CurrentItemTop);
            ReadItem("lblYourBottomsName", "Bottoms", WardrobeState.Instance.CurrentItemBottom);
            ReadItem("lblYourShoesName", "Shoes", WardrobeState.Instance.CurrentItemShoe);
        }

        /// <summary>
        /// Sets one outfit line label from a wardrobe item.
        /// </summary>
        /// <param name="slotLabel">UXML name of the label.</param>
        /// <param name="slotStr">Human-readable slot name for copy.</param>
        /// <param name="itemSlot">Equipped item, if any.</param>
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
        /// Runs scoring and ideal-hint UI pass for the current scenario.
        /// </summary>
        public void ReadScenario()
        {
            Scoring();
            IdealItemsHint();
        }

        /// <summary>
        /// Fills ideal-item hint labels from the scenario.
        /// </summary>
        public void IdealItemsHint()
        {
            if (_currentScenario == null || _currentScenario.idealOutfit == null)
            {
                Debug.LogError("ResultsController: IdealItemsHint missing scenario or idealOutfit.");
                return;
            }

            IdealOutfit outfit = _currentScenario.idealOutfit;
            IdealItemHint("lblHintIdealJacket", outfit.jacket);
            IdealItemHint("lblHintIdealTop", outfit.top);
            IdealItemHint("lblHintIdealBottoms", outfit.bottoms);
            IdealItemHint("lblHintIdealShoes", outfit.shoes);
        }

        /// <summary>
        /// Sets hint text for one ideal slot when data exists.
        /// </summary>
        /// <param name="label">UXML label name.</param>
        /// <param name="ideal">Ideal row, may be null.</param>
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
                hintLabel.text = "Hint: ";
                return;
            }

            hintLabel.text = "Hint: " + ideal.commentary;
        }

        /// <summary>
        /// Sets feedback label text for a slot.
        /// </summary>
        public void FeedBackItem(string label, string commentary)
        {
            Label feedbackLabel = _root.Q<Label>(label);
            if (feedbackLabel == null)
            {
                Debug.LogError("ResultsController: Could not find label for FeedBackItem.");
                return;
            }

            if (commentary == null)
            {
                commentary = string.Empty;
            }

            feedbackLabel.text = commentary;
        }

        /// <summary>
        /// Scores each slot against scenario rows and shows total.
        /// </summary>
        public void Scoring()
        {
            if (_currentScenario == null)
            {
                Debug.LogError("ResultsController: Scoring requires scenario.");
                return;
            }

            IncorrectSlotFeedback fb = _currentScenario.incorrectSlotFeedback;
            ScoreItem("lblJacketScore", WardrobeState.Instance.CurrentItemJacket, "lblFeedbackJacket", fb.jacket);
            ScoreItem("lblTopScore", WardrobeState.Instance.CurrentItemTop, "lblFeedbackTop", fb.top);
            ScoreItem("lblBottomsScore", WardrobeState.Instance.CurrentItemBottom, "lblFeedbackBottoms", fb.bottoms);
            ScoreItem("lblShoesScore", WardrobeState.Instance.CurrentItemShoe, "lblFeedbackShoes", fb.shoes);

            Label totalScoreDisplay = _root.Q<Label>("lblTotalScore");
            if (totalScoreDisplay == null)
            {
                Debug.LogError("ResultsController: Could not find lblTotalScore. Current score is: " + _totalScore.ToString());
                return;
            }

            totalScoreDisplay.text = _totalScore.ToString("F1") + "/4.0";
        }

        /// <summary>
        /// Matches one equipped item to scored rows; otherwise shows slot fallback feedback.
        /// </summary>
        private void ScoreItem(string label, WardrobeItem item, string commentaryLabel, string slotFeedback)
        {
            Label scoreDisplay = _root.Q<Label>(label);
            if (scoreDisplay == null)
            {
                string slotTagForLog = item != null ? item.slot : "unknown";
                Debug.LogError("ResultsController: Could not find scoreDisplay label for ScoreItem: " + slotTagForLog);
                return;
            }

            if (item == null)
            {
                scoreDisplay.text = "0.0";
                FeedBackItem(commentaryLabel, slotFeedback);
                return;
            }

            if (_currentScenario == null || _currentScenario.scoredItems == null)
            {
                Debug.LogError("ResultsController: ScoreItem requires ScoredItems list.");
                scoreDisplay.text = "0.0";
                FeedBackItem(commentaryLabel, slotFeedback);
                return;
            }

            bool anyFeedback = false;
            foreach (ScoredItem scoredRow in _currentScenario.scoredItems)
            {
                if (scoredRow == null)
                {
                    Debug.LogError("ResultsController: Null entry in ScoredItems for slot " + item.slot);
                    continue;
                }

                if (scoredRow.itemId == item.id)
                {
                    _totalScore += scoredRow.score;
                    scoreDisplay.text = scoredRow.score.ToString("F1");
                    anyFeedback = true;
                    FeedBackItem(commentaryLabel, scoredRow.commentary);
                }
            }

            if (!anyFeedback)
            {
                FeedBackItem(commentaryLabel, slotFeedback);
            }
        }
    }
}
