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
        private void UpdateTrackerTransitionTab()
        {
            switch (_settings.SelectedTrackerTransitionGrouping)
            {
                case 2:
                    // Transitions by Rooms by (Map) Area
                    var transitionsByRoomByMapArea = _trackerLogReader.GetTransitionsByRoomByMapArea();
                    UpdateUX(() => UpdateTrackerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByMapArea));
                    break;
                case 3:
                    // Transitions by Rooms by (Titled) Area
                    var transitionsByRoomByTitledArea = _trackerLogReader.GetTransitionsByRoomByTitledArea();
                    UpdateUX(() => UpdateTrackerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByTitledArea));
                    break;
                case 4:
                    // Transitions by Rooms
                    var transitionsByRoom = _trackerLogReader.GetTransitionsByRoom();
                    UpdateUX(() => UpdateTrackerTransitionListWithTransitionsByZone(transitionsByRoom));
                    break;
                case 5:
                    // All transitions
                    var transitions = _trackerLogReader.GetTransitions();
                    UpdateUX(() => UpdateTrackerTransitionListWithTransitions(transitions));
                    break;
                case 1:
                    // Transitions by (Titled) Area
                    var transitionsByTitledArea = _trackerLogReader.GetTransitionsByTitledArea();
                    UpdateUX(() => UpdateTrackerTransitionListWithTransitionsByZone(transitionsByTitledArea));
                    break;
                default:
                    // Transitions by (Map) Area
                    var transitionsByMapArea = _trackerLogReader.GetTransitionsByMapArea();
                    UpdateUX(() => UpdateTrackerTransitionListWithTransitionsByZone(transitionsByMapArea));
                    break;
            }
        }

        #region Tracked Transitions

        #region Transitions by Area OR Room

        private void UpdateTrackerTransitionListWithTransitionsByZone(Dictionary<string, List<TransitionWithDestination>> transitionsByZone)
        {
            TrackerTransitionList.Items.Clear();
            foreach (var zone in transitionsByZone)
            {
                var zoneName = zone.Key.WithoutUnderscores();
                var zoneExpanderName = zoneName.AsObjectName();
                var transitions = zone.Value;
                switch (_settings.SelectedTrackerTransitionOrder)
                {
                    case 1:
                        // Do nothing, the order is originally in time order
                        break;
                    default:
                        transitions = transitions.OrderBy(x => x.Name).ToList();
                        break;
                }
                var expander = new Expander
                {
                    Name = zoneExpanderName,
                    Header = $"{zoneName} [Transitions: {transitions.Count}]",
                    Content = GetTransitionsObject(transitions),
                    IsExpanded = ExpandedTrackedZonesWithTransitions.Contains(zoneExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Remove((e.Source as Expander).Name);
                TrackerTransitionList.Items.Add(expander);
            }
        }

        #endregion

        #region Transitions by Rooms by Area

        private void UpdateTrackerTransitionListWithTransitionsByRoomByArea(Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> transitionsByRoomByArea)
        {
            TrackerTransitionList.Items.Clear();
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();

                var areaName = area.Key.WithoutUnderscores();
                var areaExpanderName = areaName.AsObjectName();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetRoomWithTransitionsExpander(x)));

                var expander = new Expander
                {
                    Name = areaExpanderName,
                    Header = $"{areaName} [Rooms: {rooms.Count} / Transitions: {rooms.Sum(x => x.Value.Count)}]",
                    Content = roomStacker,
                    IsExpanded = ExpandedTrackedZonesWithTransitions.Contains(areaExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Remove((e.Source as Expander).Name);
                TrackerTransitionList.Items.Add(expander);
            }
        }

        private Expander GetRoomWithTransitionsExpander(KeyValuePair<string, List<TransitionWithDestination>> roomWithTransitions)
        {
            var roomName = roomWithTransitions.Key.WithoutUnderscores();
            var expanderName = roomName.AsObjectName();
            var transitions = roomWithTransitions.Value;

            switch (_settings.SelectedTrackerTransitionOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    transitions = transitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            var transitionStacker = GenerateStackPanel();
            transitions.ForEach(y =>
            {
                transitionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()} --> {y.DestinationName.WithoutUnderscores()}" });
            });
            var expander = new Expander
            {
                Name = expanderName,
                Header = $"{roomName}\t[Transitions: {transitions.Count}]",
                Content = GetTransitionsObject(transitions),
                IsExpanded = ExpandedTrackedRoomsWithTransitions.Contains(expanderName)
            };
            expander.Expanded += (object _, RoutedEventArgs e) => ExpandedTrackedRoomsWithTransitions.Add((e.Source as Expander).Name);
            expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedTrackedRoomsWithTransitions.Remove((e.Source as Expander).Name);
            return expander;
        }

        private HashSet<string> ExpandedTrackedRoomsWithTransitions = new HashSet<string>();

        #endregion

        #region Transitions without grouping

        private void UpdateTrackerTransitionListWithTransitions(List<TransitionWithDestination> transitions)
        {
            TrackerTransitionList.Items.Clear();

            var orderedTransitions = transitions.Select(x => x).ToList();

            switch (_settings.SelectedTrackerTransitionOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedTransitions = orderedTransitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            TrackerTransitionList.Items.Add(GetTransitionsObject(orderedTransitions));
        }

        #endregion

        private HashSet<string> ExpandedTrackedZonesWithTransitions = new HashSet<string>();

        private object GetTransitionsObject(List<TransitionWithDestination> transitions)
        {
            var transtionStacker = GenerateStackPanel();
            transitions.ForEach(y => transtionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()} --> {y.DestinationName.WithoutUnderscores()}" }));
            return transtionStacker;
        }

        #endregion

        #region Events

        private void TrackerTransitionGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerTransitionGrouping = (_settings.SelectedTrackerTransitionGrouping + 1) % _helperGroupings.Length;
            TrackerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedTrackerTransitionGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerTransitionOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerTransitionOrder = (_settings.SelectedTrackerTransitionOrder + 1) % _trackerTransitionOrders.Length;
            TrackerTransitionOrder.Content = _trackerTransitionOrders[(int)_settings.SelectedTrackerTransitionOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerTransitionExpand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(TrackerTransitionList);

        private void TrackerTransitionCollapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(TrackerTransitionList);

        #endregion

    }
}
