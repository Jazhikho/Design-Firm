using UnityEngine;

/// <summary>
/// Loads wardrobe item definitions from a JSON TextAsset and registers them on the static <see cref="wardrobeItemList"/> store.
/// </summary>
[DefaultExecutionOrder(-100)]
public class wardrobeManagerScript : MonoBehaviour
{
    [SerializeField]
    private TextAsset jsonFile;

    /// <summary>
    /// Deserializes <see cref="jsonFile"/> and forwards each entry to <see cref="wardrobeItemList.NewItemAdd"/>.
    /// </summary>
    private void Start()
    {
        if (jsonFile == null)
        {
            Debug.LogError("wardrobeManagerScript: Assign jsonFile in the Inspector.");
            return;
        }

        SerialItems itemsInFile = JsonUtility.FromJson<SerialItems>(jsonFile.text);
        if (itemsInFile == null || itemsInFile.items == null)
        {
            Debug.LogError("wardrobeManagerScript: JSON did not deserialize to SerialItems with a non-null items array.");
            return;
        }

        foreach (NewSerialItem newItem in itemsInFile.items)
        {
            if (newItem == null)
            {
                Debug.LogError("wardrobeManagerScript: Encountered a null item entry in JSON.");
                continue;
            }

            Sprite newItemSprite = Resources.Load<Sprite>(newItem.itemSprite);
            if (newItemSprite == null)
            {
                Debug.LogError("wardrobeManagerScript: Resources.Load failed for sprite path: " + newItem.itemSprite);
                continue;
            }

            wardrobeItemList.NewItemAdd(newItem.ID, newItem.itemName, newItem.slotTag, newItemSprite, newItem.itemDescription, newItem.coversBottomPiece);
        }
    }
}

[System.Serializable]
public class NewSerialItem
{
    public string ID;
    public string itemName;
    public string slotTag;
    public string itemSprite;
    public string itemDescription;
    public bool coversBottomPiece = false;
}

[System.Serializable]
public class SerialItems
{
    public NewSerialItem[] items;
}
