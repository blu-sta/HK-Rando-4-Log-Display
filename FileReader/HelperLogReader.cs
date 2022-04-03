using HK_Rando_4_Log_Display.DTO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IHelperLogReader : ILogReader
    {
        public void PreloadData();

        public void SaveState();

        public Dictionary<string, List<LocationWithTime>> GetLocationsByTitledArea();

        public Dictionary<string, List<LocationWithTime>> GetLocationsByMapArea();

        public Dictionary<string, List<LocationWithTime>> GetLocationsByRoom();

        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByTitledArea();

        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByMapArea();

        public List<LocationWithTime> GetLocations();

        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedLocations3();

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

        public bool IsFileFound { get; private set; }

        public HelperLogReader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public void PreloadData()
        {
            _helperLogLocations = _resourceLoader.GetHelperLogLocations();
            _helperLogTransitions = _resourceLoader.GetHelperLogTransitions();
        }

        public void LoadData()
        {
            if (!File.Exists(Constants.HelperLogPath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;
            var helperLogData = File.ReadAllLines(Constants.HelperLogPath).ToList();


            LoadReachableLocations(helperLogData);
            LoadPreviewedLocations2(helperLogData);
            LoadReachableTransitions(helperLogData);
        }

        #region Locations

        private Dictionary<string, LocationWithTime> _helperLogLocations = new Dictionary<string, LocationWithTime>();

        private void LoadReachableLocations(List<string> helperLogData)
        {
            var uncheckedReachableLocations = LoadSection(helperLogData, "UNCHECKED REACHABLE LOCATIONS")?.Select(x => x.Trim()).ToList();
            if (uncheckedReachableLocations == null)
            {
                return;
            }

            var now = DateTime.Now;
            _helperLogLocations.Keys.Except(uncheckedReachableLocations).ToList()
                .ForEach(x => _helperLogLocations.Remove(x));
            uncheckedReachableLocations.Except(_helperLogLocations.Keys).ToList()
                .ForEach(x => _helperLogLocations.Add(x, new LocationWithTime(_resourceLoader.Locations.FirstOrDefault(y => y.Name == (x.StartsWith("*") ? x.Replace("*", "") : x)) ?? new Location { Name = x.StartsWith("*") ? x.Replace("*", "") : x, MapArea = "undefined", TitledArea = "undefined", SceneName = "undefined" }, now, x.StartsWith("*"))));
        }

        public Dictionary<string, List<LocationWithTime>> GetLocationsByTitledArea() =>
            _helperLogLocations.Values.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<LocationWithTime>> GetLocationsByMapArea() =>
            _helperLogLocations.Values.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<LocationWithTime>> GetLocationsByRoom() =>
            _helperLogLocations.Values.GroupBy(x => x.SceneName).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByTitledArea() =>
            _helperLogLocations.Values.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<LocationWithTime>>> GetLocationsByRoomByMapArea() =>
            _helperLogLocations.Values.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public List<LocationWithTime> GetLocations() =>
            _helperLogLocations.Values.ToList();

        #endregion

        #region Previewed Locations

        List<PreviewedItemAtLocation> _helperLogPreviewedItemsAtLocations = new List<PreviewedItemAtLocation>();

        private void LoadPreviewedLocations2(List<string> helperLogData)
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

                        var itemWithCost = itemLine.Split("  -  ");
                        var itemNames = itemWithCost[0];
                        var itemCost = itemWithCost.Length > 1 ? itemWithCost[1] : "";

                        foreach (var itemName in itemNames.Split(", "))
                        {
                            var modifiedItemName = ModifyItemNameForPreview(itemName);
                            var itemPool = _resourceLoader.PreviewItems.FirstOrDefault(y => y.Name == modifiedItemName)?.Pool ?? GetPreviewItemPool(itemName);

                            _helperLogPreviewedItemsAtLocations.Add(new PreviewedItemAtLocation
                            {
                                LocationName = location,
                                LocationRoom = sceneName,
                                LocationPool = locationPool,
                                ItemName = itemName,
                                ItemPool = AddSpacesBeforeUppercaseChars(itemPool),
                                ItemCost = itemCost,
                                ItemCostValue = int.TryParse(Regex.Match(itemCost, "\\d+").Value, out var cost) ? cost : 0
                            });
                        }
                    }
                }
            }
        }

        private string ModifyItemNameForPreview(string itemName)
        {
            return Regex.Replace(itemName, @"((^A )|( \[\d\]))", "");
        }

        private string GetPreviewItemPool(string itemName)
        {
            return Regex.Replace(itemName, @"[\s\d]","");
        }

        Dictionary<string, Dictionary<string, List<string>>> _helperLogPreviewedLocations2 = new Dictionary<string, Dictionary<string, List<string>>>();

        private string GetPreviewedLocationPool(string pool, string location)
        {
            if (string.IsNullOrWhiteSpace(pool))
            {
                switch (location)
                {
                    case "Sly":
                    case "Sly_(Key)":
                    case "Iselda":
                    case "Salubra":
                    case "Leg_Eater":
                    case "Grubfather":
                    case "Seer":
                    case "Egg_Shop":
                        return "Shop";
                    default:
                        return "Other Previewed Items";
                }
            }

            return AddSpacesBeforeUppercaseChars(pool);
        }

        private static string AddSpacesBeforeUppercaseChars(string input)
        {
            if (input.Length <= 1)
            {
                return input;
            }

            var newText = new StringBuilder(input.Length * 2);
            newText.Append(input[0]);
            for (int i = 1; i < input.Length; i++)
            {
                if (char.IsUpper(input[i]) && input[i - 1] != ' ')
                    newText.Append(' ');
                newText.Append(input[i]);
            }
            return newText.ToString();
        }

        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedLocations3() => 
            _helperLogPreviewedItemsAtLocations.GroupBy(x => x.LocationPool).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<PreviewedItemAtLocation>> GetPreviewedItems() =>
            _helperLogPreviewedItemsAtLocations.GroupBy(x => x.ItemPool).ToDictionary(x => x.Key, x => x.ToList());

        #endregion

        #region Transitions

        private Dictionary<string, TransitionWithTime> _helperLogTransitions = new Dictionary<string, TransitionWithTime>();

        private void LoadReachableTransitions(List<string> helperLogData)
        {
            var uncheckedReachableTransitions = LoadSection(helperLogData, "UNCHECKED REACHABLE TRANSITIONS")?.Select(x => x.Trim()).ToList();
            if (uncheckedReachableTransitions == null)
            {
                return;
            }

            var now = DateTime.Now;
            _helperLogTransitions.Keys.Except(uncheckedReachableTransitions).ToList()
                .ForEach(x => _helperLogTransitions.Remove(x));
            uncheckedReachableTransitions.Except(_helperLogTransitions.Keys).ToList()
                .ForEach(x => _helperLogTransitions.Add(x, new TransitionWithTime(_resourceLoader.Transitions.FirstOrDefault(y => y.Name == (x.StartsWith("*") ? x.Replace("*", "") : x)) ?? GetNewTransitionFromName(x), now, x.StartsWith("*"))));
        }

        private Transition GetNewTransitionFromName(string transitionString)
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

        private List<string> LoadSection(List<string> helperLogData, string startLine)
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
