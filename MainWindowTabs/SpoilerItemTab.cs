using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly HashSet<string> ExpandedSpoilerPoolsWithItems = new();
        private readonly HashSet<string> ExpandedSpoilerPoolsWithLocations = new();
        private bool _showSpoilerItemObtained = true;
        private int _showSpoilerRoomState = 0;

        private void UpdateSpoilerItemsTab()
        {
            UpdateUX(() =>
            {
                SpoilerItemsList.Items.Clear();

                var spoilerItemGrouping = (PoolGrouping)_appSettings.SelectedSpoilerItemGrouping;
                var spoilerItemOrdering = (SpoilerSorting)_appSettings.SelectedSpoilerItemOrder;

                var trackedItems = _trackerLogReader.GetItems();

                switch (spoilerItemGrouping)
                {
                    case PoolGrouping.CuratedItems:
                        UpdateSpoilerItems(_itemSpoilerReader.GetCuratedItemsByPool(), spoilerItemOrdering, trackedItems);
                        break;
                    case PoolGrouping.AllItems:
                        UpdateSpoilerItems(_itemSpoilerReader.GetItemsByPool(), spoilerItemOrdering, trackedItems);
                        break;
                    case PoolGrouping.AllLocations:
                        UpdateSpoilerLocations(_itemSpoilerReader.GetLocationsByPool(), spoilerItemOrdering, trackedItems);
                        break;
                    case PoolGrouping.None:
                    default:
                        UpdateSpoilerItems(_itemSpoilerReader.GetItems(), spoilerItemOrdering, trackedItems);
                        break;
                }
            });
        }

        private void UpdateSpoilerItems(Dictionary<string, List<SpoilerItemWithLocation>> itemsByPool, SpoilerSorting ordering, List<ItemWithLocation> trackedItems)
        {
            foreach (var pool in itemsByPool)
            {
                SpoilerItemsList.Items.Add(GetSpoilerPoolWithItemsExpander(pool, ExpandedSpoilerPoolsWithItems, ordering, trackedItems));
            }
        }

        private void UpdateSpoilerLocations(Dictionary<string, List<SpoilerItemWithLocation>> locationsByPool, SpoilerSorting ordering, List<ItemWithLocation> trackedItems)
        {
            foreach (var pool in locationsByPool)
            {
                SpoilerItemsList.Items.Add(GetSpoilerPoolWithLocationsExpander(pool, ExpandedSpoilerPoolsWithLocations, ordering, trackedItems));
            }
        }

        private void UpdateSpoilerItems(List<SpoilerItemWithLocation> items, SpoilerSorting ordering, List<ItemWithLocation> trackedItems)
        {
            var orderedItems = ordering switch
            {
                SpoilerSorting.Alpha => items.OrderBy(x => x.Item.Name).ToList(),
                SpoilerSorting.SeedDefault => items.ToList(),
                _ => items.OrderBy(x => x.Item.Name).ToList(),
            };
            var itemsWithLocationGrid = GetSpoiledItemsGrid(orderedItems, trackedItems);
            SpoilerItemsList.Items.Add(itemsWithLocationGrid);
        }

        private Expander GetSpoilerPoolWithItemsExpander(KeyValuePair<string, List<SpoilerItemWithLocation>> poolWithItems, HashSet<string> expandedHashset, SpoilerSorting ordering, List<ItemWithLocation> trackedItems)
        {
            var poolName = poolWithItems.Key.WithoutUnderscores();
            var orderedItems = (PoolGrouping)_appSettings.SelectedSpoilerItemGrouping == PoolGrouping.CuratedItems
                ? poolWithItems.Value.ToList()
                : ordering switch
                {
                    SpoilerSorting.Alpha => poolWithItems.Value.OrderBy(x => x.Item.Name).ThenBy(x => x.Location.Name).ToList(),
                    SpoilerSorting.SeedDefault => poolWithItems.Value.ToList(),
                    _ => poolWithItems.Value.OrderBy(x => x.Item.Name).ToList(),
                };
            var itemsWithLocationGrid = GetSpoiledItemsGrid(orderedItems, trackedItems);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset);
        }

        private Expander GetSpoilerPoolWithLocationsExpander(KeyValuePair<string, List<SpoilerItemWithLocation>> poolWithLocations, HashSet<string> expandedHashset, SpoilerSorting ordering, List<ItemWithLocation> trackedItems)
        {
            var poolName = poolWithLocations.Key.WithoutUnderscores();
            var orderedLocations = ordering switch
            {
                SpoilerSorting.Alpha => poolWithLocations.Value.OrderBy(x => x.Location.Name).ThenBy(x => x.Item.Name).ToList(),
                SpoilerSorting.SeedDefault => poolWithLocations.Value.ToList(),
                _ => poolWithLocations.Value.OrderBy(x => x.Location.Name).ToList(),
            };
            var itemsWithLocationGrid = GetSpoiledLocationsGrid(orderedLocations, trackedItems);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset);
        }

        private Grid GetSpoiledItemsGrid(List<SpoilerItemWithLocation> items, List<ItemWithLocation> trackedItems)
        {
            var itemKvps = items.Select(x =>
            {
                var itemName = x.Item.Name;
                var locationName = x.Location.Name;
                var isTracked = _showSpoilerItemObtained && IsTracked(itemName, locationName, trackedItems);
                return new KeyValuePair<string, string>(
                    $"{(isTracked ? "<s>" : "")}{x.Item.Name.WithoutUnderscores()}",
                    $"{(isTracked ? "<s>" : "")}found at {x.Location.Name.WithoutUnderscores()}" +
                    ((ShowLocationRoom)_showSpoilerRoomState switch
                    {
                        ShowLocationRoom.RoomCode => $" [{x.Location.SceneName}]",
                        ShowLocationRoom.RoomDescription => $" [{x.Location.SceneDescription}]",
                        _ => "",
                    }) +
                    (!string.IsNullOrEmpty(x.Cost) ? $" ({x.Cost})" : "")
                );
            }).ToList();
            return GenerateAutoStarGrid(itemKvps);
        }

        private Grid GetSpoiledLocationsGrid(List<SpoilerItemWithLocation> locations, List<ItemWithLocation> trackedItems)
        {
            var locationKvps = locations.Select((x, i) =>
            {
                var itemName = x.Item.Name;
                var locationName = x.Location.Name;
                var isTracked = _showSpoilerItemObtained && IsTracked(itemName, locationName, trackedItems);
                return new KeyValuePair<string, string>(
                    $"{(isTracked ? "<s>" : "")}{x.Location.Name.WithoutUnderscores()}" +
                    ((ShowLocationRoom)_showSpoilerRoomState switch
                    {
                        ShowLocationRoom.RoomCode => $" [{x.Location.SceneName}]",
                        ShowLocationRoom.RoomDescription => $" [{x.Location.SceneDescription}]",
                        _ => "",
                    }),
                    $"{(isTracked ? "<s>" : "")}provided {x.Item.Name.WithoutUnderscores()}" +
                    (!string.IsNullOrEmpty(x.Cost) ? $" ({x.Cost})" : "")
                );
            }).ToList();
            return GenerateAutoStarGrid(locationKvps);
        }

        private static bool IsTracked(string itemName, string locationName, List<ItemWithLocation> trackedItems)
        {
            var trackedLocations = trackedItems.Where(x => x.Location.Name == locationName).ToList();
            if (!trackedLocations.Any())
                return false;

            var trackedItemWithLocation = trackedLocations.FirstOrDefault(y => y.Item.Name == itemName)
                 ?? GetProgressiveItem(itemName, trackedLocations)
                 ?? GetHunterItem(itemName, trackedLocations);

            if (trackedItemWithLocation != null)
            {
                trackedItems.Remove(trackedItemWithLocation);
                return true; 
            }

            return false;
        }

        private static ItemWithLocation GetProgressiveItem(string itemName, List<ItemWithLocation> trackedLocations) =>
            allProgressiveItems.Contains(itemName)
                ? progressiveItemBuckets.Select(x =>
                        x.Contains(itemName)
                            ? trackedLocations.FirstOrDefault(y => x.Contains(y.Item.Name))
                            : null)
                    .FirstOrDefault(x => x != null)
                : null;

        private static ItemWithLocation GetHunterItem(string itemName, List<ItemWithLocation> trackedLocations) =>
            itemName.StartsWith("Hunter's_Notes") || itemName.StartsWith("Journal_Entry")
                ? trackedLocations.FirstOrDefault(y => y.Item.Name == InvertHunterItemPool(itemName))
                : null;

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

        #region Events

        private void SetSpoilerItemsTabButtonContent()
        {
            Spoiler_Item_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {SpoilerItemGroupingOptions[_appSettings.SelectedSpoilerItemGrouping]}");
            var isCuratedPool = (PoolGrouping)_appSettings.SelectedSpoilerItemGrouping == PoolGrouping.CuratedItems;
            Spoiler_Item_SortBy_Button.Content = !isCuratedPool 
                ? GenerateButtonTextBlock($"Sort: {SpoilerItemOrderingOptions[_appSettings.SelectedSpoilerItemOrder]}")
                : "Sort: Curated";
            Spoiler_Item_SortBy_Button.IsEnabled = !isCuratedPool;
            Spoiler_Item_Obtained_Button.Content = GenerateButtonTextBlock(_showSpoilerItemObtained ? "Obtained: Show" : "Obtained: Hide");
            Spoiler_Item_ShowRoom_Button.Content = GenerateButtonTextBlock($"Room: {SpoilerShowLocationRoomOptions[_showSpoilerRoomState]}");
        }

        private void Spoiler_Item_GroupBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedSpoilerItemGrouping = (_appSettings.SelectedSpoilerItemGrouping + 1) % SpoilerItemGroupingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Item_SortBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedSpoilerItemOrder = (_appSettings.SelectedSpoilerItemOrder + 1) % SpoilerItemOrderingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Item_Obtained_Click(object sender, RoutedEventArgs e)
        {
            _showSpoilerItemObtained = !_showSpoilerItemObtained;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Item_ShowRoom_Click(object sender, RoutedEventArgs e)
        {
            _showSpoilerRoomState = (_showSpoilerRoomState + 1) % SpoilerShowLocationRoomOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Item_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(SpoilerItemsList);
        private void Spoiler_Item_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(SpoilerItemsList);

        #endregion
    }
}
