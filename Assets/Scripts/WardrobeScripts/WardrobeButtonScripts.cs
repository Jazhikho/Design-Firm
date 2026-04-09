using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Binds wardrobe UI Toolkit grids and stacked avatar images to the static <see cref="WardrobeItemList"/> data.
/// Runs after <see cref="WardrobeManagerScript"/> so JSON items exist before list wiring and defaults apply.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class WardrobeButtonScripts : MonoBehaviour
{
    private UIDocument _uiDocument;
    private Button _nextSceneButton;
    private Button _backButton;
    private Label _timerLabel;

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

    /// <summary>
    /// Chooses next-scene routing, caches UI references, and binds callbacks.
    /// </summary>
    private void OnEnable()
    {
        if (_sandboxMode == false)
        {
            _nextScene = GameConstants.TaskResult;
        }
        else
        {
            _nextScene = GameConstants.MainMenuScene;
        }

        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null)
        {
            Debug.LogError("WardrobeButtonScripts: UIDocument is missing on this GameObject.");
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("WardrobeButtonScripts: rootVisualElement is null.");
            return;
        }

        _backButton = root.Q<Button>("btnBack");
        if (_backButton != null)
        {
            _backButton.clicked += GoToScenarios;
        }

        _nextSceneButton = root.Q<Button>("nextSceneButton");
        if (_nextSceneButton == null)
        {
            Debug.LogError("WardrobeButtonScripts: nextSceneButton not found in UXML.");
            return;
        }

        _nextSceneButton.RegisterCallback<ClickEvent>(NextSceneScript);

        _timerLabel = root.Q<Label>("lblTimer");
        if (_timerLabel == null)
        {
            Debug.LogError("WardrobeButtonScripts: lblTimer not found in UXML.");
            return;
        }

        SetUpClothing(gridName: "topsGrid",
            slotList: WardrobeItemList.Instance.WardrobeListItemsChest,
            setItemSlot: "chest",
            setItemUi: "activeTop");
        SetUpClothing(gridName: "bottomsGrid",
            slotList: WardrobeItemList.Instance.WardrobeListItemsBottom,
            setItemSlot: "bottom",
            setItemUi: "activeBottoms");
        SetUpClothing(gridName: "shoesGrid",
            slotList: WardrobeItemList.Instance.WardrobeListItemsShoe,
            setItemSlot: "shoe",
            setItemUi: "activeShoes");
        SetUpClothing(gridName: "jacketsGrid",
            slotList: WardrobeItemList.Instance.WardrobeListItemsJacket,
            setItemSlot: "jacket",
            setItemUi: "activeJackets");

        ApplyDefaultOutfit();
        SyncInitialSelectionVisuals();
    }

    /// <summary>
    /// Updates the countdown label every frame.
    /// </summary>
    private void Update()
    {
        _wardrobeTimer -= Time.deltaTime;
        if (_wardrobeTimer < 0f)
        {
            _wardrobeTimer = 0f;
        }

        if (_timerLabel != null)
        {
            _timerLabel.text = Mathf.CeilToInt(_wardrobeTimer).ToString();
        }
    }

    /// <summary>
    /// Unbinds callbacks when this view is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (_backButton != null)
        {
            _backButton.clicked -= GoToScenarios;
        }

        _nextSceneButton?.UnregisterCallback<ClickEvent>(NextSceneScript);
    }

    /// <summary>
    /// Returns from wardrobe to scenario selection.
    /// </summary>
    private void GoToScenarios()
    {
        SceneManager.LoadScene(GameConstants.TaskScenario);
    }

    /// <summary>
    /// Applies the first non-placeholder item per slot so the mannequin matches JSON-driven gear instead of seeded "Nothing" rows.
    /// </summary>
    private void ApplyDefaultOutfit()
    {
        WardrobeItemClothing chestDefault =
            WardrobeItemList.Instance.GetFirstDisplayableItem(WardrobeItemList.Instance.WardrobeListItemsChest);
        WardrobeItemClothing bottomDefault =
            WardrobeItemList.Instance.GetFirstDisplayableItem(WardrobeItemList.Instance.WardrobeListItemsBottom);
        WardrobeItemClothing shoeDefault =
            WardrobeItemList.Instance.GetFirstDisplayableItem(WardrobeItemList.Instance.WardrobeListItemsShoe);
        WardrobeItemClothing jacketDefault =
            WardrobeItemList.Instance.GetFirstDisplayableItem(WardrobeItemList.Instance.WardrobeListItemsJacket);

        if (chestDefault != null)
        {
            SetCurrentItem(chestDefault, "chest", "activeTop");
        }

        if (bottomDefault != null)
        {
            SetCurrentItem(bottomDefault, "bottom", "activeBottoms");
        }

        if (shoeDefault != null)
        {
            SetCurrentItem(shoeDefault, "shoe", "activeShoes");
        }

        if (jacketDefault != null)
        {
            SetCurrentItem(jacketDefault, "jacket", "activeJackets");
        }
    }

    /// <summary>
    /// Sets a stacked <see cref="Image"/> background from a clothing sprite.
    /// </summary>
    /// <param name="elementName">UXML name of the target Image.</param>
    /// <param name="spriteUI">Sprite to display, or null to clear.</param>
    public void ChangeUISlotSprite(string elementName, Sprite spriteUI)
    {
        if (_uiDocument == null)
        {
            Debug.LogError("WardrobeButtonScripts: ChangeUISlotSprite called before UIDocument was cached.");
            return;
        }

        VisualElement root = _uiDocument.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("WardrobeButtonScripts: rootVisualElement is null.");
            return;
        }

        Image slotUI = root.Q<Image>(elementName);
        if (slotUI == null)
        {
            Debug.LogError("WardrobeButtonScripts: UI Image not found: " + elementName);
            return;
        }

        if (spriteUI != null)
        {
            slotUI.style.backgroundImage = new StyleBackground(spriteUI);
        }
        else
        {
            slotUI.style.backgroundImage = new StyleBackground();
        }
    }

    /// <summary>
    /// Loads the configured next scene when the submit button is used.
    /// </summary>
    /// <param name="evt">Click event from UI Toolkit.</param>
    public void NextSceneScript(ClickEvent evt)
    {
        if (string.IsNullOrEmpty(_nextScene))
        {
            Debug.LogError("WardrobeButtonScripts: nextScene is not set; check sandbox mode and Build Settings.");
            return;
        }

        SceneManager.LoadScene(_nextScene);
    }

    /// <summary>
    /// Populates a grid with one button per clothing item and wires selection.
    /// </summary>
    /// <param name="gridName">UXML name of the container VisualElement.</param>
    /// <param name="slotList">Items shown in the list.</param>
    /// <param name="setItemSlot">Slot tag passed to <see cref="WardrobeItemList.SetItemSlot"/>.</param>
    /// <param name="setItemUi">UXML name of the stacked Image to update.</param>
    public void SetUpClothing(string gridName,
        List<WardrobeItemClothing> slotList,
        string setItemSlot,
        string setItemUi)
    {
        if (slotList == null)
        {
            Debug.LogError($"slotList is null for grid {gridName}");
            return;
        }

        VisualElement grid = _uiDocument.rootVisualElement.Q<VisualElement>(gridName);
        if (grid == null)
        {
            Debug.LogError($"WardrobeButtonScripts: Could not find grid: {gridName}");
            return;
        }

        grid.Clear();

        foreach (WardrobeItemClothing item in slotList)
        {
            Button tileButton = CreateClothingButton(item, gridName, setItemSlot, setItemUi);
            grid.Add(tileButton);
        }
    }

    /// <summary>
    /// Builds one clickable clothing tile for a grid.
    /// </summary>
    /// <param name="item">Data entry represented by this tile.</param>
    /// <param name="gridName">Target grid name for selection tracking.</param>
    /// <param name="setItemSlot">Slot key to update in shared state.</param>
    /// <param name="setItemUi">Stacked image element name to refresh.</param>
    private Button CreateClothingButton(WardrobeItemClothing item, string gridName, string setItemSlot, string setItemUi)
    {
        Button button = new();
        button.AddToClassList("wardrobe-tile");
        button.userData = item;

        // Optional: remove button text so Unity doesn't reserve weird text space.
        button.text = string.Empty;

        if (item.ItemSprite != null)
        {
            VisualElement image = new();
            image.AddToClassList("wardrobe-tile-image");
            image.style.backgroundImage = new StyleBackground(item.ItemSprite);
            image.pickingMode = PickingMode.Ignore;
            button.Add(image);
        }
        else // Missing art or placeholder item.
        {
            button.AddToClassList("missing-art");

            Label fallbackLabel = new(item.ItemName);
            fallbackLabel.AddToClassList("wardrobe-tile-placeholder");
            fallbackLabel.pickingMode = PickingMode.Ignore;
            button.Add(fallbackLabel);
        }

        button.clicked += () =>
        {
            SetCurrentItem(item, setItemSlot, setItemUi);
            UpdateGridSelection(gridName, button);
        };

        return button;
    }

    /// <summary>
    /// Updates selected CSS state for one grid and clears prior selection.
    /// </summary>
    /// <param name="gridName">Grid key in the selection map.</param>
    /// <param name="newlySelectedButton">Button that should appear selected.</param>
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
    /// Applies selected CSS class to initially active slot items.
    /// </summary>
    private void SyncInitialSelectionVisuals()
    {
        SyncSelectionForGrid("topsGrid", WardrobeItemList.Instance.CurrentItemTop);
        SyncSelectionForGrid("bottomsGrid", WardrobeItemList.Instance.CurrentItemBottom);
        SyncSelectionForGrid("shoesGrid", WardrobeItemList.Instance.CurrentItemShoe);
        SyncSelectionForGrid("jacketsGrid", WardrobeItemList.Instance.CurrentItemJacket);
    }

    /// <summary>
    /// Finds and marks the button that matches the current slot item.
    /// </summary>
    /// <param name="gridName">UXML grid to search.</param>
    /// <param name="currentItem">Currently selected clothing item for the slot.</param>
    private void SyncSelectionForGrid(string gridName, WardrobeItemClothing currentItem)
    {
        if (currentItem == null)
        {
            return;
        }

        VisualElement grid = _uiDocument.rootVisualElement.Q<VisualElement>(gridName);
        if (grid == null)
        {
            return;
        }

        foreach (VisualElement child in grid.Children())
        {
            if (child is Button button && button.userData is WardrobeItemClothing item && item == currentItem)
            {
                button.AddToClassList("selected");
                _selectedButtonsByGrid[gridName] = button;
                return;
            }
        }
    }

    /// <summary>
    /// Updates static slot state and the stacked avatar image for one layer.
    /// </summary>
    /// <param name="itemToSet">Clothing row to apply.</param>
    /// <param name="listSlot">Slot tag: chest, bottom, shoe, or jacket.</param>
    /// <param name="slotUi">UXML Image name for that layer.</param>
    private void SetCurrentItem(WardrobeItemClothing itemToSet, string listSlot, string slotUi)
    {
        if (itemToSet == null)
        {
            Debug.LogError("WardrobeButtonScripts: itemToSet is null for slot " + listSlot);
            return;
        }

        WardrobeItemList.Instance.SetItemSlot(listSlot, itemToSet);
        ChangeUISlotSprite(slotUi, itemToSet.ItemSprite);
    }
}
