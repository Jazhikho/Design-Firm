using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class wardrobeItemList : MonoBehaviour
{
    //Test Values
    public Sprite testSprite;
    //
    public List<wardrobeItemClothing> wardrobeListItems = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsChest = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsBottom = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsShoe = new List<wardrobeItemClothing>();
    public List<wardrobeItemClothing> wardrobeListItemsJacket = new List<wardrobeItemClothing>();
    public List<string> wardrobeIDList = new List<string>();


    void Awake()
    {
        //Append empty items
        newItemAdd("nothing_chest", "Nothing", "chest", null, "No clothing", false);
        newItemAdd("nothing_jacket", "Nothing", "jacket", null, "No clothing", false);
        newItemAdd("nothing_bottom", "Nothing", "bottom", null, "No clothing", false);
        newItemAdd("nothing_shoe", "Nothing", "shoe", null, "No clothing", false);
        //


        // New Item
        /*
        wardrobeItemClothing testAddItem = new wardrobeItemClothing();
        testAddItem.ID = "testShirt";
        testAddItem.itemName = "Test Shirt";
        testAddItem.slotTag = "chest";
        testAddItem.itemSprite = testSprite;
        testAddItem.itemDescription = "This is a shirt made for testing.";
        //
        wardrobeListItems.Add(testAddItem);
        wardrobeListItemsChest.Add(testAddItem);
        wardrobeIDList.Add(testAddItem.ID);
        //
        Debug.Log(wardrobeListItems[0].ID);
        */
        //newItemAdd("testShirt", "Test Shirt", "chest", testSprite, "This is a shirt made for testing", false);
    }

    //AppendItemFunction

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
            Debug.Log("Error: No Slot detected");
        }
    }

}
