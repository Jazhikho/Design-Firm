using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ResultsController : MonoBehaviour
{
    private Button _retryButton;
    private Button _mainMenuButton;
    private Button _retryScenarioButton;

    //
    private VisualElement root;
    public temporaryScenario tempScenario = new temporaryScenario();

    public tempScenarioItemTesting currentScenario; //Change this to whatever Markus makes

    public float totalScore;
    //

    /// <summary>
    /// Caches result screen buttons and binds handlers.
    /// </summary>
    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        _retryButton = root.Q<Button>("btnRetry");
        _mainMenuButton = root.Q<Button>("btnMainMenu");
        _retryScenarioButton = root.Q<Button>("btnNewScenario");
        if (_retryButton == null)
        {
            Debug.LogError("ResultsController: btnRetry not found in UXML.");
            return;
        }

        if (_mainMenuButton == null)
        {
            Debug.LogError("ResultsController: btnMainMenu not found in UXML.");
            return;
        }

        if (_retryScenarioButton == null)
        {
            Debug.LogError("ResultsController: btnNewScenario not found in UXML.");
            return;
        }

        _retryButton.clicked += GoToWardrobe;
        _mainMenuButton.clicked += GoToMainMenu;
        _retryScenarioButton.clicked += NewScenario;

        //RunSomeThings -ORGANIZE THIS
            tempScenario.tempScenarioRun();
            currentScenario = tempScenario.Scenario;
            tempScenario.debugTempScenario(currentScenario);
            readWardrobeItems();
            readScenario();
        //
    }

    /// <summary>
    /// Unbinds result screen button handlers.
    /// </summary>
    private void OnDisable()
    {
        if (_retryButton != null)
        {
            _retryButton.clicked -= GoToWardrobe;
        }

        if (_mainMenuButton != null)
        {
            _mainMenuButton.clicked -= GoToMainMenu;
        }

        if (_retryScenarioButton != null)
        {
            _retryScenarioButton.clicked -= NewScenario;
        }
    }

    /// <summary>
    /// Loads the wardrobe scene for another outfit attempt.
    /// </summary>
    private void GoToWardrobe()
    {
        SceneManager.LoadScene(GameConstants.WardrobeScene);
    }

    /// <summary>
    /// Returns to the main menu scene.
    /// </summary>
    private void GoToMainMenu()
    {
        SceneManager.LoadScene(GameConstants.MainMenuScene);
    }

    /// <summary>
    /// Loads another scenario prompt.
    /// </summary>
    private void NewScenario()
    {
        SceneManager.LoadScene(GameConstants.TaskScenarioScene);
    }

    //Read wardrobe items
    public void readWardrobeItems()
    {
        readItem("lblYourJacketName", "Jacket", WardrobeItemList.Instance.CurrentItemJacket);
        readItem("lblYourTopName", "Top", WardrobeItemList.Instance.CurrentItemTop);
        readItem("lblYourBottomsName", "Bottoms", WardrobeItemList.Instance.CurrentItemBottom);
        readItem("lblYourShoesName", "Shoes", WardrobeItemList.Instance.CurrentItemShoe);
    }
    public void readItem(string slotLabel, string slotStr, WardrobeItemClothing itemSlot)
    {
        Label itemDisplay = root.Q<Label>(slotLabel);
        itemDisplay.text = "Your " + slotStr + ": " + itemSlot.ItemName;
    }
    //Read Scenario stuffs
    public void readScenario()
    {
        scoring();
        idealItemsHint();
    }
    public void idealItemsHint() //Clean this up
    {
        idealItemHint("lblHintIdealJacket", currentScenario.outfitGood.jacketIdeal.commentary);
        idealItemHint("lblHintIdealTop", currentScenario.outfitGood.topIdeal.commentary);
        idealItemHint("lblHintIdealBottoms", currentScenario.outfitGood.bottomsIdeal.commentary);
        idealItemHint("lblHintIdealShoes", currentScenario.outfitGood.shoesIdeal.commentary);
    }
    public void idealItemHint(string label, string commentary)
    {
        Label idealHint;
        idealHint = root.Q<Label>(label);
        idealHint.text = "Hint: " + commentary;
    }
    public void feedBackItem(string label, string commentary)
    {
        Label idealHint;
        idealHint = root.Q<Label>(label);

        idealHint.text = commentary;
    }
    public void scoring()
    {
        scoreItem("lblJacketScore", WardrobeItemList.Instance.CurrentItemJacket, "lblFeedbackJacket", currentScenario.slotFeedback.jacketFeedback);
        scoreItem("lblTopScore", WardrobeItemList.Instance.CurrentItemTop, "lblFeedbackTop", currentScenario.slotFeedback.topFeedback);
        scoreItem("lblBottomsScore", WardrobeItemList.Instance.CurrentItemBottom, "lblFeedbackBottoms", currentScenario.slotFeedback.bottomsFeedback);
        scoreItem("lblShoesScore", WardrobeItemList.Instance.CurrentItemShoe, "lblFeedbackShoes", currentScenario.slotFeedback.shoesFeedback);

        Label totalScoreDispaly;
        totalScoreDispaly = root.Q<Label>("lblTotalScore");
        totalScoreDispaly.text = totalScore.ToString() + "/4.0";
    }
    private void scoreItem(string label, WardrobeItemClothing item, string commentaryLabel, string slotFeedback)
    {
        Label scoreDisplay;
        scoreDisplay = root.Q<Label>(label);

        bool anyFeedback = false;

        foreach (scoredItem scoreItem in currentScenario.scoredItems)
        {
            if (scoreItem.itemID == item.Id)
            {
                totalScore += scoreItem.score;
                scoreDisplay.text = scoreItem.score.ToString();
                
                anyFeedback = true;
                feedBackItem(commentaryLabel, scoreItem.commentary);
            }
        }
        if (anyFeedback == false)
        {
            feedBackItem(commentaryLabel, slotFeedback);
        }
    }
}
