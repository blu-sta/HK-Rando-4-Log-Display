using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Utils.Utils;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private void UpdateSpoilerItemTab()
        {
            var trackedItems = _trackerLogReader.GetItems();
            switch (_settings.SelectedSpoilerItemGrouping)
            {
                case 1:
                    // All items
                    var items = _itemSpoilerReader.GetItems();
                    UpdateUX(() => UpdateSpoilerItemListWithItems(items, trackedItems));
                    // Alphabetical
                    // Find order
                    break;
                default:
                    // Items by Pool
                    var itemsByPool = _itemSpoilerReader.GetItemsByPool();
                    UpdateUX(() => UpdateSpoilerItemListWithItemsByPool(itemsByPool, trackedItems));
                    // Alphabetical
                    // Find order
                    break;
            }
        }

        #region Items

        #region ItemsByPool

        private void UpdateSpoilerItemListWithItemsByPool(Dictionary<string, List<ItemWithLocation>> itemsByPool, List<ItemWithLocation> trackedItems)
        {
            SpoilerItemList.Items.Clear();
            foreach (var pool in itemsByPool)
            {
                var poolName = pool.Key.WithoutUnderscores();
                var poolExpanderName = poolName.AsObjectName();
                var items = pool.Value;
                switch (_settings.SelectedSpoilerItemOrder)
                {
                    case 1:
                        // Do nothing, the order is originally in time order
                        break;
                    default:
                        items = items.OrderBy(x => x.Name).ThenBy(x => x.Location).ToList();
                        break;
                }
                var expander = new Expander
                {
                    Name = poolExpanderName,
                    Header = poolName,
                    Content = GetSpoilerItemsObject(items, trackedItems),
                    IsExpanded = ExpandedSpoilerPoolsWithItems.Contains(poolExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSpoilerPoolsWithItems.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSpoilerPoolsWithItems.Remove((e.Source as Expander).Name);
                SpoilerItemList.Items.Add(expander);
            }

        }

        #endregion

        #region Items

        private void UpdateSpoilerItemListWithItems(List<ItemWithLocation> items, List<ItemWithLocation> trackedItems)
        {
            SpoilerItemList.Items.Clear();
            var orderedItems = items.Select(x => x).ToList();
            switch (_settings.SelectedSpoilerItemOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedItems = orderedItems.OrderBy(x => x.Name).ThenBy(x => x.Location).ToList();
                    break;
            }
            SpoilerItemList.Items.Add(GetSpoilerItemsObject(orderedItems, trackedItems));
        }

        #endregion

        private object GetSpoilerItemsObject(List<ItemWithLocation> items, List<ItemWithLocation> trackedItems)
        {
            var itemKvps = items.Select(x =>
            {
                var isInTrackerLog = trackedItems.Any(y => y.Name == x.Name && y.Location == x.Location);
                return new KeyValuePair<string, string>(
                    $"{(isInTrackerLog ? "<s>" : "")}{x.Name.WithoutUnderscores()}",
                    $"{(isInTrackerLog ? "<s>" : "")}found at {x.Location.WithoutUnderscores()}"
                );
            }).ToList();
            return GenerateAutoStarGrid(itemKvps);
        }

        private HashSet<string> ExpandedSpoilerPoolsWithItems = new HashSet<string>();

        #endregion

        #region 

        private void SpoilerItemGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedSpoilerItemGrouping = (_settings.SelectedSpoilerItemGrouping + 1) % _spoilerItemGroupings.Length;
            SpoilerItemGrouping.Content = _spoilerItemGroupings[(int)_settings.SelectedSpoilerItemGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void SpoilerItemOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedSpoilerItemOrder = (_settings.SelectedSpoilerItemOrder + 1) % _spoilerItemOrders.Length;
            SpoilerItemOrder.Content = _spoilerItemOrders[(int)_settings.SelectedSpoilerItemOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void SpoilerItemExpand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(SpoilerItemList);

        private void SpoilerItemCollapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(SpoilerItemList);

        #endregion
    }
}
