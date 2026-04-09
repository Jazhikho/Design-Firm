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

    public tempScenarioItemTesting CurrentScenario; //Change this to whatever Markus makes

    public float TotalScore;
    //

    /// <summary>
    /// Caches result screen buttons and binds handlers. and runs the wardrobe and scenario reading.
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

        //This debugs and runs the temp Scenario; delete this later.
        tempScenario.tempScenarioRun();
        CurrentScenario = tempScenario.Scenario;
        tempScenario.debugTempScenario(CurrentScenario);
        //
        if (CurrentScenario == null)
        {
            Debug.LogError("ResultsController: There is no CurrentScenario");
            return;
        }
        ReadWardrobeItems();
        ReadScenario();

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

    /// <summary>
    /// uses the ReadItem function to read the current Jacket, Top, Bottom, and Shoe items and then assigns them to UI elements.
    /// </summary>
    public void ReadWardrobeItems()
    {
        ReadItem("lblYourJacketName", "Jacket", WardrobeItemList.Instance.CurrentItemJacket);
        ReadItem("lblYourTopName", "Top", WardrobeItemList.Instance.CurrentItemTop);
        ReadItem("lblYourBottomsName", "Bottoms", WardrobeItemList.Instance.CurrentItemBottom);
        ReadItem("lblYourShoesName", "Shoes", WardrobeItemList.Instance.CurrentItemShoe);
    }

    /// <summary>
    /// The read item function that ReadWardrobeItems uses to set text.
    /// </summary>
    public void ReadItem(string slotLabel, string slotStr, WardrobeItemClothing itemSlot)
    {
        Label itemDisplay = root.Q<Label>(slotLabel);
        if (itemDisplay == null)
        {
            Debug.LogError("ResultsController: Could not find itemDisplay for ReadItem " + slotStr);
            return;
        }
        if (itemSlot == null)
        {
            Debug.LogError("ResultsController: Could not find the item for ReadItem " + slotStr);
            return;
        }
        itemDisplay.text = "Your " + slotStr + ": " + itemSlot.ItemName;
    }
    /// <summary>
    /// Calculates the score of each item and combines them, also provides feedback.
    /// </summary>
    public void ReadScenario()
    {
        Scoring();
        IdealItemsHint();
    }

    /// <summary>
    /// Uses the IdealItemHint function to set the text of the hints to that of the scenario's hints.
    /// </summary>
    public void IdealItemsHint()
    {
        IdealItemHint("lblHintIdealJacket", CurrentScenario.outfitGood.jacketIdeal.commentary);
        IdealItemHint("lblHintIdealTop", CurrentScenario.outfitGood.topIdeal.commentary);
        IdealItemHint("lblHintIdealBottoms", CurrentScenario.outfitGood.bottomsIdeal.commentary);
        IdealItemHint("lblHintIdealShoes", CurrentScenario.outfitGood.shoesIdeal.commentary);
    }

    /// <summary>
    /// The function that idealItemsHint uses to set text
    /// </summary>
    public void IdealItemHint(string label, string commentary)
    {
        Label idealHint;
        idealHint = root.Q<Label>(label);
        if (idealHint == null)
        {
            Debug.LogError("ResultsController: Could not find idealHint label for IdealItemHint");
            return;
        }
        idealHint.text = "Hint: " + commentary;
    }

    /// <summary>
    /// The function that ScoreItem uses to set the fail text for a slot.
    /// </summary>
    public void FeedBackItem(string label, string commentary)
    {
        Label idealHint;
        idealHint = root.Q<Label>(label);
        if (idealHint == null)
        {
            Debug.LogError("ResultsController: Could not find idealHint label for FeedBackItem");
            return;
        }
        idealHint.text = commentary;
    }

    /// <summary>
    /// Calculates the score of each item, assigns relevant text for each item's feedback, and then displays the total score.
    /// </summary>
    public void Scoring()
    {
        ScoreItem("lblJacketScore", WardrobeItemList.Instance.CurrentItemJacket, "lblFeedbackJacket", CurrentScenario.slotFeedback.jacketFeedback);
        ScoreItem("lblTopScore", WardrobeItemList.Instance.CurrentItemTop, "lblFeedbackTop", CurrentScenario.slotFeedback.topFeedback);
        ScoreItem("lblBottomsScore", WardrobeItemList.Instance.CurrentItemBottom, "lblFeedbackBottoms", CurrentScenario.slotFeedback.bottomsFeedback);
        ScoreItem("lblShoesScore", WardrobeItemList.Instance.CurrentItemShoe, "lblFeedbackShoes", CurrentScenario.slotFeedback.shoesFeedback);

        Label totalScoreDispaly;
        totalScoreDispaly = root.Q<Label>("lblTotalScore");

        if (totalScoreDispaly == null)
        {
            Debug.LogError("ResultsController: Could not find totalScoreDisplay label for scoring. Current score is: " + TotalScore.ToString());
            return;
        }
        totalScoreDispaly.text = TotalScore.ToString() + "/4.0";
    }

    /// <summary>
    /// Checks one of the current items ID against each of the items in the scenarios scoreItems and provides the relevant score and feedback for matching items.
    /// </summary>
    private void ScoreItem(string label, WardrobeItemClothing item, string commentaryLabel, string slotFeedback)
    {
        Label scoreDisplay;
        scoreDisplay = root.Q<Label>(label);

        if (scoreDisplay == null)
        {
            Debug.LogError("ResultsController: Could not find scoreDisplay label for ScoreItem: " + item.SlotTag);
            return;
        }
        //This is to help provide the fallback/fail text.
        bool anyFeedback = false;

        foreach (scoredItem ScoreItem in CurrentScenario.scoredItems)
        {
            if (ScoreItem == null)
            {
                Debug.LogError("ResultsController: Null item found in ScoreItem loop: " + item.SlotTag);
                return;
            }
            if (ScoreItem.itemID == item.Id)
            {
                TotalScore += ScoreItem.score;
                scoreDisplay.text = ScoreItem.score.ToString();
                
                anyFeedback = true;
                FeedBackItem(commentaryLabel, ScoreItem.commentary);
            }
        }
        if (anyFeedback == false)
        {
            FeedBackItem(commentaryLabel, slotFeedback);
        }
    }
}
