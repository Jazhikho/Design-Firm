using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds runtime lists of wardrobe items per slot and supports JSON-driven registration.
/// </summary>
public class wardrobeItemList : MonoBehaviour
{

    public List<wardrobeItemClothing> wardrobeListItems = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsChest = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsBottom = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsShoe = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsJacket = new List<wardrobeItemClothing>();
    public List<string> wardrobeIDList = new List<string>();


    /// <summary>
    /// Seeds placeholder "nothing" items for each slot so lists are never empty.
    /// </summary>
    private void Awake()
    {
        //Append empty items
        newItemAdd("nothing_chest", "Nothing", "chest", null, "No clothing", false);
        newItemAdd("nothing_jacket", "Nothing", "jacket", null, "No clothing", false);
        newItemAdd("nothing_bottom", "Nothing", "bottom", null, "No clothing", false);
        newItemAdd("nothing_shoe", "Nothing", "shoe", null, "No clothing", false);
        //
    }

    /// <summary>
    /// Adds a clothing entry to the master list and the matching slot list.
    /// </summary>
    /// <param name="newID">Stable item identifier.</param>
    /// <param name="newName">Display name.</param>
    /// <param name="newSlot">Slot tag: chest, bottom, shoe, or jacket.</param>
    /// <param name="newSprite">Sprite asset for the slot renderer.</param>
    /// <param name="newDesc">Description text.</param>
    /// <param name="newCover">Whether this item covers the bottom piece.</param>
    public void newItemAdd(string newID, string newName, string newSlot, Sprite newSprite, string newDesc, bool newCover)
    {
        wardrobeItemClothing newItem = new wardrobeItemClothing();
        newItem.ID = newID;
        newItem.itemName = newName;
        newItem.slotTag = newSlot;
        newItem.itemSprite = newSprite;
        newItem.itemDescription = newDesc;
        newItem.coversBottomPiece = newCover;

        wardrobeListItems.Add(newItem);
        wardrobeIDList.Add(newItem.ID);
        if (newItem.slotTag == "chest")
        {
            wardrobeListItemsChest.Add(newItem);
        }
        else if (newItem.slotTag == "bottom")
        {
            wardrobeListItemsBottom.Add(newItem);
        }
        else if (newItem.slotTag == "shoe")
        {
            wardrobeListItemsShoe.Add(newItem);
        }
        else if (newItem.slotTag == "jacket")
        {
            wardrobeListItemsJacket.Add(newItem);
        }
        else
        {
            Debug.LogError("wardrobeItemList: Unknown slotTag '" + newItem.slotTag + "' for item " + newID);
        }
    }

}
