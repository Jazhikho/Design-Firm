using UnityEngine;

public class wardrobeManagerScript : MonoBehaviour
{
    public TextAsset jsonFile;

    void Start()
    {
        serialItems itemsInFile = JsonUtility.FromJson<serialItems>(jsonFile.text);
        foreach (newSerialItem newItem in itemsInFile.items)
        {
            Sprite newItemSprite = Resources.Load<Sprite>(newItem.itemSprite);
            GameObject.Find("ItemsList").GetComponent<wardrobeItemList>().newItemAdd(newItem.ID, newItem.itemName, newItem.slotTag, newItemSprite, newItem.itemDescription, newItem.coversBottomPiece);
        }
    }

}

[System.Serializable]
public class newSerialItem
{
    public string ID;
    public string itemName;
    //chest, bottom, shoe, jacket
    public string slotTag;
    public string itemSprite;
    public string itemDescription;
    public bool coversBottomPiece = false;
}

[System.Serializable]
public class serialItems
{
    public newSerialItem[] items;
}