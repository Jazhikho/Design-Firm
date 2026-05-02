using Assets.Scripts.Core;
using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UIElements;

namespace Assets.Scripts.UI
{
    /// <summary>
    /// Binds wardrobe UI Toolkit controls and stacked avatar images to <see cref="WardrobeState"/>.
    /// A single scroll list shows all clothing items; the ToggleButtonGroup filters by slot.
    /// Each tile toggles selection for its slot — clicking the active item deselects it (slot → null).
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class WardrobeController : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private Button _nextSceneButton;
        private Button _backButton;
        private ScrollView _clothingScrollView;
        private ToggleButtonGroup _filterGroup;
        private Label _lblWarning;
        private Label _timerLabel;
        private Image _avatarImage;
        private Image _jacketImage;
        private Image _topImage;
        private Image _bottomsImage;
        private Image _shoesImage;

        private string _nextScene;
        private bool _wardrobeTimeExpiredHandled;

        /// <summary>When true, submit returns to main menu instead of result scene.</summary>
        [SerializeField]
        private bool _sandboxMode;

        /// <summary>Remaining wardrobe selection time in seconds.</summary>
        [SerializeField]
        private float _wardrobeTime = 60f;

        /// <summary>The currently-selected tile button for each clothing slot.</summary>
        private readonly Dictionary<ClothingSlot, Button> _selectedButtonBySlot = new();

        /// <summary>
        /// Slots that are currently shown by the filter. Empty set means "show all".
        /// </summary>
        private readonly HashSet<ClothingSlot> _activeFilters = new();

        /// <summary>All tile buttons that have registered callbacks, for cleanup.</summary>
        private readonly List<(Button button, TileButtonData data)> _tileCallbacks = new();

        private readonly List<AsyncOperationHandle<Sprite>> _spriteHandles = new();

        /// <summary>
        /// Maps ToggleButtonGroup child index → ClothingSlot.
        /// Order must match UXML child order: Jackets (0), Tops (1), Bottoms (2), Shoes (3).
        /// </summary>
        private static readonly ClothingSlot[] FilterSlotOrder =
        {
            ClothingSlot.Jacket,
            ClothingSlot.Top,
            ClothingSlot.Bottom,
            ClothingSlot.Shoes,
        };

        /// <summary>
        /// Chooses next-scene routing, caches UI references, and binds callbacks.
        /// </summary>
        private void OnEnable()
        {
            Scenario activeScenarioForSandbox = ScenarioState.Instance.ActiveScenario;
            if (activeScenarioForSandbox == null || activeScenarioForSandbox.name == "sandbox")
                _sandboxMode = true;

            _wardrobeTimeExpiredHandled = false;
            _nextScene = _sandboxMode ? GameConstants.MainMenuScene : GameConstants.ResultScene;

            _uiDocument = GetComponent<UIDocument>();
            VisualElement root = _uiDocument.rootVisualElement;

            _backButton = root.Q<Button>("btnBack");
            _backButton?.RegisterCallback<ClickEvent>(GoToScenarios);

            _nextSceneButton = root.Q<Button>("nextSceneButton");
            _nextSceneButton?.RegisterCallback<ClickEvent>(NextSceneScript);

            Label displayDesc = root.Q<Label>("hoverItemDesc");
            if (displayDesc == null)
                Debug.LogError("WardrobeController: hoverItemDesc not found in UXML.");
            else
                displayDesc.text = string.Empty;

            _timerLabel = root.Q<Label>("lblTimer");
            if (_timerLabel == null)
                Debug.LogError("WardrobeController: lblTimer not found in UXML.");

            if (_sandboxMode && _timerLabel != null)
                _timerLabel.visible = false;

            _lblWarning = _uiDocument.rootVisualElement.Q<Label>("lblWarning");
            _lblWarning.text = string.Empty;

            _clothingScrollView = root.Q<ScrollView>("clothingItemsList");
            if (_clothingScrollView == null)
                Debug.LogError("WardrobeController: clothingItemsList ScrollView not found in UXML.");

            _filterGroup = root.Q<ToggleButtonGroup>();
            _filterGroup?.RegisterValueChangedCallback(OnFilterChanged);

            _avatarImage  = root.Q<Image>("activeAvatar");
            _jacketImage  = root.Q<Image>("activeJacket");
            _topImage     = root.Q<Image>("activeTop");
            _bottomsImage = root.Q<Image>("activeBottoms");
            _shoesImage   = root.Q<Image>("activeShoes");

            SubscribeToSlotChanges();

            if (WardrobeState.Instance.IsWardrobeItemsLoaded)
                RefreshUI();
            else
                WardrobeState.Instance.WardrobeItemsLoaded += OnWardrobeItemsLoaded;

            ApplyScenarioDescriptionLabel(root);
        }

