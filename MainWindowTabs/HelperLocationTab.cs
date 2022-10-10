using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using HK_Rando_4_Log_Display.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Constants.Constants;
using static HK_Rando_4_Log_Display.Utils.Utils;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly BoolWrapper _expandCountables = new(true);
        private readonly BoolWrapper _expandPreviewedLocationSection = new();
        private readonly HashSet<string> ExpandedPreviewedLocationPools = new();
        private readonly BoolWrapper _expandPreviewedItemSection = new();
        private readonly HashSet<string> ExpandedPreviewedItemPools = new();
        private readonly HashSet<string> ExpandedRoomsWithLocations = new();
        private readonly HashSet<string> ExpandedZonesWithLocations = new();
        private bool _showHelperLocationsTime = true;

        private void UpdateHelperLocationsTab()
        {
            UpdateUX(() =>
            {
                HelperLocationsList.Items.Clear();
                GetMajorCountables();
                UpdatePreviewedLocations(_helperLogReader.GetPreviewedLocations());
                UpdatePreviewedItems(_helperLogReader.GetPreviewedItems());

                var helperLocationGrouping = (RoomGrouping)_appSettings.SelectedHelperLocationGrouping;
                var helperLocationOrdering = (Sorting)_appSettings.SelectedHelperLocationOrder;

                switch (helperLocationGrouping)
                {
                    case RoomGrouping.MapArea:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByMapArea(), helperLocationOrdering);
                        break;
                    case RoomGrouping.TitleArea:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByTitledArea(), helperLocationOrdering);
                        break;
                    case RoomGrouping.RoomMapArea:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByRoomByMapArea(), helperLocationOrdering);
                        break;
                    case RoomGrouping.RoomTitleArea:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByRoomByTitledArea(), helperLocationOrdering);
                        break;
                    case RoomGrouping.Room:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByRoom(), helperLocationOrdering);
                        break;
                    case RoomGrouping.None:
                    default:
                        UpdateHelperLocations(_helperLogReader.GetLocations(), helperLocationOrdering);
                        break;
                };
            });
        }

        private void GetMajorCountables()
        {
            var trackedItemsByPool = _trackerLogReader.GetItemsByPool();

            var majorCountables = new List<string>();
            // Grubs
            var grubCount = GetItemCount(trackedItemsByPool, "Grub");
            if (grubCount != null)
            {
                majorCountables.Add($"Grubs: {grubCount}");
            }
            // Charms
            var charms = trackedItemsByPool.FirstOrDefault(x => x.Key == "Charm").Value;
            var transcendenceCharms = trackedItemsByPool.FirstOrDefault(x => x.Key == "Charm - Transcendence").Value;
            if (charms != null || transcendenceCharms != null)
            {
                var charmCollections = new[] { charms, transcendenceCharms }.Where(x => x != null).Aggregate(new List<ItemWithLocation>(), (current, next) => current.Concat(next).ToList());
                var charmHashSet = new HashSet<ItemWithLocation>(charmCollections);
                var charmCount = charms?.Count(x => !x.Item.Name.Contains("_Fragment") && !x.Item.Name.Contains("Unbreakable_") && x.Item.Name != "Void_Heart");
                if (charmCount != null)
                {
                    majorCountables.Add($"Charms: {charmCount}");
                }
            }
            // Essence
            var essenceCount = _trackerLogReader.GetEssenceFromPools();
            if (essenceCount != null)
            {
                majorCountables.Add($"Essence: {essenceCount}");
            }
            // Rancid Eggs
            var eggCount = GetItemCount(trackedItemsByPool, "Egg");
            if (eggCount != null)
            {
                majorCountables.Add($"Rancid Eggs: {eggCount}");
            }
            // Pale Ore
            var oreCount = GetItemCount(trackedItemsByPool, "Ore");
            if (oreCount != null)
            {
                majorCountables.Add($"Pale Ore: {oreCount}");
            }
            // Grimmkin Flames
            var flameCount = GetItemCount(trackedItemsByPool, "Flame");
            if (flameCount != null)
            {
                majorCountables.Add($"Grimmkin Flames: {flameCount}");
            }
            // MrMushroom Levels
            var mushroomCount = GetItemCount(trackedItemsByPool, "MrMushroom");
            if (mushroomCount != null)
            {
                majorCountables.Add($"Mr Mushroom Level: {mushroomCount}");
            }

            if (!majorCountables.Any())
            {
                return;
            }

            var poolStacker = GenerateStackPanel();
            majorCountables.ForEach(x => poolStacker.Children.Add(new TextBlock { Text = x }));
            HelperLocationsList.Items.Add(GenerateExpanderWithContent("Countables", poolStacker, _expandCountables));
        }

        private static int? GetItemCount(Dictionary<string, List<ItemWithLocation>> pooledItems, string itemPool) =>
            pooledItems.FirstOrDefault(x => x.Key == itemPool).Value?.Count;

        private void UpdatePreviewedLocations(Dictionary<string, List<LocationPreview>> previewedLocations)
        {
            if (!previewedLocations.Any())
            {
                return;
            }

            var poolStacker = GenerateStackPanel();
            previewedLocations.OrderBy(x => x.Key).ToList().ForEach(poolWithLocations =>
            {
                var locationPool = poolWithLocations.Key.WithoutUnderscores();
                var locationStacker = GenerateStackPanel();
                var locationsWithItems = poolWithLocations.Value.OrderBy(x => x.Location.Name).GroupBy(x => x.Location.Name).ToDictionary(x => x.Key, x => x.ToList()).ToList();
                locationsWithItems.ForEach(locationWithItems =>
                {
                    var location = locationWithItems.Key;
                    var itemsWithCosts = locationWithItems.Value
                        .OrderBy(x => x.SecondaryCost)
                        .ThenBy(x => x.PrimaryCost)
                        .ThenBy(x =>
                        {
                            var startNumbers = Regex.Match(x.Item.PreviewName, "^(\\d+)").Groups[1].Value;
                            return string.IsNullOrEmpty(startNumbers) ? x.Item.PreviewName : startNumbers;
                        }, new SemiNumericComparer())
                        .Select(x => new KeyValuePair<string, string>(x.Item.PreviewName, x.CostString))
                        .ToList();

                    if (locationPool == "Shop")
                    {
                        var shopExpander = GenerateExpanderWithContent(location, GenerateAutoStarGrid(itemsWithCosts), ExpandedPreviewedLocationPools);
                        locationStacker.Children.Add(shopExpander);
                    }
                    else
                    {
                        locationStacker.Children.Add(new TextBlock { Text = location });
                        locationStacker.Children.Add(GenerateAutoStarGrid(itemsWithCosts));
                    }
                });

                poolStacker.Children.Add(GenerateExpanderWithContent($"{locationPool} [{locationsWithItems.Count}]" , locationStacker, ExpandedPreviewedLocationPools));
            });

            HelperLocationsList.Items.Add(GenerateExpanderWithContent("Previewed Locations", poolStacker, _expandPreviewedLocationSection));
        }

        private void UpdatePreviewedItems(Dictionary<string, List<LocationPreview>> previewedItems)
        {
            if (!previewedItems.Any())
            {
                return;
            }

            var poolStacker = GenerateStackPanel();
            previewedItems.OrderBy(x => x.Key).ToList().ForEach(poolWithLocations =>
            {
                var itemPool = poolWithLocations.Key.WithoutUnderscores();
                var itemsWithLocationsAndCosts = poolWithLocations.Value
                    .OrderBy(x =>
                    {
                        var startNumbers = Regex.Match(x.Item.PreviewName, "^(\\d+)").Groups[1].Value;
                        return string.IsNullOrEmpty(startNumbers) ? x.Item.PreviewName : startNumbers;
                    }, new SemiNumericComparer())
                    .Select(x => new KeyValuePair<string, string>(
                        x.Item.PreviewName,
                        $"at {x.Location.Name}{(!string.IsNullOrWhiteSpace(x.CostString) ? $" ({x.CostString})" : "")}"))
                    .ToList();

                poolStacker.Children.Add(GenerateExpanderWithContent($"{itemPool} [{poolWithLocations.Value.Count}]", GenerateAutoStarGrid(itemsWithLocationsAndCosts), ExpandedPreviewedItemPools));
            });

            HelperLocationsList.Items.Add(GenerateExpanderWithContent("Previewed Items", poolStacker, _expandPreviewedItemSection));
        }

        private void UpdateHelperLocations(Dictionary<string, List<Location>> locationsByArea, Sorting ordering)
        {
            foreach (var area in locationsByArea)
            {
                HelperLocationsList.Items.Add(GetZoneWithLocationsExpander(area, ExpandedZonesWithLocations, ordering));
            }
        }

        private void UpdateHelperLocations(Dictionary<string, Dictionary<string, List<Location>>> locationsByRoomByArea, Sorting ordering)
        {
            foreach (var area in locationsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();
                var areaName = area.Key.WithoutUnderscores();
                var roomsWithLocations = area.Value.OrderBy(x => x.Key).ToList();
                roomsWithLocations.ForEach(roomWithLocations => roomStacker.Children.Add(GetZoneWithLocationsExpander(roomWithLocations, ExpandedRoomsWithLocations, ordering)));
                var areaExpander = GenerateExpanderWithContent(areaName, roomStacker, ExpandedZonesWithLocations, $"[Rooms: {roomsWithLocations.Count} / Locations: {roomsWithLocations.Sum(x => x.Value.Count)}]");
                HelperLocationsList.Items.Add(areaExpander);
            }
        }

        private void UpdateHelperLocations(List<Location> locations, Sorting ordering)
        {
            var orderedLocations = ordering switch
            {
                Sorting.Alpha => locations.OrderBy(x => x.Name).ToList(),
                Sorting.Time => locations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList(),
                _ => locations.OrderBy(x => x.Name).ToList(),
            };
            var locationsGrid = GetLocationsGrid(orderedLocations);
            HelperLocationsList.Items.Add(locationsGrid);
        }

        private Expander GetZoneWithLocationsExpander(KeyValuePair<string, List<Location>> zoneWithLocations, HashSet<string> expandedHashset, Sorting helperLocationOrdering)
        {
            var zoneName = zoneWithLocations.Key.WithoutUnderscores();
            var orderedLocations = helperLocationOrdering switch
            {
                Sorting.Alpha => zoneWithLocations.Value.OrderBy(x => x.Name).ToList(),
                Sorting.Time => zoneWithLocations.Value.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList(),
                _ => zoneWithLocations.Value.OrderBy(x => x.Name).ToList(),
            };
            var locationsGrid = GetLocationsGrid(orderedLocations);
            return GenerateExpanderWithContent(zoneName, locationsGrid, expandedHashset, $"[Locations: {orderedLocations.Count}]");
        }

        private Grid GetLocationsGrid(List<Location> locations)
        {
            var locationKvps = locations.ToDictionary(
                    x => $"{(x.IsOutOfLogic ? "*" : "")}{x.Name.WithoutUnderscores()}",
                    x => _showHelperLocationsTime ? GetAgeInMinutes(_referenceTime, x.TimeAdded) : "")
                .ToList();
            return GenerateAutoStarGrid(locationKvps);
        }

        #region Events

        private void SetHelperLocationsTabButtonContent()
        {
            Helper_Location_GroupBy_Button.Content = GenerateButtonTextBlock($"Group: {HelperLocationGroupingOptions[_appSettings.SelectedHelperLocationGrouping]}");
            Helper_Location_SortBy_Button.Content = GenerateButtonTextBlock($"Sort: {HelperLocationOrderingOptions[_appSettings.SelectedHelperLocationOrder]}");
            Helper_Location_Time_Button.Content = GenerateButtonTextBlock(_showHelperLocationsTime ? "Time: Show" : "Time: Hide");
        }

        private void Helper_Location_GroupBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedHelperLocationGrouping = (_appSettings.SelectedHelperLocationGrouping + 1) % HelperLocationGroupingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Location_SortBy_Click(object sender, RoutedEventArgs e)
        {
            _appSettings.SelectedHelperLocationOrder = (_appSettings.SelectedHelperLocationOrder + 1) % HelperLocationOrderingOptions.Length;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Location_Time_Click(object sender, RoutedEventArgs e)
        {
            _showHelperLocationsTime = !_showHelperLocationsTime;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Location_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            _helperLogReader.OpenFile();
        }

        private void Helper_Location_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(HelperLocationsList);
        private void Helper_Location_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(HelperLocationsList);

        #endregion
    }
}
