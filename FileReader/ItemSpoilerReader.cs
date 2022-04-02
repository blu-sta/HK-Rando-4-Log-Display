using HK_Rando_4_Log_Display.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IItemSpoilerReader : ILogReader
    {
        public List<ItemWithLocation> GetItems();
        public Dictionary<string, List<ItemWithLocation>> GetCuratedItems();
        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool();
    }

    public class ItemSpoilerReader : IItemSpoilerReader
    {
        private readonly IResourceLoader _resourceLoader;

        public bool IsFileFound { get; private set; }

        public ItemSpoilerReader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
        }

        public void LoadData()
        {
            var filepath = Constants.ItemSpoilerLogPath;
            if (!File.Exists(filepath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;
            var itemSpoilerData = File.ReadAllLines(filepath).ToList();

            LoadItemSpoiler(itemSpoilerData);
        }

        private List<ItemWithLocation> _spoilerItems = new List<ItemWithLocation>();

        private void LoadItemSpoiler(List<string> itemSpoilerData)
        {
            _spoilerItems.Clear();
            var start = itemSpoilerData.IndexOf("[");
            if (start < 0)
            {
                return;
            }
            var end = itemSpoilerData.IndexOf("]", start);
            if (end < 0)
            {
                return;
            }

            var itemSpoilerString = string.Join("", itemSpoilerData.Where((_, i) => i >= start && i <= end));

            var spoilerItems = JsonConvert.DeserializeObject<List<SpoilerItem>>(itemSpoilerString);
            spoilerItems.ForEach(x =>
            {
                var item = x.Item;
                var location = x.Location;
                var cost = x.Costs != null ? string.Join(",", x.Costs) : null;
                var referenceItem = _resourceLoader.Items.FirstOrDefault(y => y.Name == item) ?? new Item { Name = item, Pool = location == "Start" ? "Start" : "undefined" };

                var itemWithLocation = new ItemWithLocation(referenceItem, x.Costs == null ? location : $"{location} [{cost}]");
                _spoilerItems.Add(itemWithLocation);
            });
        }

        private class SpoilerItem
        {
            public string Item { get; set; }
            public string Location { get; set; }
            public string[] Costs { get; set; }
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
            _spoilerItems.Where(x => itemsInPool.Contains(x.Name)).ToList();

        #endregion

        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool() =>
            _spoilerItems.GroupBy(x => x.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public List<ItemWithLocation> GetItems() 
            => _spoilerItems;
    }
}
