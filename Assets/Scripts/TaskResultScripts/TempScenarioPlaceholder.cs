using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TEMP: Hard-coded scenario data until real scenario JSON is loaded in-game. Remove when replaced.
/// </summary>
public class TempScenarioItemTesting
{
    public string Name;
    public string Desc;
    public string Era;
    public Sprite Illustration;
    public List<ScoredItem> ScoredItems;
    public IncorrectSlotFeedback SlotFeedback;
    public IdealOutfit OutfitGood;
}

public class ScoredItem
{
    public string ItemId;
    public float Score;
    public string Commentary;
}

public class IncorrectSlotFeedback
{
    public string JacketFeedback;
    public string TopFeedback;
    public string BottomsFeedback;
    public string ShoesFeedback;
}

public class IdealOutfit
{
    public IdealOutfitItem JacketIdeal;
    public IdealOutfitItem TopIdeal;
    public IdealOutfitItem BottomsIdeal;
    public IdealOutfitItem ShoesIdeal;
}

public class IdealOutfitItem
{
    public string ItemId;
    public string Commentary;
}

/// <summary>
/// TEMP: Builds <see cref="TempScenarioItemTesting"/> for results UI testing. Remove with scenario pipeline.
/// </summary>
public class TemporaryScenario
{
    public TempScenarioItemTesting Scenario = new TempScenarioItemTesting();

    /// <summary>
    /// Fills <see cref="Scenario"/> with placeholder content matching current wardrobe item IDs.
    /// </summary>
    public void RunTempScenario()
    {
        Scenario.Name = "Temp Scen";
        Scenario.Desc = "Temp Desc";
        Scenario.Era = "1980s";
        Scenario.Illustration = null;

        Scenario.ScoredItems = new List<ScoredItem>();

        ScoredItem emptyJacket = new ScoredItem();
        emptyJacket.ItemId = "nothing_jacket";
        emptyJacket.Score = 1.0f;
        emptyJacket.Commentary = "tempComGood";
        Scenario.ScoredItems.Add(emptyJacket);

        ScoredItem scoreTop = new ScoredItem();
        scoreTop.ItemId = "casualTop2000";
        scoreTop.Score = 1.0f;
        scoreTop.Commentary = "tempComGood";
        Scenario.ScoredItems.Add(scoreTop);

        ScoredItem scoredBottom = new ScoredItem();
        scoredBottom.ItemId = "casualBottom2000";
        scoredBottom.Score = 1.0f;
        scoredBottom.Commentary = "tempComGood";
        Scenario.ScoredItems.Add(scoredBottom);

        ScoredItem scoredShoe = new ScoredItem();
        scoredShoe.ItemId = "casualShoes2000";
        scoredShoe.Score = 1.0f;
        scoredShoe.Commentary = "tempComGood";
        Scenario.ScoredItems.Add(scoredShoe);

        IncorrectSlotFeedback wrongSlot = new IncorrectSlotFeedback();
        Scenario.SlotFeedback = wrongSlot;
        wrongSlot.JacketFeedback = "jacketsWrong";
        wrongSlot.TopFeedback = "topsWrong";
        wrongSlot.BottomsFeedback = "bottomsWrong";
        wrongSlot.ShoesFeedback = "shoesWrong";

        IdealOutfitItem goodOutfitItemJacket = new IdealOutfitItem();
        goodOutfitItemJacket.ItemId = "nothing_jacket";
        goodOutfitItemJacket.Commentary = "jacketcomment";
        IdealOutfitItem goodOutfitItemTop = new IdealOutfitItem();
        goodOutfitItemTop.ItemId = "casualTop2000";
        goodOutfitItemTop.Commentary = "shirtcomment";
        IdealOutfitItem goodOutfitItemBottoms = new IdealOutfitItem();
        goodOutfitItemBottoms.ItemId = "casualBottom2000";
        goodOutfitItemBottoms.Commentary = "pantcomment";
        IdealOutfitItem goodOutfitItemShoes = new IdealOutfitItem();
        goodOutfitItemShoes.ItemId = "casualShoes2000";
        goodOutfitItemShoes.Commentary = "shoecomment";

        IdealOutfit goodOutfit = new IdealOutfit();
        Scenario.OutfitGood = goodOutfit;
        goodOutfit.JacketIdeal = goodOutfitItemJacket;
        goodOutfit.TopIdeal = goodOutfitItemTop;
        goodOutfit.BottomsIdeal = goodOutfitItemBottoms;
        goodOutfit.ShoesIdeal = goodOutfitItemShoes;
    }

#if UNITY_EDITOR
    /// <summary>
    /// Logs placeholder scenario data (editor only).
    /// </summary>
    /// <param name="testScenario">Scenario to log.</param>
    public void LogTempScenarioDebug(TempScenarioItemTesting testScenario)
    {
        if (testScenario == null)
        {
            return;
        }

        Debug.Log(testScenario.Name);
        if (testScenario.ScoredItems != null && testScenario.ScoredItems.Count > 0 && testScenario.ScoredItems[0] != null)
        {
            Debug.Log(testScenario.ScoredItems[0].ItemId);
        }
    }
#endif
}
