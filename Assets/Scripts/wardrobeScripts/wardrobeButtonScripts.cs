using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Binds temporary wardrobe UI Toolkit controls to slot scripts and the shared item list for class testing.
/// </summary>
public class wardrobeButtonTestScripts : MonoBehaviour
{
    private UIDocument thisDoc;
    private Button testButton;
    private Button nextSceneButton;

// nextScene will probably always be "mainMenuScene" or "taskResultScene"
    public string nextScene;
    public bool sandboxMode;

    //
    /*
    [SerializeField]
    private wardrobeSlotsScript chestSlot;

    [SerializeField]
    private wardrobeSlotsScript bottomSlot;

    [SerializeField]
    private wardrobeSlotsScript shoeSlot;

    [SerializeField]
    private wardrobeSlotsScript jacketSlot;

    [SerializeField]
    private wardrobeItemList itemList;
    */
    //


    //WardrobeSlotPort

    //

    void Start()
    {

        //
        if (sandboxMode == false)
        {
            nextScene = "taskResultScene";
        }
        else
        {
            nextScene = "mainMenuScene";
        }
        //

        thisDoc = GetComponent<UIDocument>();

        //Buttons
        nextSceneButton = thisDoc.rootVisualElement.Q("nextSceneButton") as Button;
        if (nextSceneButton == null)
        {
            Debug.Log("wardrobeButtonTestScripts: Cannot Find nextSceneButton");
            return;
        }
        //RegisterButtons
        nextSceneButton.RegisterCallback<ClickEvent>(nextSceneScript);
        //

        //
        setUpClothing(wardrobeItemList.wardrobeListItemsChest, "topsList", "chest", "activeTop");
        setUpClothing(wardrobeItemList.wardrobeListItemsBottom, "bottomsList", "bottom", "activeBottoms");
        setUpClothing(wardrobeItemList.wardrobeListItemsShoe, "shoesList", "shoe", "activeShoes");
        setUpClothing(wardrobeItemList.wardrobeListItemsJacket, "jacketsList", "jacket", "activeJackets");

        setCurrentItem(wardrobeItemList.wardrobeListItemsChest[0], "chest", "activeTop");
        setCurrentItem(wardrobeItemList.wardrobeListItemsBottom[0], "bottom", "activeBottoms");
        setCurrentItem(wardrobeItemList.wardrobeListItemsShoe[0], "shoe", "activeShoes");
        setCurrentItem(wardrobeItemList.wardrobeListItemsJacket[0], "jacket", "activeJackets");
        //

    }

    public void changeUISlotSprite(string elementName, Sprite spriteUI)
    {
        Image slotUI = thisDoc.rootVisualElement.Q(elementName) as Image;
        slotUI.style.backgroundImage = new StyleBackground(spriteUI);
    }

    void OnDisable()
    {
        nextSceneButton.UnregisterCallback<ClickEvent>(nextSceneScript);
    }

    public void nextSceneScript(ClickEvent evt)
    {
        if (nextScene == null)
        {
            Debug.Log("wardrobeButtonTestScripts: Could not find next scene");
            return;
        }
        SceneManager.LoadScene(nextScene);
    }

    public void setUpClothing(List<wardrobeItemClothing> slotList, string listViewItem, string setItemSlot, string setItemUi)
    {
        Func<VisualElement> makeItem = () => new Image();
        Action<VisualElement, int> bindItem = (e, i) => ((Image)e).sprite = slotList[i].itemSprite;

        ListView topsList = thisDoc.rootVisualElement.Q(listViewItem) as ListView;
        if (topsList == null || listViewItem == null)
        {
            Debug.Log("wardrobeButtonTestScripts: could not get listView: " + listViewItem);
            return;
        }
        topsList.makeItem = makeItem;
        topsList.bindItem = bindItem;
        topsList.itemsSource = slotList;
        topsList.selectionType = SelectionType.Single;

        topsList.itemsChosen += (selectedItems) =>
        {
            wardrobeItemClothing selectedClothingItem = selectedItems.Cast<wardrobeItemClothing>().ElementAt(0);
            setCurrentItem(selectedClothingItem, setItemSlot, setItemUi);
        };
    }

    public void setCurrentItem(wardrobeItemClothing itemToSet, string listSlot, string slotUI)
    {
        
        if (itemToSet == null)
        {
            Debug.LogError("wardrobeSlotsScript: itemToSet is null for slot " + listSlot);
            return;
        }


        wardrobeItemList.setItemSlot(listSlot, itemToSet);
        changeUISlotSprite(slotUI, itemToSet.itemSprite);
        //
    }

}
