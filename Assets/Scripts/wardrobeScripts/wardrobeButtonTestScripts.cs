using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Binds temporary wardrobe UI Toolkit controls to slot scripts and the shared item list for class testing.
/// </summary>
public class wardrobeButtonTestScripts : MonoBehaviour
{
    private UIDocument thisDoc;
    private Button testButton;
    private Button nextSceneButton;

    public string currentSlot = "shirt";
    public string nextScene = "mainMenuScene";

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

    /// <summary>
    /// Resolves UI elements and registers click callbacks.
    /// </summary>
    private void Awake()
    {
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

        testButton = root.Q("nextShirtTemp") as Button;
        nextSceneButton = root.Q("nextSceneButton") as Button;

        if (testButton != null)
        {
            testButton.RegisterCallback<ClickEvent>(nextShirtButtonClick);
        }
        else
        {
            Debug.LogError("wardrobeButtonTestScripts: nextShirtTemp button not found in UXML.");
        }

        if (nextSceneButton != null)
        {
            nextSceneButton.RegisterCallback<ClickEvent>(nextSceneScript);
        }
        else
        {
            Debug.LogError("wardrobeButtonTestScripts: nextSceneButton not found in UXML.");
        }
    }

    /// <summary>
    /// Unregisters UI callbacks when this behaviour is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (testButton != null)
        {
            testButton.UnregisterCallback<ClickEvent>(nextShirtButtonClick);
        }
        if (nextSceneButton != null)
        {
            nextSceneButton.UnregisterCallback<ClickEvent>(nextSceneScript);
        }
    }

    /// <summary>
    /// Placeholder for navigating to the next scene once build settings exist.
    /// </summary>
    /// <param name="evt">The click event from the Proceed button.</param>
    public void nextSceneScript(ClickEvent evt)
    {
        Debug.Log("wardrobeButtonTestScripts: Scene load not yet implemented. Target scene name: " + nextScene);
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
    }
}
