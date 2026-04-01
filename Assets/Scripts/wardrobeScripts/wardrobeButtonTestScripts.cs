using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class wardrobeButtonTestScripts : MonoBehaviour
{
    private UIDocument thisDoc;
    private Button testButton;
    private Button nextSceneButton;
    public string currentSlot = "shirt";

// nextScene will probably always be "mainMenuScene" or "taskResultScene"
    public String nextScene = "mainMenuScene";

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

        thisDoc = GetComponent<UIDocument>();
        testButton = thisDoc.rootVisualElement.Q("nextShirtTemp") as Button;
        nextSceneButton = thisDoc.rootVisualElement.Q("nextSceneButton") as Button;
        testButton.RegisterCallback<ClickEvent>(nextShirtButtonClick);
        nextSceneButton.RegisterCallback<ClickEvent>(nextSceneScript);

    }

    void OnDisable()
    {
        testButton.UnregisterCallback<ClickEvent>(nextShirtButtonClick);
        nextSceneButton.UnregisterCallback<ClickEvent>(nextSceneScript);
    }

    public void nextSceneScript(ClickEvent evt)
    {
        //This is for later when build settings get set up.
        //SceneManager.LoadScene(nextScene);
        Debug.Log("Not yet implemented");
    }
    public void nextShirtButtonClick(ClickEvent evt)
    {
        chestSlot.setCurrentItem(itemList.wardrobeListItemsChest[1]);
    }
}
