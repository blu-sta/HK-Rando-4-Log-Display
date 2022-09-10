using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private void UpdateTrackerItemTab()
        {
            switch (_settings.SelectedTrackerItemGrouping)
            {
                case 1:
                    // All items
                    var items = _trackerLogReader.GetItems();
                    UpdateUX(() => UpdateTrackerItemListWithItems(items));
                    // Alphabetical
                    // Find order
                    break;
                default:
                    // Items by Pool
                    var itemsByPool = _trackerLogReader.GetItemsByPool();
                    UpdateUX(() => UpdateTrackerItemListWithItemsByPool(itemsByPool));
                    // Alphabetical
                    // Find order
                    break;
            }
        }

        #region Items

        #region ItemsByPool

        private void UpdateTrackerItemListWithItemsByPool(Dictionary<string, List<ItemWithLocation>> itemsByPool)
        {
            TrackerItemList.Items.Clear();
            foreach (var pool in itemsByPool)
            {
                var poolName = pool.Key.WithoutUnderscores();
                var poolExpanderName = poolName.AsObjectName();
                var items = pool.Value;
                switch (_settings.SelectedTrackerItemOrder)
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
                    Content = GetItemsObject(items),
                    IsExpanded = ExpandedPoolsWithItems.Contains(poolExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedPoolsWithItems.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedPoolsWithItems.Remove((e.Source as Expander).Name);
                TrackerItemList.Items.Add(expander);
            }

        }

        #endregion

        #region Items

        private void UpdateTrackerItemListWithItems(List<ItemWithLocation> items)
        {
            TrackerItemList.Items.Clear();
            var orderedItems = items.Select(x => x).ToList();
            switch (_settings.SelectedTrackerItemOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedItems = orderedItems.OrderBy(x => x.Name).ThenBy(x => x.Location).ToList();
                    break;
            }
            TrackerItemList.Items.Add(GetItemsObject(orderedItems));
        }

        #endregion

        private object GetItemsObject(List<ItemWithLocation> items)
        {
            var itemKvps = items.Select(x => new KeyValuePair<string, string>(x.Name.WithoutUnderscores(), $"found at {x.Location.WithoutUnderscores()}")).ToList();
            return GenerateAutoStarGrid(itemKvps);
        }

        private HashSet<string> ExpandedPoolsWithItems = new HashSet<string>();

        #endregion

        #region Events

        private void TrackerItemGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerItemGrouping = (_settings.SelectedTrackerItemGrouping + 1) % _trackerItemGroupings.Length;
            TrackerItemGrouping.Content = _trackerItemGroupings[(int)_settings.SelectedTrackerItemGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerItemOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerItemOrder = (_settings.SelectedTrackerItemOrder + 1) % _trackerItemOrders.Length;
            TrackerItemOrder.Content = _trackerItemOrders[(int)_settings.SelectedTrackerItemOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerItemExpand_Click(object sender, RoutedEventArgs e)
            => ExpandExpanders(TrackerItemList);

        private void TrackerItemCollapse_Click(object sender, RoutedEventArgs e)
            => CollapseExpanders(TrackerItemList);

        #endregion

    }
}
