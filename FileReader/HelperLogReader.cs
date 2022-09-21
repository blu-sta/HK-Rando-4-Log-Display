using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using HK_Rando_4_Log_Display.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IHelperLogReader : ILogReader
    {
        public void SaveState();
        public Dictionary<string, List<LocationWithTime>> GetLocationsByTitledArea();
        public Dictionary<string, List<LocationWithTime>> GetLocationsByMapArea();
        public Dictionary<string, List<LocationWithTime>> GetLocationsByRoom();
        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByTitledArea();
        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByMapArea();
        public List<LocationWithTime> GetLocations();
        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedLocations();
        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedItems();
        public Dictionary<string, List<TransitionWithTime>> GetTransitionsByTitledArea();
        public Dictionary<string, List<TransitionWithTime>> GetTransitionsByMapArea();
        public Dictionary<string, List<TransitionWithTime>> GetTransitionsByRoom();
        public Dictionary<string, Dictionary<string, List<TransitionWithTime>>> GetTransitionsByRoomByMapArea();
        public Dictionary<string, Dictionary<string, List<TransitionWithTime>>> GetTransitionsByRoomByTitledArea();
        public List<TransitionWithTime> GetTransitions();
    }

    public class HelperLogReader : IHelperLogReader
    {
        private readonly IResourceLoader _resourceLoader;
        private readonly Dictionary<string, LocationWithTime> _helperLogLocations = new();
        private readonly Dictionary<string, TransitionWithTime> _helperLogTransitions = new();
        private readonly List<PreviewedItemAtLocation> _helperLogPreviewedItemsAtLocations = new();
        private DateTime _referenceTime;

        public bool IsFileFound { get; private set; }

        public HelperLogReader(IResourceLoader resourceLoader, ISettingsReader settingsReader)
        {
            _resourceLoader = resourceLoader;

            if (!settingsReader.IsFileFound ||
                (settingsReader.IsFileFound && settingsReader.GetSeed() == resourceLoader.GetSeed()))
            {
                _helperLogLocations = _resourceLoader.GetHelperLogLocations();
                _helperLogTransitions = _resourceLoader.GetHelperLogTransitions();
            }

            LoadData();
        }

        public void LoadData()
        {
            if (!File.Exists(Constants.HelperLogPath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;

            _referenceTime = DateTime.Now;
            var helperLogData = File.ReadAllLines(Constants.HelperLogPath).ToList();
            LoadReachableLocations(helperLogData);
            LoadPreviewedLocations(helperLogData);
            LoadReachableTransitions(helperLogData);
        }

        #region Locations

        private void LoadReachableLocations(List<string> helperLogData)
        {
            var uncheckedReachableLocations = LoadSection(helperLogData, "UNCHECKED REACHABLE LOCATIONS")?.Select(x => x.Trim()).ToList();
            if (uncheckedReachableLocations == null)
            {
                return;
            }

            _helperLogLocations.Keys.Except(uncheckedReachableLocations).ToList()
                .ForEach(x => _helperLogLocations.Remove(x));
            uncheckedReachableLocations.Except(_helperLogLocations.Keys).ToList()
                .ForEach(x => _helperLogLocations.Add(
                    x,
                    new LocationWithTime(
                        _resourceLoader.Locations.FirstOrDefault(y => y.Name == (x.StartsWith("*") ? x.Replace("*", "") : x))
                            ?? new Location {
                                Name = x.StartsWith("*") ? x.Replace("*", "") : x,
                                MapArea = GetUnrecognisedLocationDefault(x),
                                TitledArea = GetUnrecognisedLocationDefault(x),
                                SceneName = GetUnrecognisedLocationDefault(x)
                            },
                        _referenceTime,
                        x.StartsWith("*"))));
        }

        private string GetUnrecognisedLocationDefault(string x) =>
            !x.Contains("-") ? "undefined" : $"> {RemoveInitialStar(x.Split('-')[0])}";

        private static string RemoveInitialStar(string x) =>
            x.StartsWith("*") ? x.Replace("*", "") : x;

        public Dictionary<string, List<LocationWithTime>> GetLocationsByTitledArea() =>
            _helperLogLocations.Values.GroupBy(x => x.TitledArea)
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<LocationWithTime>> GetLocationsByMapArea() =>
            _helperLogLocations.Values.GroupBy(x => x.MapArea)
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<LocationWithTime>> GetLocationsByRoom() =>
            _helperLogLocations.Values.GroupBy(x => x.SceneName)
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByTitledArea() =>
            _helperLogLocations.Values.GroupBy(x => x.TitledArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByMapArea() =>
            _helperLogLocations.Values.GroupBy(x => x.MapArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public List<LocationWithTime> GetLocations() =>
            _helperLogLocations.Values.ToList();

        #endregion

        #region Previewed Locations

        private void LoadPreviewedLocations(List<string> helperLogData)
        {
            var previewedLocations = LoadSection(helperLogData, "PREVIEWED LOCATIONS");
            if (previewedLocations == null)
            {
                return;
            }

            _helperLogPreviewedItemsAtLocations.Clear();
            for (var i = 0; i < previewedLocations.Count; i++)
            {
                var line = previewedLocations[i];
                if (!line.StartsWith("    "))
                {
                    var location = line.Trim().Replace("*", "");
                    var locationDetails = _resourceLoader.Locations.FirstOrDefault(y => y.Name == location);
                    var vanillaItemDetails = _resourceLoader.Items.FirstOrDefault(y => y.Name == location);
                    var locationPool = GetPreviewedLocationPool(vanillaItemDetails?.Pool, location);
                    var sceneName = locationDetails?.SceneName ?? "Unknown Scene";

                    while (i + 1 < previewedLocations.Count && previewedLocations[i + 1].StartsWith("    "))
                    {
                        var itemLine = previewedLocations[++i].Trim().Replace("*", "");

                        if (location == "Salubra")
                        {
                            itemLine = itemLine.Replace("Once you own", "Need").Replace(", I'll gladly sell it to you.", "");
                        }

                        if (locationPool == "Hunter's_Notes")
                        {
                            var killCountMatches = Regex.Matches(itemLine, "Defeat (\\d+) more");
                            var killsRequired = string.Join("/", killCountMatches.Select(x => x.Groups[1].Value).OrderBy(x =>
                            {
                                var startNumbers = Regex.Match(x, "^(\\d+)").Groups[1].Value;
                                return string.IsNullOrEmpty(startNumbers) ? x : startNumbers;
                            }, new SemiNumericComparer()));
                            var itemProvided = Regex.Match(itemLine, "to decipher the (.+)\\.").Groups[1].Value;
                            itemLine = $"{itemProvided}  -  Defeat {killsRequired}";
                        }

                        var itemWithCost = itemLine.Split("  -  ");
                        var itemNames = itemWithCost[0];
                        var itemCost = itemWithCost.Length > 1 ? itemWithCost[1] : "";

                        foreach (var itemName in itemNames.Split(",").Select(x => x.Trim()))
                        {
                            string itemPool = itemName.Contains("...") ? "Area Blitz ..." : ConvertSplitItemsToSkills(GetPreviewedItemPool(itemName));

                            _helperLogPreviewedItemsAtLocations.Add(new PreviewedItemAtLocation
                            {
                                LocationName = location.WithoutUnderscores(),
                                LocationRoom = sceneName,
                                LocationPool = locationPool,
                                ItemName = itemName,
                                ItemPool = itemPool.AddSpacesBeforeCapitals(true),
                                ItemCost = itemCost,
                                ItemCostValue = locationPool != "Hunter's_Notes" ? (int.TryParse(Regex.Match(itemCost, "\\d+").Value, out var cost) ? cost : 0) : 0,
                                SecondaryCostValue = int.TryParse(Regex.Match(itemCost, "(\\d+)(\\D+)(\\d+)").Groups[3].Value, out var secondaryCost) ? secondaryCost : 0
                            });
                        }
                    }
                }
            }
        }

        private string GetPreviewedItemPool(string itemName)
        {
            var modifiedItemName = ModifyItemNameForPreview(itemName);
            var matchingItem = _resourceLoader.PreviewItems.FirstOrDefault(y => y.Name == modifiedItemName);
            if (matchingItem != null)
                return matchingItem.Pool;

            var nearlyMatchingItems = _resourceLoader.PreviewItems.Where(y => y.Name.Contains(modifiedItemName));
            if (nearlyMatchingItems.Count() == 1)
                return nearlyMatchingItems.First().Pool;

            if (itemName.Contains("Journal Entry"))
                return "Journal Entry";

            if (itemName.Contains("Hunter's Notes"))
                return "Hunter's Notes";

            return GetPreviewItemPool(itemName);
        }

        private static string ConvertSplitItemsToSkills(string pool) =>
            pool.Contains("Split") ? "Skill" : pool;

        private static string ModifyItemNameForPreview(string itemName) =>
            Regex.Replace(itemName, @"((^A )|( \[\d\]))", "");

        private static string GetPreviewItemPool(string itemName) =>
            Regex.Replace(itemName, @"[\s\d\[\]]", "");

        private readonly string[] ShopLocations = new[]
        {
            "Sly",
            "Sly_(Key)",
            "Iselda",
            "Salubra",
            "Leg_Eater",
            "Grubfather",
            "Seer",
            "Egg_Shop"
        };

        private readonly string[] NailsmithLocations = new[]
        {
            "Nailsmith_Upgrade_1",
            "Nailsmith_Upgrade_2",
            "Nailsmith_Upgrade_3",
            "Nailsmith_Upgrade_4",
        };

        private string GetPreviewedLocationPool(string pool, string location)
        {
            if (!string.IsNullOrWhiteSpace(pool))
            {
                return pool.Contains("Split") 
                    ? "Skill"
                    : pool.AddSpacesBeforeCapitals();
            }

            if (ShopLocations.Any(x => x == location))
                return "Shop";
            
            if (NailsmithLocations.Any(x => x == location))
                return "Nailsmith";

            if ("Vessel_Fragment-Basin" == location)
                return "Basin Fountain";

            if (location.Contains("-"))
                return location.Split('-')[0];

            return "Other Previewed Items";
        }

        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedLocations() =>
            _helperLogPreviewedItemsAtLocations.GroupBy(x => x.LocationPool).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedItems() =>
            _helperLogPreviewedItemsAtLocations.GroupBy(x => x.ItemPool).ToDictionary(x => x.Key, x => x.ToList());

        #endregion

        #region Transitions

        private void LoadReachableTransitions(List<string> helperLogData)
        {
            var uncheckedReachableTransitions = LoadSection(helperLogData, "UNCHECKED REACHABLE TRANSITIONS")?.Select(x => x.Trim()).ToList();
            if (uncheckedReachableTransitions == null)
            {
                return;
            }

            _helperLogTransitions.Keys.Except(uncheckedReachableTransitions).ToList()
                .ForEach(x => _helperLogTransitions.Remove(x));
            uncheckedReachableTransitions.Except(_helperLogTransitions.Keys).ToList()
                .ForEach(x => _helperLogTransitions.Add(
                    x,
                    new TransitionWithTime(
                        _resourceLoader.Transitions.FirstOrDefault(y => y.Name == (x.StartsWith("*") ? x.Replace("*", "") : x))
                            ?? GetNewTransitionFromName(x),
                        _referenceTime,
                        x.StartsWith("*"))));
        }

        private static Transition GetNewTransitionFromName(string transitionString)
        {
            var matches = Regex.Match(transitionString, "(.*)\\[(.*)\\]");
            var sceneName = matches.Groups[1].Value;
            var doorName = matches.Groups[2].Value;

            return new Transition { SceneName = sceneName, DoorName = doorName, MapArea = "undefined", TitledArea = "undefined" };
        }

        public Dictionary<string, List<TransitionWithTime>> GetTransitionsByMapArea() =>
            _helperLogTransitions.Values.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithTime>> GetTransitionsByTitledArea() =>
            _helperLogTransitions.Values.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithTime>> GetTransitionsByRoom() =>
            _helperLogTransitions.Values.GroupBy(x => x.SceneName).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<TransitionWithTime>>> GetTransitionsByRoomByMapArea() =>
            _helperLogTransitions.Values.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<TransitionWithTime>>> GetTransitionsByRoomByTitledArea() =>
            _helperLogTransitions.Values.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public List<TransitionWithTime> GetTransitions() =>
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
    }
}
