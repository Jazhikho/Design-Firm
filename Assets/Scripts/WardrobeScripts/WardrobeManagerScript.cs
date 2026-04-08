using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Persistent singleton that loads wardrobe item definitions from a JSON TextAsset once at startup.
/// Survives scene transitions via <see cref="DontDestroyOnLoad"/> so items are never loaded twice.
/// Can live in any scene—whichever scene is opened first will bootstrap the data.
/// </summary>
[DefaultExecutionOrder(-100)]
public class WardrobeManagerScript : MonoBehaviour
{
    private static WardrobeManagerScript _instance;

    [SerializeField]
    [FormerlySerializedAs("jsonFile")]
    private TextAsset _jsonFile;

    /// <summary>
    /// Ensures a single persistent manager instance and loads wardrobe data once.
    /// </summary>
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadItemsFromJson();
    }

    /// <summary>
    /// Deserializes <see cref="_jsonFile"/> and forwards each entry to <see cref="WardrobeItemList.Instance"/>.
    /// </summary>
    private void LoadItemsFromJson()
    {
        if (_jsonFile == null)
        {
            Debug.LogError("WardrobeManagerScript: Assign the items JSON TextAsset in the Inspector.");
            return;
        }

        SerialItems itemsInFile = JsonUtility.FromJson<SerialItems>(_jsonFile.text);
        if (itemsInFile == null || itemsInFile.items == null)
        {
            Debug.LogError("WardrobeManagerScript: JSON did not deserialize to SerialItems with a non-null items array.");
            return;
        }

        foreach (NewSerialItem newItem in itemsInFile.items)
        {
            if (newItem == null)
            {
                Debug.LogError("WardrobeManagerScript: Encountered a null item entry in JSON.");
                continue;
            }

            Sprite newItemSprite = Resources.Load<Sprite>(newItem.itemSprite);
            if (newItemSprite == null)
            {
                Debug.LogError("WardrobeManagerScript: Resources.Load failed for sprite path: " + newItem.itemSprite);
                continue;
            }

            WardrobeItemList.Instance.NewItemAdd(
                newItem.ID,
                newItem.itemName,
                newItem.slotTag,
                newItemSprite,
                newItem.itemDescription,
                newItem.coversBottomPiece);
        }
    }
}

/// <summary>
/// JSON row shape for <c>itemsJson.json</c>; field names must match file keys for <see cref="JsonUtility"/>.
/// </summary>
[System.Serializable]
public class NewSerialItem
{
    // JSON field names must match source data keys exactly.
    public string ID;
    public string itemName;
    public string slotTag;
    public string itemSprite;
    public string itemDescription;
    public bool coversBottomPiece = false;
}

/// <summary>
/// Root JSON object containing the items array for wardrobe deserialization.
/// </summary>
[System.Serializable]
public class SerialItems
{
    public NewSerialItem[] items;
}
