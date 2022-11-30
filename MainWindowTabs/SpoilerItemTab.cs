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
                SpoilerSorting.SeedDefault => items.ToList(),
                // SpoilerSorting.Alpha
                _ => items.OrderBy(x => x.Item.MWPlayerName).ThenBy(x => x.Item.Name).ThenBy(x => x.Location.MWPlayerName).ThenBy(x => x.Location.Name).ToList(),
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
                    SpoilerSorting.SeedDefault => poolWithItems.Value.ToList(),
                    // SpoilerSorting.Alpha
                    _ => poolWithItems.Value.OrderBy(x => x.Item.MWPlayerName).ThenBy(x => x.Item.Name).ThenBy(x => x.Location.MWPlayerName).ThenBy(x => x.Location.Name).ToList(),
                };
            var itemsWithLocationGrid = GetSpoiledItemsGrid(orderedItems, trackedItems);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset);
        }

        private Expander GetSpoilerPoolWithLocationsExpander(KeyValuePair<string, List<SpoilerItemWithLocation>> poolWithLocations, HashSet<string> expandedHashset, SpoilerSorting ordering, List<ItemWithLocation> trackedItems)
        {
            var poolName = poolWithLocations.Key.WithoutUnderscores();
            var orderedLocations = ordering switch
            {
                SpoilerSorting.SeedDefault => poolWithLocations.Value.ToList(),
                // SpoilerSorting.Alpha
                _ => poolWithLocations.Value.OrderBy(x => x.Location.MWPlayerName).ThenBy(x => x.Location.Name).ThenBy(x => x.Item.MWPlayerName).ThenBy(x => x.Item.Name).ToList(),
            };
            var itemsWithLocationGrid = GetSpoiledLocationsGrid(orderedLocations, trackedItems);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset);
        }

        private Grid GetSpoiledItemsGrid(List<SpoilerItemWithLocation> items, List<ItemWithLocation> trackedItems)
        {
            var itemKvps = items.Select(x =>
            {
                var isTracked = _showSpoilerItemObtained && IsTracked(x.Item, x.Location, trackedItems);
                return new KeyValuePair<string, string>(
                    $"{(isTracked ? "<s>" : "")}{(string.IsNullOrEmpty(x.Item.MWPlayerName) ? "" : $"{x.Item.MWPlayerName}'s ")}{x.Item.Name.WithoutUnderscores()}",
                    $"{(isTracked ? "<s>" : "")}found at {(string.IsNullOrEmpty(x.Location.MWPlayerName) ? "" : $"{x.Location.MWPlayerName}'s ")}{x.Location.Name.WithoutUnderscores()}" +
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
                var isTracked = _showSpoilerItemObtained && IsTracked(x.Item, x.Location, trackedItems);
                return new KeyValuePair<string, string>(
                    $"{(isTracked ? "<s>" : "")}{(string.IsNullOrEmpty(x.Location.MWPlayerName) ? "" : $"{x.Location.MWPlayerName}'s ")}{x.Location.Name.WithoutUnderscores()}" +
                    ((ShowLocationRoom)_showSpoilerRoomState switch
                    {
                        ShowLocationRoom.RoomCode => $" [{x.Location.SceneName}]",
                        ShowLocationRoom.RoomDescription => $" [{x.Location.SceneDescription}]",
                        _ => "",
                    }),
                    $"{(isTracked ? "<s>" : "")}provided {(string.IsNullOrEmpty(x.Item.MWPlayerName) ? "" : $"{x.Item.MWPlayerName}'s ")}{x.Item.Name.WithoutUnderscores()}" +
                    (!string.IsNullOrEmpty(x.Cost) ? $" ({x.Cost})" : "")
                );
            }).ToList();
            return GenerateAutoStarGrid(locationKvps);
        }

        private static bool IsTracked(Item item, Location location, List<ItemWithLocation> trackedItems)
        {
            var trackedLocations = trackedItems.Where(x => x.Location.Name == location.Name && x.Location.MWPlayerName == location.MWPlayerName).ToList();
            if (!trackedLocations.Any())
                return false;

            var trackedItemWithLocation = TrackedSpoilerItems.GetTrackedItemWithLocation(item, trackedLocations);

            if (trackedItemWithLocation != null)
            {
                trackedItems.Remove(trackedItemWithLocation);
                return true; 
            }

            return false;
        }


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
