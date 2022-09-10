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
        private void UpdateSpoilerTransitionTab()
        {
            switch (_settings.SelectedSpoilerTransitionGrouping)
            {
                case 2:
                    // Transitions by Rooms by (Map) Area
                    var transitionsByRoomByMapArea = _transitionSpoilerReader.GetTransitionsByRoomByMapArea();
                    UpdateUX(() => UpdateSpoilerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByMapArea));
                    break;
                case 3:
                    // Transitions by Rooms by (Titled) Area
                    var transitionsByRoomByTitledArea = _transitionSpoilerReader.GetTransitionsByRoomByTitledArea();
                    UpdateUX(() => UpdateSpoilerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByTitledArea));
                    break;
                case 4:
                    // Transitions by Rooms
                    var transitionsByRoom = _transitionSpoilerReader.GetTransitionsByRoom();
                    UpdateUX(() => UpdateSpoilerTransitionListWithTransitionsByZone(transitionsByRoom));
                    break;
                case 5:
                    // All transitions
                    var transitions = _transitionSpoilerReader.GetTransitions();
                    UpdateUX(() => UpdateSpoilerTransitionListWithTransitions(transitions));
                    break;
                case 1:
                    // Transitions by (Titled) Area
                    var transitionsByTitledArea = _transitionSpoilerReader.GetTransitionsByTitledArea();
                    UpdateUX(() => UpdateSpoilerTransitionListWithTransitionsByZone(transitionsByTitledArea));
                    break;
                default:
                    // Transitions by (Map) Area
                    var transitionsByMapArea = _transitionSpoilerReader.GetTransitionsByMapArea();
                    UpdateUX(() => UpdateSpoilerTransitionListWithTransitionsByZone(transitionsByMapArea));
                    break;
            }
        }

        #region Spoiler Transitions

        #region Transitions by Area OR Room

        private void UpdateSpoilerTransitionListWithTransitionsByZone(Dictionary<string, List<TransitionWithDestination>> transitionsByZone)
        {
            SpoilerTransitionList.Items.Clear();
            foreach (var zone in transitionsByZone)
            {
                var zoneName = zone.Key.WithoutUnderscores();
                var zoneExpanderName = zoneName.AsObjectName();
                var transitions = zone.Value;
                switch (_settings.SelectedSpoilerTransitionOrder)
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
                    Content = GetSpoilerTransitionsObject(transitions),
                    IsExpanded = ExpandedSpoilerTrackedZonesWithTransitions.Contains(zoneExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSpoilerTrackedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSpoilerTrackedZonesWithTransitions.Remove((e.Source as Expander).Name);
                SpoilerTransitionList.Items.Add(expander);
            }
        }

        #endregion

        #region Transitions by Rooms by Area

        private void UpdateSpoilerTransitionListWithTransitionsByRoomByArea(Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> transitionsByRoomByArea)
        {
            SpoilerTransitionList.Items.Clear();
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();

                var areaName = area.Key.WithoutUnderscores();
                var areaExpanderName = areaName.AsObjectName();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetSpoilerRoomWithTransitionsExpander(x)));

                var expander = new Expander
                {
                    Name = areaExpanderName,
                    Header = $"{areaName} [Rooms: {rooms.Count} / Transitions: {rooms.Sum(x => x.Value.Count)}]",
                    Content = roomStacker,
                    IsExpanded = ExpandedSpoilerTrackedZonesWithTransitions.Contains(areaExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSpoilerTrackedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSpoilerTrackedZonesWithTransitions.Remove((e.Source as Expander).Name);
                SpoilerTransitionList.Items.Add(expander);
            }
        }

        private Expander GetSpoilerRoomWithTransitionsExpander(KeyValuePair<string, List<TransitionWithDestination>> roomWithTransitions)
        {
            var roomName = roomWithTransitions.Key.WithoutUnderscores();
            var expanderName = roomName.AsObjectName();
            var transitions = roomWithTransitions.Value;

            switch (_settings.SelectedSpoilerTransitionOrder)
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
                Content = GetSpoilerTransitionsObject(transitions),
                IsExpanded = ExpandedSpoilerTrackedRoomsWithTransitions.Contains(expanderName)
            };
            expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSpoilerTrackedRoomsWithTransitions.Add((e.Source as Expander).Name);
            expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSpoilerTrackedRoomsWithTransitions.Remove((e.Source as Expander).Name);
            return expander;
        }

        private HashSet<string> ExpandedSpoilerTrackedRoomsWithTransitions = new HashSet<string>();

        #endregion

        #region Transitions without grouping

        private void UpdateSpoilerTransitionListWithTransitions(List<TransitionWithDestination> transitions)
        {
            SpoilerTransitionList.Items.Clear();

            var orderedTransitions = transitions.Select(x => x).ToList();

            switch (_settings.SelectedSpoilerTransitionOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedTransitions = orderedTransitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            TrackerTransitionList.Items.Add(GetSpoilerTransitionsObject(orderedTransitions));
        }

        #endregion

        private HashSet<string> ExpandedSpoilerTrackedZonesWithTransitions = new HashSet<string>();

        private object GetSpoilerTransitionsObject(List<TransitionWithDestination> transitions)
        {
            var transtionStacker = GenerateStackPanel();
            transitions.ForEach(y => transtionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()} --> {y.DestinationName.WithoutUnderscores()}" }));
            return transtionStacker;
        }

        #endregion

        #region Events

        private void SpoilerTransitionGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedSpoilerTransitionGrouping = (_settings.SelectedSpoilerTransitionGrouping + 1) % _helperGroupings.Length;
            SpoilerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedSpoilerTransitionGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void SpoilerTransitionOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedSpoilerTransitionOrder = (_settings.SelectedSpoilerTransitionOrder + 1) % _spoilerTransitionOrders.Length;
            SpoilerTransitionOrder.Content = _spoilerTransitionOrders[(int)_settings.SelectedSpoilerTransitionOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void SpoilerTransitionExpand_Click(object sender, RoutedEventArgs e) =>
            ExpandExpanders(SpoilerTransitionList);

        private void SpoilerTransitionCollapse_Click(object sender, RoutedEventArgs e) =>
            CollapseExpanders(SpoilerTransitionList);

        #endregion
    }
}
