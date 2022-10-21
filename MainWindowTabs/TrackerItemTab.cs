using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Constants.Constants;
using static HK_Rando_4_Log_Display.Utils.Utils;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly HashSet<string> ExpandedPoolsWithItems = new();
        private readonly HashSet<string> ExpandedPoolsWithLocations = new();
        private bool _showTrackerItemsTime = true;

        private void UpdateTrackerItemsTab()
        {
            UpdateUX(() =>
            {
                TrackerItemsList.Items.Clear();

                var trackerItemGrouping = (PoolGrouping)_appSettings.SelectedTrackerItemGrouping;
                var trackerItemOrdering = (Sorting)_appSettings.SelectedTrackerItemOrder;

                switch (trackerItemGrouping)
                {
                    case PoolGrouping.CuratedItems:
                        UpdateTrackerItems(_trackerLogReader.GetCuratedItemsByPool(), trackerItemOrdering);
                        break;
                    case PoolGrouping.AllItems:
                        UpdateTrackerItems(_trackerLogReader.GetItemsByPool(), trackerItemOrdering);
                        break;
                    case PoolGrouping.AllLocations:
                        UpdateTrackerLocations(_trackerLogReader.GetLocationsByPool(), trackerItemOrdering);
                        break;
                    case PoolGrouping.None:
                    default:
                        UpdateTrackerItems(_trackerLogReader.GetItems(), trackerItemOrdering);
                        break;
                }
            });
        }

        private void UpdateTrackerItems(Dictionary<string, List<ItemWithLocation>> itemsByPool, Sorting ordering)
        {
            foreach (var pool in itemsByPool)
            {
                TrackerItemsList.Items.Add(GetPoolWithItemsExpander(pool, ExpandedPoolsWithItems, ordering));
            }
        }

        private void UpdateTrackerLocations(Dictionary<string, List<ItemWithLocation>> locationsByPool, Sorting ordering)
        {
            foreach (var pool in locationsByPool)
            {
                TrackerItemsList.Items.Add(GetPoolWithLocationsExpander(pool, ExpandedPoolsWithLocations, ordering));
            }
        }

        private void UpdateTrackerItems(List<ItemWithLocation> items, Sorting ordering)
        {
            var orderedItems = ordering switch
            {
                Sorting.Alpha => items.OrderBy(x => x.Item.Name).ToList(),
                Sorting.Time => items.OrderBy(x => x.TimeAdded).ToList(),
                _ => items.OrderBy(x => x.Item.Name).ToList(),
            };
            var itemsWithLocationGrid = GetTrackedItemsGrid(orderedItems);
            TrackerItemsList.Items.Add(itemsWithLocationGrid);
        }

        private Expander GetPoolWithItemsExpander(KeyValuePair<string, List<ItemWithLocation>> poolWithItems, HashSet<string> expandedHashset, Sorting ordering)
        {
            var poolName = poolWithItems.Key.WithoutUnderscores();
            var orderedItems = (PoolGrouping)_appSettings.SelectedTrackerItemGrouping == PoolGrouping.CuratedItems
                ? poolWithItems.Value.ToList()
                : ordering switch
                {
                    Sorting.Alpha => poolWithItems.Value.OrderBy(x => x.Item.Name).ThenBy(x => x.Location.Name).ToList(),
                    Sorting.Time => poolWithItems.Value.OrderBy(x => x.TimeAdded).ToList(),
                    _ => poolWithItems.Value.OrderBy(x => x.Item.Name).ToList(),
                };
            var itemsWithLocationGrid = GetTrackedItemsGrid(orderedItems);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset, $"[Obtained: {orderedItems.Count}]");
        }

        private Expander GetPoolWithLocationsExpander(KeyValuePair<string, List<ItemWithLocation>> poolWithLocations, HashSet<string> expandedHashset, Sorting ordering)
        {
            var poolName = poolWithLocations.Key.WithoutUnderscores();
            var orderedLocations = ordering switch
            {
                Sorting.Alpha => poolWithLocations.Value.OrderBy(x => x.Location.Name).ThenBy(x => x.Item.Name).ToList(),
                Sorting.Time => poolWithLocations.Value.OrderBy(x => x.TimeAdded).ToList(),
                _ => poolWithLocations.Value.OrderBy(x => x.Location.Name).ToList(),
            };
            var itemsWithLocationGrid = GetTrackedLocationsGrid(orderedLocations);
            return GenerateExpanderWithContent(poolName, itemsWithLocationGrid, expandedHashset, $"[Provided: {orderedLocations.Count}]");
        }

        private Grid GetTrackedItemsGrid(List<ItemWithLocation> items)
        {
            var itemKvps = items.Select(x =>
                new KeyValuePair<string, string>($"{x.Item.Name.WithoutUnderscores()}",
                $"found at {x.Location.Name.WithoutUnderscores()}{(_showTrackerItemsTime ? $" ({GetAgeInMinutes(_referenceTime, x.TimeAdded)})" : "")}")
            ).ToList();
            return GenerateAutoStarGrid(itemKvps);
        }

        private Grid GetTrackedLocationsGrid(List<ItemWithLocation> locations)
        {
            var locationKvps = locations.Select(x =>
                new KeyValuePair<string, string>($"{x.Location.Name.WithoutUnderscores()}",
                $"provided {x.Item.Name.WithoutUnderscores()}{(_showTrackerItemsTime ? $" ({GetAgeInMinutes(_referenceTime, x.TimeAdded)})" : "")}")
            ).ToList();
            return GenerateAutoStarGrid(locationKvps);
        }

        #region Events

        private void SetTrackerItemsTabButtonContent()
        {
            Tracker_Item_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {TrackerItemGroupingOptions[_appSettings.SelectedTrackerItemGrouping]}");
            var isCuratedPool = (PoolGrouping)_appSettings.SelectedTrackerItemGrouping == PoolGrouping.CuratedItems;
            Tracker_Item_SortBy_Button.Content = !isCuratedPool
                ? GenerateButtonTextBlock($"Sort: {TrackerItemOrderingOptions[_appSettings.SelectedTrackerItemOrder]}") 
                : "Sort: Curated";
            Tracker_Item_SortBy_Button.IsEnabled = !isCuratedPool;
            Tracker_Item_Time_Button.Content = GenerateButtonTextBlock(_showTrackerItemsTime ? "Time: Show" : "Time: Hide");
        }

        private void Tracker_Item_GroupBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedTrackerItemGrouping = (_appSettings.SelectedTrackerItemGrouping + 1) % TrackerItemGroupingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Item_SortBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedTrackerItemOrder = (_appSettings.SelectedTrackerItemOrder + 1) % TrackerItemOrderingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Item_Time_Click(object sender, RoutedEventArgs e)
        {
            _showTrackerItemsTime = !_showTrackerItemsTime;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Item_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            _trackerLogReader.OpenFile();
        }

        private void Tracker_Item_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(TrackerItemsList);
        private void Tracker_Item_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(TrackerItemsList);

        #endregion

    }
}
