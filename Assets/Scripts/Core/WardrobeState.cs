using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Core
{
    /// <summary>
    /// Static runtime store for wardrobe items loaded from JSON and current slot selections.
    /// Setting a CurrentItem property raises the corresponding change event.
    /// </summary>
    internal sealed class WardrobeState
    {
        internal static WardrobeState Instance { get; } = new();

        /// <summary>
        /// Raised once after wardrobe items have been loaded from Addressables.
        /// </summary>
        internal event Action WardrobeItemsLoaded;

        /// <summary>
        /// Raised whenever a slot's current item changes. The parameter is the newly assigned <see cref="WardrobeItem"/>.
        /// </summary>
        internal event Action<WardrobeItem> CurrentTopChanged;
        internal event Action<WardrobeItem> CurrentBottomChanged;
        internal event Action<WardrobeItem> CurrentShoeChanged;
        internal event Action<WardrobeItem> CurrentJacketChanged;

        internal bool IsWardrobeItemsLoaded { get; private set; }

        internal List<WardrobeItem> AllWardrobeItems { get; } = new();

        private string GenderFilter
        {
            get
            {
                if (ScenarioState.Instance.ActiveScenario?.avatarImage == "Avatars/2000sFemModel.png")
                    return "female";
                else
                    return "male";
            }
        }

        internal List<WardrobeItem> AvailableTops =>
            AllWardrobeItems
                .Where(i => i.SlotType == ClothingSlot.Top
                         && (string.IsNullOrEmpty(i.gender) || i.gender == GenderFilter))
                .ToList();

        internal List<WardrobeItem> AvailableBottoms =>
            AllWardrobeItems
                .Where(i => i.SlotType == ClothingSlot.Bottom
                         && (string.IsNullOrEmpty(i.gender) || i.gender == GenderFilter))
                .ToList();

        internal List<WardrobeItem> AvailableShoes =>
            AllWardrobeItems
                .Where(i => i.SlotType == ClothingSlot.Shoes
                         && (string.IsNullOrEmpty(i.gender) || i.gender == GenderFilter))
                .ToList();

        internal List<WardrobeItem> AvailableJackets =>
            AllWardrobeItems
                .Where(i => i.SlotType == ClothingSlot.Jacket
                         && (string.IsNullOrEmpty(i.gender) || i.gender == GenderFilter))
                .ToList();

        private WardrobeItem _currentItemTop;
        private WardrobeItem _currentItemBottom;
        private WardrobeItem _currentItemShoe;
        private WardrobeItem _currentItemJacket;

        internal WardrobeItem CurrentItemTop
        {
            get => _currentItemTop;
            set
            {
                _currentItemTop = value;
                CurrentTopChanged?.Invoke(value);
            }
        }

        internal WardrobeItem CurrentItemBottom
        {
            get => _currentItemBottom;
            set
            {
                _currentItemBottom = value;
                CurrentBottomChanged?.Invoke(value);
            }
        }

        internal WardrobeItem CurrentItemShoe
        {
            get => _currentItemShoe;
            set
            {
                _currentItemShoe = value;
                CurrentShoeChanged?.Invoke(value);
            }
        }

        internal WardrobeItem CurrentItemJacket
        {
            get => _currentItemJacket;
            set
            {
                _currentItemJacket = value;
                CurrentJacketChanged?.Invoke(value);
            }
        }

        /// <summary>
        /// Marks wardrobe items as loaded and raises <see cref="WardrobeItemsLoaded"/>.
        /// Called by StartupServices after Addressables loading completes.
        /// </summary>
        internal void NotifyWardrobeItemsLoaded()
        {
            IsWardrobeItemsLoaded = true;
            CurrentItemTop = AvailableTops.FirstOrDefault(i => i.id == "nothing_top");
            CurrentItemBottom = AvailableBottoms.FirstOrDefault(i => i.id == "nothing_bottom");
            CurrentItemShoe = AvailableShoes.FirstOrDefault(i => i.id == "nothing_shoes");
            CurrentItemJacket = AvailableJackets.FirstOrDefault(i => i.id == "nothing_jacket");
            WardrobeItemsLoaded?.Invoke();
        }

        /// <summary>
        /// Seeds each slot with a placeholder "Nothing" item.
        /// </summary>
        private WardrobeState()
        {
            AddItem("nothing_top", "top");
            AddItem("nothing_jacket", "jacket");
            AddItem("nothing_bottom", "bottom");
            AddItem("nothing_shoes", "shoes");
        }

        /// <summary>
        /// Adds a clothing entry to the master list.
        /// </summary>
        internal void AddItem(
            string newID,
            string newSlot,
            string newName = "Nothing",
            string newDesc = "No clothing",
            string newSprite = null,
            bool newCover = false,
            string newGender = null)
        {
            AllWardrobeItems.Add(new WardrobeItem
            {
                id = newID,
                name = newName,
                slot = newSlot,
                sprite = newSprite,
                description = newDesc,
                coversBottom = newCover,
                gender = newGender
            });
        }

        /// <summary>
        /// Returns the item currently assigned to <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">Which equipment slot to read.</param>
        /// <returns>The equipped item, or null if the slot is unrecognized.</returns>
        internal WardrobeItem GetCurrentItem(ClothingSlot slot)
        {
            switch (slot)
            {
                case ClothingSlot.Jacket:
                    return CurrentItemJacket;
                case ClothingSlot.Top:
                    return CurrentItemTop;
                case ClothingSlot.Bottom:
                    return CurrentItemBottom;
                case ClothingSlot.Shoes:
                    return CurrentItemShoe;
                default:
                    return null;
            }
        }
    }
}
