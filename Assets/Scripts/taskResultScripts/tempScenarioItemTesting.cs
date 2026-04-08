using System.Collections.Generic;
using UnityEngine;

public class tempScenarioItemTesting
{
    public string name;
    public string desc;
    public string era;
    public Sprite illustration;
    public List<scoredItem> scoredItems;
    public incorrectSlotFeedback slotFeedback;
    public idealOutfit outfitGood;

}
public class scoredItem
{
    public string itemID;
    public float score;
    public string commentary;
}
public class incorrectSlotFeedback
{
    public string jacketFeedback;
    public string topFeedback;
    public string bottomsFeedback;
    public string shoesFeedback;
}
public class idealOutfit
{
    public idealOutfitItem jacketIdeal;
    public idealOutfitItem topIdeal;
    public idealOutfitItem bottomsIdeal;
    public idealOutfitItem shoesIdeal;
}
public class idealOutfitItem
{
    public string itemId;
    public string commentary;
}

//
public class temporaryScenario
{
    public tempScenarioItemTesting Scenario = new tempScenarioItemTesting();

    public void tempScenarioRun()
    {
        //Params
        Scenario.name = "Temp Scen";
        Scenario.desc = "Temp Desc";
        Scenario.era = "1980s";
        Scenario.illustration = null;

        //ScoredItems
        Scenario.scoredItems = new List<scoredItem>();

        //exampleItem
        scoredItem emptyJacket = new scoredItem();
        emptyJacket.itemID = "nothing_jacket";
        emptyJacket.score = 1.0f;
        emptyJacket.commentary = "tempComGood";
        Scenario.scoredItems.Add(emptyJacket);
        //
        scoredItem scoreTop = new scoredItem();
        scoreTop.itemID = "casualTop2000";
        scoreTop.score = 1.0f;
        scoreTop.commentary = "tempComGood";
        Scenario.scoredItems.Add(scoreTop);
        //
        scoredItem scoredBottom = new scoredItem();
        scoredBottom.itemID = "casualBottom2000";
        scoredBottom.score = 1.0f;
        scoredBottom.commentary = "tempComGood";
        Scenario.scoredItems.Add(scoredBottom);
        //
        scoredItem scoredShoe = new scoredItem();
        scoredShoe.itemID = "casualShoes2000";
        scoredShoe.score = 1.0f;
        scoredShoe.commentary = "tempComGood";
        Scenario.scoredItems.Add(scoredShoe);

        //feedback
        incorrectSlotFeedback wrongSlot = new incorrectSlotFeedback();
        Scenario.slotFeedback = wrongSlot;
        wrongSlot.jacketFeedback = "jacketsWrong";
        wrongSlot.topFeedback = "topsWrong";
        wrongSlot.bottomsFeedback = "bottomsWrong";
        wrongSlot.shoesFeedback = "shoesWrong";
        //idealItems
        idealOutfitItem goodOutfitItemJacket = new idealOutfitItem();
        goodOutfitItemJacket.itemId = "nothing_jacket";
        goodOutfitItemJacket.commentary = "jacketcomment";
        idealOutfitItem goodOutfitItemTop = new idealOutfitItem();
        goodOutfitItemTop.itemId = "casualTop2000";
        goodOutfitItemTop.commentary = "shirtcomment";
        idealOutfitItem goodOutfitItemBottoms = new idealOutfitItem();
        goodOutfitItemBottoms.itemId = "casualBottom2000";
        goodOutfitItemBottoms.commentary = "pantcomment";
        idealOutfitItem goodOutfitItemShoes = new idealOutfitItem();
        goodOutfitItemShoes.itemId = "casualShoes2000";
        goodOutfitItemShoes.commentary = "shoecomment";
        
        //
        idealOutfit goodOutfit = new idealOutfit();
        Scenario.outfitGood = goodOutfit;
        goodOutfit.jacketIdeal = goodOutfitItemJacket;
        goodOutfit.topIdeal = goodOutfitItemTop;
        goodOutfit.bottomsIdeal = goodOutfitItemBottoms;
        goodOutfit.shoesIdeal = goodOutfitItemShoes;
        //

    }
    public void debugTempScenario(tempScenarioItemTesting testScenario)
    {
        Debug.Log(testScenario.name);
        Debug.Log(testScenario.scoredItems[0].itemID);
    }
    
}
//
