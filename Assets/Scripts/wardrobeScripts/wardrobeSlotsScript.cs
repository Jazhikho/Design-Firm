using UnityEngine;

/// <summary>
/// Drives a single wardrobe slot using a SpriteRenderer and a wardrobeItemClothing reference.
/// </summary>
public class wardrobeSlotsScript : MonoBehaviour
{
    public string slot;
    public string slotUI;
    public wardrobeItemClothing currentItem;

    wardrobeButtonTestScripts uiScripts;
    //

    void Awake()
    {
        uiScripts = GameObject.Find("UIRelated").GetComponent<wardrobeButtonTestScripts>();
    }
    /// <summary>
    /// Stores the active item for this slot and updates the SpriteRenderer.
    /// </summary>
    /// <param name="itemToSet">The clothing entry to display; must not be null.</param>
    public void setCurrentItem(wardrobeItemClothing itemToSet)
    {
        
        if (itemToSet == null)
        {
            Debug.LogError("wardrobeSlotsScript: itemToSet is null for slot " + slot);
            return;
        }

        currentItem = itemToSet;
        SpriteRenderer renderer = gameObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            Debug.LogError("wardrobeSlotsScript: SpriteRenderer is missing on " + gameObject.name);
            return;
        }
        renderer.sprite = itemToSet.itemSprite;
        
        uiScripts.changeUISlotSprite(slotUI, currentItem.itemSprite);
        //
    }
}
