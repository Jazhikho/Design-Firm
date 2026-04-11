using System;
using System.Collections.Generic;

namespace Assets.Scripts.Data
{
    [Serializable]
    public class ScenariosWrapper
    {
        public List<Scenario> scenarios;
    }

    [Serializable]
    public class Scenario
    {
        public string name;

        public string description;

        public string era;

        public string category;

        public string backgroundImage;

        public string avatarImage;

        public List<ScoredItem> scoredItems;

        public IncorrectSlotFeedback incorrectSlotFeedback;

        public IdealOutfit idealOutfit;
    }

    [Serializable]
    public class ScoredItem
    {
        public string itemId;

        public float score;

        public string commentary;
    }

    [Serializable]
    public class IncorrectSlotFeedback
    {
        public string jacket;

        public string top;

        public string bottoms;

        public string shoes;
    }

    [Serializable]
    public class IdealOutfit
    {
        public IdealOutfitItem jacket;

        public IdealOutfitItem top;

        public IdealOutfitItem bottoms;

        public IdealOutfitItem shoes;
    }

    [Serializable]
    public class IdealOutfitItem
    {
        public string itemId;

        public string commentary;
    }
}
