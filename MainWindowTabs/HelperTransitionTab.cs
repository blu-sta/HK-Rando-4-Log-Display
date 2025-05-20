using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Constants.Constants;
using static HK_Rando_4_Log_Display.Utils.Utils;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly HashSet<string> ExpandedRoomsWithTransitions = new();
        private readonly HashSet<string> ExpandedZonesWithTransitions = new();
        private bool _showHelperTransitionsTime = true;
        private bool _showHelperTransitionSceneDescriptions = false;

        private void UpdateHelperTransitionsTab()
        {
            UpdateUX(() =>
            {
                HelperTransitionsList.Items.Clear();

                var helperTransitionGrouping = (RoomGrouping)_appSettings.SelectedHelperTransitionGrouping;
                var helperTransitionOrdering = (Sorting)_appSettings.SelectedHelperTransitionOrder;

                switch (helperTransitionGrouping)
                {
                    case RoomGrouping.MapArea:
                        UpdateHelperTransitions(_helperLogReader.GetTransitionsByMapArea(), helperTransitionOrdering);
                        break;
                    case RoomGrouping.TitleArea:
                        UpdateHelperTransitions(_helperLogReader.GetTransitionsByTitledArea(), helperTransitionOrdering);
                        break;
                    case RoomGrouping.RoomMapArea:
                        UpdateHelperTransitions(_helperLogReader.GetTransitionsByRoomByMapArea(_showHelperTransitionSceneDescriptions), helperTransitionOrdering);
                        break;
                    case RoomGrouping.RoomTitleArea:
                        UpdateHelperTransitions(_helperLogReader.GetTransitionsByRoomByTitledArea(_showHelperTransitionSceneDescriptions), helperTransitionOrdering);
                        break;
                    case RoomGrouping.Room:
                        UpdateHelperTransitions(_helperLogReader.GetTransitionsByRoom(_showHelperTransitionSceneDescriptions), helperTransitionOrdering);
                        break;
                    case RoomGrouping.None:
                    default:
                        UpdateHelperTransitions(_helperLogReader.GetTransitions(), helperTransitionOrdering);
                        break;
                };
            });
        }

        private void UpdateHelperTransitions(Dictionary<string, List<Transition>> transitionsByArea, Sorting ordering)
        {
            foreach (var area in transitionsByArea)
            {
                HelperTransitionsList.Items.Add(GetZoneWithTransitionsExpander(area, ExpandedZonesWithTransitions, ordering));
            }
        }

        private void UpdateHelperTransitions(Dictionary<string, Dictionary<string, List<Transition>>> transitionsByRoomByArea, Sorting ordering)
        {
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();
                var areaName = area.Key.WithoutUnderscores();
                var roomsWithTransitions = area.Value.OrderBy(x => x.Key).ToList();
                roomsWithTransitions.ForEach(roomWithTransitions =>
                {
                    roomStacker.Children.Add(GetZoneWithTransitionsExpander(roomWithTransitions, ExpandedRoomsWithTransitions, ordering));
                });
                var areaExpander = GenerateExpanderWithContent(areaName, roomStacker, ExpandedZonesWithTransitions, $"[Rooms: {roomsWithTransitions.Count} / Transitions: {roomsWithTransitions.Sum(x => x.Value.Count)}]");
                HelperTransitionsList.Items.Add(areaExpander);
            }
        }

        private void UpdateHelperTransitions(List<Transition> transitions, Sorting ordering)
        {
            var orderedTransitions = ordering switch
            {
                Sorting.Alpha => transitions.OrderBy(x => x.Name).ToList(),
                Sorting.Time => transitions.OrderBy(x => x.TimeAdded).ToList(),
                _ => transitions.OrderBy(x => x.Name).ToList(),
            };
            var transitionsGrid = GetTransitionsGrid(orderedTransitions);
            HelperTransitionsList.Items.Add(transitionsGrid);
        }

        private Expander GetZoneWithTransitionsExpander(KeyValuePair<string, List<Transition>> zoneWithTransitions, HashSet<string> expandedHashset, Sorting ordering)
        {
            var zoneName = zoneWithTransitions.Key.WithoutUnderscores();
            var orderedTransitions = ordering switch
            {
                Sorting.Alpha => zoneWithTransitions.Value.OrderBy(x => x.Name).ToList(),
                Sorting.Time => zoneWithTransitions.Value.OrderBy(x => x.TimeAdded).ToList(),
                _ => zoneWithTransitions.Value.OrderBy(x => x.Name).ToList(),
            };
            var transitionsGrid = GetTransitionsGrid(orderedTransitions);
            return GenerateExpanderWithContent(zoneName, transitionsGrid, expandedHashset, $"[Transitions: {orderedTransitions.Count}]");
        }

        private Grid GetTransitionsGrid(List<Transition> transitions)
        {
            var transitionKvps = transitions.ToDictionary(
                x => TransitionStringBuilder(x),
                x => _showHelperTransitionsTime ? GetAgeInMinutes(_referenceTime, x.TimeAdded) : ""
                ).ToList();
            return GenerateAutoStarGrid(transitionKvps);
        }

        private string TransitionStringBuilder(Transition x)
        {
            var s = new StringBuilder();

            if (x.IsOutOfLogic)
                s.Append('*');

            s.Append(_showHelperTransitionSceneDescriptions ? $"{x.SceneDescription}[{x.DoorName}]" : x.Name);

            return s.ToString().WithoutUnderscores();
        }

        #region Events

        private void SetHelperTransitionsTabButtonContent()
        {
            Helper_Transition_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {HelperTransitionGroupingOptions[_appSettings.SelectedHelperTransitionGrouping]}");
            Helper_Transition_SortBy_Button.Content = GenerateButtonTextBlock($"Sort: {HelperTransitionOrderingOptions[_appSettings.SelectedHelperTransitionOrder]}");
            Helper_Transition_Time_Button.Content = GenerateButtonTextBlock(_showHelperTransitionsTime ? "Time: Show" : "Time: Hide");
            Helper_Transition_RoomDisplay_Button.Content = GenerateButtonTextBlock(_showHelperTransitionSceneDescriptions ? "Room: Desc." : "Room: Code");
        }

        private void Helper_Transition_GroupBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedHelperTransitionGrouping = (_appSettings.SelectedHelperTransitionGrouping + 1) % HelperTransitionGroupingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Transition_SortBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedHelperTransitionOrder = (_appSettings.SelectedHelperTransitionOrder + 1) % HelperTransitionOrderingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Transition_Time_Click(object sender, RoutedEventArgs e)
        {
            _showHelperTransitionsTime = !_showHelperTransitionsTime;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Transition_RoomDisplay_Click(object sender, RoutedEventArgs e)
        {
            _showHelperTransitionSceneDescriptions = !_showHelperTransitionSceneDescriptions;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Transition_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(HelperTransitionsList);
        private void Helper_Transition_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(HelperTransitionsList);

        #endregion

    }
}
