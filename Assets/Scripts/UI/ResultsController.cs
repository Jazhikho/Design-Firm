using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class ResultsController : MonoBehaviour
{
    private Button _retryButton;
    private Button _retryScenarioButton;

    private VisualElement _root;
    private readonly TemporaryScenario _temporaryScenario = new TemporaryScenario();
    private TempScenarioItemTesting _currentScenario;
    private float _totalScore;

    /// <summary>
    /// Caches result buttons, runs TEMP scenario fill, then binds wardrobe and scenario text to the UI.
    /// </summary>
    private void OnEnable()
    {
        _root = GetComponent<UIDocument>().rootVisualElement;

        _retryButton = _root.Q<Button>("btnRetry");
        _retryScenarioButton = _root.Q<Button>("btnNewScenario");
        if (_retryButton == null)
        {
            Debug.LogError("ResultsController: btnRetry not found in UXML.");
            return;
        }


        if (_retryScenarioButton == null)
        {
            Debug.LogError("ResultsController: btnNewScenario not found in UXML.");
            return;
        }

        _retryButton.clicked += GoToWardrobe;
        _retryScenarioButton.clicked += NewScenario;

        _totalScore = 0f;
        _temporaryScenario.RunTempScenario();
        _currentScenario = _temporaryScenario.Scenario;
#if UNITY_EDITOR
        _temporaryScenario.LogTempScenarioDebug(_currentScenario);
#endif
        if (_currentScenario == null)
        {
            Debug.LogError("ResultsController: There is no current scenario.");
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
    /// Loads another scenario prompt.
    /// </summary>
    private void NewScenario()
    {
        SceneManager.LoadScene(GameConstants.TaskScenarioScene);
    }

    /// <summary>
    /// Sets label text for each equipped wardrobe slot.
    /// </summary>
    public void ReadWardrobeItems()
    {
        ReadItem("lblYourJacketName", "Jacket", WardrobeItemList.Instance.CurrentItemJacket);
        ReadItem("lblYourTopName", "Top", WardrobeItemList.Instance.CurrentItemTop);
        ReadItem("lblYourBottomsName", "Bottoms", WardrobeItemList.Instance.CurrentItemBottom);
        ReadItem("lblYourShoesName", "Shoes", WardrobeItemList.Instance.CurrentItemShoe);
    }

    /// <summary>
    /// Sets one outfit line label from a wardrobe item.
    /// </summary>
    /// <param name="slotLabel">UXML name of the label.</param>
    /// <param name="slotStr">Human-readable slot name for copy.</param>
    /// <param name="itemSlot">Equipped item, if any.</param>
    public void ReadItem(string slotLabel, string slotStr, WardrobeItemClothing itemSlot)
    {
        Label itemDisplay = _root.Q<Label>(slotLabel);
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
    /// Runs scoring and ideal-hint UI pass for the current scenario.
    /// </summary>
    public void ReadScenario()
    {
        Scoring();
        IdealItemsHint();
    }

    /// <summary>
    /// Fills ideal-item hint labels from the scenario.
    /// </summary>
    public void IdealItemsHint()
    {
        if (_currentScenario == null || _currentScenario.OutfitGood == null)
        {
            Debug.LogError("ResultsController: IdealItemsHint missing scenario or OutfitGood.");
            return;
        }

        IdealOutfit outfit = _currentScenario.OutfitGood;
        IdealItemHint("lblHintIdealJacket", outfit.JacketIdeal);
        IdealItemHint("lblHintIdealTop", outfit.TopIdeal);
        IdealItemHint("lblHintIdealBottoms", outfit.BottomsIdeal);
        IdealItemHint("lblHintIdealShoes", outfit.ShoesIdeal);
    }

    /// <summary>
    /// Sets hint text for one ideal slot when data exists.
    /// </summary>
    /// <param name="label">UXML label name.</param>
    /// <param name="ideal">Ideal row, may be null.</param>
    public void IdealItemHint(string label, IdealOutfitItem ideal)
    {
        Label hintLabel = _root.Q<Label>(label);
        if (hintLabel == null)
        {
            Debug.LogError("ResultsController: Could not find label for IdealItemHint.");
            return;
        }

        if (ideal == null || string.IsNullOrEmpty(ideal.Commentary))
        {
            hintLabel.text = "Hint: ";
            return;
        }

        hintLabel.text = "Hint: " + ideal.Commentary;
    }

    /// <summary>
    /// Sets feedback label text for a slot.
    /// </summary>
    public void FeedBackItem(string label, string commentary)
    {
        Label feedbackLabel = _root.Q<Label>(label);
        if (feedbackLabel == null)
        {
            Debug.LogError("ResultsController: Could not find label for FeedBackItem.");
            return;
        }

        if (commentary == null)
        {
            commentary = string.Empty;
        }

        feedbackLabel.text = commentary;
    }

    /// <summary>
    /// Scores each slot against scenario rows and shows total.
    /// </summary>
    public void Scoring()
    {
        if (_currentScenario == null || _currentScenario.SlotFeedback == null)
        {
            Debug.LogError("ResultsController: Scoring requires scenario and SlotFeedback.");
            return;
        }

        IncorrectSlotFeedback fb = _currentScenario.SlotFeedback;
        ScoreItem("lblJacketScore", WardrobeItemList.Instance.CurrentItemJacket, "lblFeedbackJacket", fb.JacketFeedback);
        ScoreItem("lblTopScore", WardrobeItemList.Instance.CurrentItemTop, "lblFeedbackTop", fb.TopFeedback);
        ScoreItem("lblBottomsScore", WardrobeItemList.Instance.CurrentItemBottom, "lblFeedbackBottoms", fb.BottomsFeedback);
        ScoreItem("lblShoesScore", WardrobeItemList.Instance.CurrentItemShoe, "lblFeedbackShoes", fb.ShoesFeedback);

        Label totalScoreDisplay = _root.Q<Label>("lblTotalScore");
        if (totalScoreDisplay == null)
        {
            Debug.LogError("ResultsController: Could not find lblTotalScore. Current score is: " + _totalScore.ToString());
            return;
        }

        totalScoreDisplay.text = _totalScore.ToString() + "/4.0";
    }

    /// <summary>
    /// Matches one equipped item to scored rows; otherwise shows slot fallback feedback.
    /// </summary>
    private void ScoreItem(string label, WardrobeItemClothing item, string commentaryLabel, string slotFeedback)
    {
        Label scoreDisplay = _root.Q<Label>(label);
        if (scoreDisplay == null)
        {
            string slotTagForLog = item != null ? item.SlotTag : "unknown";
            Debug.LogError("ResultsController: Could not find scoreDisplay label for ScoreItem: " + slotTagForLog);
            return;
        }

        if (item == null)
        {
            scoreDisplay.text = "0";
            FeedBackItem(commentaryLabel, slotFeedback);
            return;
        }

        if (_currentScenario == null || _currentScenario.ScoredItems == null)
        {
            Debug.LogError("ResultsController: ScoreItem requires ScoredItems list.");
            scoreDisplay.text = "0";
            FeedBackItem(commentaryLabel, slotFeedback);
            return;
        }

        bool anyFeedback = false;
        foreach (ScoredItem scoredRow in _currentScenario.ScoredItems)
        {
            if (scoredRow == null)
            {
                Debug.LogError("ResultsController: Null entry in ScoredItems for slot " + item.SlotTag);
                continue;
            }

            if (scoredRow.ItemId == item.Id)
            {
                _totalScore += scoredRow.Score;
                scoreDisplay.text = scoredRow.Score.ToString();
                anyFeedback = true;
                FeedBackItem(commentaryLabel, scoredRow.Commentary);
            }
        }

        if (anyFeedback == false)
        {
            FeedBackItem(commentaryLabel, slotFeedback);
        }
    }
}
