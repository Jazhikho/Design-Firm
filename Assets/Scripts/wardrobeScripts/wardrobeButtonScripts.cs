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
    //

    void Start()
    {
        //
        chestSlot = GameObject.Find("chestSlot").GetComponent<wardrobeSlotsScript>();
        bottomSlot = GameObject.Find("bottomSlot").GetComponent<wardrobeSlotsScript>();
        shoeSlot = GameObject.Find("shoeSlot").GetComponent<wardrobeSlotsScript>();
        jacketSlot = GameObject.Find("jacketSlot").GetComponent<wardrobeSlotsScript>();
        itemList = GameObject.Find("ItemsList").GetComponent<wardrobeItemList>();

        if (chestSlot == null || bottomSlot == null || shoeSlot == null || jacketSlot == null || itemList == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: Assign all slot references and itemList in the Inspector.");
            return;
        }

        if (itemList.wardrobeListItemsChest.Count < 1)
        {
            Debug.LogError("wardrobeButtonTestScripts: wardrobeListItemsChest is empty.");
            return;
        }
        if (itemList.wardrobeListItemsBottom.Count < 1)
        {
            Debug.LogError("wardrobeButtonTestScripts: wardrobeListItemsBottom is empty.");
            return;
        }
        if (itemList.wardrobeListItemsShoe.Count < 1)
        {
            Debug.LogError("wardrobeButtonTestScripts: wardrobeListItemsShoe is empty.");
            return;
        }
        if (itemList.wardrobeListItemsJacket.Count < 1)
        {
            Debug.LogError("wardrobeButtonTestScripts: wardrobeListItemsJacket is empty.");
            return;
        }
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
        setUpClothing(itemList.wardrobeListItemsChest, chestSlot, "topsList");
        setUpClothing(itemList.wardrobeListItemsBottom, bottomSlot, "bottomsList");
        setUpClothing(itemList.wardrobeListItemsShoe, shoeSlot, "shoesList");
        setUpClothing(itemList.wardrobeListItemsJacket, jacketSlot, "jacketsList");
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

    public void setUpClothing(List<wardrobeItemClothing> slotList, wardrobeSlotsScript slot, string listViewItem)
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
            slot.setCurrentItem(selectedClothingItem);
        };
    }

}
