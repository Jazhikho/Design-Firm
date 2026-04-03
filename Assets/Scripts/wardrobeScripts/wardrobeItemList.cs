using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static runtime store for wardrobe items loaded from JSON and current slot selections.
/// Note: with Enter Play Mode Options that disable domain reload, static lists can persist between plays—prefer default domain reload for predictable behaviour.
/// </summary>
public static class wardrobeItemList
{
    public static List<wardrobeItemClothing> wardrobeListItems = new();
    public static List<wardrobeItemClothing> wardrobeListItemsChest = new();
    public static List<wardrobeItemClothing> wardrobeListItemsBottom = new();
    public static List<wardrobeItemClothing> wardrobeListItemsShoe = new();
    public static List<wardrobeItemClothing> wardrobeListItemsJacket = new();
    public static List<string> wardrobeIDList = new();

    public static wardrobeItemClothing currentItemTop;
    public static wardrobeItemClothing currentItemBottom;
    public static wardrobeItemClothing currentItemShoe;
    public static wardrobeItemClothing currentItemJacket;

    /// <summary>
    /// Returns the first item that is not a seeded <c>nothing_*</c> placeholder, or the first entry if all are placeholders.
    /// </summary>
    /// <param name="items">Per-slot list (must not be null).</param>
    /// <returns>Item to show by default, or null if the list is empty.</returns>
    public static wardrobeItemClothing GetFirstDisplayableItem(List<wardrobeItemClothing> items)
    {
        if (items == null || items.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            wardrobeItemClothing entry = items[i];
            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(entry.ID))
            {
                continue;
            }

            if (entry.ID.StartsWith("nothing_", StringComparison.Ordinal))
            {
                continue;
            }

            return entry;
        }

        return items[0];
    }

    /// <summary>
    /// Resolves the current clothing reference for a slot tag.
    /// </summary>
    /// <param name="slot">One of chest, bottom, shoe, jacket.</param>
    /// <returns>Current item, or null if the tag is invalid.</returns>
    public static wardrobeItemClothing GetItemSlot(string slot)
    {
        if (slot == "chest")
        {
            return currentItemTop;
        }

        if (slot == "bottom")
        {
            return currentItemBottom;
        }

        if (slot == "shoe")
        {
            return currentItemShoe;
        }

        if (slot == "jacket")
        {
            return currentItemJacket;
        }

        Debug.LogError("wardrobeItemList: Invalid slot at GetItemSlot: " + slot);
        return null;
    }

    /// <summary>
    /// Stores the active clothing reference for a slot tag.
    /// </summary>
    /// <param name="slot">One of chest, bottom, shoe, jacket.</param>
    /// <param name="setItem">Item to assign (may be null only if callers allow it).</param>
    public static void SetItemSlot(string slot, wardrobeItemClothing setItem)
    {
        if (slot == "chest")
        {
            currentItemTop = setItem;
        }
        else if (slot == "bottom")
        {
            currentItemBottom = setItem;
        }
        else if (slot == "shoe")
        {
            currentItemShoe = setItem;
        }
        else if (slot == "jacket")
        {
            currentItemJacket = setItem;
        }
        else
        {
            Debug.LogError("wardrobeItemList: Invalid slot at SetItemSlot: " + slot);
        }
    }

    /// <summary>
    /// Seeds placeholder "nothing" items for each slot so lists are never empty.
    /// </summary>
    static wardrobeItemList()
    {
        NewItemAdd("nothing_chest", "Nothing", "chest", null, "No clothing", false);
        NewItemAdd("nothing_jacket", "Nothing", "jacket", null, "No clothing", false);
        NewItemAdd("nothing_bottom", "Nothing", "bottom", null, "No clothing", false);
        NewItemAdd("nothing_shoe", "Nothing", "shoe", null, "No clothing", false);
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
    public static void NewItemAdd(string newID, string newName, string newSlot, Sprite newSprite, string newDesc, bool newCover)
    {
        wardrobeItemClothing newItem = new()
        {
            ID = newID,
            itemName = newName,
            slotTag = newSlot,
            itemSprite = newSprite,
            itemDescription = newDesc,
            coversBottomPiece = newCover
        };

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
