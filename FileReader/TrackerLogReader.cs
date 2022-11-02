using HK_Rando_4_Log_Display.DTO;
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
    public interface ITrackerLogReader : ILogReader
    {
        public Dictionary<string, List<ItemWithLocation>> GetCuratedItemsByPool();
        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool();
        public Dictionary<string, List<ItemWithLocation>> GetLocationsByPool();
        public List<ItemWithLocation> GetItems();
        public int? GetEssenceFromPools();
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea(bool useDestination);
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea(bool useDestination);
        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom(bool useDestination, bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea(bool useDestination, bool useSceneDescription);
        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea(bool useDestination, bool useSceneDescription);
        public List<TransitionWithDestination> GetTransitions();
        public void SaveState();
        public void PurgeMemory();
    }

    public class TrackerLogReader : ITrackerLogReader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IResourceLoader _resourceLoader;
        private DateTime _referenceTime;
        private readonly Dictionary<string, ItemWithLocation> _trackerLogItems = new();
        private readonly Dictionary<string, TransitionWithDestination> _trackerLogTransitions = new();

        public bool IsFileFound { get; private set; }
        public bool IsFileLoaded { get; private set; }

        public TrackerLogReader(IResourceLoader resourceLoader, ISeedSettingsReader settingsReader)
        {
            _resourceLoader = resourceLoader;

            if (!settingsReader.IsFileFound ||
                (settingsReader.IsFileFound && settingsReader.GetGenerationCode() == resourceLoader.GetSeedGenerationCode()))
            {
                _trackerLogItems = _resourceLoader.GetTrackerLogItems();
                _trackerLogTransitions = _resourceLoader.GetTrackerLogTransitions();
            }

            LoadData();
        }

        public void LoadData()
        {
            IsFileFound = File.Exists(TrackerLogPath);
            if (!IsFileFound)
            {
                return;
            }

            _referenceTime = DateTime.Now;
            var trackerLogData = File.ReadAllLines(TrackerLogPath).ToList();

            try
            {
                LoadItems(trackerLogData);
                LoadTransitions(trackerLogData);
                IsFileLoaded = true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "TrackerLogReader LoadData Error");
                IsFileLoaded = false;
            }
        }

        public void OpenFile()
        {
            if (File.Exists(TrackerLogPath)) Process.Start(new ProcessStartInfo(TrackerLogPath) { UseShellExecute = true });
        }

        private class TrackedItem
        {
            public string ItemName;
            public string LocationName;
        }

        private void LoadItems(List<string> trackerLogData)
        {
            var items = trackerLogData
                .Where(x => x.StartsWith("ITEM OBTAINED"))
                .Select(x =>
                {
                    var matches = Regex.Match(x, "{(.+)}.*{(.+)}.*{(.+)}");
                    return new KeyValuePair<string, TrackedItem>(
                            matches.Groups[3].Value,
                            new TrackedItem
                            {
                                ItemName = matches.Groups[1].Value,
                                LocationName = matches.Groups[2].Value,
                            }
                        );
                })
                .ToDictionary(x => x.Key, x => x.Value);

            // TODO: Update existing items without wiping times when MultiWorld names are updated

            _trackerLogItems.Keys.Except(items.Keys).ToList()
                .ForEach(x => _trackerLogItems.Remove(x));
            items.Keys.Except(_trackerLogItems.Keys).ToList()
                .ForEach(id =>
                {
                    var trackedItem = items[id];
                    var itemName = trackedItem.ItemName.Replace("100_Geo-", "");
                    var locationName = trackedItem.LocationName;

                    var itemDetails = _resourceLoader.ReferenceItems.FirstOrDefault(y => y.Name == itemName)
                        // TODO: Identify MultiWorld items correctly before showing as Unrecognised
                        ?? new ReferenceItem
                        {
                            Name = itemName,
                            Pool = locationName == "Start"
                                ? "Start"
                                : itemName.Contains("-")
                                ? $"> {itemName.Split('-')[0]}"
                                : "> Unrecognised Items",
                        };
                    var locationDetails = _resourceLoader.ReferenceLocations.FirstOrDefault(y => y.Name == locationName)
                        ?? new ReferenceLocation
                        {
                            Name = locationName,
                            Pool = locationName.Contains("-") ? $"> {locationName.Split('-')[0]}" : "> Unrecognised Location",
                            MapArea = "> Unrecognised Location",
                            TitledArea = "> Unrecognised Location",
                            SceneName = "> Unrecognised Location",
                            SceneDescription = "> Unrecognised Location"
                        };

                    _trackerLogItems.Add(
                        id,
                        new ItemWithLocation
                        {
                            Item = new Item
                            {
                                Name = itemDetails.Name,
                                Pool = itemDetails.Pool,
                            },
                            Location = new Location
                            {
                                Name = locationDetails.Name,
                                Pool = locationDetails.Pool,
                                MapArea = locationDetails.MapArea,
                                TitledArea = locationDetails.TitledArea,
                                SceneName = locationDetails.SceneName,
                                SceneDescription = locationDetails.SceneDescription,
                                TimeAdded = _referenceTime,
                            }
                        });
                });
        }

        public Dictionary<string, List<ItemWithLocation>> GetCuratedItemsByPool() =>
            new[] {
                GetTrueEndingItems(),
                GetMovementItems(),
                GetSpells(),
                GetDreamnails(),
                GetNailArts(),
                GetPaleOre(),
                GetSignificantCharms(),
                GetKeys(),
                GetStags(),
                GetGrimmFlames(),
                GetGeoCaches(),
                GetEssenceCaches()
            }.Where(x => x.Value.Count > 0).ToDictionary(x => x.Key, x => x.Value);

        #region Curated Logic

        private KeyValuePair<string, List<ItemWithLocation>> GetTrueEndingItems()
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
                "White_Fragment",
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

        private KeyValuePair<string, List<ItemWithLocation>> GetMovementItems()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetSpells()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetDreamnails()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetNailArts()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetPaleOre()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetSignificantCharms()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetKeys()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetStags()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetGrimmFlames()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetGeoCaches()
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

        private KeyValuePair<string, List<ItemWithLocation>> GetEssenceCaches()
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

        private List<ItemWithLocation> GetItems(string[] itemsInPool) =>
           _trackerLogItems.Values.Where(x => itemsInPool.Contains(x.Item.Name)).OrderBy(x => x.TimeAdded).ToList();

        #endregion

        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool() =>
            _trackerLogItems.Values.GroupBy(x => x.Item.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<ItemWithLocation>> GetLocationsByPool() =>
            _trackerLogItems.Values.GroupBy(x => x.Location.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public List<ItemWithLocation> GetItems() => _trackerLogItems.Values.ToList();

        public int? GetEssenceFromPools()
        {
            var essenceSources = _trackerLogItems.Values.Where(x => x.Item.Pool == "Root" || x.Item.Pool == "DreamWarrior" || x.Item.Pool == "DreamBoss").Select(x => x.Item.Name).ToList();
            return essenceSources.Any() ? essenceSources.Sum(x => EssenceDictionary.TryGetValue(x, out var essence) ? essence : 0) : null;
        }

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

        private void LoadTransitions(List<string> trackerLogData)
        {
            var transitions = trackerLogData
                .Where(x => x.StartsWith("TRANSITION"))
                .Select(x =>
                {
                    var matches = Regex.Match(x, "{(\\S+)}.*{(\\S+)}");
                    return new KeyValuePair<string, string>(matches.Groups[1].Value, matches.Groups[2].Value);
                })
                .ToDictionary(x => x.Key, x => x.Value);

            _trackerLogTransitions.Keys.Except(transitions.Keys).ToList()
                .ForEach(x => _trackerLogTransitions.Remove(x));
            transitions.Keys.Except(_trackerLogTransitions.Keys).ToList()
                .ForEach(sourceName =>
                {
                    var destinationName = transitions[sourceName];

                    var sourceDetails = _resourceLoader.ReferenceTransitions.FirstOrDefault(y => y.Name == sourceName)
                        ?? new ReferenceTransition
                        {
                            SceneDescription = Regex.Match(sourceName, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                            SceneName = Regex.Match(sourceName, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                            DoorName = Regex.Match(sourceName, "(\\S+)\\[(\\S+)\\]").Groups[2].Value,
                            MapArea = "> Unrecognised Transitions",
                            TitledArea = "> Unrecognised Transitions",
                        };
                    var destinationDetails = _resourceLoader.ReferenceTransitions.FirstOrDefault(y => y.Name == destinationName)
                        ?? new ReferenceTransition
                        {
                            SceneDescription = Regex.Match(destinationName, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                            SceneName = Regex.Match(destinationName, "(\\S+)\\[(\\S+)\\]").Groups[1].Value,
                            DoorName = Regex.Match(destinationName, "(\\S+)\\[(\\S+)\\]").Groups[2].Value,
                            MapArea = "> Unrecognised Transitions",
                            TitledArea = "> Unrecognised Transitions",
                        };

                    _trackerLogTransitions.Add(
                    sourceName,
                    new TransitionWithDestination
                    {
                        Source = new Transition
                        {
                            SceneName = sourceDetails.SceneName,
                            SceneDescription = sourceDetails.SceneDescription,
                            DoorName = sourceDetails.DoorName,
                            MapArea = sourceDetails.MapArea,
                            TitledArea = sourceDetails.TitledArea,
                            TimeAdded = _referenceTime
                        },
                        Destination = new Transition
                        {
                            SceneName = destinationDetails.SceneName,
                            SceneDescription = destinationDetails.SceneDescription,
                            DoorName = destinationDetails.DoorName,
                            MapArea = destinationDetails.MapArea,
                            TitledArea = destinationDetails.TitledArea,
                            TimeAdded = _referenceTime
                        },
                    });
                });
        }

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByTitledArea(bool useDestination) =>
            _trackerLogTransitions.Values.GroupBy(x => useDestination ? x.Destination.TitledArea : x.Source.TitledArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByMapArea(bool useDestination) =>
            _trackerLogTransitions.Values.GroupBy(x => useDestination ? x.Destination.MapArea : x.Source.MapArea).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<TransitionWithDestination>> GetTransitionsByRoom(bool useDestination, bool useSceneDescription) =>
            _trackerLogTransitions.Values.GroupBy(x => GetTransitionKey(useDestination, useSceneDescription, x))
                .OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByTitledArea(bool useDestination, bool useSceneDescription) =>
            _trackerLogTransitions.Values.GroupBy(x => useDestination ? x.Destination.TitledArea : x.Source.TitledArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => GetTransitionKey(useDestination, useSceneDescription, x))
                .ToDictionary(x => x.Key, x => x.ToList()));

        public Dictionary<string, Dictionary<string, List<TransitionWithDestination>>> GetTransitionsByRoomByMapArea(bool useDestination, bool useSceneDescription) =>
            _trackerLogTransitions.Values.GroupBy(x => useDestination ? x.Destination.MapArea : x.Source.MapArea)
                .OrderBy(x => x.Key).ToDictionary(y => y.Key, y => y.GroupBy(x => GetTransitionKey(useDestination, useSceneDescription, x))
                .ToDictionary(x => x.Key, x => x.ToList()));

        private static string GetTransitionKey(bool useDestination, bool useSceneDescription, TransitionWithDestination x)
        {
            var transition = useDestination ? x.Destination : x.Source;
            return useSceneDescription ? transition.SceneDescription : transition.SceneName;
        }

        public List<TransitionWithDestination> GetTransitions() =>
            _trackerLogTransitions.Values.ToList();

        public void SaveState()
        {
            _resourceLoader.SaveTrackerLogItems(_trackerLogItems);
            _resourceLoader.SaveTrackerLogTransitions(_trackerLogTransitions);
        }

        public void PurgeMemory()
        {
            _trackerLogItems.Clear();
            _trackerLogTransitions.Clear();
        }
    }
}
