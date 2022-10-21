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
        private bool _showSpoilerTransitionTraversed = true;
        private bool _showSpoilerTransitionAltRoomNames = false;

        private void UpdateSpoilerTransitionsTab()
        {
            UpdateUX(() =>
            {
                SpoilerTransitionsList.Items.Clear();

                var spoilerTransitionGrouping = (RoomGrouping)_appSettings.SelectedSpoilerTransitionGrouping;
                var spoilerTransitionOrdering = (SpoilerSorting)_appSettings.SelectedSpoilerTransitionOrder;

                var trackedTransitions = _trackerLogReader.GetTransitions();

                switch (spoilerTransitionGrouping)
                {
                    case RoomGrouping.MapArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByMapArea(_useSpoilerTransitionsDestination), spoilerTransitionOrdering, trackedTransitions);
                        break;
                    case RoomGrouping.TitleArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByTitledArea(_useSpoilerTransitionsDestination), spoilerTransitionOrdering, trackedTransitions);
                        break;
                    case RoomGrouping.RoomMapArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByRoomByMapArea(_useSpoilerTransitionsDestination, _showSpoilerTransitionAltRoomNames), spoilerTransitionOrdering, trackedTransitions);
                        break;
                    case RoomGrouping.RoomTitleArea:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByRoomByTitledArea(_useSpoilerTransitionsDestination, _showSpoilerTransitionAltRoomNames), spoilerTransitionOrdering, trackedTransitions);
                        break;
                    case RoomGrouping.Room:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitionsByRoom(_useSpoilerTransitionsDestination, _showSpoilerTransitionAltRoomNames), spoilerTransitionOrdering, trackedTransitions);
                        break;
                    case RoomGrouping.None:
                    default:
                        UpdateSpoilerTransitions(_transitionSpoilerReader.GetTransitions(), spoilerTransitionOrdering, trackedTransitions);
                        break;
                };
            });
        }

        private void UpdateSpoilerTransitions(Dictionary<string, List<TransitionWithDestination>> transitionsByArea, SpoilerSorting ordering, List<TransitionWithDestination> trackedTransitions)
        {
            foreach (var area in transitionsByArea)
            {
                SpoilerTransitionsList.Items.Add(GetSpoilerZoneWithTransitionsExpander(area, ExpandedSpoilerTrackedZonesWithTransitions, ordering, trackedTransitions));
            }
        }

        private void UpdateSpoilerTransitions(Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> transitionsByRoomByArea, SpoilerSorting ordering, List<TransitionWithDestination> trackedTransitions)
        {
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();
                var areaName = area.Key.WithoutUnderscores();
                var roomsWithTransitions = area.Value.OrderBy(x => x.Key).ToList();
                roomsWithTransitions.ForEach(roomWithTransitions =>
                {
                    roomStacker.Children.Add(GetSpoilerZoneWithTransitionsExpander(roomWithTransitions, ExpandedSpoilerTrackedRoomsWithTransitions, ordering, trackedTransitions));
                });
                var areaExpander = GenerateExpanderWithContent(areaName, roomStacker, ExpandedSpoilerTrackedZonesWithTransitions);
                SpoilerTransitionsList.Items.Add(areaExpander);
            }
        }

        private void UpdateSpoilerTransitions(List<TransitionWithDestination> transitions, SpoilerSorting ordering, List<TransitionWithDestination> trackedTransitions)
        {
            var orderedTransitions = ordering switch
            {
                SpoilerSorting.Alpha => transitions.OrderBy(x => _useSpoilerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
                SpoilerSorting.SeedDefault => transitions.ToList(),
                _ => transitions.OrderBy(x => _useSpoilerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
            };
            var transitionsGrid = GetSpoilerTransitionsGrid(orderedTransitions, trackedTransitions);
            SpoilerTransitionsList.Items.Add(transitionsGrid);
        }

        private Expander GetSpoilerZoneWithTransitionsExpander(KeyValuePair<string, List<TransitionWithDestination>> zoneWithTransitions, HashSet<string> expandedHashset, SpoilerSorting ordering, List<TransitionWithDestination> trackedTransitions)
        {
            var zoneName = zoneWithTransitions.Key.WithoutUnderscores();
            var orderedTransitions = ordering switch
            {
                SpoilerSorting.Alpha => zoneWithTransitions.Value.OrderBy(x => _useSpoilerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
                SpoilerSorting.SeedDefault => zoneWithTransitions.Value.ToList(),
                _ => zoneWithTransitions.Value.OrderBy(x => _useSpoilerTransitionsDestination ? x.Destination.Name : x.Source.Name).ToList(),
            };
            var transitionsGrid = GetSpoilerTransitionsGrid(orderedTransitions, trackedTransitions);
            return GenerateExpanderWithContent(zoneName, transitionsGrid, expandedHashset);
        }

        private Grid GetSpoilerTransitionsGrid(List<TransitionWithDestination> transitions, List<TransitionWithDestination> trackedTransitions)
        {
            var transitionKvps = transitions.Select(x =>
            {
                var sourceName = x.Source.Name;
                var destinationName = x.Destination.Name;
                var isTracked = _showSpoilerTransitionTraversed && IsTrackedTransition(sourceName, destinationName, trackedTransitions);
                return new KeyValuePair<string, string>(
                    $"{(isTracked ? "<s>" : "")}{SpoilerTransitionStringBuilder(x)}",
                    $""
                );
            }).ToList();
            return GenerateAutoStarGrid(transitionKvps);
        }

        private static bool IsTrackedTransition(string sourceName, string destinationName, List<TransitionWithDestination> trackedTransitions) =>
            trackedTransitions.Any(y => y.Source.Name == sourceName && y.Destination.Name == destinationName);

        private string SpoilerTransitionStringBuilder(TransitionWithDestination x)
        {
            var s = new StringBuilder();

            if (_useSpoilerTransitionsDestination)
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

        private void SetSpoilerTransitionsTabButtonContent()
        {
            Spoiler_Transition_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {SpoilerTransitionGroupingOptions[_appSettings.SelectedSpoilerTransitionGrouping]}");
            Spoiler_Transition_SortBy_Button.Content = GenerateButtonTextBlock($"Sort: {SpoilerTransitionOrderingOptions[_appSettings.SelectedSpoilerTransitionOrder]}");
            Spoiler_Transition_SourceDestination_Button.Content = GenerateButtonTextBlock(_useSpoilerTransitionsDestination ? "Focus: Destination" : "Focus: Source");
            Spoiler_Transition_Traversed_Button.Content = GenerateButtonTextBlock(_showSpoilerTransitionTraversed ? "Traversed: Show" : "Traversed: Hide");
            Spoiler_Transition_RoomDisplay_Button.Content = GenerateButtonTextBlock(_showSpoilerTransitionAltRoomNames ? "Room: Name" : "Room: Code");
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

        private void Spoiler_Transition_Traversed_Click(object sender, RoutedEventArgs e)
        {
            _showSpoilerTransitionTraversed = !_showSpoilerTransitionTraversed;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_SourceDestination_Click(object sender, RoutedEventArgs e)
        {
            _useSpoilerTransitionsDestination = !_useSpoilerTransitionsDestination;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_RoomDisplay_Click(object sender, RoutedEventArgs e)
        {
            _showSpoilerTransitionAltRoomNames = !_showSpoilerTransitionAltRoomNames;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Spoiler_Transition_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(SpoilerTransitionsList);
        private void Spoiler_Transition_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(SpoilerTransitionsList);

        #endregion
    }
}
