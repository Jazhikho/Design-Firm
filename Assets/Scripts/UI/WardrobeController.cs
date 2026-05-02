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
        private Button _jacketsTrunk;
        private Button _topsTrunk;
        private Button _bottomsTrunk;
        private Button _shoesRack;
        private VisualElement _jacketsListContainer;
        private VisualElement _topsListContainer;
        private VisualElement _bottomsListContainer;
        private VisualElement _shoesListContainer;
        private Label _timerLabel;
        private Image _avatarImage;
        private Image _jacketImage;
        private Image _topImage;
        private Image _bottomsImage;
        private Image _shoesImage;

        private string _nextScene;

        private bool _wardrobeTimeExpiredHandled;

        // If true, a covers-bottom top/jacket is equipped and the bottoms trunk stays closed.
        private bool _bottomDisable;

        // If true, a non-placeholder bottom is equipped and covers-bottom tiles are highlighted.
        private bool _dressDisable;

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

        /// <summary>Resolved default border color from the first wardrobe tile (USS-driven).</summary>
        private StyleColor _defaultTileBorderStyleColor;

        private bool _defaultTileBorderCaptured;

        /// <summary>
        /// Chooses next-scene routing, caches UI references, and binds callbacks.
        /// </summary>
        private void OnEnable()
        {
            Scenario activeScenarioForSandbox = ScenarioState.Instance.ActiveScenario;
            if (activeScenarioForSandbox == null || activeScenarioForSandbox.name == "sandbox")
            {
                _sandboxMode = true;
            }

            _wardrobeTimeExpiredHandled = false;
            _defaultTileBorderCaptured = false;

            _nextScene = _sandboxMode ? GameConstants.MainMenuScene : GameConstants.TaskResultScene;

            _uiDocument = GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("WardrobeController: UIDocument is missing on this GameObject.");
                return;
            }

            VisualElement root = _uiDocument.rootVisualElement;
            UiDocumentPanelRootStretch.ApplyToPanelRootAndAppShell(root);

            _backButton = root.Q<Button>("btnBack");
            if (_backButton != null)
            {
                if (_sandboxMode)
                {
                    _backButton.RegisterCallback<ClickEvent>(NextSceneScript);
                }
                else
                {
                    _backButton.RegisterCallback<ClickEvent>(GoToScenarios);
                }
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
                _nextSceneButton.text = ">Finish<";
                _timerLabel.visible = false;
            }

            // Gets clothing trunks/rack buttons and item lists
            _jacketsListContainer = root.Q<VisualElement>("jacketsListContainer");
            _jacketsTrunk = root.Q<Button>("btnJacketsRack");

            _topsListContainer = root.Q<VisualElement>("topsListContainer");
            _topsTrunk = root.Q<Button>("btnTopsRack");

            _bottomsListContainer = root.Q<VisualElement>("bottomsListContainer");
            _bottomsTrunk = root.Q<Button>("btnBottomsRack");

            _shoesListContainer = root.Q<VisualElement>("shoesListContainer");
            _shoesRack = root.Q<Button>("btnShoesRack");

            if (_jacketsTrunk != null)
            {
                _jacketsTrunk.RegisterCallback<ClickEvent>(OpenJackets);
            }
            if (_topsTrunk != null)
            {
                _topsTrunk.RegisterCallback<ClickEvent>(OpenTops);
            }
            if (_bottomsTrunk != null)
            {
                _bottomsTrunk.RegisterCallback<ClickEvent>(OpenBottoms);
            }
            if (_shoesRack != null)
            {
                _shoesRack.RegisterCallback<ClickEvent>(OpenShoes);
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

                    if (!_wardrobeTimeExpiredHandled)
                    {
                        _wardrobeTimeExpiredHandled = true;
                        NextSceneScript(null);
                    }
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
                if (_sandboxMode)
                {
                    _backButton.UnregisterCallback<ClickEvent>(NextSceneScript);
                }
                else
                {
                    _backButton.UnregisterCallback<ClickEvent>(GoToScenarios);
                }
            }

            if (_nextSceneButton != null)
            {
                _nextSceneButton.UnregisterCallback<ClickEvent>(NextSceneScript);
            }
            if (_jacketsTrunk != null)
            {
                _jacketsTrunk.UnregisterCallback<ClickEvent>(OpenJackets);
            }
            if (_topsTrunk != null)
            {
                _topsTrunk.UnregisterCallback<ClickEvent>(OpenTops);
            }
            if (_bottomsTrunk != null)
            {
                _bottomsTrunk.UnregisterCallback<ClickEvent>(OpenBottoms);
            }
            if (_shoesRack != null)
            {
                _shoesRack.UnregisterCallback<ClickEvent>(OpenShoes);
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
            AudioManager.TryPlayButtonSfx();
            SceneManager.LoadScene(GameConstants.TaskScenarioScene);
        }

        private void RefreshUI()
        {
            TryApplyDefaultOutfitFromInventory();

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

        /// <summary>
        /// Sets each clothing slot to the first available item when inventory is present.
        /// Logs an error and returns early if any required slot list is empty.
        /// </summary>
        private void TryApplyDefaultOutfitFromInventory()
        {
            List<WardrobeItem> tops = WardrobeState.Instance.AvailableTops;
            List<WardrobeItem> jackets = WardrobeState.Instance.AvailableJackets;
            List<WardrobeItem> bottoms = WardrobeState.Instance.AvailableBottoms;
            List<WardrobeItem> shoes = WardrobeState.Instance.AvailableShoes;

            if (tops != null && tops.Count > 0)
                WardrobeState.Instance.CurrentItemTop = tops[0];

            if (jackets != null && jackets.Count > 0)
                WardrobeState.Instance.CurrentItemJacket = jackets[0];

            if (bottoms != null && bottoms.Count > 0)
                WardrobeState.Instance.CurrentItemBottom = bottoms[0];

            if (shoes != null && shoes.Count > 0)
                WardrobeState.Instance.CurrentItemShoe = shoes[0];
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
                if (!_defaultTileBorderCaptured)
                {
                    _defaultTileBorderStyleColor = tileButton.style.borderBottomColor;
                    _defaultTileBorderCaptured = true;
                }
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

            if (item.coversBottom && _dressDisable)
            {
                return;
            }
            if (item.slot == "bottom" && _bottomDisable && item.id != "nothing_bottom")
            {
                return;
            }
            ItemCoverBottomChecks(WardrobeState.Instance.GetCurrentItem(item.SlotType), item);
            switch (item.SlotType)
            {
                case ClothingSlot.Top:
                    WardrobeState.Instance.CurrentItemTop = item;
                    break;
                case ClothingSlot.Bottom:
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
                scenarioDescText = ScenarioState.Instance.ActiveScenario.name ?? string.Empty;
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
        /// When the pointer enters a clothing tile, shows that item's description in the bottom panel.
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

            if (displayDesc == null)
            {
                Debug.LogError("WardrobeController: hoverItemDesc not found in UXML.");
                return;
            }
            if (data.Item.coversBottom)
            {
                string baseDesc = data.Item.description ?? string.Empty;
                displayDesc.text = baseDesc + " (Disables bottom clothing)";
            }
            else
            {
                displayDesc.text = data.Item.description ?? string.Empty;
            }
        }

        /// <summary>
        /// Applies visibility to each clothing list container when that element exists in UXML.
        /// </summary>
        /// <param name="jackets">Visibility for the jackets list container.</param>
        /// <param name="tops">Visibility for the tops list container.</param>
        /// <param name="bottoms">Visibility for the bottoms list container.</param>
        /// <param name="shoes">Visibility for the shoes list container.</param>
        private void SetListContainerVisibility(
            Visibility jackets,
            Visibility tops,
            Visibility bottoms,
            Visibility shoes)
        {
            if (_jacketsListContainer != null)
            {
                _jacketsListContainer.style.visibility = jackets;
            }

            if (_topsListContainer != null)
            {
                _topsListContainer.style.visibility = tops;
            }

            if (_bottomsListContainer != null)
            {
                _bottomsListContainer.style.visibility = bottoms;
            }

            if (_shoesListContainer != null)
            {
                _shoesListContainer.style.visibility = shoes;
            }
        }

        /// <summary>
        /// Shows the jackets item list and hides the other slot lists.
        /// </summary>
        /// <param name="clickEvent">Click event from the jackets rack button.</param>
        private void OpenJackets(ClickEvent clickEvent)
        {
            SetListContainerVisibility(
                Visibility.Visible,
                Visibility.Hidden,
                Visibility.Hidden,
                Visibility.Hidden);
        }

        /// <summary>
        /// Shows the tops item list and hides the other slot lists.
        /// </summary>
        /// <param name="clickEvent">Click event from the tops rack button.</param>
        private void OpenTops(ClickEvent clickEvent)
        {
            SetListContainerVisibility(
                Visibility.Hidden,
                Visibility.Visible,
                Visibility.Hidden,
                Visibility.Hidden);
        }

        /// <summary>
        /// Shows the bottoms item list and hides the other slot lists.
        /// </summary>
        /// <param name="clickEvent">Click event from the bottoms rack button.</param>
        private void OpenBottoms(ClickEvent clickEvent)
        {
            if (_bottomDisable)
            {
                return;
            }
            SetListContainerVisibility(
                Visibility.Hidden,
                Visibility.Hidden,
                Visibility.Visible,
                Visibility.Hidden);
        }

        /// <summary>
        /// Shows the shoes item list and hides the other slot lists.
        /// </summary>
        /// <param name="clickEvent">Click event from the shoes rack button.</param>
        private void OpenShoes(ClickEvent clickEvent)
        {
            SetListContainerVisibility(
                Visibility.Hidden,
                Visibility.Hidden,
                Visibility.Hidden,
                Visibility.Visible);
        }

        /// <summary>
        /// Updates covers-bottom UI flags, tile highlights, and wardrobe slot sounds for the pending selection.
        /// Runs before the clicked slot is written on <see cref="WardrobeState"/> so equip, swap, and unequip sounds describe the transition from previous to new item.
        /// </summary>
        /// <param name="currentItem">Equipped item in <paramref name="newItem"/>'s slot before this click.</param>
        /// <param name="newItem">Item the player selected.</param>
        private void ItemCoverBottomChecks(WardrobeItem currentItem, WardrobeItem newItem)
        {
            if (newItem == null)
            {
                Debug.LogError("WardrobeController: ItemCoverBottomChecks newItem is null.");
                return;
            }

            if (currentItem == null)
            {
                Debug.LogError("WardrobeController: ItemCoverBottomChecks currentItem is null.");
                return;
            }

            const string placeholderItemName = "Nothing";
            if ((currentItem.name == placeholderItemName) && (newItem.name != placeholderItemName))
            {
                AudioManager.TryPlayOtherSFX(AudioManager.OtherSfxEquipKey);
            }
            else if ((currentItem.name != placeholderItemName) && (newItem.name == placeholderItemName))
            {
                AudioManager.TryPlayOtherSFX(AudioManager.OtherSfxUnequipKey);
            }
            else if ((currentItem.name != placeholderItemName) && (newItem.name != placeholderItemName))
            {
                AudioManager.TryPlayOtherSFX(AudioManager.OtherSfxSwapKey);
            }

            StyleColor redBorderStyle = new(new Color(1f, 0f, 0f, 1f));

            if (newItem.coversBottom && !_bottomDisable)
            {
                _bottomDisable = true;
            }
            else if (newItem.slot == "bottom" && newItem.id != "nothing_bottom" && !_dressDisable)
            {
                _dressDisable = true;
                foreach ((Button button, TileButtonData data) in _tileCallbacks)
                {
                    if (data.Item.coversBottom)
                    {
                        button.style.borderBottomColor = redBorderStyle;
                        button.style.borderRightColor = redBorderStyle;
                        button.style.borderLeftColor = redBorderStyle;
                        button.style.borderTopColor = redBorderStyle;
                    }
                }
            }
            else
            {
                if (_dressDisable && (WardrobeState.Instance.CurrentItemBottom.id == "nothing_bottom" || newItem.id == "nothing_bottom"))
                {
                    _dressDisable = false;
                    foreach ((Button button, TileButtonData data) in _tileCallbacks)
                    {
                        if (data.Item.coversBottom)
                        {
                            button.style.borderBottomColor = _defaultTileBorderStyleColor;
                            button.style.borderRightColor = _defaultTileBorderStyleColor;
                            button.style.borderLeftColor = _defaultTileBorderStyleColor;
                            button.style.borderTopColor = _defaultTileBorderStyleColor;
                        }
                    }
                }

                WardrobeItem currentTop = WardrobeState.Instance.CurrentItemTop;
                WardrobeItem currentJacket = WardrobeState.Instance.CurrentItemJacket;
                bool topAllowsBottoms =
                    (currentTop != null && currentTop.coversBottom == false)
                    || (newItem.slot == "top" && newItem.coversBottom == false);
                bool jacketAllowsBottoms =
                    (currentJacket != null && currentJacket.coversBottom == false)
                    || (newItem.slot == "jacket" && newItem.coversBottom == false);

                if (topAllowsBottoms && jacketAllowsBottoms)
                {
                    _bottomDisable = false;
                }
            }
        }
    }
}
