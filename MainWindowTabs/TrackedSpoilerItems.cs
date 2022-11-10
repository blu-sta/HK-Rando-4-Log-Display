using HK_Rando_4_Log_Display.DTO;
using System.Collections.Generic;
using System.Linq;

namespace HK_Rando_4_Log_Display
{
    public interface IHasItem
    {
        Item Item { get; set; }
    }
    
    public static class TrackedSpoilerItems
    {
        public static ItemWithLocation GetTrackedItemWithLocation(string itemName, IEnumerable<ItemWithLocation> trackedLocations) =>
            trackedLocations.FirstOrDefault(y => y.Item.Name == itemName)
                 ?? GetProgressiveItem(itemName, trackedLocations)
                 ?? GetHunterItem(itemName, trackedLocations);

        private static ItemWithLocation GetProgressiveItem(string itemName, IEnumerable<ItemWithLocation> trackedLocations) =>
            allProgressiveItems.Contains(itemName)
                ? progressiveItemBuckets.Select(x =>
                        x.Contains(itemName)
                            ? trackedLocations.FirstOrDefault(y => x.Contains(y.Item.Name))
                            : default)
                    .FirstOrDefault(x => x != null)
                : default;

        private static ItemWithLocation GetHunterItem(string itemName, IEnumerable<ItemWithLocation> trackedLocations) =>
            itemName.StartsWith("Hunter's_Notes") || itemName.StartsWith("Journal_Entry")
                ? trackedLocations.FirstOrDefault(y => y.Item.Name == InvertHunterItemPool(itemName))
                : default;

        private static string InvertHunterItemPool(string itemName) =>
            itemName.StartsWith("Hunter's_Notes")
                ? itemName.Replace("Hunter's_Notes", "Journal_Entry")
                : itemName.StartsWith("Journal_Entry")
                ? itemName.Replace("Journal_Entry", "Hunter's_Notes")
                : itemName;

        private static readonly string[] whiteFragments = new[] { "White_Fragment", "Queen_Fragment", "King_Fragment", "Kingsoul", "Void_Heart" };
        private static readonly string[] greedCharms = new[] { "Fragile_Greed", "Unbreakable_Greed" };
        private static readonly string[] heartCharms = new[] { "Fragile_Heart", "Unbreakable_Heart" };
        private static readonly string[] strengthCharms = new[] { "Fragile_Strength", "Unbreakable_Strength" };
        private static readonly string[] dreamNails = new[] { "Dream_Nail", "Dream_Gate", "Awoken_Dream_Nail" };
        private static readonly string[] screams = new[] { "Howling_Wraiths", "Abyss_Shriek" };
        private static readonly string[] quakes = new[] { "Desolate_Dive", "Descending_Dark" };
        private static readonly string[] fireballs = new[] { "Vengeful_Spirit", "Shade_Soul" };
        private static readonly string[] dashes = new[] { "Mothwing_Cloak", "Shade_Cloak" };
        private static readonly string[] splitDashes = new[] { "Left_Mothwing_Cloak", "Right_Mothwing_Cloak", "Split_Shade_Cloak" };
        private static readonly string[] rancidEggs = new[] { "Rancid_Egg", "Red_Egg", "Orange_Egg", "Yellow_Egg", "Green_Egg", "Cyan_Egg", "Blue_Egg", "Purple_Egg", "Pink_Egg", "Trans_Egg", "Rainbow_Egg", "Arcane_Eg" };
        private static readonly string[] lanternShards = new[] { "Lantern_Shard", "Final_Lantern_Shard" };
        private static readonly string[][] progressiveItemBuckets = new[] { whiteFragments, greedCharms, heartCharms, strengthCharms, dreamNails, screams, quakes, fireballs, dashes, splitDashes, rancidEggs, lanternShards };
        private static readonly string[] allProgressiveItems = progressiveItemBuckets.SelectMany(x => x).ToArray();
    }
}
