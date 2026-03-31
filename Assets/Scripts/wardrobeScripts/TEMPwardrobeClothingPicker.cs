using UnityEngine;

public class TEMPwardrobeClothingPicker : MonoBehaviour
{
    public wardrobeSlotsScript chestSlot;
    public wardrobeSlotsScript bottomSlot;
    public wardrobeSlotsScript shoeSlot;
    public wardrobeItemList itemList;

    void Start()
    {
        chestSlot = GameObject.Find("chestSlot").GetComponent<wardrobeSlotsScript>();
        bottomSlot = GameObject.Find("bottomSlot").GetComponent<wardrobeSlotsScript>();
        shoeSlot = GameObject.Find("shoeSlot").GetComponent<wardrobeSlotsScript>();
        itemList = GameObject.Find("ItemsList").GetComponent<wardrobeItemList>();

        chestSlot.setCurrentItem(itemList.wardrobeListItems[0]);
    }
}
