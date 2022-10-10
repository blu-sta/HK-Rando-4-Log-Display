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
        private readonly HashSet<string> ExpandedTrackedRoomsWithTransitions = new();
        private readonly HashSet<string> ExpandedTrackedZonesWithTransitions = new();
        private bool _showTrackerTransitionsTime = true;
        private bool _useTrackerTransitionsDestination = false;

        private void UpdateTrackerTransitionsTab()
        {
            UpdateUX(() =>
            {
                TrackerTransitionsList.Items.Clear();

                var trackerTransitionGrouping = (RoomGrouping)_appSettings.SelectedTrackerTransitionGrouping;
                var trackerTransitionOrdering = (Sorting)_appSettings.SelectedTrackerTransitionOrder;

                switch (trackerTransitionGrouping)
                {
                    case RoomGrouping.MapArea:
                        UpdateTrackerTransitions(_trackerLogReader.GetTransitionsByMapArea(_useTrackerTransitionsDestination), trackerTransitionOrdering);
                        break;
                    case RoomGrouping.TitleArea:
                        UpdateTrackerTransitions(_trackerLogReader.GetTransitionsByTitledArea(_useTrackerTransitionsDestination), trackerTransitionOrdering);
                        break;
                    case RoomGrouping.RoomMapArea:
                        UpdateTrackerTransitions(_trackerLogReader.GetTransitionsByRoomByMapArea(_useTrackerTransitionsDestination), trackerTransitionOrdering);
                        break;
                    case RoomGrouping.RoomTitleArea:
                        UpdateTrackerTransitions(_trackerLogReader.GetTransitionsByRoomByTitledArea(_useTrackerTransitionsDestination), trackerTransitionOrdering);
                        break;
                    case RoomGrouping.Room:
                        UpdateTrackerTransitions(_trackerLogReader.GetTransitionsByRoom(_useTrackerTransitionsDestination), trackerTransitionOrdering);
                        break;
                    case RoomGrouping.None:
                    default:
                        UpdateTrackerTransitions(_trackerLogReader.GetTransitions(), trackerTransitionOrdering);
                        break;
                };
            });
        }

        private void UpdateTrackerTransitions(Dictionary<string, List<TransitionWithDestination>> transitionsByArea, Sorting ordering)
        {
            foreach (var area in transitionsByArea)
            {
                TrackerTransitionsList.Items.Add(GetZoneWithTransitionsExpander(area, ExpandedTrackedZonesWithTransitions, ordering));
            }
        }

        private void UpdateTrackerTransitions(Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> transitionsByRoomByArea, Sorting ordering)
        {
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();
                var areaName = area.Key.WithoutUnderscores();
                var roomsWithTransitions = area.Value.OrderBy(x => x.Key).ToList();
                roomsWithTransitions.ForEach(roomWithTransitions =>
                {
                    roomStacker.Children.Add(GetZoneWithTransitionsExpander(roomWithTransitions, ExpandedTrackedRoomsWithTransitions, ordering));
                });
                var areaExpander = GenerateExpanderWithContent(areaName, roomStacker, ExpandedTrackedZonesWithTransitions, $"[Rooms: {roomsWithTransitions.Count} / Transitions: {roomsWithTransitions.Sum(x => x.Value.Count)}]");
                TrackerTransitionsList.Items.Add(areaExpander);
            }
        }

        private void UpdateTrackerTransitions(List<TransitionWithDestination> transitions, Sorting ordering)
        {
            var orderedTransitions = ordering switch
            {
                Sorting.Alpha => transitions.OrderBy(x => _useTrackerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
                Sorting.Time => transitions.OrderBy(x => _useTrackerTransitionsDestination ? x.Destination.TimeAdded : x.Source.TimeAdded).ToList(),
                _ => transitions.OrderBy(x => _useTrackerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
            };
            var transitionsGrid = GetTransitionsGrid(orderedTransitions);
            TrackerTransitionsList.Items.Add(transitionsGrid);
        }

        private Expander GetZoneWithTransitionsExpander(KeyValuePair<string, List<TransitionWithDestination>> zoneWithTransitions, HashSet<string> expandedHashset, Sorting ordering)
        {
            var zoneName = zoneWithTransitions.Key.WithoutUnderscores();
            var orderedTransitions = ordering switch
            {
                Sorting.Alpha => zoneWithTransitions.Value.OrderBy(x => _useTrackerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
                Sorting.Time => zoneWithTransitions.Value.OrderBy(x => _useTrackerTransitionsDestination ? x.Destination.TimeAdded : x.Source.TimeAdded).ToList(),
                _ => zoneWithTransitions.Value.OrderBy(x => _useTrackerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
            };
            var transitionsGrid = GetTransitionsGrid(orderedTransitions);
            return GenerateExpanderWithContent(zoneName, transitionsGrid, expandedHashset, $"[Transitions: {orderedTransitions.Count}]");
        }

        private Grid GetTransitionsGrid(List<TransitionWithDestination> transitions)
        {
            var transitionKvps = transitions.ToDictionary(
                x => TransitionStringBuilder(x),
                x => _showTrackerTransitionsTime ? GetAgeInMinutes(_referenceTime, _useTrackerTransitionsDestination ? x.Destination.TimeAdded : x.Source.TimeAdded) : ""
                ).ToList();
            return GenerateAutoStarGrid(transitionKvps);
        }

        private string TransitionStringBuilder(TransitionWithDestination x)
        {
            var s = new StringBuilder();

            if (_useTrackerTransitionsDestination)
            {
                s.Append(x.Destination.Name.WithoutUnderscores());
                s.Append(" <-- ");
                s.Append(x.Source.Name.WithoutUnderscores());
            }
            else
            {
                s.Append(x.Source.Name.WithoutUnderscores());
                s.Append(" --> ");
                s.Append(x.Destination.Name.WithoutUnderscores());
            }

            return s.ToString();
        }

        #region Events

        private void SetTrackerTransitionsTabButtonContent()
        {
            Tracker_Transition_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {TrackerTransitionGroupingOptions[_appSettings.SelectedTrackerTransitionGrouping]}");
            Tracker_Transition_SortBy_Button.Content = GenerateButtonTextBlock($"Sort: {TrackerTransitionOrderingOptions[_appSettings.SelectedTrackerTransitionOrder]}");
            Tracker_Transition_SourceDestination_Button.Content = GenerateButtonTextBlock(_useTrackerTransitionsDestination ? "Focus: Destination" : "Focus: Source");
            Tracker_Transition_Time_Button.Content = GenerateButtonTextBlock(_showTrackerTransitionsTime ? "Time: Show" : "Time: Hide");
        }

        private void Tracker_Transition_GroupBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedTrackerTransitionGrouping = (_appSettings.SelectedTrackerTransitionGrouping + 1) % TrackerTransitionGroupingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Transition_SortBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedTrackerTransitionOrder = (_appSettings.SelectedTrackerTransitionOrder + 1) % TrackerTransitionOrderingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Transition_Time_Click(object sender, RoutedEventArgs e)
        {
            _showTrackerTransitionsTime = !_showTrackerTransitionsTime;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Transition_SourceDestination_Click(object sender, RoutedEventArgs e)
        {
            _useTrackerTransitionsDestination = !_useTrackerTransitionsDestination;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Tracker_Transition_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            _trackerLogReader.OpenFile();
        }

        private void Tracker_Transition_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(TrackerTransitionsList);
        private void Tracker_Transition_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(TrackerTransitionsList);

        #endregion

    }
}
