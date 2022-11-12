using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using HK_Rando_4_Log_Display.Utils;
using System;
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
        private bool _showHelperLocationSceneDescriptions = false;

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
                        UpdateHelperLocations(_helperLogReader.GetLocationsByRoomByMapArea(_showHelperLocationSceneDescriptions), helperLocationOrdering);
                        break;
                    case RoomGrouping.RoomTitleArea:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByRoomByTitledArea(_showHelperLocationSceneDescriptions), helperLocationOrdering);
                        break;
                    case RoomGrouping.Room:
                        UpdateHelperLocations(_helperLogReader.GetLocationsByRoom(_showHelperLocationSceneDescriptions), helperLocationOrdering);
                        break;
                    case RoomGrouping.None:
                    default:
                        UpdateHelperLocations(_helperLogReader.GetLocations(), helperLocationOrdering);
                        break;
                };
            });
        }

        private void ShowDeadImports()
        {
            UpdateUX(() =>
            {
                if (_resourceLoader.DeadLocations.Any())
                    HelperLocationsList.Items.Add(GenerateExpanderWithContent("Dead Locations", GenerateAutoStarGrid(_resourceLoader.DeadLocations.Select(x => new KeyValuePair<string, string>(x.SceneName, x.Name)).ToList()), new BoolWrapper(true)));
                if (_resourceLoader.DeadTransitions.Any())
                    HelperLocationsList.Items.Add(GenerateExpanderWithContent("Dead Transitions", GenerateAutoStarGrid(_resourceLoader.DeadTransitions.Select(x => new KeyValuePair<string, string>(x.SceneName, x.DoorName)).ToList()), new BoolWrapper(true)));
            });
        }

        private void GetMajorCountables()
        {
            var objectGrid = new Grid
            {
                Margin = GenerateStandardThickness()
            };
            var colDef1 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Auto)
            };
            var colDef2 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            };
            objectGrid.ColumnDefinitions.Add(colDef1);
            objectGrid.ColumnDefinitions.Add(colDef2);

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
                var charmCount = charmHashSet?.Count(x => !x.Item.Name.Contains("_Fragment") && !x.Item.Name.Contains("Unbreakable_") && x.Item.Name != "Void_Heart");
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
            // Pale Ore
            var oreCount = GetItemCount(trackedItemsByPool, "Ore");
            if (oreCount != null)
            {
                majorCountables.Add($"Pale Ore: {oreCount}");
            }
            // Simple Keys
            var keyCount = trackedItemsByPool.FirstOrDefault(x => x.Key == "Key").Value?.Count(x => x.Item.Name == "Simple_Key");
            if (keyCount != null && keyCount > 0)
            {
                majorCountables.Add($"Simple Keys: {keyCount}");
            }
            // Rancid Eggs
            var eggCount = GetItemCount(trackedItemsByPool, "Egg");
            if (eggCount != null)
            {
                var remainingEggCount = GetRemainingEggCount(eggCount.Value);

                majorCountables.Add($"Rancid Eggs: {eggCount}{(remainingEggCount != null && remainingEggCount != eggCount ? $" [{remainingEggCount} held]" : "")}");
            }
            // Grimmkin Flames
            var flameCount = GetItemCount(trackedItemsByPool, "Flame");
            if (flameCount != null)
            {
                majorCountables.Add($"Grimmkin Flames: {flameCount}");
            }
            // MrMushroom Levels
            var mushroomCount = GetItemCount(trackedItemsByPool, "Mr Mushroom");
            if (mushroomCount != null)
            {
                majorCountables.Add($"Mr Mushroom Level: {mushroomCount}");
            }

            // True Ending Items
            var teCountables = new List<string>();
            var curatedItemsByPool = _trackerLogReader.GetCuratedItemsByPool();
            var trueEndingItems = curatedItemsByPool.FirstOrDefault(x => x.Key == "True Ending Items").Value ?? new List<ItemWithLocation>();
            var fragmentCount = trueEndingItems.Count(x => x.Item.Pool == "Charm");
            var dreamerCount = trueEndingItems.Count(x => x.Item.Pool == "Dreamer");
            var dreamNails = curatedItemsByPool.FirstOrDefault(x => x.Key == "Dream Nails").Value ?? new List<ItemWithLocation>();
            var dreamNailCount = dreamNails.Count;
            
            if (fragmentCount > 0 || dreamerCount > 0 || dreamNailCount > 0)
            {
                teCountables.Add($"True Ending Items:");

                if (fragmentCount > 0)
                {
                    teCountables.Add($"{fragmentCount}/3 fragments");
                }
                if (dreamerCount > 0)
                {
                    teCountables.Add($"{dreamerCount}/3 dreamers");
                }
                if (dreamNailCount > 0)
                {
                    teCountables.Add($"{dreamNailCount}/1 dream nails");
                }
            }

            if (!majorCountables.Any() && !teCountables.Any())
            {
                return;
            }

            var countableStacker = GenerateStackPanel();
            majorCountables.ForEach(x => countableStacker.Children.Add(new TextBlock { Text = x }));
            Grid.SetColumn(countableStacker, 0);
            Grid.SetRow(countableStacker, 0);
            objectGrid.Children.Add(countableStacker);

            var teStacker = GenerateStackPanel();
            teCountables.ForEach(x => teStacker.Children.Add(new TextBlock { Text = x }));
            Grid.SetColumn(teStacker, 1);
            Grid.SetRow(teStacker, 0);
            objectGrid.Children.Add(teStacker);

            HelperLocationsList.Items.Add(GenerateExpanderWithContent("Countables", objectGrid, _expandCountables));
        }

        private static int? GetItemCount(Dictionary<string, List<ItemWithLocation>> pooledItems, string itemPool) =>
            pooledItems.FirstOrDefault(x => x.Key == itemPool).Value?.Count;

        private int? GetRemainingEggCount(int totalEggsFound)
        {
            if (!_trackerLogReader.IsFileFound || !_itemSpoilerReader.IsFileFound)
            {
                return null;
            }

            var trackedEggShopItems = _trackerLogReader.GetLocationsByPool()
                .FirstOrDefault(x => x.Key == "Shop").Value?
                .Where(x => x.Location.Name == "Egg_Shop")
                .ToList();

            var spoilerEggShopItems = _itemSpoilerReader.GetLocationsByPool()
                .FirstOrDefault(x => x.Key == "Shop").Value?
                .Where(x => x.Location.Name == "Egg_Shop")
                .OrderBy(x => int.TryParse(Regex.Match(x.Cost, "\\d+").Value, out var number) ? number : 0)
                .ToList();

            if (!trackedEggShopItems.Any() || !spoilerEggShopItems.Any())
            {
                return null;
            }

            return totalEggsFound - (trackedEggShopItems?.Max(trackedItem =>
            {
                var itemWithLocation = TrackedSpoilerItems.GetTrackedItemWithLocation(trackedItem.Item.Name, spoilerEggShopItems);
                if (itemWithLocation == null)
                {
                    return 0;
                }
                var spoilerItem = spoilerEggShopItems.FirstOrDefault(x => itemWithLocation.Item.Name == x.Item.Name);
                spoilerEggShopItems.Remove(spoilerItem);
                return int.TryParse(Regex.Match(spoilerItem.Cost, "\\d+").Value, out var number) ? number : 0;
            }) ?? 0);
        }

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

                poolStacker.Children.Add(GenerateExpanderWithContent(locationPool, locationStacker, ExpandedPreviewedLocationPools, $"[{locationsWithItems.Count}]"));
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

                poolStacker.Children.Add(GenerateExpanderWithContent(itemPool, GenerateAutoStarGrid(itemsWithLocationsAndCosts), ExpandedPreviewedItemPools, $"[{poolWithLocations.Value.Count}]"));
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
            Helper_Location_RoomDisplay_Button.Content = GenerateButtonTextBlock(_showHelperLocationSceneDescriptions ? "Room: Desc." : "Room: Code");
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

        private void Helper_Location_RoomDisplay_Click(object sender, RoutedEventArgs e)
        {
            _showHelperLocationSceneDescriptions = !_showHelperLocationSceneDescriptions;
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void Helper_Location_Expand_Click(object sender, RoutedEventArgs e) => ExpandExpanders(HelperLocationsList);
        private void Helper_Location_Collapse_Click(object sender, RoutedEventArgs e) => CollapseExpanders(HelperLocationsList);

        #endregion
    }
}
