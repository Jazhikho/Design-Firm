using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ResultsController : MonoBehaviour
{
    private Button btnRetry;
    private Button btnMainMenu;
    private Button btnRetryScenario;

    //
    private VisualElement root;
    public temporaryScenario tempScenario = new temporaryScenario();

    public tempScenarioItemTesting currentScenario; //Change this to whatever Markus makes

    public float totalScore;
    //

    public void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        btnRetry = root.Q<Button>("btnRetry");
        btnRetry.clicked += GoToWardrobe;

        btnMainMenu = root.Q<Button>("btnMainMenu");
        btnMainMenu.clicked += GoToMainMenu;

        btnRetryScenario = root.Q<Button>("btnNewScenario");
        btnRetryScenario.clicked += NewScenario;

        //RunSomeThings -ORGANIZE THIS
            tempScenario.tempScenarioRun();
            currentScenario = tempScenario.Scenario;
            tempScenario.debugTempScenario(currentScenario);
            readWardrobeItems();
            readScenario();
        //
    }

    public void OnDisable()
    {
        btnRetry.clicked -= GoToWardrobe;
        btnMainMenu.clicked -= GoToMainMenu;
        btnRetryScenario.clicked -= NewScenario;
    }

    private void GoToWardrobe()
    {
        SceneManager.LoadScene("wardrobeScene"); // TODO: replace scene name with game constant
    }

    private void GoToMainMenu()
    {
        SceneManager.LoadScene("mainMenuScene"); // TODO: replace scene name with game constant
    }

    private void NewScenario()
    {
        SceneManager.LoadScene("taskScenarioScene"); // TODO: replace scene name with game constant
    }

    //Read wardrobe items
    public void readWardrobeItems()
    {
        readItem("lblYourJacketName", "Jacket", wardrobeItemList.Instance.currentItemJacket);
        readItem("lblYourTopName", "Top", wardrobeItemList.Instance.currentItemTop);
        readItem("lblYourBottomsName", "Bottoms", wardrobeItemList.Instance.currentItemBottom);
        readItem("lblYourShoesName", "Shoes", wardrobeItemList.Instance.currentItemShoe);
    }
    public void readItem(string slotLabel, string slotStr, wardrobeItemClothing itemSlot)
    {
        Label itemDisplay = root.Q<Label>(slotLabel);
        itemDisplay.text = "Your " + slotStr + ": " + itemSlot.itemName;
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
        scoreItem("lblJacketScore", wardrobeItemList.Instance.currentItemJacket.ID, "lblFeedbackJacket");
        scoreItem("lblTopScore", wardrobeItemList.Instance.currentItemTop.ID, "lblFeedbackTop");
        scoreItem("lblBottomsScore", wardrobeItemList.Instance.currentItemBottom.ID, "lblFeedbackBottoms");
        scoreItem("lblShoesScore", wardrobeItemList.Instance.currentItemShoe.ID, "lblFeedbackShoes");

        Label totalScoreDispaly;
        totalScoreDispaly = root.Q<Label>("lblTotalScore");
        totalScoreDispaly.text = totalScore.ToString() + "/4.0";
    }
    private void scoreItem(string label, string itemID, string commentaryLabel)
    {
        Label scoreDisplay;
        scoreDisplay = root.Q<Label>(label);

        bool anyFeedback = false;

        foreach (scoredItem scoreItem in currentScenario.scoredItems)
        {
            if (scoreItem.itemID == itemID)
            {
                totalScore += scoreItem.score;
                scoreDisplay.text = scoreItem.score.ToString();
                
                anyFeedback = true;
                feedBackItem(commentaryLabel, scoreItem.commentary);
            }
        }
        if (anyFeedback == false)
        {
            feedBackItem(commentaryLabel, "");
        }
    }
}
