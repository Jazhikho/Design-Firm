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
    /// <summary>
    /// Binds wardrobe UI Toolkit grids and stacked avatar images to <see cref="WardrobeState"/>.
    /// Tile buttons set state via <see cref="WardrobeState.SetItemSlot"/>; avatar images react
    /// to change events raised by WardrobeState.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WardrobeController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private Button _nextSceneButton;
        private Button _backButton;
        private Label _timerLabel;
        private Image _avatarImage;
        private Image _jacketImage;
        private Image _topImage;
        private Image _bottomsImage;
        private Image _shoesImage;

        private string _nextScene;

        /// <summary>
        /// When true, submit returns to main menu instead of result scene.
        /// </summary>
        [SerializeField]
        private bool _sandboxMode;

        /// <summary>
        /// Remaining wardrobe selection time in seconds.
        /// </summary>
        [SerializeField]
        private float _wardrobeTimer = 60f;

        private readonly Dictionary<string, Button> _selectedButtonsByGrid = new();
        private readonly List<AsyncOperationHandle<Sprite>> _spriteHandles = new();
        private readonly List<(Button button, TileButtonData data)> _tileCallbacks = new();

        /// <summary>
        /// Chooses next-scene routing, caches UI references, and binds callbacks.
        /// </summary>
        private void OnEnable()
        {
            if (WardrobeState.Instance.AllWardrobeItems[0] == null)
            {
                Debug.LogError("WardrobeController: There are no items.");
            }

            WardrobeState.Instance.CurrentItemTop = WardrobeState.Instance.AvailableTops[0];
            WardrobeState.Instance.CurrentItemJacket = WardrobeState.Instance.AvailableJackets[0];
            WardrobeState.Instance.CurrentItemBottom = WardrobeState.Instance.AvailableBottoms[0];
            WardrobeState.Instance.CurrentItemShoe = WardrobeState.Instance.AvailableShoes[0];

            _nextScene = _sandboxMode ? GameConstants.MainMenuScene : GameConstants.TaskResultScene;

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("WardrobeController: UIDocument is missing on this GameObject.");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;

            _backButton = root.Q<Button>("btnBack");
            if (_backButton != null)
            {
                _backButton.RegisterCallback<ClickEvent>(GoToScenarios);
            }

            _nextSceneButton = root.Q<Button>("nextSceneButton");
            if (_nextSceneButton == null)
            {
                Debug.LogError("WardrobeController: nextSceneButton not found in UXML.");
                return;
            }
            _nextSceneButton.RegisterCallback<ClickEvent>(NextSceneScript);

            _timerLabel = root.Q<Label>("lblTimer");
            if (_timerLabel == null)
            {
                Debug.LogError("WardrobeController: lblTimer not found in UXML.");
                return;
            }
            if (_sandboxMode)
            {
                _timerLabel.visible = false;
            }

            _avatarImage = root.Q<Image>("activeAvatar");
            _jacketImage = root.Q<Image>("activeJacket");
            _topImage = root.Q<Image>("activeTop");
            _bottomsImage = root.Q<Image>("activeBottoms");
            _shoesImage = root.Q<Image>("activeShoes");

            SubscribeToSlotChanges();

            if (WardrobeState.Instance.IsWardrobeItemsLoaded)
            {
                RefreshUI();
            }
            else
            {
                WardrobeState.Instance.WardrobeItemsLoaded += OnWardrobeItemsLoaded;
            }

            ApplyScenarioDescriptionLabel(root);
        }

        /// <summary>
        /// Updates the countdown label every frame.
        /// </summary>
        private void Update()
        {
            if (_sandboxMode == false)
            {
                _wardrobeTimer -= Time.deltaTime;
                if (_wardrobeTimer < 0f)
                {
                    _wardrobeTimer = 0f;

                    NextSceneScript(null);

                }
            }
            if (_timerLabel != null)
            {
                _timerLabel.text = Mathf.CeilToInt(_wardrobeTimer).ToString();
            }
        }

        /// <summary>
        /// Unbinds callbacks, unsubscribes from state events, and releases loaded sprite handles.
        /// </summary>
        private void OnDisable()
        {
            if (_backButton != null)
            {
                _backButton.UnregisterCallback<ClickEvent>(GoToScenarios);
            }

            if (_nextSceneButton != null)
            {
                _nextSceneButton.UnregisterCallback<ClickEvent>(NextSceneScript);
            }

            foreach ((Button button, TileButtonData data) in _tileCallbacks)
            {
                button.UnregisterCallback<ClickEvent, TileButtonData>(WardrobeItemClicked);
                button.UnregisterCallback<PointerEnterEvent, TileButtonData>(WardrobeTilePointerEntered);
            }
            _tileCallbacks.Clear();

            UnsubscribeFromSlotChanges();
            WardrobeState.Instance.WardrobeItemsLoaded -= OnWardrobeItemsLoaded;

            foreach (AsyncOperationHandle<Sprite> handle in _spriteHandles)
            {
                Addressables.Release(handle);
            }
            _spriteHandles.Clear();
        }

        #region State event subscriptions

        private void SubscribeToSlotChanges()
        {
            WardrobeState.Instance.CurrentTopChanged += OnTopChanged;
            WardrobeState.Instance.CurrentBottomChanged += OnBottomChanged;
            WardrobeState.Instance.CurrentShoeChanged += OnShoeChanged;
            WardrobeState.Instance.CurrentJacketChanged += OnJacketChanged;
        }

        private void UnsubscribeFromSlotChanges()
        {
            WardrobeState.Instance.CurrentTopChanged -= OnTopChanged;
            WardrobeState.Instance.CurrentBottomChanged -= OnBottomChanged;
            WardrobeState.Instance.CurrentShoeChanged -= OnShoeChanged;
            WardrobeState.Instance.CurrentJacketChanged -= OnJacketChanged;
        }

        private void OnTopChanged(WardrobeItem item) => UpdateSlotSprite(_topImage, item?.sprite);
        private void OnBottomChanged(WardrobeItem item) => UpdateSlotSprite(_bottomsImage, item?.sprite);
        private void OnShoeChanged(WardrobeItem item) => UpdateSlotSprite(_shoesImage, item?.sprite);
        private void OnJacketChanged(WardrobeItem item) => UpdateSlotSprite(_jacketImage, item?.sprite);

        #endregion

        /// <summary>
        /// Returns from wardrobe to scenario selection.
        /// </summary>
        private void GoToScenarios(ClickEvent e)
        {
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
        }

        private void RefreshUI()
        {
            string avatarKey = ScenarioState.Instance.ActiveScenario?.avatarImage;
            if (string.IsNullOrEmpty(avatarKey))
            {
                Debug.LogWarning("WardrobeController: No avatarImage key found. ActiveScenario may not be set.");
            }
            UpdateSlotSprite(_avatarImage, avatarKey);

            SetUpClothing("topsGrid", WardrobeState.Instance.AvailableTops);
            SetUpClothing("bottomsGrid", WardrobeState.Instance.AvailableBottoms);
            SetUpClothing("shoesGrid", WardrobeState.Instance.AvailableShoes);
            SetUpClothing("jacketsGrid", WardrobeState.Instance.AvailableJackets);

            SyncInitialSelectionVisuals();
        }

        private void OnWardrobeItemsLoaded()
        {
            RefreshUI();
        }

        /// <summary>
        /// Populates a grid with one button per clothing item and wires selection.
        /// </summary>
        /// <param name="gridName">UXML name of the container VisualElement.</param>
        /// <param name="slotList">Items to display.</param>
        private void SetUpClothing(string gridName, List<WardrobeItem> slotList)
        {
            if (slotList == null)
            {
                Debug.LogError($"slotList is null for grid {gridName}");
                return;
            }

            VisualElement grid = _uiDocument.rootVisualElement.Q<VisualElement>(gridName);
            if (grid == null)
            {
                Debug.LogError($"WardrobeController: Could not find grid: {gridName}");
                return;
            }

            grid.Clear();

            foreach (WardrobeItem item in slotList)
            {
                Button tileButton = CreateClothingButton(item, gridName);
                grid.Add(tileButton);
            }
        }

        /// <summary>
        /// Loads a sprite from Addressables by key and sets it as the background of the given UI Image.
        /// Clears the background if the key is null or empty.
        /// </summary>
        /// <param name="slotImage">The cached Image element to update.</param>
        /// <param name="spriteKey">Addressable key for the sprite asset, or null to clear.</param>
        private void UpdateSlotSprite(Image slotImage, string spriteKey)
        {
            if (slotImage == null)
            {
                Debug.LogError("WardrobeController: slot Image reference is null.");
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
            },
            () =>
            {
                slotImage.sprite = null;
            });
        }

        /// <summary>
        /// Loads the configured next scene when the submit button is used.
        /// </summary>
        public void NextSceneScript(ClickEvent evt)
        {
            if (string.IsNullOrEmpty(_nextScene))
            {
                Debug.LogError("WardrobeController: nextScene is not set; check sandbox mode and Build Settings.");
                return;
            }

            SceneManager.LoadScene(_nextScene);
        }

        /// <summary>
        /// Builds one clickable clothing tile. The click handler sets the appropriate
        /// CurrentItem property on WardrobeState based on the item's slot; the avatar
        /// image updates reactively through the slot-change event.
        /// </summary>
        private Button CreateClothingButton(WardrobeItem item, string gridName)
        {
            Button button = new();
            button.AddToClassList("wardrobe-tile");
            button.userData = item;
            button.text = string.Empty;

            button.name = item.id + "_button";

            if (!string.IsNullOrEmpty(item.sprite))
            {
                VisualElement imageElement = new();
                imageElement.AddToClassList("wardrobe-tile-image");
                imageElement.pickingMode = PickingMode.Ignore;
                button.Add(imageElement);

                imageElement.name = item.id + "_sprite";

                LoadSprite(item.sprite, loadedSprite =>
                {
                    imageElement.style.backgroundImage = new StyleBackground(loadedSprite);
                },
                () =>
                {
                    button.Remove(imageElement);
                    DisableButton(button, item.name);
                });
            }
            else
            {
                AddFallbackLabel(button, item.name);
            }

            TileButtonData tileButtonData = new(item, gridName);
            button.RegisterCallback<ClickEvent, TileButtonData>(WardrobeItemClicked, tileButtonData);
            button.RegisterCallback<PointerEnterEvent, TileButtonData>(WardrobeTilePointerEntered, tileButtonData);
            _tileCallbacks.Add((button, tileButtonData));

            return button;
        }

        public class TileButtonData
        {
            public WardrobeItem Item { get; }
            public string GridName { get; }
            public TileButtonData(WardrobeItem item, string gridName)
            {
                Item = item;
                GridName = gridName;
            }
        }

        private void WardrobeItemClicked(ClickEvent evt, TileButtonData data)
        {
            WardrobeItem item = data.Item;
            switch (item.SlotType)
            {
                case ClothingSlot.Top:
                    WardrobeState.Instance.CurrentItemTop = item;
                    break;
                case ClothingSlot.Bottoms:
                    WardrobeState.Instance.CurrentItemBottom = item;
                    break;
                case ClothingSlot.Shoes:
                    WardrobeState.Instance.CurrentItemShoe = item;
                    break;
                case ClothingSlot.Jacket:
                    WardrobeState.Instance.CurrentItemJacket = item;
                    break;
            }
            UpdateGridSelection(data.GridName, evt.currentTarget as Button);
        }

        /// <summary>
        /// Adds a text label as a placeholder when no sprite is available (e.g. "Nothing" items).
        /// The button remains clickable.
        /// </summary>
        private void AddFallbackLabel(Button button, string text)
        {
            Label fallbackLabel = new(text);
            fallbackLabel.AddToClassList("wardrobe-tile-placeholder");
            fallbackLabel.pickingMode = PickingMode.Ignore;
            button.Add(fallbackLabel);
        }

        /// <summary>
        /// Disables a tile button and shows a text label when a sprite key exists but failed to load.
        /// </summary>
        private void DisableButton(Button button, string text)
        {
            button.SetEnabled(false);
            button.AddToClassList("missing-art");

            Label fallbackLabel = new(text);
            fallbackLabel.AddToClassList("wardrobe-tile-placeholder");
            fallbackLabel.pickingMode = PickingMode.Ignore;
            button.Add(fallbackLabel);
        }

        /// <summary>
        /// Loads a sprite from Addressables, tracks the handle for cleanup, and invokes
        /// the appropriate callback on success or failure.
        /// </summary>
        private void LoadSprite(string key, Action<Sprite> onSuccess, Action onFailure = null)
        {
            AsyncOperationHandle<IList<IResourceLocation>> locHandle =
                Addressables.LoadResourceLocationsAsync(key, typeof(Sprite));

            locHandle.Completed += locOp =>
            {
                if (locOp.Status != AsyncOperationStatus.Succeeded || locOp.Result.Count == 0)
                {
                    Debug.LogWarning("WardrobeController: no Addressable location found for key: " + key);
                    Addressables.Release(locHandle);
                    onFailure?.Invoke();
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
                        Debug.LogWarning("WardrobeController: failed to load sprite from Addressables with key: " + key);
                        onFailure?.Invoke();
                    }
                };
            };
        }

        /// <summary>
        /// Updates selected USS class for one grid and clears prior selection.
        /// </summary>
        private void UpdateGridSelection(string gridName, Button newlySelectedButton)
        {
            if (_selectedButtonsByGrid.TryGetValue(gridName, out Button previousButton) && previousButton != null)
            {
                previousButton.RemoveFromClassList("selected");
            }

            newlySelectedButton.AddToClassList("selected");
            _selectedButtonsByGrid[gridName] = newlySelectedButton;
        }

        /// <summary>
        /// Applies selected USS class to initially active slot items.
        /// </summary>
        private void SyncInitialSelectionVisuals()
        {
            SyncSelectionForGrid("topsGrid", WardrobeState.Instance.CurrentItemTop);
            SyncSelectionForGrid("bottomsGrid", WardrobeState.Instance.CurrentItemBottom);
            SyncSelectionForGrid("shoesGrid", WardrobeState.Instance.CurrentItemShoe);
            SyncSelectionForGrid("jacketsGrid", WardrobeState.Instance.CurrentItemJacket);
        }

        /// <summary>
        /// Finds and marks the button that matches the current slot item.
        /// </summary>
        private void SyncSelectionForGrid(string gridName, WardrobeItem currentItem)
        {
            if (currentItem == null) { return; }

            VisualElement grid = _uiDocument.rootVisualElement.Q<VisualElement>(gridName);
            if (grid == null)
            {
                return;
            }

            foreach (VisualElement child in grid.Children())
            {
                if (child is Button button && button.userData is WardrobeItem item && item == currentItem)
                {
                    button.AddToClassList("selected");
                    _selectedButtonsByGrid[gridName] = button;
                    return;
                }
            }
        }

        /// <summary>
        /// Sets the header scenario description label from <see cref="ScenarioState.ActiveScenario"/> or a clear fallback.
        /// </summary>
        /// <param name="root">Root visual element of the wardrobe UIDocument.</param>
        private void ApplyScenarioDescriptionLabel(VisualElement root)
        {
            Label scenarioDesc = root.Q<Label>("scenarioDesc");
            if (scenarioDesc == null)
            {
                Debug.LogError("WardrobeController: scenarioDesc label not found in UXML.");
                return;
            }

            string scenarioDescText;
            if (ScenarioState.Instance.ActiveScenario != null)
            {
                scenarioDescText = ScenarioState.Instance.ActiveScenario.description ?? string.Empty;
            }
            else if (_sandboxMode)
            {
                scenarioDescText = "Sandbox Mode";
            }
            else
            {
                Debug.LogWarning("WardrobeController: ActiveScenario is null but sandbox mode is off; scenario header left empty.");
                scenarioDescText = "No scenario description (none active).";
            }

            scenarioDesc.text = scenarioDescText;
        }

        /// <summary>
        /// When the pointer enters a clothing tile, shows that item's description and sprite preview in the bottom panel.
        /// </summary>
        /// <param name="evt">Pointer enter event from the tile.</param>
        /// <param name="data">Tile item and grid name.</param>
        private void WardrobeTilePointerEntered(PointerEnterEvent evt, TileButtonData data)
        {
            if (data == null)
            {
                Debug.LogError("WardrobeController: WardrobeTilePointerEntered received null TileButtonData.");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;

            Label displayDesc = root.Q<Label>("hoverItemDesc");
            Label displayImage = root.Q<Label>("hoverItemImage");
            if (displayDesc == null || displayImage == null)
            {
                Debug.LogError("WardrobeController: hoverItemDesc or hoverItemImage not found in UXML.");
                return;
            }

            displayDesc.text = data.Item.description ?? string.Empty;

            Button evtButton = evt.currentTarget as Button;
            if (evtButton == null)
            {
                Debug.LogError("WardrobeController: WardrobeTilePointerEntered expected currentTarget to be a Button.");
                return;
            }

            VisualElement evtImage = evtButton.Q<VisualElement>(data.Item.id + "_sprite");
            if (evtImage != null)
            {
                displayImage.style.backgroundImage = evtImage.style.backgroundImage;
            }
            else
            {
                displayImage.style.backgroundImage = null;
            }
        }
    }
}