        /// <summary>Updates the countdown label every frame.</summary>
        private void Update()
        {
            if (!_sandboxMode)
            {
                _wardrobeTime -= Time.deltaTime;
                if (_wardrobeTime < 0f)
                {
                    _wardrobeTime = 0f;
                    if (!_wardrobeTimeExpiredHandled)
                    {
                        _wardrobeTimeExpiredHandled = true;
                        NextSceneScript(null);
                    }
                }
            }

            if (_timerLabel != null)
            {
                int minutes = Mathf.FloorToInt(_wardrobeTime / 60f);
                float seconds = _wardrobeTime % 60f;
                _timerLabel.text = $"{minutes}:{seconds:00.0}";
                _timerLabel.EnableInClassList("timer-warning", _wardrobeTime < 10f);
            }
        }

        /// <summary>
        /// Unbinds callbacks, unsubscribes from state events, and releases loaded sprite handles.
        /// </summary>
        private void OnDisable()
        {
            _backButton?.UnregisterCallback<ClickEvent>(GoToScenarios);
            _nextSceneButton?.UnregisterCallback<ClickEvent>(NextSceneScript);

            _filterGroup?.UnregisterValueChangedCallback(OnFilterChanged);

            foreach ((Button button, TileButtonData data) in _tileCallbacks)
            {
                button.UnregisterCallback<ClickEvent, TileButtonData>(WardrobeItemClicked);
                button.UnregisterCallback<PointerEnterEvent, TileButtonData>(WardrobeTilePointerEntered);
                button.UnregisterCallback<PointerLeaveEvent, TileButtonData>(WardrobeTilePointerExited);
            }
            _tileCallbacks.Clear();

            UnsubscribeFromSlotChanges();
            WardrobeState.Instance.WardrobeItemsLoaded -= OnWardrobeItemsLoaded;

            foreach (AsyncOperationHandle<Sprite> handle in _spriteHandles)
                Addressables.Release(handle);
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

        #region Refresh / populate

        private void OnWardrobeItemsLoaded() => RefreshUI();

        private void RefreshUI()
        {
            string avatarKey = ScenarioState.Instance.ActiveScenario?.avatarImage;
            if (string.IsNullOrEmpty(avatarKey))
                Debug.LogWarning("WardrobeController: No avatarImage key found. ActiveScenario may not be set.");
            UpdateSlotSprite(_avatarImage, avatarKey);

            PopulateClothingList();
            SyncInitialSelectionVisuals();
        }

        /// <summary>
        /// Clears and rebuilds the unified clothing scroll list from all available items, grouped by slot.
        /// </summary>
        private void PopulateClothingList()
        {
            if (_clothingScrollView == null) return;

            _clothingScrollView.Clear();
            _selectedButtonBySlot.Clear();

            AddItemsToList(WardrobeState.Instance.AvailableJackets);
            AddItemsToList(WardrobeState.Instance.AvailableTops);
            AddItemsToList(WardrobeState.Instance.AvailableBottoms);
            AddItemsToList(WardrobeState.Instance.AvailableShoes);
        }

        /// <summary>Creates a tile for each item in <paramref name="slotList"/> and appends it to the scroll view.</summary>
        private void AddItemsToList(List<WardrobeItem> slotList)
        {
            if (slotList == null) return;
            foreach (WardrobeItem item in slotList)
                _clothingScrollView.Add(CreateClothingButton(item));
        }

        #endregion

        #region Filter

        /// <summary>
        /// Rebuilds <see cref="_activeFilters"/> from the ToggleButtonGroup state, then refreshes tile visibility.
        /// Button order in UXML: index 0 = Jackets, 1 = Tops, 2 = Bottoms, 3 = Shoes.
        /// </summary>
        private void OnFilterChanged(ChangeEvent<ToggleButtonGroupState> evt)
        {
            _activeFilters.Clear();

            Span<int> buffer = stackalloc int[64];
            Span<int> activeIndices = evt.newValue.GetActiveOptions(buffer);

            foreach (int idx in activeIndices)
            {
                if (idx >= 0 && idx < FilterSlotOrder.Length)
                    _activeFilters.Add(FilterSlotOrder[idx]);
            }

            ApplyFilterVisibility();
        }

        /// <summary>
        /// Shows or hides each tile based on <see cref="_activeFilters"/>.
        /// An empty filter set shows all tiles.
        /// </summary>
        private void ApplyFilterVisibility()
        {
            if (_clothingScrollView == null) return;

            bool showAll = _activeFilters.Count == 0;

            foreach (VisualElement child in _clothingScrollView.contentContainer.Children())
            {
                if (child.userData is WardrobeItem item)
                {
                    child.style.display = showAll || _activeFilters.Contains(item.SlotType)
                        ? DisplayStyle.Flex
                        : DisplayStyle.None;
                }
            }
        }

        #endregion

        # region Selection

        /// <summary>
        /// Gets the current WardrobeState item for the given slot.
        /// </summary>
        private WardrobeItem GetCurrentSlotItem(ClothingSlot slot) => slot switch
        {
            ClothingSlot.Top => WardrobeState.Instance.CurrentItemTop,
            ClothingSlot.Bottom => WardrobeState.Instance.CurrentItemBottom,
            ClothingSlot.Shoes => WardrobeState.Instance.CurrentItemShoe,
            ClothingSlot.Jacket => WardrobeState.Instance.CurrentItemJacket,
            _ => null,
        };

        /// <summary>
        /// Sets the WardrobeState item for the given slot. Pass null to clear the slot.
        /// </summary>
        private void SetSlotItem(ClothingSlot slot, WardrobeItem item)
        {
            switch (slot)
            {
                case ClothingSlot.Top: WardrobeState.Instance.CurrentItemTop = item; break;
                case ClothingSlot.Bottom: WardrobeState.Instance.CurrentItemBottom = item; break;
                case ClothingSlot.Shoes: WardrobeState.Instance.CurrentItemShoe = item; break;
                case ClothingSlot.Jacket: WardrobeState.Instance.CurrentItemJacket = item; break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Moves the "selected" USS class from the previous button for this slot to <paramref name="newButton"/>.
        /// Pass null to clear selection without selecting another tile.
        /// </summary>
        private void UpdateSlotSelection(ClothingSlot slot, Button newButton)
        {
            if (_selectedButtonBySlot.TryGetValue(slot, out Button previous) && previous != null)
                previous.RemoveFromClassList("selected");

            if (newButton != null)
            {
                newButton.AddToClassList("selected");
                _selectedButtonBySlot[slot] = newButton;
            }
            else
            {
                _selectedButtonBySlot.Remove(slot);
            }
        }

        /// <summary>
        /// Marks the button matching each slot's current item as selected.
        /// Called once after the list is first populated.
        /// </summary>
        private void SyncInitialSelectionVisuals()
        {
            SyncSelectionForSlot(ClothingSlot.Top, WardrobeState.Instance.CurrentItemTop);
            SyncSelectionForSlot(ClothingSlot.Bottom, WardrobeState.Instance.CurrentItemBottom);
            SyncSelectionForSlot(ClothingSlot.Shoes, WardrobeState.Instance.CurrentItemShoe);
            SyncSelectionForSlot(ClothingSlot.Jacket, WardrobeState.Instance.CurrentItemJacket);
        }

        private void SyncSelectionForSlot(ClothingSlot slot, WardrobeItem currentItem)
        {
            if (currentItem == null || _clothingScrollView == null) return;

            foreach (VisualElement child in _clothingScrollView.contentContainer.Children())
            {
                if (child is Button button && button.userData is WardrobeItem item && item == currentItem)
                {
                    button.AddToClassList("selected");
                    _selectedButtonBySlot[slot] = button;
                    return;
                }
            }
        }

        /// <summary>
        /// Click handler for a clothing tile. Toggles selection for the item's slot:
        /// clicking the already-active item deselects it (slot → null); otherwise selects it.
        /// </summary>
        private void WardrobeItemClicked(ClickEvent evt, TileButtonData data)
        {
            WardrobeItem item = data.Item;
            ClothingSlot slot = item.SlotType;
            Button clickedButton = evt.currentTarget as Button;

            if (GetCurrentSlotItem(slot) == item)
            {
                // Toggle off: clear the slot
                SetSlotItem(slot, null);
                UpdateSlotSelection(slot, null);
            }
            else
            {
                // Select this item
                SetSlotItem(slot, item);
                UpdateSlotSelection(slot, clickedButton);
            }
        }

        #endregion

        #region Navigation

        /// <summary>Returns from wardrobe to scenario selection.</summary>
        private void GoToScenarios(ClickEvent e)
        {
            AudioManager.TryPlayButtonSfx();
            NavigationManager.Instance.NavigateToScene(GameConstants.ScenarioScene);
        }

        /// <summary>Loads the configured next scene when the submit button is used.</summary>
        public void NextSceneScript(ClickEvent evt)
        {
            if (string.IsNullOrEmpty(_nextScene))
            {
                Debug.LogError("WardrobeController: nextScene is not set; check sandbox mode and Build Settings.");
                return;
            }
            AudioManager.TryPlaySubmitOutfitSfx();
            NavigationManager.Instance.NavigateToScene(_nextScene);
        }

        #endregion

        #region Tile creation

        /// <summary>
        /// Builds one clickable clothing tile. The userData is set to <paramref name="item"/> so filter
        /// and selection logic can recover the item without a closure.
        /// </summary>
        private Button CreateClothingButton(WardrobeItem item)
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
                imageElement.AddToClassList(SlotImageClass(item.SlotType));
                imageElement.pickingMode = PickingMode.Ignore;
                imageElement.name = item.id + "_sprite";
                button.Add(imageElement);

                LoadSprite(item.sprite,
                    loadedSprite =>
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

            TileButtonData tileButtonData = new(item);
            button.RegisterCallback<ClickEvent, TileButtonData>(WardrobeItemClicked, tileButtonData);
            button.RegisterCallback<PointerEnterEvent, TileButtonData>(WardrobeTilePointerEntered, tileButtonData);
            button.RegisterCallback<PointerLeaveEvent, TileButtonData>(WardrobeTilePointerExited, tileButtonData);
            _tileCallbacks.Add((button, tileButtonData));

            return button;
        }

        public class TileButtonData
        {
            public WardrobeItem Item { get; }
            public TileButtonData(WardrobeItem item) { Item = item; }
        }

        private static string SlotImageClass(ClothingSlot slot) => slot switch
        {
            ClothingSlot.Jacket => "wardrobe-tile-image--jacket",
            ClothingSlot.Top => "wardrobe-tile-image--top",
            ClothingSlot.Bottom => "wardrobe-tile-image--bottom",
            ClothingSlot.Shoes => "wardrobe-tile-image--shoes",
            _ => string.Empty,
        };

        #endregion

        #region Sprite helpers

        /// <summary>
        /// Loads a sprite from Addressables by key and assigns it to <paramref name="slotImage"/>.
        /// Clears the image if the key is null or empty.
        /// </summary>
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

            LoadSprite(spriteKey,
                loadedSprite => { slotImage.sprite = loadedSprite; },
                () => { slotImage.sprite = null; });
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
                        onSuccess(op.Result);
                    else
                    {
                        Debug.LogWarning("WardrobeController: failed to load sprite from Addressables with key: " + key);
                        onFailure?.Invoke();
                    }
                };
            };
        }

