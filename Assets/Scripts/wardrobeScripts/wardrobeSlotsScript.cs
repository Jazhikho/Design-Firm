using UnityEngine;

public class wardrobeSlotsScript : MonoBehaviour
{

    //Params
    public string slot;
    public wardrobeItemClothing currentItem;
    //

    public void setCurrentItem(wardrobeItemClothing itemToSet)
    {
        currentItem = itemToSet;
        gameObject.GetComponent<SpriteRenderer>().sprite = itemToSet.itemSprite;

        Debug.Log(currentItem.ID + " set to " + slot);
    }
}
