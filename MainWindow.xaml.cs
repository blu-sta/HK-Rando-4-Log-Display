using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using HK_Rando_4_Log_Display.FileReader;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow : Window
    {
        private object _lock = new object(); 
        private readonly Timer dataExtractor = new(5000);
        private readonly IHelperLogReader _helperLogReader;
        private readonly ITrackerLogReader _trackerLogReader;
        private readonly ISettingsReader _settingsReader;
        private readonly IItemSpoilerReader _itemSpoilerReader;
        private readonly ITransitionSpoilerReader _transitionSpoilerReader;
        private readonly IResourceLoader _resourceLoader;
        private Settings _settings;
        private string _selectedTab;
        private DateTime _referenceTime;

        public MainWindow(IHelperLogReader helperLogReader, ITrackerLogReader trackerLogReader, ISettingsReader settingsReader,
            IItemSpoilerReader itemSpoilerReader, ITransitionSpoilerReader transitionSpoilerReader, IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
            _helperLogReader = helperLogReader;
            _trackerLogReader = trackerLogReader;
            _settingsReader = settingsReader;
            _itemSpoilerReader = itemSpoilerReader;
            _transitionSpoilerReader = transitionSpoilerReader;
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitialiseSettings();
            InitialiseButtons();
#if DEBUG
            dataExtractor.Interval = 60000;
#endif
            dataExtractor.Elapsed += UpdateTabs;
            dataExtractor.AutoReset = true;
            dataExtractor.Enabled = true;
            UpdateTabs();

            Dispatcher.Invoke(new Action(() => Footer.Text = "v0.6.0.2 blu.sta"), DispatcherPriority.ContextIdle);
        }

        private void InitialiseSettings()
        {
            _settings = _resourceLoader.GetUserSettings();
        }

        private void InitialiseButtons()
        {
            HelperLocationGrouping.Content = _helperGroupings[(int)_settings.SelectedHelperLocationGrouping];
            HelperLocationOrder.Content = _helperOrders[(int)_settings.SelectedHelperLocationOrder];
            HelperTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedHelperTransitionGrouping];
            HelperTransitionOrder.Content = _helperOrders[(int)_settings.SelectedHelperTransitionOrder];
            TrackerItemGrouping.Content = _trackerItemGroupings[(int)_settings.SelectedTrackerItemGrouping];
            TrackerItemOrder.Content = _trackerItemOrders[(int)_settings.SelectedTrackerItemOrder];
            TrackerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedTrackerTransitionGrouping];
            TrackerTransitionOrder.Content = _trackerTransitionOrders[(int)_settings.SelectedTrackerTransitionOrder];
            SpoilerItemGrouping.Content = _spoilerItemGroupings[(int)_settings.SelectedSpoilerItemGrouping];
            SpoilerItemOrder.Content = _spoilerItemOrders[(int)_settings.SelectedSpoilerItemOrder];
            SpoilerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedSpoilerTransitionGrouping];
            SpoilerTransitionOrder.Content = _spoilerTransitionOrders[(int)_settings.SelectedSpoilerTransitionOrder];
        }

        private void UpdateTabs(object _, ElapsedEventArgs __) => UpdateTabs();

        private bool _firstRun = true;

        private void UpdateTabs()
        {
            lock(_lock)
            {
                if (_firstRun)
                {
                    PreloadHelperLog();
                }

                LoadFiles();
                SetReferenceTime();

                if (_firstRun)
                {
                    UpdateHelperLocationTab();
                    UpdateHelperTransitionTab();
                    UpdateTrackerItemTab();
                    UpdateTrackerTransitionTab();
                    UpdateSettingsTab();
                    UpdateSpoilerItemTab();
                    UpdateSpoilerTransitionTab();
                    UpdateHeader();
                    _firstRun = false;
                    return;
                }

                UpdateHelperLocationTab();
                UpdateHelperTransitionTab();

                switch (_selectedTab)
                {
                    case "Item Helper":
                    case "Transition Helper":
                        // Always update the helper tabs
                        break;
                    case "Item Tracker":
                        UpdateTrackerItemTab();
                        break;
                    case "Transition Tracker":
                        UpdateTrackerTransitionTab();
                        break;
                    case "Settings":
                        UpdateSettingsTab();
                        break;
                    case "Item Spoiler":
                        UpdateSpoilerItemTab();
                        break;
                    case "Transition Spoiler":
                        UpdateSpoilerTransitionTab();
                        break;
                }

                UpdateHeader();
            }
        }

        private void PreloadHelperLog()
        {
            _helperLogReader.PreloadData();
        }

        private void LoadFiles()
        {
            _helperLogReader.LoadData();
            _trackerLogReader.LoadData();
            _settingsReader.LoadData();
            _itemSpoilerReader.LoadData();
            _transitionSpoilerReader.LoadData();
        }

        private void UpdateHeader()
        {
            var headerStrings = new List<string>();
            if (!_helperLogReader.IsFileFound || !_trackerLogReader.IsFileFound)
            {
                headerStrings.Add("Some randomisation files were not found");

                if (!_helperLogReader.IsFileFound)
                {
                    headerStrings.Add("HelperLog.txt not found");
                }
                if (!_trackerLogReader.IsFileFound)
                {
                    headerStrings.Add("TrackerLog.txt not found");
                }
                if (!_settingsReader.IsFileFound)
                {
                    headerStrings.Add("settings.txt not found");
                }
                if (!_itemSpoilerReader.IsFileFound)
                {
                    headerStrings.Add("ItemSpoilerLog.json not found");
                }
                if (!_transitionSpoilerReader.IsFileFound)
                {
                    headerStrings.Add("TransitionSpoilerLog.json not found");
                }
            }

            if (_trackerLogReader.IsFileFound)
            {
                headerStrings.Add(string.Join(": ", new List<string> { _settingsReader.GetMode(), _settingsReader.GetSeed() }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }

            Dispatcher.Invoke(new Action(() => UpdateHeaderText(string.Join("\n", headerStrings))), DispatcherPriority.ContextIdle);
        }

        private void UpdateHeaderText(string headerText) => Header.Text = headerText;

        private void SetReferenceTime() =>
            _referenceTime = DateTime.Now;

        private string GetAgeInMinutes(DateTime time)
        {
            if ((int)(_referenceTime - time).TotalDays > 10)
            {
                return $"A long time ago";
            }

            var totalMinutes = (int)(_referenceTime - time).TotalMinutes;

            if (totalMinutes <= 0)
            {
                return "*new*";
            }

            if (totalMinutes < 60)
            {
                return $"{totalMinutes} min{(totalMinutes > 1 ? "s" : "")} ago";
            }

            var totalHours = totalMinutes / 60;
            var minutes = totalMinutes % 60;

            if (totalHours < 24)
            {
                return $"{totalHours} hr{(totalHours > 1 ? "s" : "")} {minutes} min{(minutes > 1 ? "s" : "")} ago";
            }

            var totalDays = totalHours / 24;
            var hours = totalHours % 24;

            return $"{totalDays} day{(totalDays > 1 ? "s" : "")} {hours}:{minutes:00} hrs ago";
        }

        private string[] _helperGroupings = new[] { "Group by map area", "Group by titled area", "Group by map area by room", "Group by titled area by room", "Group by room", "No grouping" };
        private string[] _helperOrders = new[] { "Order alphabetically", "Order alphabetically (show time)", "Order by time" };

        private void UpdateHelperLocationTab()
        {
            switch (_settings.SelectedHelperLocationGrouping)
            {
                case 2:
                    // Locations by Rooms by (Map) Area
                    var locationsByRoomByMapArea = _helperLogReader.GetLocationsByRoomByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperLocationListWithLocationsByRoomByArea(locationsByRoomByMapArea)), DispatcherPriority.ContextIdle);
                    break;
                case 3:
                    // Locations by Rooms by (Titled) Area
                    var locationsByRoomByTitledArea = _helperLogReader.GetLocationsByRoomByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperLocationListWithLocationsByRoomByArea(locationsByRoomByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                case 4:
                    // Locations by Rooms
                    var locationsByRoom = _helperLogReader.GetLocationsByRoom();
                    Dispatcher.Invoke(new Action(() => UpdateHelperLocationListWithLocationsByZone(locationsByRoom)), DispatcherPriority.ContextIdle);
                    break;
                case 5:
                    // All locations
                    var locations = _helperLogReader.GetLocations();
                    Dispatcher.Invoke(new Action(() => UpdateHelperLocationListWithLocations(locations)), DispatcherPriority.ContextIdle);
                    break;
                case 1:
                    // Locations by (Titled) Area
                    var locationsByTitledArea = _helperLogReader.GetLocationsByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperLocationListWithLocationsByZone(locationsByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                default:
                    // Locations by (Map) Area
                    var locationsByMapArea = _helperLogReader.GetLocationsByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperLocationListWithLocationsByZone(locationsByMapArea)), DispatcherPriority.ContextIdle);
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
                        locationStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()}\t{GetAgeInMinutes(y.TimeAdded)}" });
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
                    var locationKvps = locations.ToDictionary(x => $"{(x.IsOutOfLogic ? "*" : "")}{x.Name.WithoutUnderscores()}" , x => GetAgeInMinutes(x.TimeAdded)).ToList();
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
            var previewedLocations = _helperLogReader.GetPreviewedLocations3();
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
                    .OrderBy(x => x.ItemCostValue)
                    .ThenBy(x => x.ItemName)
                    .Select(x => new KeyValuePair<string, string>(x.ItemName, x.ItemCost))
                    .ToList();

                    locationStacker.Children.Add(new TextBlock { Text = location });
                    locationStacker.Children.Add(GenerateAutoStarGrid(itemsWithCosts));
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
                    .OrderBy(x => x.ItemCostValue)
                    .ThenBy(x => x.ItemName)
                    .Select(x => new KeyValuePair<string, string>(x.ItemName, $"{x.LocationName} {(!string.IsNullOrWhiteSpace(x.ItemCost) ? $"({x.ItemCost})" : "")}"))
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

        private void UpdateHelperTransitionTab()
        {
            switch (_settings.SelectedHelperTransitionGrouping)
            {
                case 2:
                    // Transitions by Rooms by (Map) Area
                    var transitionsByRoomByMapArea = _helperLogReader.GetTransitionsByRoomByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperTransitionListWithTransitionsByRoomByArea(transitionsByRoomByMapArea)), DispatcherPriority.ContextIdle);
                    break;
                case 3:
                    // Transitions by Rooms by (Titled) Area
                    var transitionsByRoomByTitledArea = _helperLogReader.GetTransitionsByRoomByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperTransitionListWithTransitionsByRoomByArea(transitionsByRoomByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                case 4:
                    // Transitions by Rooms
                    var transitionsByRoom = _helperLogReader.GetTransitionsByRoom();
                    Dispatcher.Invoke(new Action(() => UpdateHelperTransitionListWithTransitionsByZone(transitionsByRoom)), DispatcherPriority.ContextIdle);
                    break;
                case 5:
                    // All transitions
                    var transitions = _helperLogReader.GetTransitions();
                    Dispatcher.Invoke(new Action(() => UpdateHelperTransitionListWithTransitions(transitions)), DispatcherPriority.ContextIdle);
                    break;
                case 1:
                    // Transitions by (Titled) Area
                    var transitionsByTitledArea = _helperLogReader.GetTransitionsByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperTransitionListWithTransitionsByZone(transitionsByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                default:
                    // Transitions by (Map) Area
                    var transitionsByMapArea = _helperLogReader.GetTransitionsByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateHelperTransitionListWithTransitionsByZone(transitionsByMapArea)), DispatcherPriority.ContextIdle);
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
                var roomStacker = new StackPanel();
                roomStacker.Margin = new Thickness(20, 0, 0, 0);

                var areaName = area.Key.WithoutUnderscores();
                var areaExpanderName = areaName.AsObjectName();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetRoomWithTransitionsExpander(x)));

                var expander = new Expander
                {
                    Name = areaExpanderName,
                    Header = new TextBlock { Text = $"{areaName} [Rooms: {rooms.Count} / Transitions: {rooms.Sum(x => x.Value.Count)}]" },
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

            var transitionStacker = new StackPanel();
            transitionStacker.Margin = new Thickness(20, 0, 0, 0);
            transitions.ForEach(y =>
            {
                switch (_settings.SelectedHelperTransitionOrder)
                {
                    case 2:
                        transitionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()}\t{GetAgeInMinutes(y.TimeAdded)}" });
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
                    var transitionKvps = transitions.ToDictionary(x => $"{(x.IsOutOfLogic ? "*" : "")}{x.Name.WithoutUnderscores()}" , x => GetAgeInMinutes(x.TimeAdded)).ToList();
                    return GenerateAutoStarGrid(transitionKvps);
                default:
                    var transtionStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
                    transitions.ForEach(y => transtionStacker.Children.Add(new TextBlock { Text = $"{(y.IsOutOfLogic ? "*" : "")}{y.Name.WithoutUnderscores()}" }));
                    return transtionStacker;
            }
        }

        #endregion

        private string[] _trackerItemGroupings = new[] { "Curated groups", "Group by pool", "No grouping" };
        private string[] _trackerItemOrders = new[] { "Order alphabetically", "Order by time" };

        private void UpdateTrackerItemTab()
        {
            switch (_settings.SelectedTrackerItemGrouping)
            {
                case 1:
                    // Items by Pool
                    var itemsByPool = _trackerLogReader.GetItemsByPool();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerItemListWithItemsByPool(itemsByPool)), DispatcherPriority.ContextIdle);
                    // Alphabetical
                    // Find order
                    break;
                case 2:
                    // All items
                    var items = _trackerLogReader.GetItems();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerItemListWithItems(items)), DispatcherPriority.ContextIdle);
                    // Alphabetical
                    // Find order
                    break;
                default:
                    // Curated item list
                    var itemsByCuratedPool = _trackerLogReader.GetCuratedItems();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerItemListWithItemsByPool(itemsByCuratedPool)), DispatcherPriority.ContextIdle);
                    // Alphabetical
                    // Find order
                    break;
            }
        }

        #region Items

        #region ItemsByPool

        private void UpdateTrackerItemListWithItemsByPool(Dictionary<string, List<ItemWithLocation>> itemsByPool)
        {
            TrackerItemList.Items.Clear();
            foreach (var pool in itemsByPool)
            {
                var poolName = pool.Key.WithoutUnderscores();
                var poolExpanderName = poolName.AsObjectName();
                var items = pool.Value;
                switch (_settings.SelectedTrackerItemOrder)
                {
                    case 1:
                        // Do nothing, the order is originally in time order
                        break;
                    default:
                        items = items.OrderBy(x => x.Name).ToList();
                        break;
                }
                var expander = new Expander
                {
                    Name = poolExpanderName,
                    Header = poolName,
                    Content = GetItemsObject(items),
                    IsExpanded = ExpandedPoolsWithItems.Contains(poolExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedPoolsWithItems.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedPoolsWithItems.Remove((e.Source as Expander).Name);
                TrackerItemList.Items.Add(expander);
            }

        }

        #endregion

        #region Items

        private void UpdateTrackerItemListWithItems(List<ItemWithLocation> items)
        {
            TrackerItemList.Items.Clear();
            var orderedItems = items.Select(x => x).ToList();
            switch (_settings.SelectedTrackerItemOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedItems = orderedItems.OrderBy(x => x.Name).ToList();
                    break;
            }
            TrackerItemList.Items.Add(GetItemsObject(orderedItems));
        }

        #endregion

        private object GetItemsObject(List<ItemWithLocation> items)
        {
            var itemKvps = items.Select(x => new KeyValuePair<string, string>(x.Name.WithoutUnderscores(), $"found at {x.Location.WithoutUnderscores()}")).ToList();
            return GenerateAutoStarGrid(itemKvps);
        }

        private HashSet<string> ExpandedPoolsWithItems = new HashSet<string>();

        #endregion

        private string[] _trackerTransitionOrders = new[] { "Order alphabetically", "Order by time" };

        private void UpdateTrackerTransitionTab()
        {
            switch (_settings.SelectedTrackerTransitionGrouping)
            {
                case 2:
                    // Transitions by Rooms by (Map) Area
                    var transitionsByRoomByMapArea = _trackerLogReader.GetTransitionsByRoomByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByMapArea)), DispatcherPriority.ContextIdle);
                    break;
                case 3:
                    // Transitions by Rooms by (Titled) Area
                    var transitionsByRoomByTitledArea = _trackerLogReader.GetTransitionsByRoomByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                case 4:
                    // Transitions by Rooms
                    var transitionsByRoom = _trackerLogReader.GetTransitionsByRoom();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerTransitionListWithTransitionsByZone(transitionsByRoom)), DispatcherPriority.ContextIdle);
                    break;
                case 5:
                    // All transitions
                    var transitions = _trackerLogReader.GetTransitions();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerTransitionListWithTransitions(transitions)), DispatcherPriority.ContextIdle);
                    break;
                case 1:
                    // Transitions by (Titled) Area
                    var transitionsByTitledArea = _trackerLogReader.GetTransitionsByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerTransitionListWithTransitionsByZone(transitionsByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                default:
                    // Transitions by (Map) Area
                    var transitionsByMapArea = _trackerLogReader.GetTransitionsByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateTrackerTransitionListWithTransitionsByZone(transitionsByMapArea)), DispatcherPriority.ContextIdle);
                    break;
            }
        }

        #region Tracked Transitions

        #region Transitions by Area OR Room

        private void UpdateTrackerTransitionListWithTransitionsByZone(Dictionary<string, List<TransitionWithDestination>> transitionsByZone)
        {
            TrackerTransitionList.Items.Clear();
            foreach (var zone in transitionsByZone)
            {
                var zoneName = zone.Key.WithoutUnderscores();
                var zoneExpanderName = zoneName.AsObjectName();
                var transitions = zone.Value;
                switch (_settings.SelectedTrackerTransitionOrder)
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
                    Content = GetTransitionsObject(transitions),
                    IsExpanded = ExpandedTrackedZonesWithTransitions.Contains(zoneExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Remove((e.Source as Expander).Name);
                TrackerTransitionList.Items.Add(expander);
            }
        }

        #endregion

        #region Transitions by Rooms by Area

        private void UpdateTrackerTransitionListWithTransitionsByRoomByArea(Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> transitionsByRoomByArea)
        {
            TrackerTransitionList.Items.Clear();
            foreach (var area in transitionsByRoomByArea)
            {
                var roomStacker = new StackPanel();
                roomStacker.Margin = new Thickness(20, 0, 0, 0);

                var areaName = area.Key.WithoutUnderscores();
                var areaExpanderName = areaName.AsObjectName();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetRoomWithTransitionsExpander(x)));

                var expander = new Expander
                {
                    Name = areaExpanderName,
                    Header = new TextBlock { Text = $"{areaName} [Rooms: {rooms.Count} / Transitions: {rooms.Sum(x => x.Value.Count)}]" },
                    Content = roomStacker,
                    IsExpanded = ExpandedTrackedZonesWithTransitions.Contains(areaExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedTrackedZonesWithTransitions.Remove((e.Source as Expander).Name);
                TrackerTransitionList.Items.Add(expander);
            }
        }

        private Expander GetRoomWithTransitionsExpander(KeyValuePair<string, List<TransitionWithDestination>> roomWithTransitions)
        {
            var roomName = roomWithTransitions.Key.WithoutUnderscores();
            var expanderName = roomName.AsObjectName();
            var transitions = roomWithTransitions.Value;

            switch (_settings.SelectedTrackerTransitionOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    transitions = transitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            var transitionStacker = new StackPanel();
            transitionStacker.Margin = new Thickness(20, 0, 0, 0);
            transitions.ForEach(y =>
            {
                transitionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()} --> {y.DestinationName.WithoutUnderscores()}" });
            });
            var expander = new Expander
            {
                Name = expanderName,
                Header = $"{roomName}\t[Transitions: {transitions.Count}]",
                Content = GetTransitionsObject(transitions),
                IsExpanded = ExpandedTrackedRoomsWithTransitions.Contains(expanderName)
            };
            expander.Expanded += (object _, RoutedEventArgs e) => ExpandedTrackedRoomsWithTransitions.Add((e.Source as Expander).Name);
            expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedTrackedRoomsWithTransitions.Remove((e.Source as Expander).Name);
            return expander;
        }

        private HashSet<string> ExpandedTrackedRoomsWithTransitions = new HashSet<string>();

        #endregion

        #region Transitions without grouping

        private void UpdateTrackerTransitionListWithTransitions(List<TransitionWithDestination> transitions)
        {
            TrackerTransitionList.Items.Clear();

            var orderedTransitions = transitions.Select(x => x).ToList();

            switch (_settings.SelectedTrackerTransitionOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedTransitions = orderedTransitions.OrderBy(x => x.Name).ToList();
                    break;
            }

            TrackerTransitionList.Items.Add(GetTransitionsObject(orderedTransitions));
        }

        #endregion

        private HashSet<string> ExpandedTrackedZonesWithTransitions = new HashSet<string>();

        private object GetTransitionsObject(List<TransitionWithDestination> transitions)
        {
            var transtionStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
            transitions.ForEach(y => transtionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()} --> {y.DestinationName.WithoutUnderscores()}" }));
            return transtionStacker;
        }

        #endregion

        #region Settings

        private void UpdateSettingsTab()
        {
            var settings = _settingsReader.GetSettings();
            Dispatcher.Invoke(new Action(() => UpdateSettingsList(settings)), DispatcherPriority.ContextIdle);
        }

        private void UpdateSettingsList(JObject settings)
        {
            SettingsList.Items.Clear();
            if (settings == null)
            {
                return;
            }
            foreach (var setting in settings)
            {
                var settingName = setting.Key.WithoutUnderscores();
                var settingExpanderName = settingName.AsObjectName();
                var internalSettingValue = setting.Value;

                var expander = new Expander
                {
                    Name = settingExpanderName,
                    Header = settingName,
                    Content = GetSettingObject(internalSettingValue),
                    IsExpanded = ExpandedSettings.Contains(settingExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSettings.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSettings.Remove((e.Source as Expander).Name);
                SettingsList.Items.Add(expander);
            }
        }

        private object GetSettingObject(JToken internalSettingValue)
        {
            switch (internalSettingValue.Type)
            {
                case JTokenType.Object:
                    var internalSettings = internalSettingValue.ToObject<Dictionary<string, string>>().ToList();
                    return GenerateAutoStarGrid(internalSettings);
                case JTokenType.Integer:
                case JTokenType.String:
                    var stringStacker = new StackPanel
                    {
                        Margin = new Thickness(20, 0, 0, 0)
                    };
                    stringStacker.Children.Add(new TextBlock { Text = $"{internalSettingValue.Value<string>()}" });
                    return stringStacker;
                default:
                    var defaultStacker = new StackPanel
                    {
                        Margin = new Thickness(20, 0, 0, 0)
                    };
                    defaultStacker.Children.Add(new TextBlock { Text = $"Failed to read settings correctly" });
                    return defaultStacker;
            }
        }

        private HashSet<string> ExpandedSettings = new HashSet<string>();

        #endregion

        #region Spoilers

        #region TO BE UPDATED

        private string[] _spoilerItemGroupings = new[] { "Curated groups", "Group by pool", "No grouping" };
        private string[] _spoilerItemOrders = new[] { "Order alphabetically", "Order by seed default" };

        private void UpdateSpoilerItemTab()
        {
            switch (_settings.SelectedSpoilerItemGrouping)
            {
                case 1:
                    // Items by Pool
                    var itemsByPool = _itemSpoilerReader.GetItemsByPool();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerItemListWithItemsByPool(itemsByPool)), DispatcherPriority.ContextIdle);
                    // Alphabetical
                    // Find order
                    break;
                case 2:
                    // All items
                    var items = _itemSpoilerReader.GetItems();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerItemListWithItems(items)), DispatcherPriority.ContextIdle);
                    // Alphabetical
                    // Find order
                    break;
                default:
                    // Curated item list
                    var itemsByCuratedPool = _itemSpoilerReader.GetCuratedItems();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerItemListWithItemsByPool(itemsByCuratedPool)), DispatcherPriority.ContextIdle);
                    // Alphabetical
                    // Find order
                    break;
            }
        }

        #region Items

        #region ItemsByPool

        private void UpdateSpoilerItemListWithItemsByPool(Dictionary<string, List<ItemWithLocation>> itemsByPool)
        {
            SpoilerItemList.Items.Clear();
            foreach (var pool in itemsByPool)
            {
                var poolName = pool.Key.WithoutUnderscores();
                var poolExpanderName = poolName.AsObjectName();
                var items = pool.Value;
                switch (_settings.SelectedSpoilerItemOrder)
                {
                    case 1:
                        // Do nothing, the order is originally in time order
                        break;
                    default:
                        items = items.OrderBy(x => x.Name).ToList();
                        break;
                }
                var expander = new Expander
                {
                    Name = poolExpanderName,
                    Header = poolName,
                    Content = GetSpoilerItemsObject(items),
                    IsExpanded = ExpandedSpoilerPoolsWithItems.Contains(poolExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSpoilerPoolsWithItems.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSpoilerPoolsWithItems.Remove((e.Source as Expander).Name);
                SpoilerItemList.Items.Add(expander);
            }

        }

        #endregion

        #region Items

        private void UpdateSpoilerItemListWithItems(List<ItemWithLocation> items)
        {
            SpoilerItemList.Items.Clear();
            var orderedItems = items.Select(x => x).ToList();
            switch (_settings.SelectedSpoilerItemOrder)
            {
                case 1:
                    // Do nothing, the order is originally in time order
                    break;
                default:
                    orderedItems = orderedItems.OrderBy(x => x.Name).ToList();
                    break;
            }
            SpoilerItemList.Items.Add(GetSpoilerItemsObject(orderedItems));
        }

        #endregion

        private object GetSpoilerItemsObject(List<ItemWithLocation> items)
        {
            var itemKvps = items.Select(x => new KeyValuePair<string, string>(x.Name.WithoutUnderscores(), $"found at {x.Location.WithoutUnderscores()}")).ToList();
            return GenerateAutoStarGrid(itemKvps);
        }

        private HashSet<string> ExpandedSpoilerPoolsWithItems = new HashSet<string>();

        #endregion

        private string[] _spoilerTransitionOrders = new[] { "Order alphabetically", "Order by seed default" };

        private void UpdateSpoilerTransitionTab()
        {
            switch (_settings.SelectedSpoilerTransitionGrouping)
            {
                case 2:
                    // Transitions by Rooms by (Map) Area
                    var transitionsByRoomByMapArea = _transitionSpoilerReader.GetTransitionsByRoomByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByMapArea)), DispatcherPriority.ContextIdle);
                    break;
                case 3:
                    // Transitions by Rooms by (Titled) Area
                    var transitionsByRoomByTitledArea = _transitionSpoilerReader.GetTransitionsByRoomByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerTransitionListWithTransitionsByRoomByArea(transitionsByRoomByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                case 4:
                    // Transitions by Rooms
                    var transitionsByRoom = _transitionSpoilerReader.GetTransitionsByRoom();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerTransitionListWithTransitionsByZone(transitionsByRoom)), DispatcherPriority.ContextIdle);
                    break;
                case 5:
                    // All transitions
                    var transitions = _transitionSpoilerReader.GetTransitions();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerTransitionListWithTransitions(transitions)), DispatcherPriority.ContextIdle);
                    break;
                case 1:
                    // Transitions by (Titled) Area
                    var transitionsByTitledArea = _transitionSpoilerReader.GetTransitionsByTitledArea();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerTransitionListWithTransitionsByZone(transitionsByTitledArea)), DispatcherPriority.ContextIdle);
                    break;
                default:
                    // Transitions by (Map) Area
                    var transitionsByMapArea = _transitionSpoilerReader.GetTransitionsByMapArea();
                    Dispatcher.Invoke(new Action(() => UpdateSpoilerTransitionListWithTransitionsByZone(transitionsByMapArea)), DispatcherPriority.ContextIdle);
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
                var roomStacker = new StackPanel();
                roomStacker.Margin = new Thickness(20, 0, 0, 0);

                var areaName = area.Key.WithoutUnderscores();
                var areaExpanderName = areaName.AsObjectName();
                var rooms = area.Value.OrderBy(x => x.Key).ToList();
                rooms.ForEach(x => roomStacker.Children.Add(GetSpoilerRoomWithTransitionsExpander(x)));

                var expander = new Expander
                {
                    Name = areaExpanderName,
                    Header = new TextBlock { Text = $"{areaName} [Rooms: {rooms.Count} / Transitions: {rooms.Sum(x => x.Value.Count)}]" },
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

            var transitionStacker = new StackPanel();
            transitionStacker.Margin = new Thickness(20, 0, 0, 0);
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
            var transtionStacker = new StackPanel { Margin = new Thickness(20, 0, 0, 0) };
            transitions.ForEach(y => transtionStacker.Children.Add(new TextBlock { Text = $"{y.Name.WithoutUnderscores()} --> {y.DestinationName.WithoutUnderscores()}" }));
            return transtionStacker;
        }

        #endregion


        #endregion

        #endregion

        private Grid GenerateAutoStarGrid(List<KeyValuePair<string,string>> itemsForGrid)
        {
            var objectGrid = new Grid
            {
                Margin = new Thickness(20, 0, 0, 0)
            };
            var colDef1 = new ColumnDefinition();
            var colDef2 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Star);
            objectGrid.ColumnDefinitions.Add(colDef1);
            objectGrid.ColumnDefinitions.Add(colDef2);

            var textBlocks = itemsForGrid.SelectMany((x, i) =>
            {
                var rowDef = new RowDefinition();
                objectGrid.RowDefinitions.Add(rowDef);

                var leftBlock = new TextBlock { Text = $"{x.Key} \t" };
                Grid.SetColumn(leftBlock, 0);
                Grid.SetRow(leftBlock, i);

                var rightBlock = new TextBlock { Text = x.Value };
                Grid.SetColumn(rightBlock, 1);
                Grid.SetRow(rightBlock, i);

                return new[] { leftBlock, rightBlock };
            });
            textBlocks.ToList().ForEach(x => objectGrid.Children.Add(x));
            return objectGrid;
        }


        #region Events

        private void Body_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTab = (Body.SelectedItem as TabItem)?.Header as string;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = sender as ScrollViewer;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 8.0);
            e.Handled = true;
        }

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

        private void HelperTransitionExpand_Click(object sender, RoutedEventArgs e) 
            => ExpandExpanders(HelperTransitionList);

        private void HelperTransitionCollapse_Click(object sender, RoutedEventArgs e) 
            => CollapseExpanders(HelperTransitionList);

        private void TrackerItemGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerItemGrouping = (_settings.SelectedTrackerItemGrouping + 1) % _trackerItemGroupings.Length;
            TrackerItemGrouping.Content = _trackerItemGroupings[(int)_settings.SelectedTrackerItemGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerItemOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerItemOrder = (_settings.SelectedTrackerItemOrder + 1) % _trackerItemOrders.Length;
            TrackerItemOrder.Content = _trackerItemOrders[(int)_settings.SelectedTrackerItemOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerItemExpand_Click(object sender, RoutedEventArgs e) 
            => ExpandExpanders(TrackerItemList);

        private void TrackerItemCollapse_Click(object sender, RoutedEventArgs e) 
            => CollapseExpanders(TrackerItemList);

        private void TrackerTransitionGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerTransitionGrouping = (_settings.SelectedTrackerTransitionGrouping + 1) % _helperGroupings.Length;
            TrackerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedTrackerTransitionGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerTransitionOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedTrackerTransitionOrder = (_settings.SelectedTrackerTransitionOrder + 1) % _trackerTransitionOrders.Length;
            TrackerTransitionOrder.Content = _trackerTransitionOrders[(int)_settings.SelectedTrackerTransitionOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void TrackerTransitionExpand_Click(object sender, RoutedEventArgs e)
            => ExpandExpanders(TrackerTransitionList);

        private void TrackerTransitionCollapse_Click(object sender, RoutedEventArgs e)
            => CollapseExpanders(TrackerTransitionList);

        private void SpoilerItemGrouping_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedSpoilerItemGrouping = (_settings.SelectedSpoilerItemGrouping + 1) % _spoilerItemGroupings.Length;
            SpoilerItemGrouping.Content = _spoilerItemGroupings[(int)_settings.SelectedSpoilerItemGrouping];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void SpoilerItemOrder_Click(object sender, RoutedEventArgs e)
        {
            _settings.SelectedSpoilerItemOrder = (_settings.SelectedSpoilerItemOrder + 1) % _spoilerItemOrders.Length;
            SpoilerItemOrder.Content = _spoilerItemOrders[(int)_settings.SelectedSpoilerItemOrder];
            Dispatcher.Invoke(() => UpdateTabs());
        }

        private void SpoilerItemExpand_Click(object sender, RoutedEventArgs e)
            => ExpandExpanders(SpoilerItemList);

        private void SpoilerItemCollapse_Click(object sender, RoutedEventArgs e)
            => CollapseExpanders(SpoilerItemList);

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

        private void SpoilerTransitionExpand_Click(object sender, RoutedEventArgs e)
            => ExpandExpanders(SpoilerTransitionList);

        private void SpoilerTransitionCollapse_Click(object sender, RoutedEventArgs e)
            => CollapseExpanders(SpoilerTransitionList);

        private void ExpandExpanders(ListBox listBox)
        {
            listBox.Items.OfType<Expander>().ToList().ForEach(x =>
            {
                x.IsExpanded = true;
                (x.Content as StackPanel)?.Children.OfType<Expander>().ToList().ForEach(x => x.IsExpanded = true);
            });
        }

        private void CollapseExpanders(ListBox listBox)
        {
            listBox.Items.OfType<Expander>().ToList().ForEach(x =>
            {
                (x.Content as StackPanel)?.Children.OfType<Expander>().ToList().ForEach(x => x.IsExpanded = false);
                x.IsExpanded = false;
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveCurrentState();
            dataExtractor.Dispose();
            base.OnClosed(e);
        }

        private void SaveCurrentState()
        {
            _helperLogReader.SaveState();
            _resourceLoader.SaveUserSettings(_settings);
        }

        #endregion
    }
}
