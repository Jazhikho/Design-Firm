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
    public string currentSlot = "shirts";

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

    /// <summary>
    /// Sets the chest slot to the second chest item when available (test harness).
    /// </summary>
    /// <param name="evt">The click event from the Next Shirt button.</param>
    public void nextShirtButtonClick(ClickEvent evt)
    {
        if (chestSlot == null || itemList == null)
        {
            Debug.LogError("wardrobeButtonTestScripts: Assign chestSlot and itemList in the Inspector.");
            return;
        }

        if (itemList.wardrobeListItemsChest.Count < 2)
        {
            Debug.LogError("wardrobeButtonTestScripts: wardrobeListItemsChest needs at least two entries for this test.");
            return;
        }

        chestSlot.setCurrentItem(itemList.wardrobeListItemsChest[1]);
        bottomSlot.setCurrentItem(itemList.wardrobeListItemsBottom[1]);
        shoeSlot.setCurrentItem(itemList.wardrobeListItemsShoe[1]);
    }
}
