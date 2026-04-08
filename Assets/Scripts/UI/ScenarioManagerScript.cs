using System;
using System.Collections.Generic;
using UnityEngine;
[DefaultExecutionOrder(-100)]
public class ScenarioManagerScript : MonoBehaviour
{
    private static ScenarioManagerScript instance;

    [SerializeField]
    private TextAsset jsonFile;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        LoadItems();
        Debug.Log(ScenarioItemList.Instance.Items[0].name);
    }

    private void LoadItems()
    {
        if (jsonFile == null)
        {
            Debug.LogError("ScenarioManagerScript: assign jsonFile in the inspector");
            return;
        }
        SerialItemsScenario itemsinfile= JsonUtility.FromJson<SerialItemsScenario>(jsonFile.text);
        if (itemsinfile == null || itemsinfile.scenarios == null)
        {
            Debug.LogError("ScenarioManagerscript: itemsinfile is null");
            return;
        }
        foreach(ScenarioItemSerializable NewItem in itemsinfile.scenarios)
        {
            if (NewItem == null)
            {
                Debug.LogError("ScenarioManagerScript: null item entry in json");
                continue;
            }
            Sprite newbackgroundImage = null;
            Sprite newavatarImage = null;
            ScenarioItemList.Instance.NewItemAdd(
                NewItem.name, NewItem.description, NewItem.era, NewItem.category, newbackgroundImage, newavatarImage, NewItem.scoredItems, NewItem.incorrectSlotFeedback, NewItem.idealOutfit);
        }
    }
}

[Serializable]
public class ScenarioItemSerializable 
{
    public string name;
    public string description;
    public string era;
    public string category;
    public string backgroundImage;
    public string avatarImage;
    public List<ScoreItem> scoredItems = new();
    public IncorrectFeedback incorrectSlotFeedback;
    public IdealOutfit idealOutfit;
}

[System.Serializable]
public class SerialItemsScenario
{
    public ScenarioItemSerializable[] scenarios;
}

