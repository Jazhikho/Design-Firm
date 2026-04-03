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
    public wardrobeSlotsScript chestSlot;
    public wardrobeSlotsScript bottomSlot;
    public wardrobeSlotsScript shoeSlot;
    public wardrobeSlotsScript jacketSlot;
    public wardrobeItemList itemList;
    //

    void Awake()
    {
        //
        chestSlot = GameObject.Find("chestSlot").GetComponent<wardrobeSlotsScript>();
        bottomSlot = GameObject.Find("bottomSlot").GetComponent<wardrobeSlotsScript>();
        shoeSlot = GameObject.Find("shoeSlot").GetComponent<wardrobeSlotsScript>();
        jacketSlot = GameObject.Find("jacketSlot").GetComponent<wardrobeSlotsScript>();
        itemList = GameObject.Find("ItemsList").GetComponent<wardrobeItemList>();
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
        //RegisterButtons
        nextSceneButton.RegisterCallback<ClickEvent>(nextSceneScript);
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
        SceneManager.LoadScene(nextScene);
    }



    //
    void Start()
    {
        //Not actually sure how to set it up properly so I did this instead.
        setUpClothingChest();
        setUpClothingBottom();
        setUpClothingShoe();
        setUpClothingJacket();
    }
    public void setUpClothingChest()
    {
        Func<VisualElement> makeItem = () => new Image();
        Action<VisualElement, int> bindItem = (e, i) => ((Image)e).sprite = itemList.wardrobeListItemsChest[i].itemSprite;

        ListView topsList = thisDoc.rootVisualElement.Q("topsList") as ListView;
        topsList.makeItem = makeItem;
        topsList.bindItem = bindItem;
        topsList.itemsSource = itemList.wardrobeListItemsChest;
        topsList.selectionType = SelectionType.Single;

        topsList.itemsChosen += (selectedItems) =>
        {
            wardrobeItemClothing selectedClothingItem = selectedItems.Cast<wardrobeItemClothing>().ElementAt(0);
            chestSlot.setCurrentItem(selectedClothingItem);
        };
    }
    public void setUpClothingBottom()
    {
        Func<VisualElement> makeItem = () => new Image();
        Action<VisualElement, int> bindItem = (e, i) => ((Image)e).sprite = itemList.wardrobeListItemsBottom[i].itemSprite;

        ListView topsList = thisDoc.rootVisualElement.Q("bottomsList") as ListView;
        topsList.makeItem = makeItem;
        topsList.bindItem = bindItem;
        topsList.itemsSource = itemList.wardrobeListItemsBottom;
        topsList.selectionType = SelectionType.Single;

        topsList.itemsChosen += (selectedItems) =>
        {
            wardrobeItemClothing selectedClothingItem = selectedItems.Cast<wardrobeItemClothing>().ElementAt(0);
            bottomSlot.setCurrentItem(selectedClothingItem);
        };
    }
    public void setUpClothingShoe()
    {
        Func<VisualElement> makeItem = () => new Image();
        Action<VisualElement, int> bindItem = (e, i) => ((Image)e).sprite = itemList.wardrobeListItemsShoe[i].itemSprite;

        ListView topsList = thisDoc.rootVisualElement.Q("shoesList") as ListView;
        topsList.makeItem = makeItem;
        topsList.bindItem = bindItem;
        topsList.itemsSource = itemList.wardrobeListItemsShoe;
        topsList.selectionType = SelectionType.Single;

        topsList.itemsChosen += (selectedItems) =>
        {
            wardrobeItemClothing selectedClothingItem = selectedItems.Cast<wardrobeItemClothing>().ElementAt(0);
            shoeSlot.setCurrentItem(selectedClothingItem);
        };
    }
    public void setUpClothingJacket()
    {
        Func<VisualElement> makeItem = () => new Image();
        Action<VisualElement, int> bindItem = (e, i) => ((Image)e).sprite = itemList.wardrobeListItemsJacket[i].itemSprite;

        ListView topsList = thisDoc.rootVisualElement.Q("jacketsList") as ListView;
        topsList.makeItem = makeItem;
        topsList.bindItem = bindItem;
        topsList.itemsSource = itemList.wardrobeListItemsJacket;
        topsList.selectionType = SelectionType.Single;

        topsList.itemsChosen += (selectedItems) =>
        {
            wardrobeItemClothing selectedClothingItem = selectedItems.Cast<wardrobeItemClothing>().ElementAt(0);
            jacketSlot.setCurrentItem(selectedClothingItem);
        };
    }


    //

}