        #endregion

        #region Tile helpers

        /// <summary>Adds a text label as a placeholder when no sprite is available (e.g. "Nothing" items).</summary>
        private void AddFallbackLabel(Button button, string text)
        {
            Label fallbackLabel = new(text);
            fallbackLabel.AddToClassList("wardrobe-tile-placeholder");
            fallbackLabel.pickingMode = PickingMode.Ignore;
            button.Add(fallbackLabel);
        }

        /// <summary>Disables a tile and shows a text label when a sprite key exists but failed to load.</summary>
        private void DisableButton(Button button, string text)
        {
            button.SetEnabled(false);
            button.AddToClassList("missing-art");

            Label fallbackLabel = new(text);
            fallbackLabel.AddToClassList("wardrobe-tile-placeholder");
            fallbackLabel.pickingMode = PickingMode.Ignore;
            button.Add(fallbackLabel);
        }

        #endregion

        #region Scenario description

        /// <summary>
        /// Sets the header scenario description label from <see cref="ScenarioState.ActiveScenario"/>.
        /// </summary>
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
                scenarioDescText = ScenarioState.Instance.ActiveScenario.name ?? string.Empty;
            else if (_sandboxMode)
                scenarioDescText = "Sandbox Mode";
            else
            {
                Debug.LogWarning("WardrobeController: ActiveScenario is null but sandbox mode is off; scenario header left empty.");
                scenarioDescText = "No scenario description (none active).";
            }

