using HK_Rando_4_Log_Display.DTO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ITrackerLogReader : ILogReader
    {
        public Dictionary<string, List<ItemWithLocation>> GetCuratedItems();

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
                    ?? new Item { Name = item, Pool = location == "Start" ? "Start" : "undefined" };

                var itemWithLocation = new ItemWithLocation(referenceItem, location);
                _trackerLogItems.Add(itemWithLocation);
            });
        }

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

        public Dictionary<string, List<ItemWithLocation>> GetCuratedItems()
        {
            var kvps = new[] {
                GetTrueEndingItems(),
                GetMovementItems(),
                GetSpells(),
                GetDreamnails(),
                GetNailArts(),
                GetPaleOre(),
                GetSignificantCharms(),
                GetBaldurKillers(),
                GetKeys(),
                GetStags(),
                GetGrimmFlames(),
                GetGeoCaches(),
                GetEssenceCaches()
            }.Where(x => x.Value.Count > 0);
            return kvps.ToDictionary(x => x.Key, x => x.Value);
        }

        #region Curated Pool Functions

        public KeyValuePair<string, List<ItemWithLocation>> GetTrueEndingItems()
        {
            var poolName = "True Ending Items";
            var dreamers = new[] {
                "Lurien",
                "Monomon",
                "Herrah"
            };
            var dupeDreamer = new[]
            {
                "Dreamer"
            };
            var fragments = new[] {
                "King_Fragment",
                "Queen_Fragment",
                "Kingsoul",
                "Void_Heart"
            };
            var trackedItems = new[] {
                GetItems(dreamers),
                GetItems(dupeDreamer),
                GetItems(fragments)
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetMovementItems()
        {
            var poolName = "Movement abilities";
            var dashes = new[] {
                "Mothwing_Cloak",
                "Left_Mothwing_Cloak",
                "Right_Mothwing_Cloak",
                "Shade_Cloak",
                "Split_Shade_Cloak"
            };
            var claws = new[]
            {
                "Mantis_Claw",
                "Left_Mantis_Claw",
                "Right_Mantis_Claw"
            };
            var wings = new[]
            {
                "Monarch_Wings"
            };
            var cdash = new[] {
                "Crystal_Heart",
                "Left_Crystal_Heart",
                "Right_Crystal_Heart"
            };
            var tear = new[]
            {
                "Isma's_Tear"
            };
            var swim = new[]
            {
                "Swim"
            };
            var trackedItems = new[] {
                GetItems(dashes),
                GetItems(claws),
                GetItems(wings),
                GetItems(cdash),
                GetItems(tear),
                GetItems(swim),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetSpells()
        {
            var poolName = "Spells";
            var fireballs = new[] {
                "Vengeful_Spirit",
                "Shade_Soul",
            };
            var quakes = new[]
            {
                "Desolate_Dive",
                "Descending_Dark",
            };
            var screams = new[]
            {
                "Howling_Wraiths",
                "Abyss_Shriek",
            };
            var trackedItems = new[] {
                GetItems(fireballs),
                GetItems(quakes),
                GetItems(screams),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetDreamnails()
        {
            var poolName = "Dream Nails";
            var dreamNails = new[] {
                "Dream_Nail",
                "Dream_Gate",
                "Awoken_Dream_Nail",
            };
            var trackedItems = new[] {
                GetItems(dreamNails)
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetNailArts()
        {
            var poolName = "Nail Arts";
            var nailArts = new[] {
                "Great_Slash",
                "Cyclone_Slash",
                "Dash_Slash",
            };
            var trackedItems = new[] {
                GetItems(nailArts)
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetPaleOre()
        {
            var poolName = "Pale Ore";
            var paleOre = new[] {
                "Pale_Ore",
            };
            var trackedItems = new[] {
                GetItems(paleOre)
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetSignificantCharms()
        {
            var poolName = "Notable Charms";
            var spellCharms = new[] {
                "Shaman_Stone",
                "Spell_Twister",
                "Soul_Catcher",
                "Soul_Eater",
            };
            var dreamWielder = new[]
            {
                "Dream_Wielder",
            };
            var speedCharms = new[]
            {
                "Dashmaster",
                "Sprintmaster",
                "Sharp_Shadow",
            };
            var nailCharms = new[]
            {
                "Quick_Slash",
                "Fragile_Strength",
                "Unbreakable_Strength",
                "Nailmaster's_Glory"
            };
            var healthCharms = new[]
            {
                "Fragile_Heart",
                "Unbreakable_Heart",
                "Lifeblood_Heart",
                "Lifeblood_Core",
            };
            var grimmChild = new[]
            {
                "Grimmchild1",
                "Grimmchild2"
            };
            var trackedItems = new[] {
                GetItems(spellCharms).ToList(),
                GetItems(dreamWielder),
                GetItems(speedCharms),
                GetItems(nailCharms),
                GetItems(healthCharms),
                GetItems(grimmChild),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetBaldurKillers()
        {
            var poolName = "Baldur Killers";
            var fireballs = new[] {
                "Vengeful_Spirit",
                "Shade_Soul",
            };
            var quakes = new[]
            {
                "Desolate_Dive",
                "Descending_Dark",
            };
            var nailArts = new[] {
                "Cyclone_Slash",
                "Dash_Slash",
            };
            var charms = new[]
            {
                "Glowing_Womb",
                "Weaversong",
                "Spore_Shroom",
                "Cyclone_Slash",
                "Grubberfly's_Elegy",
            };
            var trackedItems = new[] {
                GetItems(fireballs),
                GetItems(quakes),
                GetItems(nailArts),
                GetItems(charms),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetKeys()
        {
            var poolName = "Keys";
            var keys = new[] {
                "City_Crest",
                "Lumafly_Lantern",
                "Tram_Pass",
                "Simple_Key",
                "Shopkeeper's_Key",
                "Elegant_Key",
                "Love_Key",
                "King's_Brand",
                "Elevator_Pass",
            };
            var trackedItems = new[] {
                GetItems(keys),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetStags()
        {
            var poolName = "Stags";
            var stags = new[] {
                "Dirtmouth_Stag",
                "Crossroads_Stag",
                "Greenpath_Stag",
                "Queen's_Station_Stag",
                "Queen's_Gardens_Stag",
                "City_Storerooms_Stag",
                "King's_Station_Stag",
                "Resting_Grounds_Stag",
                "Distant_Village_Stag",
                "Hidden_Station_Stag",
                "Stag_Nest_Stag",
            };
            var trackedItems = new[] {
                GetItems(stags),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetGrimmFlames()
        {
            var poolName = "Grimmkin Flames";
            var flames = new[] {
                "Grimmkin_Flame",
            };
            var trackedItems = new[] {
                GetItems(flames),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetGeoCaches()
        {
            var poolName = "Geo Caches";
            var arcaneEggs = new[]
            {
                "Arcane_Egg",
            };
            var kingsIdols = new[]
            {
                "King's_Idol",
            };
            var geoChests = new[] {
                "Geo_Chest-False_Knight",
                "Geo_Chest-Soul_Master",
                "Geo_Chest-Watcher_Knights",
                "Geo_Chest-Greenpath",
                "Geo_Chest-Mantis_Lords",
                "Geo_Chest-Resting_Grounds",
                "Geo_Chest-Crystal_Peak",
                "Geo_Chest-Weavers_Den",
            };
            var bossGeo = new[] {
                "Boss_Geo-Massive_Moss_Charger",
                "Boss_Geo-Gorgeous_Husk",
                "Boss_Geo-Sanctum_Soul_Warrior",
                "Boss_Geo-Elegant_Soul_Warrior",
                "Boss_Geo-Crystal_Guardian",
                "Boss_Geo-Enraged_Guardian",
                "Boss_Geo-Gruz_Mother",
                "Boss_Geo-Vengefly_King",
            };
            var fourTwentyRock = new[]
            {
                "Geo_Rock-Outskirts420"
            };
            var trackedItems = new[] {
                GetItems(arcaneEggs),
                GetItems(kingsIdols),
                GetItems(geoChests),
                GetItems(bossGeo),
                GetItems(fourTwentyRock),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public KeyValuePair<string, List<ItemWithLocation>> GetEssenceCaches()
        {
            var poolName = "Essence Caches";
            var dreamWarriors = new[] {
                "Boss_Essence-Elder_Hu",
                "Boss_Essence-Xero",
                "Boss_Essence-Gorb",
                "Boss_Essence-Marmu",
                "Boss_Essence-No_Eyes",
                "Boss_Essence-Galien",
                "Boss_Essence-Markoth",
            };
            var dreamBosses = new[] {
                "Boss_Essence-Failed_Champion",
                "Boss_Essence-Soul_Tyrant",
                "Boss_Essence-Lost_Kin",
                "Boss_Essence-White_Defender",
                "Boss_Essence-Grey_Prince_Zote",
            };
            var trackedItems = new[] {
                GetItems(dreamWarriors),
                GetItems(dreamBosses),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<ItemWithLocation>>(poolName, trackedItems);
        }

        public List<ItemWithLocation> GetItems(string[] itemsInPool) =>
            _trackerLogItems.Where(x => itemsInPool.Contains(x.Name)).ToList();

        #endregion

        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool() =>
            _trackerLogItems.GroupBy(x => x.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public List<ItemWithLocation> GetItems() =>
            _trackerLogItems;

        public int? GetEssenceFromPools()
        {
            var essenceSources = _trackerLogItems.Where(x => x.Pool == "Root" || x.Pool == "DreamWarrior" || x.Pool == "DreamBoss").Select(x => x.Name).ToList();
            if (!essenceSources.Any())
            {
                return null;
            }

            return essenceSources.Sum(x => EssenceDictionary.TryGetValue(x, out var essence) ? essence : 0);
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
