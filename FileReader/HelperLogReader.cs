using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Utils;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IHelperLogReader : ILogReader
    {
        public Dictionary<string, List<Location>> GetLocationsByTitledArea(string mwPlayerName);
        public Dictionary<string, List<Location>> GetLocationsByMapArea(string mwPlayerName);
        public Dictionary<string, List<Location>> GetLocationsByRoom(string mwPlayerName, bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<Location>>> GetLocationsByRoomByTitledArea(string mwPlayerName, bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<Location>>> GetLocationsByRoomByMapArea(string mwPlayerName, bool useSceneDescription);
        public List<Location> GetLocations(string mwPlayerName);
        public Dictionary<string, List<LocationPreview>> GetPreviewedLocations();
        public Dictionary<string, List<LocationPreview>> GetPreviewedItems();
        public Dictionary<string, List<Transition>> GetTransitionsByTitledArea();
        public Dictionary<string, List<Transition>> GetTransitionsByMapArea();
        public Dictionary<string, List<Transition>> GetTransitionsByRoom(bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<Transition>>> GetTransitionsByRoomByMapArea(bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<Transition>>> GetTransitionsByRoomByTitledArea(bool useSceneDescription);
        public List<Transition> GetTransitions();
        public void SaveState();
        public void PurgeMemory();
    }

    public class HelperLogReader : IHelperLogReader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IResourceLoader _resourceLoader;
        private readonly ISeedSettingsReader _settingsReader;
        private string _loadedSeed;
        private DateTime _referenceTime;
        private readonly Dictionary<string, Location> _helperLogLocations = new();
        private readonly List<LocationPreview> _previewedLocationsWithItems = new();
        private readonly Dictionary<string, Transition> _helperLogTransitions = new();

        public bool IsFileFound { get; private set; }
        public bool IsFileLoaded { get; private set; }

        public HelperLogReader(IResourceLoader resourceLoader, ISeedSettingsReader settingsReader)
        {
            _resourceLoader = resourceLoader;
            _settingsReader = settingsReader;

            if (_settingsReader.IsFileFound && _settingsReader.GetGenerationCode() == _resourceLoader.GetSeedGenerationCode())
            {
                _helperLogLocations = _resourceLoader.GetHelperLogLocations();
                _helperLogTransitions = _resourceLoader.GetHelperLogTransitions();
                _loadedSeed = _settingsReader.GetGenerationCode();
            }

            LoadData(Array.Empty<string>());
        }

        public void LoadData(string[] multiWorldPlayerNames)
        {
            IsFileFound = File.Exists(HelperLogPath);
            if (!IsFileFound)
            {
                return;
            }

            _referenceTime = DateTime.Now;
            var helperLogData = File.ReadAllLines(HelperLogPath).ToList();

            try
            {
                LoadReachableLocations(helperLogData, multiWorldPlayerNames);
                LoadPreviewedLocations(helperLogData, multiWorldPlayerNames);
                LoadReachableTransitions(helperLogData);
                IsFileLoaded = true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "HelperLogReader LoadData Error");
                IsFileLoaded = false;
            }
        }

        public void OpenFile()
        {
            if (File.Exists(HelperLogPath)) Process.Start(new ProcessStartInfo(HelperLogPath) { UseShellExecute = true });
        }

        #region Locations

        private void LoadReachableLocations(List<string> helperLogData, string[] multiWorldPlayerNames)
        {
            var uncheckedReachableLocations = LoadSection(helperLogData, "UNCHECKED REACHABLE LOCATIONS")?.Select(x => x.Trim()).ToList();
            if (uncheckedReachableLocations == null)
            {
                return;
            }

            if (_settingsReader.IsFileFound && (_settingsReader.GetGenerationCode() != _loadedSeed))
            {
                _loadedSeed = _settingsReader.GetGenerationCode();
                _helperLogLocations.Clear();
            }

            _helperLogLocations.Keys.Except(uncheckedReachableLocations).ToList()
                .ForEach(x => _helperLogLocations.Remove(x));
            uncheckedReachableLocations.Except(_helperLogLocations.Keys).ToList()
                .ForEach(x =>
                {
                    var isOutOfLogic = x.StartsWith("*");
                    var cleanedLocationName = x.Replace("*", "");

                    var mwPlayerLocation = multiWorldPlayerNames.FirstOrDefault(mwPlayerName => cleanedLocationName.StartsWith($"{mwPlayerName}'s "));
                    var locationName = string.IsNullOrEmpty(mwPlayerLocation) ? cleanedLocationName : Regex.Replace(cleanedLocationName, $"^{mwPlayerLocation}'s ", "");

                    var locationFallbackValue = GetLocationFallbackValue(locationName);
                    var locationDetails = _resourceLoader.ReferenceLocations.FirstOrDefault(y => y.Name == locationName);
                    _helperLogLocations.Add(
                    x,
                    new Location
                    {
                        MWPlayerName = mwPlayerLocation,
                        Name = locationName,
                        Pool = locationDetails?.Pool ?? locationFallbackValue,
                        SceneName = locationDetails?.SceneName ?? locationFallbackValue,
                        SceneDescription = locationDetails?.SceneDescription ?? locationFallbackValue,
                        MapArea = locationDetails?.MapArea ?? locationFallbackValue,
                        TitledArea = locationDetails?.TitledArea ?? locationFallbackValue,
                        IsOutOfLogic = isOutOfLogic,
                        TimeAdded = _referenceTime
                    });
                });
            _helperLogLocations.Where(x =>
                x.Value.Pool.StartsWith(">") &&
                multiWorldPlayerNames.Any(y => x.Value.Name.StartsWith($"{y}'s ")))
                .ToList()
                .ForEach(x =>
                {
                    var locationToUpdate = x.Value;

                    var mwPlayerLocation = multiWorldPlayerNames.FirstOrDefault(mwPlayerName => locationToUpdate.Name.StartsWith($"{mwPlayerName}'s "));
                    var locationName = string.IsNullOrEmpty(mwPlayerLocation) ? locationToUpdate.Name : Regex.Replace(locationToUpdate.Name, $"^{mwPlayerLocation}'s ", "");
                    var locationDetails = _resourceLoader.ReferenceLocations.FirstOrDefault(y => y.Name == locationName);
                    if (locationDetails != null)
                    {
                        locationToUpdate.Name = locationName;
                        locationToUpdate.Pool = locationDetails.Pool;
                        locationToUpdate.SceneName = locationDetails.SceneName;
                        locationToUpdate.SceneDescription = locationDetails.SceneDescription;
                        locationToUpdate.MapArea = locationDetails.MapArea;
                        locationToUpdate.TitledArea = locationDetails.TitledArea;
                        locationToUpdate.MWPlayerName = mwPlayerLocation;
                    }
                });
        }

        private static string GetLocationFallbackValue(string locationName) =>
            locationName.Contains("-") ? $"> {locationName.Split('-')[0]}" : "> Unrecognised Location";

        public Dictionary<string, List<Location>> GetLocationsByTitledArea(string mwPlayerName) =>
            _helperLogLocations.Values.Where(x => x.MWPlayerName == mwPlayerName)
                .GroupBy(x => x.TitledArea).OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<Location>> GetLocationsByMapArea(string mwPlayerName) =>
            _helperLogLocations.Values.Where(x => x.MWPlayerName == mwPlayerName)
                .GroupBy(x => x.MapArea).OrderBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<Location>> GetLocationsByRoom(string mwPlayerName,bool useSceneDescription) =>
            _helperLogLocations.Values.Where(x => x.MWPlayerName == mwPlayerName)
                .GroupBy(x => useSceneDescription ? x.SceneDescription : x.SceneName)
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<Location>>> GetLocationsByRoomByTitledArea(string mwPlayerName, bool useSceneDescription) =>
            _helperLogLocations.Values.Where(x => x.MWPlayerName == mwPlayerName)
                .GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => 
                    y.GroupBy(x => useSceneDescription ? x.SceneDescription : x.SceneName)
                        .ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<Location>>> GetLocationsByRoomByMapArea(string mwPlayerName, bool useSceneDescription) =>
            _helperLogLocations.Values.Where(x => x.MWPlayerName == mwPlayerName)
                .GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => 
                    y.GroupBy(x => useSceneDescription ? x.SceneDescription : x.SceneName)
                        .ToDictionary(x => x.Key, x => x.ToList()));

        public List<Location> GetLocations(string mwPlayerName) =>
            _helperLogLocations.Values.Where(x => x.MWPlayerName == mwPlayerName).ToList();

        #endregion

        #region Previewed Locations

        private void LoadPreviewedLocations(List<string> helperLogData, string[] multiWorldPlayerNames)
        {
            var previewedLocations = LoadSection(helperLogData, "PREVIEWED LOCATIONS");
            if (previewedLocations == null)
            {
                return;
            }

            _previewedLocationsWithItems.Clear();
            for (var i = 0; i < previewedLocations.Count; i++)
            {
                var line = previewedLocations[i];
                if (!line.StartsWith("    "))
                {
                    var previewLocationName = line.Trim();
                    var isOutOfLogic = previewLocationName.StartsWith("*");
                    var locationName = previewLocationName.Replace("*", "");
                    var locationFallbackValue = GetLocationFallbackValue(locationName);
                    var locationDetails = _resourceLoader.ReferenceLocations.FirstOrDefault(y => y.Name == locationName);

                    var previewLocation = new Location
                    {
                        Name = locationDetails?.Name ?? locationName,
                        Pool = locationDetails?.Pool ?? (locationName.Contains("-") ? locationName.Split('-')[0] : "Other Previewed Locations"),
                        SceneName = locationDetails?.SceneName ?? locationFallbackValue,
                        SceneDescription = locationDetails?.SceneDescription ?? locationFallbackValue,
                        MapArea = locationDetails?.MapArea ?? locationFallbackValue,
                        TitledArea = locationDetails?.TitledArea ?? locationFallbackValue,
                        IsOutOfLogic = isOutOfLogic,
                        TimeAdded = _referenceTime
                    };

                    while (i + 1 < previewedLocations.Count && previewedLocations[i + 1].StartsWith("    "))
                    {
                        var itemLine = previewedLocations[++i].Trim();

                        var editedItemLine = UpdateItemLine(previewLocation, itemLine);
                        var editedItemWithCost = editedItemLine.Split("  -  ");
                        var editedItemNames = editedItemWithCost[0];
                        var editedItemCost = editedItemWithCost.Length > 1 ? editedItemWithCost[1] : "";
                        foreach (var itemPreviewName in editedItemNames.Split(",").Select(x => x.Trim()))
                        {
                            var location = previewLocation;
                            var item = GetItemFromPreviewName(itemPreviewName, multiWorldPlayerNames);
                            var costString = editedItemCost;
                            var primaryCost = int.TryParse(Regex.Match(editedItemCost, "(\\d+) ([a-zA-Z]+)").Groups[1].Value, out var cost1) ? cost1 : 0;
                            var secondaryCost = int.TryParse(Regex.Match(editedItemCost, "(\\d+)(\\D+)(\\d+) ([a-zA-Z]+)").Groups[3].Value, out var cost2) ? cost2 : 0;

                            var previewedLocationWithItem = new LocationPreview
                            {
                                Location = location,
                                Item = item,
                                CostString = costString,
                                PrimaryCost = primaryCost,
                                SecondaryCost = secondaryCost
                            };
                            _previewedLocationsWithItems.Add(previewedLocationWithItem);
                        }
                    }
                }
            }
        }

        private static string UpdateItemLine(Location previewLocation, string itemLine)
        {
            if (previewLocation.Name == "Salubra")
            {
                return itemLine.Replace("Once you own", "Need").Replace(", I'll gladly sell it to you.", "");
            }

            if (previewLocation.Pool == "Hunter's_Notes")
            {
                var killCountMatches = Regex.Matches(itemLine, "Defeat (\\d+) more");
                var killsRequired = string.Join("/", killCountMatches.Select(x => x.Groups[1].Value).OrderBy(x =>
                {
                    var startNumbers = Regex.Match(x, "^(\\d+)").Groups[1].Value;
                    return string.IsNullOrEmpty(startNumbers) ? x : startNumbers;
                }, new SemiNumericComparer()));
                var itemProvided = Regex.Match(itemLine, "to decipher the (.+)\\.").Groups[1].Value;
                return $"{itemProvided}  -  Defeat {killsRequired}";
            }

            return itemLine;
        }

        private Item GetItemFromPreviewName(string previewName, string[] multiWorldPlayerNames)
        {
            var referenceItem = _resourceLoader.ReferenceItems.FirstOrDefault(y => y.PreviewName == previewName);
            if (referenceItem != null)
            {
                return new Item
                {
                    Name = referenceItem.Name,
                    PreviewName = referenceItem.PreviewName,
                    Pool = referenceItem.Pool
                };
            }

            var multiWorldPlayerName = multiWorldPlayerNames.FirstOrDefault(x => previewName.Contains($"{x}'s "));
            if (!string.IsNullOrEmpty(multiWorldPlayerName))
            {
                var item = GetItemFromPreviewName(previewName.Split($"{multiWorldPlayerName}'s ")[1], Array.Empty<string>());
                if (item.Pool != "> Unrecognised Items")
                {
                    return new Item
                    {
                        MWPlayerName = multiWorldPlayerName,
                        Name = item.Name,
                        Pool = item.Pool,
                        PreviewName = item.PreviewName
                    };
                }
            }

            Item GeneratePreviewItem(string pool) => new()
            {
                Name = previewName,
                PreviewName = previewName,
                Pool = pool,
            };

            if (previewName.Contains("..."))
            {
                return GeneratePreviewItem("> Area Blitz ...");
            }

            var previewNameWithoutPrefix = Regex.Replace(previewName, @"((^A )|( \[\d\]))", "");
            var matchingItem = _resourceLoader.ReferenceItems.FirstOrDefault(x => x.PreviewName == previewNameWithoutPrefix);
            if (matchingItem != null)
            {
                return new Item { Name = matchingItem.Name, PreviewName = previewName, Pool = matchingItem.Pool };
            }

            var nearlyMatchingItems = _resourceLoader.ReferenceItems.Where(x => x.Name.Contains(previewNameWithoutPrefix));
            if (nearlyMatchingItems.Count() == 1)
            {
                return new Item { Name = nearlyMatchingItems.First().Name, PreviewName = previewName, Pool = nearlyMatchingItems.First().Pool };
            }

            if (previewName.Contains("Journal Entry"))
            {
                return GeneratePreviewItem("Journal_Entry");
            }

            if (previewName.Contains("Hunter's Notes"))
            {
                return GeneratePreviewItem("Hunter's_Notes");
            }

            if (previewName == "Lore")
            {
                return GeneratePreviewItem("Lore");
            }

            var currency = Regex.Match(previewName, "\\d+ ([a-zA-Z]+)");
            if (currency.Success)
            {
                return GeneratePreviewItem(currency.Groups[1].Value);
            }

            return GeneratePreviewItem("> Unrecognised Items");
        }

        public Dictionary<string, List<LocationPreview>> GetPreviewedLocations() =>
            _previewedLocationsWithItems.GroupBy(x => x.Location.Pool).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<LocationPreview>> GetPreviewedItems() =>
            _previewedLocationsWithItems.GroupBy(x => x.Item.Pool).ToDictionary(x => x.Key, x => x.ToList());

        #endregion

        #region Transitions

        private void LoadReachableTransitions(List<string> helperLogData)
        {
            var uncheckedReachableTransitions = LoadSection(helperLogData, "UNCHECKED REACHABLE TRANSITIONS")?.Select(x => x.Trim()).ToList();
            if (uncheckedReachableTransitions == null)
            {
                _helperLogTransitions.Clear();
                return;
            }

            _helperLogTransitions.Keys.Except(uncheckedReachableTransitions).ToList()
                .ForEach(x => _helperLogTransitions.Remove(x));
            uncheckedReachableTransitions.Except(_helperLogTransitions.Keys).ToList()
                .ForEach(x =>
                {
                    var isOutOfLogic = x.StartsWith("*");
                    var transitionName = x.Replace("*", "");
                    var transitionNamePatternMatches = Regex.Match(transitionName, "(.*)\\[(.*)\\]");
                    var transitionDetails = _resourceLoader.ReferenceTransitions.FirstOrDefault(y => y.Name == transitionName)
                        ?? new ReferenceTransition
                        {
                            SceneName = transitionNamePatternMatches.Groups[1].Value,
                            SceneDescription = transitionNamePatternMatches.Groups[1].Value,
                            DoorName = transitionNamePatternMatches.Groups[2].Value,
                            MapArea = "> Unrecognised Transitions",
                            TitledArea = "> Unrecognised Transitions",
                        };
                    _helperLogTransitions.Add(
                    x,
                    new Transition
                    {
                        SceneName = transitionDetails.SceneName,
                        SceneDescription = transitionDetails.SceneDescription,
                        DoorName = transitionDetails.DoorName,
                        MapArea = transitionDetails.MapArea,
                        TitledArea = transitionDetails.TitledArea,
                        IsOutOfLogic = isOutOfLogic,
                        TimeAdded = _referenceTime
                    });
                });
        }

        public Dictionary<string, List<Transition>> GetTransitionsByMapArea() =>
            _helperLogTransitions.Values.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<Transition>> GetTransitionsByTitledArea() =>
            _helperLogTransitions.Values.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<Transition>> GetTransitionsByRoom(bool useSceneDescription) =>
            _helperLogTransitions.Values.GroupBy(x => useSceneDescription ? x.SceneDescription : x.SceneName)
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<Transition>>> GetTransitionsByRoomByMapArea(bool useSceneDescription) =>
            _helperLogTransitions.Values.GroupBy(x => x.MapArea).OrderBy(x => x.Key)
                .ToDictionary(y => y.Key, y => y.GroupBy(x => useSceneDescription ? x.SceneDescription : x.SceneName)
                .ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<Transition>>> GetTransitionsByRoomByTitledArea(bool useSceneDescription) =>
            _helperLogTransitions.Values.GroupBy(x => x.TitledArea).OrderBy(x => x.Key)
                .ToDictionary(y => y.Key, y => y.GroupBy(x => useSceneDescription ? x.SceneDescription : x.SceneName)
                .ToDictionary(x => x.Key, x => x.ToList()));

        public List<Transition> GetTransitions() =>
            _helperLogTransitions.Values.ToList();

        #endregion

        private static List<string> LoadSection(List<string> helperLogData, string startLine)
        {
            var start = helperLogData.IndexOf(startLine);
            if (start < 0)
            {
                return null;
            }
            var end = helperLogData.IndexOf("", start);
            if (end < 0)
            {
                return null;
            }

            return new List<string>(helperLogData.Where((_, i) => i > start && i < end));
        }

        public void SaveState()
        {
            _resourceLoader.SaveHelperLogLocations(_helperLogLocations);
            _resourceLoader.SaveHelperLogTransitions(_helperLogTransitions);
        }

        public void PurgeMemory()
        {
            _helperLogLocations.Clear();
            _helperLogTransitions.Clear();
        }
    }
}
