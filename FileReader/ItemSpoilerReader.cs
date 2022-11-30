using HK_Rando_4_Log_Display.DTO;
using Newtonsoft.Json;
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
    public interface IItemSpoilerReader : ILogReader
    {
        public Dictionary<string, List<SpoilerItemWithLocation>> GetCuratedItemsByPool();
        public Dictionary<string, List<SpoilerItemWithLocation>> GetItemsByPool();
        public Dictionary<string, List<SpoilerItemWithLocation>> GetLocationsByPool();
        public List<SpoilerItemWithLocation> GetItems();
    }

    public class ItemSpoilerReader : IItemSpoilerReader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IResourceLoader _resourceLoader;
        private readonly List<SpoilerItemWithLocation> _spoilerItems = new();

        public bool IsFileFound { get; private set; }
        public bool IsFileLoaded { get; private set; }

        public ItemSpoilerReader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
            LoadData(Array.Empty<string>());
        }

        public void LoadData(string[] multiWorldPlayerNames)
        {
            IsFileFound = File.Exists(ItemSpoilerLogPath);
            if (!IsFileFound)
            {
                return;
            }
            var itemSpoilerData = File.ReadAllLines(ItemSpoilerLogPath).ToList();

            try
            {
                LoadItemSpoiler(itemSpoilerData, multiWorldPlayerNames);
                IsFileLoaded = true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "ItemSpoilerReader LoadData Error");
                IsFileLoaded = false;
            }
        }

        public void OpenFile()
        {
            if (File.Exists(ItemSpoilerLogPath)) Process.Start(new ProcessStartInfo(ItemSpoilerLogPath) { UseShellExecute = true });
        }

        private void LoadItemSpoiler(List<string> itemSpoilerData, string[] multiWorldPlayerNames)
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
                var mwPlayerItem = multiWorldPlayerNames.FirstOrDefault(mwPlayerName => x.Item.StartsWith($"{mwPlayerName}'s "));
                var itemName = string.IsNullOrEmpty(mwPlayerItem) ? x.Item : Regex.Replace(x.Item, $"(^{mwPlayerItem}'s |_\\(\\d+\\)$)", "");
                var mwPlayerLocation = multiWorldPlayerNames.FirstOrDefault(mwPlayerName => x.Location.StartsWith($"{mwPlayerName}'s "));
                var locationName = string.IsNullOrEmpty(mwPlayerLocation) ? x.Location : Regex.Replace(x.Location, $"^{mwPlayerLocation}'s ", "");
                var cost = x.Costs != null ? string.Join(", ", x.Costs.Select(FormatCost)) : null;

                var itemDetails = _resourceLoader.ReferenceItems.FirstOrDefault(y => y.Name == itemName)
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

                _spoilerItems.Add(
                       new SpoilerItemWithLocation
                       {
                           Item = new Item
                           {
                               MWPlayerName = mwPlayerItem,
                               Name = itemDetails.Name,
                               Pool = itemDetails.Pool,
                           },
                           Location = new Location
                           {
                               MWPlayerName = mwPlayerLocation,
                               Name = locationDetails.Name,
                               Pool = locationDetails.Pool,
                               MapArea = locationDetails.MapArea,
                               TitledArea = locationDetails.TitledArea,
                               SceneName = locationDetails.SceneName,
                               SceneDescription = locationDetails.SceneDescription,
                           },
                           Cost = cost
                       });
            });
        }

        private static string FormatCost(string x)
        {
            try
            {
                if (x.Contains("LogicGeoCost"))
                {
                    return $"{Regex.Match(x, "\\d+").Value} Geo";
                }

                if (x.Contains("DREAMNAIL"))
                {
                    return $"Requires Dreamnail";
                }

                if (x.Contains("Spore_Shroom"))
                {
                    return $"Requires Spore Shroom";
                }

                if (x.Contains("RANCIDEGGS"))
                {
                    return $"{Regex.Match(x, "\\d+").Value} Eggs";
                }

                if (x.Contains("GRUBS"))
                {
                    return $"{Regex.Match(x, "\\d+").Value} Grubs";
                }

                if (x.Contains("CHARMS"))
                {
                    return $"{Regex.Match(x, "\\d+").Value} Charms";
                }

                if (x.Contains("ESSENCE"))
                {
                    return $"{Regex.Match(x, "\\d+").Value} Essence";
                }

                if (x.Contains("SCREAM"))
                {
                    return $"Requires Wraiths/Shriek";
                }

                if (x.Contains("SIMPLE"))
                {
                    return $"Requires Simple Key";
                }

                if (x.Contains("LogicEnemyKillCost") ||
                    x.Contains("BLUGGSACS") ||
                    x.Contains("CRYSTALGUARDIANS") ||
                    x.Contains("ELDERBALDURS") ||
                    x.Contains("MIMICS") ||
                    x.Contains("GRUZMOTHERS") ||
                    x.Contains("HORNETS") ||
                    x.Contains("KINGSMOULDS") ||
                    x.Contains("VENGEFLYKINGS"))
                {
                    return $"Kill {Regex.Match(x, "\\d+").Value}";
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "ItemSpoilerReader FormatCost Regex failure");
            }

            return x;
        }

        public Dictionary<string, List<SpoilerItemWithLocation>> GetCuratedItemsByPool() =>
            GetCuratedItems();

        #region Curated Logic

        private Dictionary<string, List<SpoilerItemWithLocation>> GetCuratedItems()
        {
            var kvps = new[] {
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
            }.Where(x => x.Value.Count > 0);
            return kvps.ToDictionary(x => x.Key, x => x.Value);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetTrueEndingItems()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetMovementItems()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetSpells()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetDreamnails()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetNailArts()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetPaleOre()
        {
            var poolName = "Pale Ore";
            var paleOre = new[] {
                "Pale_Ore",
            };
            var trackedItems = new[] {
                GetItems(paleOre)
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetSignificantCharms()
        {
            var poolName = "Notable Charms";
            // TODO: Use this as default, but add this as editable
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetKeys()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetStags()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetGrimmFlames()
        {
            var poolName = "Grimmkin Flames";
            var flames = new[] {
                "Grimmkin_Flame",
            };
            var trackedItems = new[] {
                GetItems(flames),
            }.SelectMany(x => x).ToList();
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetGeoCaches()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private KeyValuePair<string, List<SpoilerItemWithLocation>> GetEssenceCaches()
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
            return new KeyValuePair<string, List<SpoilerItemWithLocation>>(poolName, trackedItems);
        }

        private List<SpoilerItemWithLocation> GetItems(string[] itemsInPool) =>
           _spoilerItems.Where(x => itemsInPool.Contains(x.Item.Name) && x.Item.MWPlayerName == null).ToList();

        #endregion

        public Dictionary<string, List<SpoilerItemWithLocation>> GetItemsByPool() =>
            _spoilerItems.GroupBy(x => x.Item.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public Dictionary<string, List<SpoilerItemWithLocation>> GetLocationsByPool() =>
            _spoilerItems.GroupBy(x => x.Location.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public List<SpoilerItemWithLocation> GetItems() => _spoilerItems.ToList();


        private class SpoilerItem
        {
            public string Item { get; set; }
            public string Location { get; set; }
            public string[] Costs { get; set; }
        }
    }
}
