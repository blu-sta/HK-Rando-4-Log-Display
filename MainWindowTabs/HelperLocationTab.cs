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
        private void UpdateHelperLocationTab()
        {
            switch (_settings.SelectedHelperLocationGrouping)
            {
                case 2:
                    // Locations by Rooms by (Map) Area
                    var locationsByRoomByMapArea = _helperLogReader.GetLocationsByRoomByMapArea();
                    UpdateUX(() => UpdateHelperLocationListWithLocationsByRoomByArea(locationsByRoomByMapArea));
                    break;
                case 3:
                    // Locations by Rooms by (Titled) Area
                    var locationsByRoomByTitledArea = _helperLogReader.GetLocationsByRoomByTitledArea();
                    UpdateUX(() => UpdateHelperLocationListWithLocationsByRoomByArea(locationsByRoomByTitledArea));
                    break;
                case 4:
                    // Locations by Rooms
                    var locationsByRoom = _helperLogReader.GetLocationsByRoom();
                    UpdateUX(() => UpdateHelperLocationListWithLocationsByZone(locationsByRoom));
                    break;
                case 5:
                    // All locations
                    var locations = _helperLogReader.GetLocations();
                    UpdateUX(() => UpdateHelperLocationListWithLocations(locations));
                    break;
                case 1:
                    // Locations by (Titled) Area
                    var locationsByTitledArea = _helperLogReader.GetLocationsByTitledArea();
                    UpdateUX(() => UpdateHelperLocationListWithLocationsByZone(locationsByTitledArea));
                    break;
                default:
                    // Locations by (Map) Area
                    var locationsByMapArea = _helperLogReader.GetLocationsByMapArea();
                    UpdateUX(() => UpdateHelperLocationListWithLocationsByZone(locationsByMapArea));
                    break;
            }
        }

        #region Locations

        #region Locations by Area OR Room

        private void UpdateHelperLocationListWithLocationsByZone(Dictionary<string, List<LocationWithTime>> locationsByZone)
        {
            HelperLocationList.Items.Clear();

            GetMajorCountables();
            GetPreviewedLocations();
            GetPreviewedItems();

            foreach (var zone in locationsByZone)
            {
                var zoneName = zone.Key.WithoutUnderscores();
                var zoneExpanderName = zoneName.AsObjectName();
                var locations = zone.Value;
                switch (_settings.SelectedHelperLocationOrder)
                {
                    case 2:
                        locations = locations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList();
                        break;
                    default:
                        locations = locations.OrderBy(x => x.Name).ToList();
                        break;
                }
                var expander = new Expander
                {
                    Name = zoneExpanderName,
                    Header = $"{zoneName} [Locations: {locations.Count}]",
                    Content = GetLocationsObject(locations),
                    IsExpanded = ExpandedZonesWithLocations.Contains(zoneExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedZonesWithLocations.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedZonesWithLocations.Remove((e.Source as Expander).Name);
                HelperLocationList.Items.Add(expander);
            }
        }

        #endregion

        #region Locations by Rooms by Area

        private void UpdateHelperLocationListWithLocationsByRoomByArea(Dictionary<string, Dictionary<string, List<LocationWithTime>>> locationsByRoomByArea)
        {
            HelperLocationList.Items.Clear();

            GetMajorCountables();
            GetPreviewedLocations();
            GetPreviewedItems();

            foreach (var area in locationsByRoomByArea)
            {
                var roomStacker = new StackPanel();
                roomStacker.Margin = new Thickness(20, 0, 0, 0);

                var areaName = area.Key.WithoutUnderscores();
                var areaExpanderName = areaName.AsObjectName();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetRoomWithLocationsExpander(x)));

                var expander = new Expander
                {
                    Name = areaExpanderName,
                    Header = new TextBlock { Text = $"{areaName} [Rooms: {rooms.Count} / Locations: {rooms.Sum(x => x.Value.Count)}]" },
                    Content = roomStacker,
                    IsExpanded = ExpandedZonesWithLocations.Contains(areaExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedZonesWithLocations.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedZonesWithLocations.Remove((e.Source as Expander).Name);
                HelperLocationList.Items.Add(expander);
            }
        }

        private Expander GetRoomWithLocationsExpander(KeyValuePair<string, List<LocationWithTime>> roomWithLocations)
        {
            var roomName = roomWithLocations.Key.WithoutUnderscores();
            var expanderName = roomName.AsObjectName();
            var locations = roomWithLocations.Value;

            switch (_settings.SelectedHelperLocationOrder)
            {
                case 2:
                    locations = locations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList();
                    break;
                default:
                    locations = locations.OrderBy(x => x.Name).ToList();
                    break;
            }

            var locationStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
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
            var expander = new Expander
            {
                Name = expanderName,
                Header = $"{roomName}\t[Locations: {locations.Count}]",
                Content = GetLocationsObject(locations),
                IsExpanded = ExpandedRoomsWithLocations.Contains(expanderName)
            };
            expander.Expanded += (object _, RoutedEventArgs e) => ExpandedRoomsWithLocations.Add((e.Source as Expander).Name);
            expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedRoomsWithLocations.Remove((e.Source as Expander).Name);
            return expander;
        }

        private HashSet<string> ExpandedRoomsWithLocations = new HashSet<string>();

        #endregion

        #region Locations without grouping

        private void UpdateHelperLocationListWithLocations(List<LocationWithTime> locations)
        {
            HelperLocationList.Items.Clear();

            GetMajorCountables();
            GetPreviewedLocations();
            GetPreviewedItems();

            var orderedLocations = locations.Select(x => x).ToList();

            switch (_settings.SelectedHelperLocationOrder)
            {
                case 2:
                    orderedLocations = orderedLocations.OrderBy(x => x.TimeAdded).ThenBy(x => x.Name).ToList();
                    break;
                default:
                    orderedLocations = orderedLocations.OrderBy(x => x.Name).ToList();
                    break;
            }

            HelperLocationList.Items.Add(GetLocationsObject(orderedLocations));
        }

        #endregion

        private HashSet<string> ExpandedZonesWithLocations = new HashSet<string>();

        private object GetLocationsObject(List<LocationWithTime> locations)
        {
            switch (_settings.SelectedHelperLocationOrder)
            {
                case 1:
                case 2:
                    var locationKvps = locations.ToDictionary(x => $"{(x.IsOutOfLogic ? "*" : "")}{x.Name.WithoutUnderscores()}", x => GetAgeInMinutes(_referenceTime, x.TimeAdded)).ToList();
                    return GenerateAutoStarGrid(locationKvps);
                default:
                    var locationStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
                    locations.ForEach(y => locationStacker.Children.Add(new TextBlock { Text = $"{(y.IsOutOfLogic ? "*" : "")}{y.Name.WithoutUnderscores()}" }));
                    return locationStacker;
            }
        }

        private void GetMajorCountables()
        {
            var pooledItems = _trackerLogReader.GetItemsByPool();

            // Grubs
            var grubCount = pooledItems.FirstOrDefault(x => x.Key == "Grub").Value?.Count;
            if (grubCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Grubs: {grubCount}" });
            }
            // Charms
            var charmCount = pooledItems.FirstOrDefault(x => x.Key == "Charm").Value?.Count(x => !x.Name.Contains("_Fragment") & !x.Name.Contains("Unbreakable_"));
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
            var eggCount = pooledItems.FirstOrDefault(x => x.Key == "Egg").Value?.Count;
            if (eggCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Rancid Eggs: {eggCount}" });
            }
            // Pale Ore
            var oreCount = pooledItems.FirstOrDefault(x => x.Key == "Ore").Value?.Count;
            if (oreCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Pale Ore: {oreCount}" });
            }
            // Grimmkin Flames
            var flameCount = pooledItems.FirstOrDefault(x => x.Key == "Flame").Value?.Count;
            if (flameCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Grimmkin Flames: {flameCount}" });
            }
            // MrMushroom Levels
            var mushroomCount = pooledItems.FirstOrDefault(x => x.Key == "MrMushroom").Value?.Count;
            if (mushroomCount != null)
            {
                HelperLocationList.Items.Add(new TextBlock { Text = $"Mr Mushroom Level: {mushroomCount}" });
            }
        }

        private void GetPreviewedLocations()
        {
            var previewedLocations = _helperLogReader.GetPreviewedLocations();
            if (previewedLocations.Count == 0)
            {
                return;
            }

            var poolStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
            previewedLocations.OrderBy(x => x.Key).ToList().ForEach(poolWithLocations =>
            {
                var pool = poolWithLocations.Key.WithoutUnderscores();
                var locationStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
                poolWithLocations.Value.OrderBy(x => x.LocationName).GroupBy(x => x.LocationName).ToDictionary(x => x.Key, x => x.ToList()).ToList().ForEach(locationWithItems =>
                {
                    var location = locationWithItems.Key;
                    var itemsWithCosts = locationWithItems.Value
                    .OrderBy(x => x.SecondaryCostValue)
                    .ThenBy(x => x.ItemCostValue)
                    .ThenBy(x => x.ItemName)
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

                var locationExpander = new Expander
                {
                    Name = pool.AsObjectName(),
                    Header = pool,
                    Content = locationStacker,
                    IsExpanded = ExpandedPreviewedLocationPools.Contains(pool.AsObjectName())
                };
                locationExpander.Expanded += (object _, RoutedEventArgs e) => ExpandedPreviewedLocationPools.Add((e.Source as Expander).Name);
                locationExpander.Collapsed += (object _, RoutedEventArgs e) => ExpandedPreviewedLocationPools.Remove((e.Source as Expander).Name);
                poolStacker.Children.Add(locationExpander);
            });

            var expander = new Expander
            {
                Name = "PreviewedLocations",
                Header = "Previewed Locations",
                Content = poolStacker,
                IsExpanded = _expandPreviewedLocationSection
            };
            expander.Expanded += (object _, RoutedEventArgs e) => _expandPreviewedLocationSection = true;
            expander.Collapsed += (object _, RoutedEventArgs e) => _expandPreviewedLocationSection = false;

            HelperLocationList.Items.Add(expander);
        }

        private bool _expandPreviewedLocationSection;
        private HashSet<string> ExpandedPreviewedLocationPools = new HashSet<string>();

        private void GetPreviewedItems()
        {
            var previewedItems = _helperLogReader.GetPreviewedItems();
            if (previewedItems.Count == 0)
            {
                return;
            }

            var poolStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
            previewedItems.OrderBy(x => x.Key).ToList().ForEach(poolWithLocations =>
            {
                var pool = poolWithLocations.Key.WithoutUnderscores();
                var locationStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
                poolWithLocations.Value.OrderBy(x => x.LocationName).GroupBy(x => x.LocationName).ToDictionary(x => x.Key, x => x.ToList()).ToList().ForEach(locationWithItems =>
                {
                    var location = locationWithItems.Key;
                    var itemsWithLocationsAndCosts = locationWithItems.Value
                    .OrderBy(x => x.SecondaryCostValue)
                    .ThenBy(x => x.ItemCostValue)
                    .ThenBy(x => x.ItemName)
                    .Select(x => new KeyValuePair<string, string>(x.ItemName, $"at {x.LocationName} {(!string.IsNullOrWhiteSpace(x.ItemCost) ? $"({x.ItemCost})" : "")}"))
                    .ToList();

                    locationStacker.Children.Add(GenerateAutoStarGrid(itemsWithLocationsAndCosts));
                });

                var locationExpander = new Expander
                {
                    Name = pool.AsObjectName(),
                    Header = pool,
                    Content = locationStacker,
                    IsExpanded = ExpandedPreviewedItemPools.Contains(pool.AsObjectName())
                };
                locationExpander.Expanded += (object _, RoutedEventArgs e) => ExpandedPreviewedItemPools.Add((e.Source as Expander).Name);
                locationExpander.Collapsed += (object _, RoutedEventArgs e) => ExpandedPreviewedItemPools.Remove((e.Source as Expander).Name);
                poolStacker.Children.Add(locationExpander);
            });

            var expander = new Expander
            {
                Name = "PreviewedItems",
                Header = "Previewed Items",
                Content = poolStacker,
                IsExpanded = _expandPreviewedItemSection
            };
            expander.Expanded += (object _, RoutedEventArgs e) => _expandPreviewedItemSection = true;
            expander.Collapsed += (object _, RoutedEventArgs e) => _expandPreviewedItemSection = false;

            HelperLocationList.Items.Add(expander);
        }

        private bool _expandPreviewedItemSection;
        private HashSet<string> ExpandedPreviewedItemPools = new HashSet<string>();

        #endregion

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

        private void HelperLocationExpand_Click(object sender, RoutedEventArgs e)
            => ExpandExpanders(HelperLocationList);

        private void HelperLocationCollapse_Click(object sender, RoutedEventArgs e)
            => CollapseExpanders(HelperLocationList);

        #endregion
    }
}
