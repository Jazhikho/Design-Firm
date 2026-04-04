using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Binds wardrobe UI Toolkit ScrollViews and stacked avatar images to the static <see cref="wardrobeItemList"/> data.
/// Runs after <see cref="wardrobeManagerScript"/> so JSON items exist before list wiring and defaults apply.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class wardrobeButtonTestScripts : MonoBehaviour
{
    private UIDocument thisDoc;
    private Button nextSceneButton;

    [SerializeField]
    private bool sandboxMode;

    /// <summary>
    /// Target scene after submit; set at runtime from <see cref="sandboxMode"/> unless overridden in the Inspector for testing.
    /// </summary>
    [SerializeField]
    private string nextScene;

    private Label timerModule;

    [SerializeField]
    private float wardrobeTimer = 60f;

    private readonly Dictionary<string, Button> selectedButtonsByGrid = new();

    private void OnEnable()
    {
        if (sandboxMode == false)
        {
            nextScene = "taskResultScene";
        }
        else
        {
            nextScene = "mainMenuScene";
        }

        thisDoc = GetComponent<UIDocument>();
        if (thisDoc == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: UIDocument is missing on this GameObject.");
            return;
        }

        VisualElement root = thisDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: rootVisualElement is null.");
            return;
        }

        nextSceneButton = root.Q<Button>("nextSceneButton");
        if (nextSceneButton == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: nextSceneButton not found in UXML.");
            return;
        }

        nextSceneButton.RegisterCallback<ClickEvent>(NextSceneScript);

        timerModule = root.Q<Label>("lblTimer");
        if (timerModule == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: lblTimer not found in UXML.");
            return;
        }

        SetUpClothing(gridName: "topsGrid",
            slotList: wardrobeItemList.Instance.wardrobeListItemsChest,
            setItemSlot: "chest",
            setItemUi: "activeTop");
        SetUpClothing(gridName: "bottomsGrid",
            slotList: wardrobeItemList.Instance.wardrobeListItemsBottom,
            setItemSlot: "bottom",
            setItemUi: "activeBottoms");
        SetUpClothing(gridName: "shoesGrid",
            slotList: wardrobeItemList.Instance.wardrobeListItemsShoe,
            setItemSlot: "shoe",
            setItemUi: "activeShoes");
        SetUpClothing(gridName: "jacketsGrid",
            slotList: wardrobeItemList.Instance.wardrobeListItemsJacket,
            setItemSlot: "jacket",
            setItemUi: "activeJackets");

        ApplyDefaultOutfit();
        SyncInitialSelectionVisuals();
    }

    private void Update()
    {
        wardrobeTimer -= Time.deltaTime;
        if (wardrobeTimer < 0f)
        {
            wardrobeTimer = 0f;
        }

        if (timerModule != null)
        {
            timerModule.text = Mathf.CeilToInt(wardrobeTimer).ToString();
        }
    }

    private void OnDisable()
    {
        nextSceneButton?.UnregisterCallback<ClickEvent>(NextSceneScript);
    }

    /// <summary>
    /// Applies the first non-placeholder item per slot so the mannequin matches JSON-driven gear instead of seeded "Nothing" rows.
    /// </summary>
    private void ApplyDefaultOutfit()
    {
        wardrobeItemClothing chestDefault = 
            wardrobeItemList.Instance.GetFirstDisplayableItem(wardrobeItemList.Instance.wardrobeListItemsChest);
        wardrobeItemClothing bottomDefault = 
            wardrobeItemList.Instance.GetFirstDisplayableItem(wardrobeItemList.Instance.wardrobeListItemsBottom);
        wardrobeItemClothing shoeDefault = 
            wardrobeItemList.Instance.GetFirstDisplayableItem(wardrobeItemList.Instance.wardrobeListItemsShoe);
        wardrobeItemClothing jacketDefault = 
            wardrobeItemList.Instance.GetFirstDisplayableItem(wardrobeItemList.Instance.wardrobeListItemsJacket);

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
        if (thisDoc == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: ChangeUISlotSprite called before UIDocument was cached.");
            return;
        }

        VisualElement root = thisDoc.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: rootVisualElement is null.");
            return;
        }

        Image slotUI = root.Q<Image>(elementName);
        if (slotUI == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: UI Image not found: " + elementName);
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
        if (string.IsNullOrEmpty(nextScene))
        {
            Debug.LogError("wardrobeButtonTestScripts: nextScene is not set; check sandbox mode and Build Settings.");
            return;
        }

        SceneManager.LoadScene(nextScene);
    }

    /// <summary>
    /// Wires a ListView to a per-slot clothing list and selection handler.
    /// </summary>
    /// <param name="slotList">Items shown in the list.</param>
    /// <param name="listViewItem">UXML name of the ListView.</param>
    /// <param name="setItemSlot">Slot tag passed to <see cref="wardrobeItemList.SetItemSlot"/>.</param>
    /// <param name="setItemUi">UXML name of the stacked Image to update.</param>
    public void SetUpClothing(string gridName,
        List<wardrobeItemClothing> slotList,
        string setItemSlot,
        string setItemUi)
    {
        if (slotList == null)
        {
            Debug.LogError($"slotList is null for grid {gridName}");
            return;
        }

        VisualElement grid = thisDoc.rootVisualElement.Q<VisualElement>(gridName);
        if (grid == null)
        {
            Debug.LogError($"wardrobeButtonTestScripts: Could not find grid: {gridName}");
            return;
        }

        grid.Clear();

        foreach (wardrobeItemClothing item in slotList)
        {
            Button tileButton = CreateClothingButton(item, gridName, setItemSlot, setItemUi);
            grid.Add(tileButton);
        }
    }

    private Button CreateClothingButton(wardrobeItemClothing item, string gridName, string setItemSlot, string setItemUi)
    {
        Button button = new();
        button.AddToClassList("wardrobe-tile");
        button.userData = item;

        // Optional: remove button text so Unity doesn't reserve weird text space.
        button.text = string.Empty;

        if (item.itemSprite != null)
        {
            VisualElement image = new();
            image.AddToClassList("wardrobe-tile-image");
            image.style.backgroundImage = new StyleBackground(item.itemSprite);
            image.pickingMode = PickingMode.Ignore;
            button.Add(image);
        }
        else // Missing art or placeholder item.
        {
            button.AddToClassList("missing-art");

            Label fallbackLabel = new(item.itemName);
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

    private void UpdateGridSelection(string gridName, Button newlySelectedButton)
    {
        if (selectedButtonsByGrid.TryGetValue(gridName, out Button previousButton) && previousButton != null)
        {
            previousButton.RemoveFromClassList("selected");
        }

        newlySelectedButton.AddToClassList("selected");
        selectedButtonsByGrid[gridName] = newlySelectedButton;
    }

    private void SyncInitialSelectionVisuals()
    {
        SyncSelectionForGrid("topsGrid", wardrobeItemList.Instance.currentItemTop);
        SyncSelectionForGrid("bottomsGrid", wardrobeItemList.Instance.currentItemBottom);
        SyncSelectionForGrid("shoesGrid", wardrobeItemList.Instance.currentItemShoe);
        SyncSelectionForGrid("jacketsGrid", wardrobeItemList.Instance.currentItemJacket);
    }

    private void SyncSelectionForGrid(string gridName, wardrobeItemClothing currentItem)
    {
        if (currentItem == null) return;

        VisualElement grid = thisDoc.rootVisualElement.Q<VisualElement>(gridName);
        if (grid == null) return;

        foreach (VisualElement child in grid.Children())
        {
            if (child is Button button && button.userData is wardrobeItemClothing item && item == currentItem)
            {
                button.AddToClassList("selected");
                selectedButtonsByGrid[gridName] = button;
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
    private void SetCurrentItem(wardrobeItemClothing itemToSet, string listSlot, string slotUi)
    {
        if (itemToSet == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: itemToSet is null for slot " + listSlot);
            return;
        }

        wardrobeItemList.Instance.SetItemSlot(listSlot, itemToSet);
        ChangeUISlotSprite(slotUi, itemToSet.itemSprite);
    }
}
