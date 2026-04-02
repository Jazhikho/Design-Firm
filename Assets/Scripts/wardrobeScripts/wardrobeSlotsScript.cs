using UnityEngine;

public class wardrobeSlotsScript : MonoBehaviour
{

    //Params
    public string slot;
    public string slotUI;
    public wardrobeItemClothing currentItem;
    //
    private wardrobeButtonTestScripts uiScripts;

    void Awake()
    {
        uiScripts = GameObject.Find("UIRelated").GetComponent<wardrobeButtonTestScripts>();
    }

    public void setCurrentItem(wardrobeItemClothing itemToSet)
    {
        currentItem = itemToSet;
        //OldMethod
        gameObject.GetComponent<SpriteRenderer>().sprite = itemToSet.itemSprite;
        Debug.Log(currentItem.ID + " set to " + slot);
        //NewMethod
        uiScripts.changeUISlotSprite(slotUI, currentItem.itemSprite);
        //
    }
}
