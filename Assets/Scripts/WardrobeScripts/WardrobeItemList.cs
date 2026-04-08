using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static runtime store for wardrobe items loaded from JSON and current slot selections.
/// Note: with Enter Play Mode Options that disable domain reload, static lists can persist between plays—prefer default domain reload for predictable behaviour.
/// </summary>
public class WardrobeItemList
{
    private static WardrobeItemList _instance;
    public static WardrobeItemList Instance
    {
        get
        {
            _instance ??= new WardrobeItemList();
            return _instance;
        }
    }

    public readonly List<WardrobeItemClothing> WardrobeListItems = new();
    public readonly List<WardrobeItemClothing> WardrobeListItemsChest = new();
    public readonly List<WardrobeItemClothing> WardrobeListItemsBottom = new();
    public readonly List<WardrobeItemClothing> WardrobeListItemsShoe = new();
    public readonly List<WardrobeItemClothing> WardrobeListItemsJacket = new();
    public readonly List<string> WardrobeIdList = new();

    public WardrobeItemClothing CurrentItemTop;
    public WardrobeItemClothing CurrentItemBottom;
    public WardrobeItemClothing CurrentItemShoe;
    public WardrobeItemClothing CurrentItemJacket;

    /// <summary>
    /// Seeds each slot with a placeholder "Nothing" item.
    /// </summary>
    private WardrobeItemList()
    {
        NewItemAdd("nothing_chest", "Nothing", "chest", null, "No clothing", false);
        NewItemAdd("nothing_jacket", "Nothing", "jacket", null, "No clothing", false);
        NewItemAdd("nothing_bottom", "Nothing", "bottom", null, "No clothing", false);
        NewItemAdd("nothing_shoe", "Nothing", "shoe", null, "No clothing", false);
    }

    /// <summary>
    /// Returns the first item that is not a seeded <c>nothing_*</c> placeholder, or the first entry if all are placeholders.
    /// </summary>
    /// <param name="items">Per-slot list (must not be null).</param>
    /// <returns>Item to show by default, or null if the list is empty.</returns>
    public WardrobeItemClothing GetFirstDisplayableItem(List<WardrobeItemClothing> items)
    {
        if (items == null || items.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < items.Count; i++)
        {
            WardrobeItemClothing entry = items[i];
            if (entry == null)
            {
                continue;
            }

            if (string.IsNullOrEmpty(entry.Id))
            {
                continue;
            }

            if (entry.Id.StartsWith("nothing_", StringComparison.Ordinal))
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
    public WardrobeItemClothing GetItemSlot(string slot)
    {
        if (slot == "chest")
        {
            return CurrentItemTop;
        }

        if (slot == "bottom")
        {
            return CurrentItemBottom;
        }

        if (slot == "shoe")
        {
            return CurrentItemShoe;
        }

        if (slot == "jacket")
        {
            return CurrentItemJacket;
        }

        Debug.LogError("WardrobeItemList: Invalid slot at GetItemSlot: " + slot);
        return null;
    }

    /// <summary>
    /// Stores the active clothing reference for a slot tag.
    /// </summary>
    /// <param name="slot">One of chest, bottom, shoe, jacket.</param>
    /// <param name="setItem">Item to assign (may be null only if callers allow it).</param>
    public void SetItemSlot(string slot, WardrobeItemClothing setItem)
    {
        if (slot == "chest")
        {
            CurrentItemTop = setItem;
        }
        else if (slot == "bottom")
        {
            CurrentItemBottom = setItem;
        }
        else if (slot == "shoe")
        {
            CurrentItemShoe = setItem;
        }
        else if (slot == "jacket")
        {
            CurrentItemJacket = setItem;
        }
        else
        {
            Debug.LogError("WardrobeItemList: Invalid slot at SetItemSlot: " + slot);
        }
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
    public void NewItemAdd(string newID, string newName, string newSlot, Sprite newSprite, string newDesc, bool newCover)
    {
        WardrobeItemClothing newItem = new()
        {
            Id = newID,
            ItemName = newName,
            SlotTag = newSlot,
            ItemSprite = newSprite,
            ItemDescription = newDesc,
            CoversBottomPiece = newCover
        };

        WardrobeListItems.Add(newItem);
        WardrobeIdList.Add(newItem.Id);
        if (newItem.SlotTag == "chest")
        {
            WardrobeListItemsChest.Add(newItem);
        }
        else if (newItem.SlotTag == "bottom")
        {
            WardrobeListItemsBottom.Add(newItem);
        }
        else if (newItem.SlotTag == "shoe")
        {
            WardrobeListItemsShoe.Add(newItem);
        }
        else if (newItem.SlotTag == "jacket")
        {
            WardrobeListItemsJacket.Add(newItem);
        }
        else
        {
            Debug.LogError("WardrobeItemList: Unknown slotTag '" + newItem.SlotTag + "' for item " + newID);
        }
    }
}
