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
        private void UpdateHelperTransitionTab()
        {
            switch (_settings.SelectedHelperTransitionGrouping)
            {
                case 2:
                    // Transitions by Rooms by (Map) Area
                    var transitionsByRoomByMapArea = _helperLogReader.GetTransitionsByRoomByMapArea();
                    UpdateUX(() => UpdateHelperTransitionListWithTransitionsByRoomByArea(transitionsByRoomByMapArea));
                    break;
                case 3:
                    // Transitions by Rooms by (Titled) Area
                    var transitionsByRoomByTitledArea = _helperLogReader.GetTransitionsByRoomByTitledArea();
                    UpdateUX(() => UpdateHelperTransitionListWithTransitionsByRoomByArea(transitionsByRoomByTitledArea));
                    break;
                case 4:
                    // Transitions by Rooms
                    var transitionsByRoom = _helperLogReader.GetTransitionsByRoom();
                    UpdateUX(() => UpdateHelperTransitionListWithTransitionsByZone(transitionsByRoom));
                    break;
                case 5:
                    // All transitions
                    var transitions = _helperLogReader.GetTransitions();
                    UpdateUX(() => UpdateHelperTransitionListWithTransitions(transitions));
                    break;
                case 1:
                    // Transitions by (Titled) Area
                    var transitionsByTitledArea = _helperLogReader.GetTransitionsByTitledArea();
                    UpdateUX(() => UpdateHelperTransitionListWithTransitionsByZone(transitionsByTitledArea));
                    break;
                default:
                    // Transitions by (Map) Area
                    var transitionsByMapArea = _helperLogReader.GetTransitionsByMapArea();
                    UpdateUX(() => UpdateHelperTransitionListWithTransitionsByZone(transitionsByMapArea));
                    break;
            }
        }

        #region Transitions

        #region Transitions by Area OR Room

        private void UpdateHelperTransitionListWithTransitionsByZone(Dictionary<string, List<TransitionWithTime>> transitionsByZone)
        {
            HelperTransitionList.Items.Clear();
            foreach (var zone in transitionsByZone)
            {
                var zoneName = zone.Key.WithoutUnderscores();
                var zoneExpanderName = zoneName.AsObjectName();
                var transitions = zone.Value;
                switch (_settings.SelectedHelperTransitionOrder)
                {
                    case 2:
                        transitions = transitions.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList();
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
                    IsExpanded = ExpandedZonesWithTransitions.Contains(zoneExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedZonesWithTransitions.Remove((e.Source as Expander).Name);
                HelperTransitionList.Items.Add(expander);
            }
        }

        #endregion

        #region Transitions by Rooms by Area

        private void UpdateHelperTransitionListWithTransitionsByRoomByArea(Dictionary<string, Dictionary<string, List<TransitionWithTime>>> transitionsByRoomByArea)
        {
            HelperTransitionList.Items.Clear();
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
                    IsExpanded = ExpandedZonesWithTransitions.Contains(areaExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedZonesWithTransitions.Remove((e.Source as Expander).Name);
                HelperTransitionList.Items.Add(expander);
            }
        }

        private Expander GetRoomWithTransitionsExpander(KeyValuePair<string, List<TransitionWithTime>> roomWithTransitions)
        {
            var roomName = roomWithTransitions.Key.WithoutUnderscores();
            var expanderName = roomName.AsObjectName();
            var transitions = roomWithTransitions.Value;

            switch (_settings.SelectedHelperTransitionOrder)
            {
                case 2:
                    transitions = transitions.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList();
                    break;
                default:
                    transitions = transitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            var transitionStacker = GenerateStackPanel();
            transitions.ForEach(y =>
            {
                switch (_settings.SelectedHelperTransitionOrder)
                {
                    case 2:
                        transitionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()}\t{GetAgeInMinutes(_referenceTime, y.TimeAdded)}" });
                        break;
                    default:
                        transitionStacker.Children.Add(new TextBlock { Text = y.Name.WithoutUnderscores() });
                        break;
                }
            });
            var expander = new Expander
            {
                Name = expanderName,
                Header = $"{roomName}\t[Transitions: {transitions.Count}]",
                Content = GetTransitionsObject(transitions),
                IsExpanded = ExpandedRoomsWithTransitions.Contains(expanderName)
            };
            expander.Expanded += (object _, RoutedEventArgs e) => ExpandedRoomsWithTransitions.Add((e.Source as Expander).Name);
            expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedRoomsWithTransitions.Remove((e.Source as Expander).Name);
            return expander;
        }

        private HashSet<string> ExpandedRoomsWithTransitions = new HashSet<string>();

        #endregion

        #region Transitions without grouping

        private void UpdateHelperTransitionListWithTransitions(List<TransitionWithTime> transitions)
        {
            HelperTransitionList.Items.Clear();

            var orderedTransitions = transitions.Select(x => x).ToList();

            switch (_settings.SelectedHelperTransitionOrder)
            {
                case 2:
                    orderedTransitions = orderedTransitions.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList();
                    break;
                default:
                    orderedTransitions = orderedTransitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            HelperTransitionList.Items.Add(GetTransitionsObject(orderedTransitions));
        }

        #endregion

        private HashSet<string> ExpandedZonesWithTransitions = new HashSet<string>();

        private object GetTransitionsObject(List<TransitionWithTime> transitions)
        {
            switch (_settings.SelectedHelperTransitionOrder)
            {
                case 1:
                case 2:
                    var transitionKvps = transitions.ToDictionary(x => $"{(x.IsOutOfLogic ? "*" : "")}{x.Name.WithoutUnderscores()}", x => GetAgeInMinutes(_referenceTime, x.TimeAdded)).ToList();
                    return GenerateAutoStarGrid(transitionKvps);
                default:
                    var transtionStacker = GenerateStackPanel();
                    transitions.ForEach(y => transtionStacker.Children.Add(new TextBlock { Text = $"{(y.IsOutOfLogic ? "*" : "")}{y.Name.WithoutUnderscores()}" }));
                    return transtionStacker;
            }
        }

        #endregion

        #region Events

        private void HelperTransitionGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedHelperTransitionGrouping = (_settings.SelectedHelperTransitionGrouping + 1) % _helperGroupings.Length;
            HelperTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedHelperTransitionGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void HelperTransitionOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedHelperTransitionOrder = (_settings.SelectedHelperTransitionOrder + 1) % _helperOrders.Length;
            HelperTransitionOrder.Content = _helperOrders[(int)_settings.SelectedHelperTransitionOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void HelperTransitionExpand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(HelperTransitionList);

        private void HelperTransitionCollapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(HelperTransitionList);

        #endregion

    }
}