            scenarioDesc.text = scenarioDescText;
        }

        #endregion

        #region Hover tooltip

        /// <summary>
        /// When the pointer enters a clothing tile, shows that item's description in the info panel.
        /// </summary>
        private void WardrobeTilePointerEntered(PointerEnterEvent evt, TileButtonData data)
        {
            if (data == null) return;

            Label displayDesc = _uiDocument.rootVisualElement.Q<Label>("hoverItemDesc");
            if (displayDesc == null)
                Debug.LogWarning("WardrobeController: hoverItemDesc not found in UXML.");
            else
                displayDesc.text = data.Item.description ?? string.Empty;

            if (data.Item.coversBottom)
                _lblWarning.text = "This item is a dress so choosing it as your top will also remove any selected bottoms.";
        }

        /// <summary>
        /// Handles the pointer exit event for a wardrobe tile, clearing the item description display.
        /// </summary>
        /// <param name="evt">The pointer leave event data associated with the pointer exiting the tile.</param>
        /// <param name="data">The data representing the wardrobe tile button from which the pointer exited.</param>
        private void WardrobeTilePointerExited(PointerLeaveEvent evt, TileButtonData data)
        {
            Label displayDesc = _uiDocument.rootVisualElement.Q<Label>("hoverItemDesc");
            if (displayDesc == null)
                Debug.LogWarning("WardrobeController: hoverItemDesc not found in UXML.");
            else
                displayDesc.text = string.Empty;

            if (data.Item.coversBottom)
                _lblWarning.text = string.Empty;
        }

        #endregion
    }
}
