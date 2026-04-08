using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable] public class ScenarioItem 
{
    public string name;
    public string description;
    public string era;
    public string category;
    public UnityEngine.Sprite backgroundImage;
    public UnityEngine.Sprite avatarImage;
    public List<ScoreItem> scoredItems = new();
    public IncorrectFeedback incorrectSlotFeedback;
    public IdealOutfit idealOutfit;
}
[Serializable] public class ScoreItem
{
    public string itemId;
    public float score;
    public string commentary;
}


[Serializable] public class IncorrectFeedback
{
    public string jacket;
    public string top;
    public string bottoms;
    public string shoes;
}

[Serializable] public class IdealOutfit
{
    public IdealOutfitItem jacket;
    public IdealOutfitItem top;
    public IdealOutfitItem bottoms;
    public IdealOutfitItem shoes;
}

[Serializable] public class IdealOutfitItem
{
    public string itemId;
    public string commentary;
}