using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class wardrobeButtonTestScripts : MonoBehaviour
{
    private UIDocument thisDoc;
    private Button testButton;
    private Button nextSceneButton;
    public string currentSlot = "shirts";

// nextScene will probably always be "mainMenuScene" or "taskResultScene"
    public String nextScene;
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
        testButton = thisDoc.rootVisualElement.Q("nextShirtTemp") as Button;
        nextSceneButton = thisDoc.rootVisualElement.Q("nextSceneButton") as Button;
        //RegisterButtons
        testButton.RegisterCallback<ClickEvent>(nextShirtButtonClick);
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
        testButton.UnregisterCallback<ClickEvent>(nextShirtButtonClick);
        nextSceneButton.UnregisterCallback<ClickEvent>(nextSceneScript);
    }

    public void nextSceneScript(ClickEvent evt)
    {
        SceneManager.LoadScene(nextScene);
    }
    public void nextShirtButtonClick(ClickEvent evt)
    {
        chestSlot.setCurrentItem(itemList.wardrobeListItemsChest[1]);
        bottomSlot.setCurrentItem(itemList.wardrobeListItemsBottom[1]);
        shoeSlot.setCurrentItem(itemList.wardrobeListItemsShoe[1]);
    }
}
