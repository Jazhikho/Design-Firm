using Assets.Scripts.Core;
using Assets.Scripts.Data;
using Assets.Scripts.UI;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class ResultsControllerPlayModeTests
{
    private const string UxmlPath = "Assets/UI/UXML/ResultsUI.uxml";

    private GameObject _go;
    private ResultsController _controller;
    private VisualElement _root;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        _go = new GameObject("ResultsControllerTest");
        _go.SetActive(false); // prevent OnEnable from firing during AddComponent

        _go.AddComponent<UIDocument>(); // required by [RequireComponent]
        _controller = _go.AddComponent<ResultsController>();

        // Clone the real UXML into an in-memory tree — no PanelSettings needed.
        // Tests break if a label is renamed in the UXML, which is the desired behaviour.
        var visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
        _root = new VisualElement();
        visualTreeAsset.CloneTree(_root);

        typeof(ResultsController)
            .GetField("_root", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(_controller, _root);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        ScenarioState.Instance.ActiveScenario    = null;
        WardrobeState.Instance.CurrentItemJacket = null;
        WardrobeState.Instance.CurrentItemTop    = null;
        WardrobeState.Instance.CurrentItemBottom = null;
        WardrobeState.Instance.CurrentItemShoe   = null;
        Object.Destroy(_go);
        yield return null;
    }

    private Scenario MakeScenario() => new()
    {
        name        = "Test Scenario",
        description = "Test",
        scoredItems = new List<ScoredItem>(),
        incorrectSlotFeedback = new IncorrectSlotFeedback
        {
            jacket  = "Try a different jacket.",
            top     = "Try a different top.",
            bottoms = "Try different bottoms.",
            shoes   = "Try different shoes."
        },
        idealOutfit = new IdealOutfit
        {
            jacket  = new IdealOutfitItem { itemId = "jacket_ideal",  commentary = "Great jacket choice." },
            top     = new IdealOutfitItem { itemId = "top_ideal",     commentary = "Great top choice." },
            bottoms = new IdealOutfitItem { itemId = "bottoms_ideal", commentary = "Great bottoms choice." },
            shoes   = new IdealOutfitItem { itemId = "shoes_ideal",   commentary = "Great shoes choice." }
        }
    };

    /// Jacket wrong (0.0), top/bottoms/shoes nothing+null ideal (1.0 each) → "3.0/4.0"
    [UnityTest]
    public IEnumerator Scoring_UpdatesTotalScoreLabel()
    {
        Scenario scenario = MakeScenario();
        scenario.idealOutfit.top     = null;
        scenario.idealOutfit.bottoms = null;
        scenario.idealOutfit.shoes   = null;
        ScenarioState.Instance.ActiveScenario = scenario;

        WardrobeState.Instance.CurrentItemJacket = new WardrobeItem { id = "jacket_wrong",   slot = "jacket" };
        WardrobeState.Instance.CurrentItemTop    = new WardrobeItem { id = "nothing_top",     slot = "top" };
        WardrobeState.Instance.CurrentItemBottom = new WardrobeItem { id = "nothing_bottoms", slot = "bottoms" };
        WardrobeState.Instance.CurrentItemShoe   = new WardrobeItem { id = "nothing_shoes",   slot = "shoes" };

        _controller.Scoring();
        yield return null;

        Assert.That(_root.Q<Label>("lblTotalScore").text, Is.EqualTo("3.0/4.0"));
    }

    /// All 4 slots match ideal → "4.0/4.0"
    [UnityTest]
    public IEnumerator Scoring_UpdatesTotalScoreLabel_When_AllSelectionsAreIdeal()
    {
        ScenarioState.Instance.ActiveScenario = MakeScenario();

        WardrobeState.Instance.CurrentItemJacket = new WardrobeItem { id = "jacket_ideal",  slot = "jacket" };
        WardrobeState.Instance.CurrentItemTop    = new WardrobeItem { id = "top_ideal",     slot = "top" };
        WardrobeState.Instance.CurrentItemBottom = new WardrobeItem { id = "bottoms_ideal", slot = "bottoms" };
        WardrobeState.Instance.CurrentItemShoe   = new WardrobeItem { id = "shoes_ideal",   slot = "shoes" };

        _controller.Scoring();
        yield return null;

        Assert.That(_root.Q<Label>("lblTotalScore").text, Is.EqualTo("4.0/4.0"));
    }

    /// Top slot item is in scoredItems → partial-credit commentary written to lblFeedbackTop
    [UnityTest]
    public IEnumerator Scoring_WritesPartialCreditFeedback_When_ScoredItemMatches()
    {
        Scenario scenario = MakeScenario();
        scenario.idealOutfit.jacket  = null;
        scenario.idealOutfit.bottoms = null;
        scenario.idealOutfit.shoes   = null;
        scenario.scoredItems.Add(new ScoredItem
        {
            itemId     = "top_02",
            score      = 0.5f,
            commentary = "Close, but not ideal."
        });
        ScenarioState.Instance.ActiveScenario = scenario;

        WardrobeState.Instance.CurrentItemJacket = new WardrobeItem { id = "nothing_jacket", slot = "jacket" };
        WardrobeState.Instance.CurrentItemTop    = new WardrobeItem { id = "top_02",          slot = "top" };
        WardrobeState.Instance.CurrentItemBottom = new WardrobeItem { id = "nothing_bottoms", slot = "bottoms" };
        WardrobeState.Instance.CurrentItemShoe   = new WardrobeItem { id = "nothing_shoes",   slot = "shoes" };

        _controller.Scoring();
        yield return null;

        Assert.That(_root.Q<Label>("lblFeedbackTop").text, Is.EqualTo("Close, but not ideal."));
    }

    /// Jacket wrong, not in scoredItems → incorrectSlotFeedback.jacket written to lblFeedbackJacket
    [UnityTest]
    public IEnumerator Scoring_WritesFallbackFeedback_When_NoMatchExists()
    {
        Scenario scenario = MakeScenario();
        scenario.idealOutfit.top     = null;
        scenario.idealOutfit.bottoms = null;
        scenario.idealOutfit.shoes   = null;
        ScenarioState.Instance.ActiveScenario = scenario;

        WardrobeState.Instance.CurrentItemJacket = new WardrobeItem { id = "jacket_wrong",   slot = "jacket" };
        WardrobeState.Instance.CurrentItemTop    = new WardrobeItem { id = "nothing_top",     slot = "top" };
        WardrobeState.Instance.CurrentItemBottom = new WardrobeItem { id = "nothing_bottoms", slot = "bottoms" };
        WardrobeState.Instance.CurrentItemShoe   = new WardrobeItem { id = "nothing_shoes",   slot = "shoes" };

        _controller.Scoring();
        yield return null;

        Assert.That(_root.Q<Label>("lblFeedbackJacket").text, Is.EqualTo("Try a different jacket."));
    }

    /// ActiveScenario null → warning logged, lblTotalScore is not overwritten
    [UnityTest]
    public IEnumerator Scoring_DoesNothingOrWarns_When_ActiveScenarioIsNull()
    {
        ScenarioState.Instance.ActiveScenario = null;
        Label lblTotalScore = _root.Q<Label>("lblTotalScore");
        lblTotalScore.text = "SENTINEL"; // prove Scoring() does not touch the label

        LogAssert.Expect(LogType.Warning, "ResultsController: Scoring requires scenario.");

        _controller.Scoring();
        yield return null;

        Assert.That(lblTotalScore.text, Is.EqualTo("SENTINEL"));
    }
}
