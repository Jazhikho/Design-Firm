using UnityEngine;

/// <summary>
/// Applies default wardrobe items from the shared list to each slot on Start (temporary setup helper).
/// </summary>
public class TEMPwardrobeClothingPicker : MonoBehaviour
{
    [SerializeField]
    private wardrobeSlotsScript chestSlot;

    [SerializeField]
    private wardrobeSlotsScript bottomSlot;

    [SerializeField]
    private wardrobeSlotsScript shoeSlot;

    [SerializeField]
    private wardrobeSlotsScript jacketSlot;

    [SerializeField]
    private wardrobeItemList itemList;

    /// <summary>
    /// Assigns the first item in each slot list to the matching slot renderer.
    /// </summary>
    private void Start()
    {
        if (chestSlot == null || bottomSlot == null || shoeSlot == null || jacketSlot == null || itemList == null)
        {
            Debug.LogError("TEMPwardrobeClothingPicker: Assign all slot references and itemList in the Inspector.");
            return;
        }

        if (itemList.wardrobeListItemsChest.Count < 1)
        {
            Debug.LogError("TEMPwardrobeClothingPicker: wardrobeListItemsChest is empty.");
            return;
        }
        if (itemList.wardrobeListItemsBottom.Count < 1)
        {
            Debug.LogError("TEMPwardrobeClothingPicker: wardrobeListItemsBottom is empty.");
            return;
        }
        if (itemList.wardrobeListItemsShoe.Count < 1)
        {
            Debug.LogError("TEMPwardrobeClothingPicker: wardrobeListItemsShoe is empty.");
            return;
        }
        if (itemList.wardrobeListItemsJacket.Count < 1)
        {
            Debug.LogError("TEMPwardrobeClothingPicker: wardrobeListItemsJacket is empty.");
            return;
        }

        chestSlot.setCurrentItem(itemList.wardrobeListItemsChest[0]);
        bottomSlot.setCurrentItem(itemList.wardrobeListItemsBottom[0]);
        shoeSlot.setCurrentItem(itemList.wardrobeListItemsShoe[0]);
        jacketSlot.setCurrentItem(itemList.wardrobeListItemsJacket[0]);
    }
}
