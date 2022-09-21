using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using HK_Rando_4_Log_Display.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Utils.Utils;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly BoolWrapper _expandPreviewedLocationSection = new();
        private readonly HashSet<string> ExpandedPreviewedLocationPools = new();

        private readonly BoolWrapper _expandPreviewedItemSection = new();
        private readonly HashSet<string> ExpandedPreviewedItemPools = new();

        private readonly HashSet<string> ExpandedRoomsWithLocations = new();
        private readonly HashSet<string> ExpandedZonesWithLocations = new();


        private void UpdateHelperLocationTab()
        {
            UpdateUX(_settings.SelectedHelperLocationGrouping switch
            {
                0 => () => UpdateHelperLocationListWithLocationsByZone(_helperLogReader.GetLocationsByMapArea()),                   // Locations by (Map) Area
                1 => () => UpdateHelperLocationListWithLocationsByZone(_helperLogReader.GetLocationsByTitledArea()),                // Locations by (Titled) Area
                2 => () => UpdateHelperLocationListWithLocationsByRoomByArea(_helperLogReader.GetLocationsByRoomByMapArea()),       // Locations by Rooms by (Map) Area
                3 => () => UpdateHelperLocationListWithLocationsByRoomByArea(_helperLogReader.GetLocationsByRoomByTitledArea()),    // Locations by Rooms by (Titled) Area
                4 => () => UpdateHelperLocationListWithLocationsByZone(_helperLogReader.GetLocationsByRoom()),                      // Locations by Rooms
                5 => () => UpdateHelperLocationListWithLocations(_helperLogReader.GetLocations()),                                  // All locations

                _ => () => UpdateHelperLocationListWithLocationsByZone(_helperLogReader.GetLocationsByMapArea()),                   // Locations by (Map) Area
            });
        }

        private void InitializeHelperLocationList()
        {
            HelperLocationList.Items.Clear();
            GetMajorCountables();
            GetPreviewedLocations();
            GetPreviewedItems();
        }

        private void GetMajorCountables()
        {
            var trackedItemsByPool = _trackerLogReader.GetItemsByPool();

            // Grubs
            var grubCount = GetItemCount(trackedItemsByPool, "Grub");
            if (grubCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Grubs: {grubCount}" });
            }
            // Charms
            var charmCount = trackedItemsByPool.FirstOrDefault(x => x.Key == "Charm").Value?.Count(x => !x.Name.Contains("_Fragment") & !x.Name.Contains("Unbreakable_"));
            if (charmCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Charms: {charmCount}" });
            }
            // Essence
            var essenceCount = _trackerLogReader.GetEssenceFromPools();
            if (essenceCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Rando Essence: {essenceCount}" });
            }
            // Rancid Eggs
            var eggCount = GetItemCount(trackedItemsByPool, "Egg");
            if (eggCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Rancid Eggs: {eggCount}" });
            }
            // Pale Ore
            var oreCount = GetItemCount(trackedItemsByPool, "Ore");
            if (oreCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Pale Ore: {oreCount}" });
            }
            // Grimmkin Flames
            var flameCount = GetItemCount(trackedItemsByPool, "Flame");
            if (flameCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Grimmkin Flames: {flameCount}" });
            }
            // MrMushroom Levels
            var mushroomCount = GetItemCount(trackedItemsByPool, "MrMushroom");
            if (mushroomCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Mr Mushroom Level: {mushroomCount}" });
            }
        }

        private static int? GetItemCount(Dictionary<string, List<ItemWithLocation>> pooledItems, string itemPool) =>
            pooledItems.FirstOrDefault(x => x.Key == itemPool).Value?.Count;

        private void GetPreviewedLocations()
        {
            var previewedLocations = _helperLogReader.GetPreviewedLocations();
            if (!previewedLocations.Any())
            {
                return;
            }

            var poolStacker = GenerateStackPanel();
            previewedLocations.OrderBy(x => x.Key).ToList().ForEach(poolWithLocations =>
            {
                var pool = poolWithLocations.Key.WithoutUnderscores();
                var locationStacker = GenerateStackPanel();
                poolWithLocations.Value.OrderBy(x => x.LocationName).GroupBy(x => x.LocationName).ToDictionary(x => x.Key, x => x.ToList()).ToList().ForEach(locationWithItems =>
                {
                    var location = locationWithItems.Key;
                    var itemsWithCosts = locationWithItems.Value
                        .OrderBy(x => x.SecondaryCostValue)
                        .ThenBy(x => x.ItemCostValue)
                        .ThenBy(x =>
                        {
                            var startNumbers = Regex.Match(x.ItemName, "^(\\d+)").Groups[1].Value;
                            return string.IsNullOrEmpty(startNumbers) ? x.ItemName : startNumbers;
                        }, new SemiNumericComparer())
                        .Select(x => new KeyValuePair<string, string>(x.ItemName, x.ItemCost))
                        .ToList();

                    if (pool == "Shop")
                    {
                        var shopExpander = new Expander
                        {
                            Name = location.AsObjectName(),
                            Header = location,
                            Content = GenerateAutoStarGrid(itemsWithCosts),
                            IsExpanded = ExpandedPreviewedLocationPools.Contains(location.AsObjectName())
                        };
                        locationStacker.Children.Add(shopExpander);
                    }
                    else
                    {
                        locationStacker.Children.Add(new TextBlock { Text = location });
                        locationStacker.Children.Add(GenerateAutoStarGrid(itemsWithCosts));
                    }
                });

                poolStacker.Children.Add(GenerateExpanderWithContent(pool, locationStacker, ExpandedPreviewedLocationPools));
            });

            HelperLocationList.Items.Add(GenerateExpanderWithContent("Previewed Locations", poolStacker, _expandPreviewedLocationSection));
        }

        private void GetPreviewedItems()
        {
            var previewedItems = _helperLogReader.GetPreviewedItems();
            if (!previewedItems.Any())
            {
                return;
            }

            var poolStacker = GenerateStackPanel();
            previewedItems.OrderBy(x => x.Key).ToList().ForEach(poolWithLocations =>
            {
                var pool = poolWithLocations.Key.WithoutUnderscores();
                var itemsWithLocationsAndCosts = poolWithLocations.Value
                        .OrderBy(x =>
                        {
                            var startNumbers = Regex.Match(x.ItemName, "^(\\d+)").Groups[1].Value;
                            return string.IsNullOrEmpty(startNumbers) ? x.ItemName : startNumbers;
                        }, new SemiNumericComparer())
                        .Select(x => new KeyValuePair<string, string>(x.ItemName, $"at {x.LocationName}{(!string.IsNullOrWhiteSpace(x.ItemCost) ? $" ({x.ItemCost})" : "")}"))
                        .ToList();

                poolStacker.Children.Add(GenerateExpanderWithContent(pool, GenerateAutoStarGrid(itemsWithLocationsAndCosts), ExpandedPreviewedItemPools));
            });

            HelperLocationList.Items.Add(GenerateExpanderWithContent("Previewed Items", poolStacker, _expandPreviewedItemSection));
        }

        private void UpdateHelperLocationListWithLocationsByZone(Dictionary<string, List<LocationWithTime>> locationsByZone)
        {
            InitializeHelperLocationList();

            foreach (var zone in locationsByZone)
            {
                var zoneName = zone.Key.WithoutUnderscores();
                var locations = zone.Value;
                locations = _settings.SelectedHelperLocationOrder switch
                {
                    2 => locations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList(),
                    _ => locations.OrderBy(x => x.Name).ToList(),
                };
                HelperLocationList.Items.Add(GenerateExpanderWithContent(zoneName, GetLocationsObject(locations), ExpandedZonesWithLocations, $"[Locations: {locations.Count}]"));
            }
        }

        private void UpdateHelperLocationListWithLocationsByRoomByArea(Dictionary<string, Dictionary<string, List<LocationWithTime>>> locationsByRoomByArea)
        {
            InitializeHelperLocationList();

            foreach (var area in locationsByRoomByArea)
            {
                var roomStacker = GenerateStackPanel();

                var areaName = area.Key.WithoutUnderscores();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetRoomWithLocationsExpander(x)));

                HelperLocationList.Items.Add(GenerateExpanderWithContent(areaName, roomStacker, ExpandedZonesWithLocations, $"[Rooms: {rooms.Count} / Locations: {rooms.Sum(x => x.Value.Count)}]"));
            }
        }

        private Expander GetRoomWithLocationsExpander(KeyValuePair<string, List<LocationWithTime>> roomWithLocations)
        {
            var roomName = roomWithLocations.Key.WithoutUnderscores();
            var expanderName = roomName.AsObjectName();
            var locations = roomWithLocations.Value;

            locations = _settings.SelectedHelperLocationOrder switch
            {
                2 => locations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList(),
                _ => locations.OrderBy(x => x.Name).ToList(),
            };
            var locationStacker = GenerateStackPanel();
            locations.ForEach(y =>
            {
                switch (_settings.SelectedHelperLocationOrder)
                {
                    case 1:
                    case 2:
                        locationStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()}\t{GetAgeInMinutes(_referenceTime, y.TimeAdded)}" });
                        break;
                    default:
                        locationStacker.Children.Add(new TextBlock { Text = y.Name.WithoutUnderscores() });
                        break;
                }
            });
            return GenerateExpanderWithContent(roomName, GetLocationsObject(locations), ExpandedRoomsWithLocations, $"[Locations: {locations.Count}]");
        }

        private void UpdateHelperLocationListWithLocations(List<LocationWithTime> locations)
        {
            InitializeHelperLocationList();

            var orderedLocations = locations.Select(x => x).ToList();

            orderedLocations = _settings.SelectedHelperLocationOrder switch
            {
                2 => orderedLocations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList(),
                _ => orderedLocations.OrderBy(x => x.Name).ToList(),
            };
            HelperLocationList.Items.Add(GetLocationsObject(orderedLocations));
        }

        private object GetLocationsObject(List<LocationWithTime> locations)
        {
            switch (_settings.SelectedHelperLocationOrder)
            {
                case 1:
                case 2:
                    var locationKvps = locations.ToDictionary(x => $"{(x.IsOutOfLogic ? "*" : "")}{x.Name.WithoutUnderscores()}", x => GetAgeInMinutes(_referenceTime, x.TimeAdded)).ToList();
                    return GenerateAutoStarGrid(locationKvps);
                default:
                    var locationStacker = GenerateStackPanel();
                    locations.ForEach(y => locationStacker.Children.Add(new TextBlock { Text = $"{(y.IsOutOfLogic ? "*" : "")}{y.Name.WithoutUnderscores()}" }));
                    return locationStacker;
            }
        }

        #region Events

        private void HelperLocationGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedHelperLocationGrouping = (_settings.SelectedHelperLocationGrouping + 1) % _helperGroupings.Length;
            HelperLocationGrouping.Content = _helperGroupings[(int)_settings.SelectedHelperLocationGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void HelperLocationOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedHelperLocationOrder = (_settings.SelectedHelperLocationOrder + 1) % _helperOrders.Length;
            HelperLocationOrder.Content = _helperOrders[(int)_settings.SelectedHelperLocationOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void HelperLocationExpand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(HelperLocationList);

        private void HelperLocationCollapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(HelperLocationList);

        #endregion
    }
}
