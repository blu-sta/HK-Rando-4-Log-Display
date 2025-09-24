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
        private int _showSpoilerRoomState = 0;

        private void UpdateSpoilerItemsTab()
        {
            UpdateUX(() =>
            {
                SpoilerItemsList.Items.Clear();

                var trackedItems = _trackerLogReader.GetItems();

                switch ((PoolGrouping)_appSettings.SelectedSpoilerItemGrouping)
                {
                    case PoolGrouping.CuratedItems:
                        UpdateSpoilerItems(_itemSpoilerReader.GetCuratedItemsByPool(), trackedItems);
                        break;
                    case PoolGrouping.AllItems:
                        UpdateSpoilerItems(_itemSpoilerReader.GetItemsByPool(), trackedItems);
                        break;
                    case PoolGrouping.AllLocations:
                        UpdateSpoilerLocations(_itemSpoilerReader.GetLocationsByPool(), trackedItems);
                        break;
                    case PoolGrouping.None:
                    default:
                        UpdateSpoilerItems(_itemSpoilerReader.GetItems(), trackedItems);
                        break;
                }
            });
        }

        private void UpdateSpoilerItems(Dictionary<string, List<SpoilerItemWithLocation>> itemsByPool, List<ItemWithLocation> trackedItems)
        {
            foreach (var pool in itemsByPool)
            {
                SpoilerItemsList.Items.Add(GetSpoilerPoolWithItemsExpander(pool, ExpandedSpoilerPoolsWithItems, trackedItems));
            }
        }

        private void UpdateSpoilerLocations(Dictionary<string, List<SpoilerItemWithLocation>> locationsByPool, List<ItemWithLocation> trackedItems)
        {
            foreach (var pool in locationsByPool)
            {
                SpoilerItemsList.Items.Add(GetSpoilerPoolWithLocationsExpander(pool, ExpandedSpoilerPoolsWithLocations, trackedItems));
            }
        }

        private void UpdateSpoilerItems(List<SpoilerItemWithLocation> items, List<ItemWithLocation> trackedItems)
        {
            var orderedItems = (SpoilerSorting)_appSettings.SelectedSpoilerItemOrder switch
            {
                SpoilerSorting.SeedDefault => items.ToList(),
                // SpoilerSorting.Alpha
                _ => items.OrderBy(x => x.Item.MWPlayerName).ThenBy(x => x.Item.Name).ThenBy(x => x.Location.MWPlayerName).ThenBy(x => x.Location.Name).ToList(),
            };
            var spoilerItemsWithTracking = GetSpoilerItemsWithTracking(orderedItems, trackedItems);
            var itemCount = spoilerItemsWithTracking.Count;
            var obtainedCount = spoilerItemsWithTracking.Count(x => x.IsObtained);
            var itemsWithLocationGrid = GetSpoiledItemsGrid(spoilerItemsWithTracking);
            SpoilerItemsList.Items.Add(itemsWithLocationGrid);
            if ((SpoilerObtainedOrTraversedDisplay)_appSettings.SelectedSpoilerObtainedDisplay == SpoilerObtainedOrTraversedDisplay.Hide
                && obtainedCount > 0
                && obtainedCount != itemCount)
            {
                SpoilerItemsList.Items.Add(GenerateAutoStarGrid([new KeyValuePair<string, string>($"+{obtainedCount} obtained", "")]));
            }
        }

        private Expander GetSpoilerPoolWithItemsExpander(KeyValuePair<string, List<SpoilerItemWithLocation>> poolWithItems, HashSet<string> expandedHashset, List<ItemWithLocation> trackedItems)
        {
            var poolName = poolWithItems.Key.WithoutUnderscores();
            var orderedItems = (PoolGrouping)_appSettings.SelectedSpoilerItemGrouping == PoolGrouping.CuratedItems
                ? poolWithItems.Value.ToList()
                : (SpoilerSorting)_appSettings.SelectedSpoilerItemOrder switch
                {
                    SpoilerSorting.SeedDefault => poolWithItems.Value.ToList(),
                    // SpoilerSorting.Alpha
                    _ => poolWithItems.Value.OrderBy(x => x.Item.MWPlayerName).ThenBy(x => x.Item.Name).ThenBy(x => x.Location.MWPlayerName).ThenBy(x => x.Location.Name).ToList(),
                };
            var spoilerItemsWithTracking = GetSpoilerItemsWithTracking(orderedItems, trackedItems);
            var itemCount = spoilerItemsWithTracking.Count;
            var obtainedCount = spoilerItemsWithTracking.Count(x => x.IsObtained);
            var itemsWithLocationGrid = GetSpoiledItemsGrid(spoilerItemsWithTracking);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset, $"[Obtained: {obtainedCount}/{itemCount}]");
        }

        private Expander GetSpoilerPoolWithLocationsExpander(KeyValuePair<string, List<SpoilerItemWithLocation>> poolWithLocations, HashSet<string> expandedHashset, List<ItemWithLocation> trackedItems)
        {
            var poolName = poolWithLocations.Key.WithoutUnderscores();
            var orderedLocations = (SpoilerSorting)_appSettings.SelectedSpoilerItemOrder switch
            {
                SpoilerSorting.SeedDefault => poolWithLocations.Value.ToList(),
                // SpoilerSorting.Alpha
                _ => poolWithLocations.Value.OrderBy(x => x.Location.MWPlayerName).ThenBy(x => x.Location.Name).ThenBy(x => x.Item.MWPlayerName).ThenBy(x => x.Item.Name).ToList(),
            };
            var spoilerLocationsWithTracking = GetSpoilerItemsWithTracking(orderedLocations, trackedItems);
            var locationCount = spoilerLocationsWithTracking.Count;
            var obtainedCount = spoilerLocationsWithTracking.Count(x => x.IsObtained);
            var itemsWithLocationGrid = GetSpoiledLocationKvps(spoilerLocationsWithTracking);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset, $"[Obtained: {obtainedCount}/{locationCount}]");
        }

        private List<SpoilerItemWithLocation> GetSpoilerItemsWithTracking(List<SpoilerItemWithLocation> spoilerItems, List<ItemWithLocation> trackedItems) =>
            [
                .. spoilerItems
                    .Select(x => IsTrackedItem(x.Item, x.Location, trackedItems)
                        ? new SpoilerItemWithLocation (x) { IsObtained = true }
                        : x),
            ];

        private static bool IsTrackedItem(Item item, Location location, List<ItemWithLocation> trackedItems)
        {
            var trackedLocations = trackedItems.Where(x => x.Location.Name == location.Name && x.Location.MWPlayerName == location.MWPlayerName).ToList();
            if (trackedLocations.Count == 0)
            {
                return false;
            }

            var trackedItemWithLocation = TrackedSpoilerItems.GetTrackedItemWithLocation(item, trackedLocations);

            if (trackedItemWithLocation != null)
            {
                trackedItems.Remove(trackedItemWithLocation);
            }

            return trackedItemWithLocation != null;
        }

        private Grid GetSpoiledItemsGrid(List<SpoilerItemWithLocation> items)
        {
            var obtainedDisplayMode = (SpoilerObtainedOrTraversedDisplay)_appSettings.SelectedSpoilerObtainedDisplay;
            var itemKvps = items.Where(x => obtainedDisplayMode != SpoilerObtainedOrTraversedDisplay.Hide || !x.IsObtained).Select(x =>
                {
                    var isStrikethrough = obtainedDisplayMode == SpoilerObtainedOrTraversedDisplay.Mark && x.IsObtained;
                    return new KeyValuePair<string, string>(
                            $"{(isStrikethrough ? "<s>" : "")}{(string.IsNullOrEmpty(x.Item.MWPlayerName) ? "" : $"{x.Item.MWPlayerName}'s ")}{x.Item.Name.WithoutUnderscores()}",
                            $"{(isStrikethrough ? "<s>" : "")}found at {(string.IsNullOrEmpty(x.Location.MWPlayerName) ? "" : $"{x.Location.MWPlayerName}'s ")}{x.Location.Name.WithoutUnderscores()}" +
                                ((ShowLocationRoom)_showSpoilerRoomState switch
                                {
                                    ShowLocationRoom.RoomCode => $" [{x.Location.SceneName}]",
                                    ShowLocationRoom.RoomDescription => $" [{x.Location.SceneDescription}]",
                                    _ => "",
                                }) +
                                (!string.IsNullOrEmpty(x.Cost) ? $" ({x.Cost})" : ""));
                }).ToList();

            if (itemKvps.Count == 0)
            {
                itemKvps.Add(new KeyValuePair<string, string>("All obtained", ""));
            }

            return GenerateAutoStarGrid(itemKvps);
        }

        private Grid GetSpoiledLocationKvps(List<SpoilerItemWithLocation> locations)
        {
            var obtainedDisplayMode = (SpoilerObtainedOrTraversedDisplay)_appSettings.SelectedSpoilerObtainedDisplay;
            var locationKvps = locations.Where(x => obtainedDisplayMode != SpoilerObtainedOrTraversedDisplay.Hide || !x.IsObtained).Select(x =>
                {
                    var isStrikethrough = obtainedDisplayMode == SpoilerObtainedOrTraversedDisplay.Mark && x.IsObtained;
                    return new KeyValuePair<string, string>(
                            $"{(isStrikethrough ? "<s>" : "")}{(string.IsNullOrEmpty(x.Location.MWPlayerName) ? "" : $"{x.Location.MWPlayerName}'s ")}{x.Location.Name.WithoutUnderscores()}" +
                                ((ShowLocationRoom)_showSpoilerRoomState switch
                                {
                                    ShowLocationRoom.RoomCode => $" [{x.Location.SceneName}]",
                                    ShowLocationRoom.RoomDescription => $" [{x.Location.SceneDescription}]",
                                    _ => "",
                                }),
                            $"{(isStrikethrough ? "<s>" : "")}provided {(string.IsNullOrEmpty(x.Item.MWPlayerName) ? "" : $"{x.Item.MWPlayerName}'s ")}{x.Item.Name.WithoutUnderscores()}" +
                                (!string.IsNullOrEmpty(x.Cost) ? $" ({x.Cost})" : ""));
                }).ToList();

            if (locationKvps.Count == 0)
            {
                locationKvps.Add(new KeyValuePair<string, string>("All obtained", ""));
            }

            return GenerateAutoStarGrid(locationKvps);
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
            Spoiler_Item_Obtained_Button.Content = GenerateButtonTextBlock($"Obtained: {SpoilerObtainedDisplayOptions[_appSettings.SelectedSpoilerObtainedDisplay]}");
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

        private void Spoiler_Item_Obtained_Display_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedSpoilerObtainedDisplay = (_appSettings.SelectedSpoilerObtainedDisplay + 1) % SpoilerObtainedDisplayOptions.Length;
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
