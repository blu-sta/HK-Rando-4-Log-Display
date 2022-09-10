using HK_Rando_4_Log_Display.DTO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ITrackerLogReader : ILogReader
    {
        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool();
        public List<ItemWithLocation> GetItems();
        public int? GetEssenceFromPools();
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea();
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea();
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom();
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea();
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea();
        public List<TransitionWithDestination> GetTransitions();
    }

    public class TrackerLogReader : ITrackerLogReader
    {
        private readonly IResourceLoader _resourceLoader;
        private readonly List<ItemWithLocation> _trackerLogItems = new();
        private readonly List<TransitionWithDestination> _trackerLogTransitions = new();

        public bool IsFileFound { get; private set; }

        public TrackerLogReader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
            LoadData();
        }

        public void LoadData()
        {
            if (!File.Exists(Constants.TrackerLogPath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;
            var trackerLogData = File.ReadAllLines(Constants.TrackerLogPath).ToList();

            LoadItems(trackerLogData);
            LoadTransitions(trackerLogData);
        }

        private void LoadItems(List<string> trackerLogData)
        {
            _trackerLogItems.Clear();
            var items = trackerLogData.Where(x => x.StartsWith("ITEM OBTAINED")).ToList();
            items.ForEach(x =>
            {
                var matches = Regex.Matches(x, "{(.*?)}").ToList();
                var item = matches[0].Groups[1].Value.Replace("100_Geo-", "");
                var location = matches[1].Groups[1].Value;
                var referenceItem = _resourceLoader.Items.FirstOrDefault(y => y.Name == item) 
                    ?? new Item { Name = item, Pool = GetPool(location, item) };

                var itemWithLocation = new ItemWithLocation(referenceItem, location);
                _trackerLogItems.Add(itemWithLocation);
            });
        }

        private static string GetPool(string location, string item) =>
            location == "Start"
                ? "Start"
                : item.Contains("-")
                    ? item.Split('-')[0]
                    : "undefined";

        private void LoadTransitions(List<string> trackerLogData)
        {
            _trackerLogTransitions.Clear();
            var items = trackerLogData.Where(x => x.StartsWith("TRANSITION")).ToList();
            items.ForEach(x =>
            {
                var matches = Regex.Match(x, "{(.*)\\[(.*)\\]}.*{(.*)\\[(.*)\\]}");
                var sceneName = matches.Groups[1].Value;
                var doorName = matches.Groups[2].Value;
                var destinationSceneName = matches.Groups[3].Value;
                var destinationDoorName = matches.Groups[4].Value;
                var referenceTransition = _resourceLoader.Transitions.FirstOrDefault(y => y.SceneName == sceneName && y.DoorName == doorName) 
                    ?? new Transition { SceneName = sceneName, DoorName = doorName, MapArea = "undefined", TitledArea = "undefined" };

                var transitionWithDestination = new TransitionWithDestination(referenceTransition, destinationSceneName, destinationDoorName);
                _trackerLogTransitions.Add(transitionWithDestination);
            });
        }

        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool() =>
            _trackerLogItems.GroupBy(x => x.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public List<ItemWithLocation> GetItems() => _trackerLogItems;

        public int? GetEssenceFromPools()
        {
            var essenceSources = _trackerLogItems.Where(x => x.Pool == "Root" || x.Pool == "DreamWarrior" || x.Pool == "DreamBoss").Select(x => x.Name).ToList();
            return essenceSources.Any() ? essenceSources.Sum(x => EssenceDictionary.TryGetValue(x, out var essence) ? essence : 0) : null;
        }

        #region EssenceDictionary
        private readonly Dictionary<string, int> EssenceDictionary = new Dictionary<string, int>
        {
            { "Whispering_Root-Crossroads", 29 },
            { "Whispering_Root-Greenpath", 44 },
            { "Whispering_Root-Leg_Eater", 20 },
            { "Whispering_Root-Mantis_Village", 18 },
            { "Whispering_Root-Deepnest", 45 },
            { "Whispering_Root-Queens_Gardens", 29 },
            { "Whispering_Root-Kingdoms_Edge", 51 },
            { "Whispering_Root-Waterways", 35 },
            { "Whispering_Root-City", 28 },
            { "Whispering_Root-Resting_Grounds", 20 },
            { "Whispering_Root-Spirits_Glade", 34 },
            { "Whispering_Root-Crystal_Peak", 21 },
            { "Whispering_Root-Howling_Cliffs", 46 },
            { "Whispering_Root-Ancestral_Mound", 42 },
            { "Whispering_Root-Hive", 20 },
            { "Boss_Essence-Elder_Hu", 100 },
            { "Boss_Essence-Xero", 100 },
            { "Boss_Essence-Gorb", 100 },
            { "Boss_Essence-Marmu", 150 },
            { "Boss_Essence-No_Eyes", 200 },
            { "Boss_Essence-Galien", 200 },
            { "Boss_Essence-Markoth", 250 },
            { "Boss_Essence-Failed_Champion", 300 },
            { "Boss_Essence-Soul_Tyrant", 300 },
            { "Boss_Essence-Lost_Kin", 400 },
            { "Boss_Essence-White_Defender", 300 },
            { "Boss_Essence-Grey_Prince_Zote", 300 },
        };
        #endregion

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea() =>
            _trackerLogTransitions.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea() =>
            _trackerLogTransitions.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom() =>
            _trackerLogTransitions.GroupBy(x => x.SceneName).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea() =>
            _trackerLogTransitions.GroupBy(x => x.TitledArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea() =>
            _trackerLogTransitions.GroupBy(x => x.MapArea).OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => x.SceneName).ToDictionary(x => x.Key, x => x.ToList()));

        public List<TransitionWithDestination> GetTransitions() =>
            _trackerLogTransitions;
    }
}
