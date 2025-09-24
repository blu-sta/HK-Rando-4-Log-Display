using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly HashSet<string> ExpandedSpoilerTrackedRoomsWithTransitions = new();
        private readonly HashSet<string> ExpandedSpoilerTrackedZonesWithTransitions = new();
        private bool _useSpoilerTransitionsDestination = false;
        private bool _showSpoilerTransitionSceneDescriptions = false;

        private void UpdateSpoilerTransitionsTab()
        {
            UpdateUX(() =>
            {
                SpoilerTransitionsList.Items.Clear();

                var trackedTransitions = _trackerLogReader.GetTransitions();

                switch ((RoomGrouping)_appSettings.SelectedSpoilerTransitionGrouping)
                {
                    case RoomGrouping.MapArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByMapArea(_useSpoilerTransitionsDestination), trackedTransitions);
                        break;
                    case RoomGrouping.TitleArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByTitledArea(_useSpoilerTransitionsDestination), trackedTransitions);
                        break;
                    case RoomGrouping.RoomMapArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByRoomByMapArea(_useSpoilerTransitionsDestination, _showSpoilerTransitionSceneDescriptions), trackedTransitions);
                        break;
                    case RoomGrouping.RoomTitleArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByRoomByTitledArea(_useSpoilerTransitionsDestination, _showSpoilerTransitionSceneDescriptions), trackedTransitions);
                        break;
                    case RoomGrouping.Room:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByRoom(_useSpoilerTransitionsDestination, _showSpoilerTransitionSceneDescriptions), trackedTransitions);
                        break;
                    case RoomGrouping.None:
                    default:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitions(), trackedTransitions);
                        break;
                };
            });
        }

        private void UpdateSpoilerTransitions(Dictionary<string, List<TransitionWithDestination>> transitionsByArea, List<TransitionWithDestination> trackedTransitions)
        {
            foreach (var area in transitionsByArea)
            {
                SpoilerTransitionsList.Items.Add(GetSpoilerZoneWithTransitionsExpander(area, ExpandedSpoilerTrackedZonesWithTransitions, trackedTransitions));
            }
        }

        private void UpdateSpoilerTransitions(Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> transitionsByRoomByArea, List<TransitionWithDestination> trackedTransitions)
        {
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();
                var areaName = area.Key.WithoutUnderscores();
                var roomsWithTransitions = area.Value
                    .ToDictionary(
                        x => x.Key,
                        x => GetSpoilerTransitionsWithTracking(x.Value, trackedTransitions)
                    )
                    .OrderBy(x => x.Key).ToList();

                var transitionCount = roomsWithTransitions.Sum(x => x.Value.Count);
                var traversedCount = roomsWithTransitions.Sum(x => x.Value.Count(y => y.IsTraversed));
                
                roomsWithTransitions.ForEach(roomWithTransitions =>
                {
                    roomStacker.Children.Add(GetSpoilerZoneWithTransitionsExpander(roomWithTransitions, ExpandedSpoilerTrackedRoomsWithTransitions, []));
                });
                var areaExpander = GenerateExpanderWithContent(areaName, roomStacker, ExpandedSpoilerTrackedZonesWithTransitions, $"[Traversed: {traversedCount}/{transitionCount}]"); // TODO: Add traversed counter
                SpoilerTransitionsList.Items.Add(areaExpander);
            }
        }

        private void UpdateSpoilerTransitions(List<TransitionWithDestination> transitions, List<TransitionWithDestination> trackedTransitions)
        {
            var orderedTransitions = (SpoilerSorting)_appSettings.SelectedSpoilerTransitionOrder switch
            {
                SpoilerSorting.SeedDefault => transitions.ToList(),
                // SpoilerSorting.Alpha
                _ => transitions.OrderBy(x => _useSpoilerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
            };
            var spoilerTransitionsWithTracking = GetSpoilerTransitionsWithTracking(orderedTransitions, trackedTransitions);
            var transitionCount = spoilerTransitionsWithTracking.Count;
            var traversedCount = spoilerTransitionsWithTracking.Count(x => x.IsTraversed);
            var transitionsGrid = GetSpoilerTransitionsGrid(spoilerTransitionsWithTracking);
            SpoilerTransitionsList.Items.Add(transitionsGrid);
            if ((SpoilerObtainedOrTraversedDisplay)_appSettings.SelectedSpoilerTraversedDisplay == SpoilerObtainedOrTraversedDisplay.Hide
                && traversedCount > 0
                && traversedCount != transitionCount)
            {
                SpoilerItemsList.Items.Add(GenerateAutoStarGrid([new KeyValuePair<string, string>($"+{traversedCount} traversed", "")]));
            }
        }

        private Expander GetSpoilerZoneWithTransitionsExpander(KeyValuePair<string, List<TransitionWithDestination>> zoneWithTransitions, HashSet<string> expandedHashset, List<TransitionWithDestination> trackedTransitions)
        {
            var zoneName = zoneWithTransitions.Key.WithoutUnderscores();
            var orderedTransitions = (SpoilerSorting)_appSettings.SelectedSpoilerTransitionOrder switch
            {
                SpoilerSorting.SeedDefault => zoneWithTransitions.Value.ToList(),
                // SpoilerSorting.Alpha
                _ => zoneWithTransitions.Value.OrderBy(x => _useSpoilerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
            };
            var spoilerTransitionsWithTracking = GetSpoilerTransitionsWithTracking(orderedTransitions, trackedTransitions);
            var transitionCount = spoilerTransitionsWithTracking.Count;
            var traversedCount = spoilerTransitionsWithTracking.Count(x => x.IsTraversed);
            var transitionsGrid = GetSpoilerTransitionsGrid(spoilerTransitionsWithTracking);
            return GenerateExpanderWithContent(zoneName, transitionsGrid, expandedHashset, $"[Traversed: {traversedCount}/{transitionCount}]");
        }

        private List<TransitionWithDestination> GetSpoilerTransitionsWithTracking(List<TransitionWithDestination> spoilerTransitions, List<TransitionWithDestination> trackedTransitions) =>
            [
                .. spoilerTransitions
                    .Select(x => IsTrackedTransition(x.Source.Name, x.Destination.Name, trackedTransitions)
                        ? new TransitionWithDestination (x) { IsTraversed = true }
                        : x),
            ];

        private static bool IsTrackedTransition(string sourceName, string destinationName, List<TransitionWithDestination> trackedTransitions) =>
            trackedTransitions.Any(y => y.Source.Name == sourceName && y.Destination.Name == destinationName);

        private Grid GetSpoilerTransitionsGrid(List<TransitionWithDestination> transitions)
        {
            var traversedDisplayMode = (SpoilerObtainedOrTraversedDisplay)_appSettings.SelectedSpoilerTraversedDisplay;
            var transitionKvps = transitions.Where(x => traversedDisplayMode != SpoilerObtainedOrTraversedDisplay.Hide || !x.IsTraversed).Select(x =>
            {
                var isStrikethrough = traversedDisplayMode == SpoilerObtainedOrTraversedDisplay.Mark && x.IsTraversed;
                return new KeyValuePair<string, string>(
                    $"{(isStrikethrough ? "<s>" : "")}{SpoilerTransitionStringBuilder(x)}",
                    $""
                );
            }).ToList();

            if (transitionKvps.Count == 0)
            {
                transitionKvps.Add(new KeyValuePair<string, string>("All traversed", ""));
            }

            return GenerateAutoStarGrid(transitionKvps);
        }

        private string SpoilerTransitionStringBuilder(TransitionWithDestination x)
        {
            var s = new StringBuilder();

            var destination = _showSpoilerTransitionSceneDescriptions ? $"{x.Destination.SceneDescription}[{x.Destination.DoorName}]" : x.Destination.Name;
            var source = _showSpoilerTransitionSceneDescriptions ? $"{x.Source.SceneDescription}[{x.Source.DoorName}]" : x.Source.Name;

            if (_useSpoilerTransitionsDestination)
            {
                s.Append(destination);
                s.Append(" <-- ");
                s.Append(source);
            }
            else
            {
                s.Append(source);
                s.Append(" --> ");
                s.Append(destination);
            }

            return s.ToString().WithoutUnderscores();
        }

        #region Events

        private void SetSpoilerTransitionsTabButtonContent()
        {
            Spoiler_Transition_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {SpoilerTransitionGroupingOptions[_appSettings.SelectedSpoilerTransitionGrouping]}");
            Spoiler_Transition_SortBy_Button.Content = GenerateButtonTextBlock($"Sort: {SpoilerTransitionOrderingOptions[_appSettings.SelectedSpoilerTransitionOrder]}");
            Spoiler_Transition_SourceDestination_Button.Content = GenerateButtonTextBlock(_useSpoilerTransitionsDestination ? "Focus: Destination" : "Focus: Source");
            Spoiler_Transition_Traversed_Button.Content = GenerateButtonTextBlock($"Traversed: {SpoilerTraversedDisplayOptions[_appSettings.SelectedSpoilerTraversedDisplay]}");
            Spoiler_Transition_RoomDisplay_Button.Content = GenerateButtonTextBlock(_showSpoilerTransitionSceneDescriptions ? "Room: Desc." : "Room: Code");
        }

        private void Spoiler_Transition_GroupBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedSpoilerTransitionGrouping = (_appSettings.SelectedSpoilerTransitionGrouping + 1) % SpoilerTransitionGroupingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_SortBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedSpoilerTransitionOrder = (_appSettings.SelectedSpoilerTransitionOrder + 1) % SpoilerTransitionOrderingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_Traversed_Display_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedSpoilerTraversedDisplay = (_appSettings.SelectedSpoilerTraversedDisplay + 1) % SpoilerTraversedDisplayOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_SourceDestination_Click(object sender, RoutedEventArgs e)
        {
            _useSpoilerTransitionsDestination = !_useSpoilerTransitionsDestination;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_RoomDisplay_Click(object sender, RoutedEventArgs e)
        {
            _showSpoilerTransitionSceneDescriptions = !_showSpoilerTransitionSceneDescriptions;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(SpoilerTransitionsList);
        private void Spoiler_Transition_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(SpoilerTransitionsList);

        #endregion
    }
}
