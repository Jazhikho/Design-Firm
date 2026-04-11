using System;

namespace Assets.Scripts.Data
{
    /// <summary>
    /// Equipment slot for a clothing item.
    /// </summary>
    public enum ClothingSlot
    {
        Top,
        Bottoms,
        Shoes,
        Jacket
    }

    /// <summary>
    /// JSON row shape for <c>clothing_items.json</c>; field names must match file keys for <see cref="JsonUtility"/>.
    /// Use <see cref="SlotType"/> for type-safe slot comparisons in code.
    /// </summary>
    [Serializable]
    public class WardrobeItem
    {
        public string id;
        public string name;
        public string slot;
        public string gender;
        public bool coversBottoms;
        public string sprite;
        public string description;

        /// <summary>
        /// Parsed <see cref="ClothingSlot"/> from the JSON <see cref="slot"/> string.
        /// </summary>
        public ClothingSlot SlotType => slot switch
        {
            "top" => ClothingSlot.Top,
            "bottoms" => ClothingSlot.Bottoms,
            "shoes" => ClothingSlot.Shoes,
            "jacket" => ClothingSlot.Jacket,
            _ => throw new ArgumentException($"Unknown clothing slot: '{slot}'")
        };
    }

    /// <summary>
    /// Root JSON object containing the clothingItems array for wardrobe de/serialization.
    /// </summary>
    [Serializable]
    public class WardrobeItemsWrapper
    {
        public WardrobeItem[] clothingItems;
    }
}
