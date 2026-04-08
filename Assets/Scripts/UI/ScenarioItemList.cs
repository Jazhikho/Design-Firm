using System.Collections.Generic;
using UnityEngine;

public class ScenarioItemList
{
    private static ScenarioItemList _instance;
    public static ScenarioItemList Instance 
    {  get 
        {
            _instance ??= new ScenarioItemList();
            return _instance;
        } 
    }
    public List<ScenarioItem> Items;
    public ScenarioItem currentItem;

    public void NewItemAdd(string NewName,string NewDescription,string NewEra,string NewCategory,Sprite NewBackgroundimage, Sprite NewAvatarImage, List<ScoreItem> NewScoredItems,IncorrectFeedback NewFeedback, IdealOutfit NewIdealOutfits )
    {
        ScenarioItem NewItem = new()
        {
            name = NewName,
            description = NewDescription,
            era = NewEra,
            category = NewCategory,
            backgroundImage = null,
            avatarImage = null,
            scoredItems = NewScoredItems,
            incorrectSlotFeedback = NewFeedback,
            idealOutfit = NewIdealOutfits

        };
        Items.Add(NewItem);
    }
}

