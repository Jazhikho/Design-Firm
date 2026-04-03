using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Binds wardrobe UI Toolkit ListViews and stacked avatar images to the static <see cref="wardrobeItemList"/> data.
/// Runs after <see cref="wardrobeManagerScript"/> so JSON items exist before list wiring and defaults apply.
/// </summary>
[RequireComponent(typeof(UIDocument))]
[DefaultExecutionOrder(0)]
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

    /// <summary>
    /// Optional countdown label; assign a named Label from UXML in the Inspector when the timer UI is restored.
    /// </summary>
    [SerializeField]
    private Label timerModule;

    [SerializeField]
    private float wardrobeTimer = 60f;

    private void Start()
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

        nextSceneButton = root.Q("nextSceneButton") as Button;
        if (nextSceneButton == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: nextSceneButton not found in UXML.");
            return;
        }

        nextSceneButton.RegisterCallback<ClickEvent>(NextSceneScript);

        SetUpClothing(wardrobeItemList.wardrobeListItemsChest, "topsList", "chest", "activeTop");
        SetUpClothing(wardrobeItemList.wardrobeListItemsBottom, "bottomsList", "bottom", "activeBottoms");
        SetUpClothing(wardrobeItemList.wardrobeListItemsShoe, "shoesList", "shoe", "activeShoes");
        SetUpClothing(wardrobeItemList.wardrobeListItemsJacket, "jacketsList", "jacket", "activeJackets");

        ApplyDefaultOutfit();
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
        if (nextSceneButton != null)
        {
            nextSceneButton.UnregisterCallback<ClickEvent>(NextSceneScript);
        }
    }

    /// <summary>
    /// Applies the first non-placeholder item per slot so the mannequin matches JSON-driven gear instead of seeded "Nothing" rows.
    /// </summary>
    private void ApplyDefaultOutfit()
    {
        wardrobeItemClothing chestDefault = wardrobeItemList.GetFirstDisplayableItem(wardrobeItemList.wardrobeListItemsChest);
        wardrobeItemClothing bottomDefault = wardrobeItemList.GetFirstDisplayableItem(wardrobeItemList.wardrobeListItemsBottom);
        wardrobeItemClothing shoeDefault = wardrobeItemList.GetFirstDisplayableItem(wardrobeItemList.wardrobeListItemsShoe);
        wardrobeItemClothing jacketDefault = wardrobeItemList.GetFirstDisplayableItem(wardrobeItemList.wardrobeListItemsJacket);

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

        Image slotUI = root.Q(elementName) as Image;
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
    public void SetUpClothing(List<wardrobeItemClothing> slotList, string listViewItem, string setItemSlot, string setItemUi)
    {
        if (slotList == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: slotList is null for ListView " + listViewItem);
            return;
        }

        if (string.IsNullOrEmpty(listViewItem))
        {
            Debug.LogError("wardrobeButtonTestScripts: listViewItem name is null or empty.");
            return;
        }

        Func<VisualElement> makeItem = () => new Image();
        Action<VisualElement, int> bindItem = (e, i) =>
        {
            if (i < 0 || i >= slotList.Count)
            {
                Debug.LogError("wardrobeButtonTestScripts: ListView bind index out of range for " + listViewItem);
                return;
            }

            ((Image)e).sprite = slotList[i].itemSprite;
        };

        ListView listView = thisDoc.rootVisualElement.Q(listViewItem) as ListView;
        if (listView == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: Could not get ListView: " + listViewItem);
            return;
        }

        listView.makeItem = makeItem;
        listView.bindItem = bindItem;
        listView.itemsSource = slotList;
        listView.selectionType = SelectionType.Single;

        listView.itemsChosen += (selectedItems) =>
        {
            if (selectedItems == null)
            {
                return;
            }

            wardrobeItemClothing selectedClothingItem = selectedItems.OfType<wardrobeItemClothing>().FirstOrDefault();
            if (selectedClothingItem == null)
            {
                return;
            }

            SetCurrentItem(selectedClothingItem, setItemSlot, setItemUi);
        };
    }

    /// <summary>
    /// Updates static slot state and the stacked avatar image for one layer.
    /// </summary>
    /// <param name="itemToSet">Clothing row to apply.</param>
    /// <param name="listSlot">Slot tag: chest, bottom, shoe, or jacket.</param>
    /// <param name="slotUi">UXML Image name for that layer.</param>
    public void SetCurrentItem(wardrobeItemClothing itemToSet, string listSlot, string slotUi)
    {
        if (itemToSet == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: itemToSet is null for slot " + listSlot);
            return;
        }

        wardrobeItemList.SetItemSlot(listSlot, itemToSet);
        ChangeUISlotSprite(slotUi, itemToSet.itemSprite);
    }
}
