using UnityEngine;

public class TEMPwardrobeClothingPicker : MonoBehaviour
{
    public wardrobeSlotsScript chestSlot;
    public wardrobeSlotsScript bottomSlot;
    public wardrobeSlotsScript shoeSlot;
    public wardrobeSlotsScript jacketSlot;
    public wardrobeItemList itemList;

    void Start()
    {
        chestSlot = GameObject.Find("chestSlot").GetComponent<wardrobeSlotsScript>();
        bottomSlot = GameObject.Find("bottomSlot").GetComponent<wardrobeSlotsScript>();
        shoeSlot = GameObject.Find("shoeSlot").GetComponent<wardrobeSlotsScript>();
        jacketSlot = GameObject.Find("jacketSlot").GetComponent<wardrobeSlotsScript>();
        itemList = GameObject.Find("ItemsList").GetComponent<wardrobeItemList>();

        chestSlot.setCurrentItem(itemList.wardrobeListItemsChest[0]);
        bottomSlot.setCurrentItem(itemList.wardrobeListItemsBottom[0]);
        shoeSlot.setCurrentItem(itemList.wardrobeListItemsShoe[0]);
        jacketSlot.setCurrentItem(itemList.wardrobeListItemsJacket[0]);
    }
}
