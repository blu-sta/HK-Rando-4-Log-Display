using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IResourceLoader
    {
        public List<ReferenceItem> ReferenceItems { get; }
        public List<ReferenceLocation> ReferenceLocations { get; }
        public List<ReferenceTransition> ReferenceTransitions { get; }
        public Dictionary<string, Location> GetHelperLogLocations();
        public Dictionary<string, Transition> GetHelperLogTransitions();
        public Dictionary<string, ItemWithLocation> GetTrackerLogItems();
        public Dictionary<string, TransitionWithDestination> GetTrackerLogTransitions();

        public AppSettings GetAppSettings();
        public string GetSeed();
        public void SaveHelperLogLocations(Dictionary<string, Location> helperLogLocations);
        public void SaveHelperLogTransitions(Dictionary<string, Transition> helperLogTransitions);
        public void SaveTrackerLogItems(Dictionary<string, ItemWithLocation> trackerLogItems);
        public void SaveTrackerLogTransitions(Dictionary<string, TransitionWithDestination> trackerLogTransitions);
        public void SaveAppSettings(AppSettings appSettings);
        public void SaveSeed(string seed);
    }

    public class ResourceLoader : IResourceLoader
    {
        public List<ReferenceItem> ReferenceItems { get; private set; } = new();
        public List<ReferenceLocation> ReferenceLocations { get; private set; } = new();
        public List<ReferenceTransition> ReferenceTransitions { get; private set; } = new();

        private const string MrMushroomPool = "Mr Mushroom";
        private const string SkillUpgradePool = "Skill Upgrade";
        private const string LeverPool = "Lever";
        private const string TranscendencePool = "Charm - Transcendence";
        private const string EggPool = "Egg";
        private const string SkillPool = "Skill";
        private const string BenchPool = "Bench";
        private const string MoreDoorsPool = "Key - MoreDoors";
        private const string LostArtifactPool = "Lost Artifact";

        public ResourceLoader()
        {
            LoadReferenceItems();
            LoadReferenceLocations();
            LoadReferenceTransitions();
        }

        private void LoadReferenceItems()
        {
            var itemImports = GetItemImportsFromFile();
            ReferenceItems = itemImports.Select(x => new ReferenceItem
            {
                Name = x.Name,
                PreviewName = x.Name.Replace("-", " ").Replace("_", " "),
                Pool = x.Pool
            })
                //flibber-hk
                .Concat(MrMushroomItemImport())
                .Concat(NailUpgradeItemImport())
                .Concat(SkillUpgradeItemImport())
                .Concat(FungalCityGateKeyItemImport())
                .Concat(LeverItemImport())

                //dpinela
                .Concat(TranscendenceItemImport())
                .Concat(RainbowEggItemImport())

                //homothetyhk
                .Concat(BenchItemImport())

                //BadMagic100
                // TRJR
                // https://github.com/BadMagic100/TheRealJournalRando

                //dplochcoder
                .Concat(MoreDoorsItemImport())
                .Concat(DarknessItemImport())

                //Hoo-Knows
                .Concat(LostArtifactsItemImport())
                .ToList();
        }

        private static List<ItemImport> GetItemImportsFromFile() =>
            LoadDictionaryFile<ItemImport>(ReferenceItemsFilePath).Concat(new List<ItemImport>
            {
                new ItemImport { Name = "White_Fragment", Pool = "Charm" },
                new ItemImport { Name = "Kingsoul", Pool = "Charm" },
                new ItemImport { Name = "Grimmchild", Pool = "Charm" },
            }).ToList();

        private static List<ReferenceItem> MrMushroomItemImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new ReferenceItem { Name = "Mr_Mushroom_Level_Up", PreviewName = "Mr Mushroom Level Up" , Pool = MrMushroomPool },
            };

        private static List<ReferenceItem> NailUpgradeItemImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new ReferenceItem { Name = "Nail_Upgrade", PreviewName = "Nail Upgrade" , Pool = SkillPool },
            };

        private static List<ReferenceItem> SkillUpgradeItemImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.SkillUpgrades
                new ReferenceItem { Name = "DirectionalDash", PreviewName = "Directional Dash", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "DownwardFireball", PreviewName = "Downward Fireball", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "ExtraAirDash", PreviewName = "Extra Air Dash", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "GreatSlashShockwave", PreviewName = "Great Slash Shockwave", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "HorizontalDive", PreviewName = "Horizontal Dive", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "SpiralScream", PreviewName = "Spiral Scream", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "TripleJump", PreviewName = "Triple Jump", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "VerticalSuperdash", PreviewName = "Vertical Superdash", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "WallClimb", PreviewName = "Wall Climb", Pool = SkillUpgradePool },
                new ReferenceItem { Name = "WingsGlide", PreviewName = "Wings Glide", Pool = SkillUpgradePool },
            };

        private static List<ReferenceItem> FungalCityGateKeyItemImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.ReopenCityDoor
                new ReferenceItem { Name = "Fungal_City_Gate_Key", PreviewName = "Fungal City Gate Key" , Pool = "Key" },
            };

        private class LeverItem
        {
            public string Key;
            public string Text;
        }
        private static List<ReferenceItem> LeverItemImport() =>
            // https://github.com/flibber-hk/HollowKnight.RandomizableLevers
            LoadListFile<LeverItem>(ReferenceRandoLeverItemsFilePath)
                .Where(x => x.Key.StartsWith("LEVERNAME"))
                .Select(x => new ReferenceItem
                {
                    Name = x.Key.Replace("LEVERNAME.", ""),
                    PreviewName = x.Text,
                    Pool = LeverPool,
                })
                .ToList();

        private static List<ReferenceItem> TranscendenceItemImport() =>
            new()
            {
                // https://github.com/dpinela/Transcendence
                new ReferenceItem { Name = "Marissa's_Audience", PreviewName = "Marissa's Audience" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Lemm's_Strength", PreviewName = "Lemm's Strength" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Snail_Slash", PreviewName = "Snail Slash" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Millibelle's_Blessing", PreviewName = "Millibelle's Blessing" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Disinfectant_Flask", PreviewName = "Disinfectant Flask" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Florist's_Blessing", PreviewName = "Florist's Blessing" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Greedsong", PreviewName = "Greedsong" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Snail_Soul", PreviewName = "Snail Soul" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Nitro_Crystal", PreviewName = "Nitro Crystal" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Shaman_Amp", PreviewName = "Shaman Amp" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Crystalmaster", PreviewName = "Crystalmaster" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Bluemoth_Wings", PreviewName = "Bluemoth Wings" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Chaos_Orb", PreviewName = "Chaos Orb" , Pool = TranscendencePool },
                new ReferenceItem { Name = "Antigravity_Amulet", PreviewName = "Antigravity Amulet" , Pool = TranscendencePool },
            };

        private static List<ReferenceItem> RainbowEggItemImport() =>
            new()
            {
                // https://github.com/dpinela/RainbowEggs
                new ReferenceItem { Name = "Red_Egg", PreviewName = "Red Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Orange_Egg", PreviewName = "Orange Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Yellow_Egg", PreviewName = "Yellow Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Green_Egg", PreviewName = "Green Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Cyan_Egg", PreviewName = "Cyan Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Blue_Egg", PreviewName = "Blue Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Purple_Egg", PreviewName = "Purple Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Pink_Egg", PreviewName = "Pink Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Trans_Egg", PreviewName = "Trans Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Rainbow_Egg", PreviewName = "Rainbow Egg" , Pool = EggPool },
                new ReferenceItem { Name = "Arcane_Eg", PreviewName = "Arcane Eg" , Pool = EggPool },
            };

        private class BenchItem
        {
            public string Key;
            public string Value;
        }
        private static List<ReferenceItem> BenchItemImport() =>
            // https://github.com/homothetyhk/BenchRando
            LoadListFile<BenchItem>(ReferenceBenchRandoItemsFilePath)
                .Where(x => x.Key.StartsWith("BENCHNAME"))
                .Select(x => new ReferenceItem
                {
                    Name = x.Key.Replace("BENCHNAME.", ""),
                    PreviewName = x.Value,
                    Pool = BenchPool,
                })
                .ToList();

        private class MoreDoorItem
        {
            public MoreDoorKey Key;
        }
        private class MoreDoorKey
        {
            public string ItemName;
            public string UIItemName;
            public MoreDoorGate Location;
        }
        private class MoreDoorGate
        {
            public string Name;
            public string SceneName;
        }
        private static List<ReferenceItem> MoreDoorsItemImport() =>
            // https://github.com/dplochcoder/HollowKnight.MoreDoors
            LoadDictionaryFile<MoreDoorItem>(ReferenceMoreDoorsFilePath).Select(x =>
            new ReferenceItem
            {
                Name = x.Key.ItemName,
                PreviewName = x.Key.UIItemName,
                Pool = MoreDoorsPool,
            }).ToList();

        private static List<ReferenceItem> LostArtifactsItemImport() =>
            new()
            {
                // https://github.com/Hoo-Knows/HollowKnight.LostArtifacts
                new ReferenceItem { Name = "TravelersGarment", PreviewName = "Travelers Garment" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "PavingStone", PreviewName = "Paving Stone" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "LushMoss", PreviewName = "Lush Moss" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "NoxiousShroom", PreviewName = "Noxious Shroom" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "CryingStatue", PreviewName = "Crying Statue" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "TotemShard", PreviewName = "Totem Shard" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "DungBall", PreviewName = "Dung Ball" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "Tumbleweed", PreviewName = "Tumbleweed" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "ChargedCrystal", PreviewName = "Charged Crystal" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "Dreamwood", PreviewName = "Dreamwood" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "LumaflyEssence", PreviewName = "Lumafly Essence" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "ThornedLeaf", PreviewName = "Thorned Leaf" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "WeaverSilk", PreviewName = "Weaver Silk" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "WyrmAsh", PreviewName = "Wyrm Ash" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "BeastShell", PreviewName = "Beast Shell" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "Honeydrop", PreviewName = "Honeydrop" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "InfectedRock", PreviewName = "Infected Rock" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "Buzzsaw", PreviewName = "Buzzsaw" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "VoidEmblem", PreviewName = "Void Emblem" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "AttunedJewel", PreviewName = "Attuned Jewel" , Pool = LostArtifactPool },
                new ReferenceItem { Name = "HiddenMemento", PreviewName = "Hidden Memento" , Pool = LostArtifactPool },
            };

        private static List<ReferenceItem> DarknessItemImport() =>
            new()
            {
                // https://github.com/dplochcoder/HollowKnight.DarknessRandomizer
                new ReferenceItem { Name = "Lantern_Shard", PreviewName = "Lantern Shard (#0)" , Pool = "Key" },
                new ReferenceItem { Name = "Final_Lantern_Shard", PreviewName = "Final Lantern Shard" , Pool = "Key" },
            };

        private static string GetPreviewName(ItemImport item) =>
            item.Name switch
            {
                _ => item.Name.Replace("-", " ").Replace("_", " ")
            };

        private void LoadReferenceLocations()
        {
            var locationImports = GetLocationImportsFromFile();
            var roomImports = GetRoomImportsFromFile();
            var customImports = GetCustomImports();

            ReferenceLocations = locationImports.Join(
                roomImports,
                location => location.SceneName,
                room => room.SceneName,
                (location, room) => new ReferenceLocation
                {
                    Name = location.Name,
                    SceneName = location.SceneName,
                    MapArea = room.MapArea,
                    TitledArea = room.TitledArea,
                    Pool = GetReferenceLocationPool(location.Name)
                })
                .Concat(customImports.Join(
                    roomImports,
                    customLocation => customLocation.SceneName,
                    room => room.SceneName,
                    (customLocation, room) => new ReferenceLocation
                    {
                        Name = customLocation.Name,
                        SceneName = customLocation.SceneName,
                        MapArea = room.MapArea,
                        TitledArea = room.TitledArea,
                        Pool = customLocation.Pool
                    })
                ).ToList();
        }

        private static List<LocationImport> GetLocationImportsFromFile() =>
            LoadDictionaryFile<LocationImport>(ReferenceLocationsFilePath);

        private static List<RoomImport> GetRoomImportsFromFile() =>
            LoadDictionaryFile<RoomImport>(ReferenceRoomsFilePath).Concat(new List<RoomImport>
            {
                new RoomImport { SceneName = "Room_Tram_RG", MapArea = "Tram", TitledArea = "Tram" },
                new RoomImport { SceneName = "Room_Tram", MapArea = "Tram", TitledArea = "Tram" },
            }).ToList();

        private readonly string[] ShopLocations = new[]
        {
            "Sly",
            "Sly_(Key)",
            "Iselda",
            "Salubra",
            "Salubra_(Requires_Charms)",
            "Leg_Eater",
            "Grubfather",
            "Seer",
            "Egg_Shop"
        };

        private string GetReferenceLocationPool(string locationName)
        {
            var perfectMatch = ReferenceItems.FirstOrDefault(x => x.Name == locationName)?.Pool;
            if (!string.IsNullOrWhiteSpace(perfectMatch))
            {
                return perfectMatch;
            }

            var locationPrefix = locationName.Split('-')[0];
            if (locationPrefix != locationName)
            {
                var prefixMatch = ReferenceItems.FirstOrDefault(x => x.Name == locationPrefix)?.Pool;
                if (!string.IsNullOrWhiteSpace(prefixMatch))
                {
                    return prefixMatch;
                }

                if (locationPrefix == "Lifeblood_Cocoon")
                    return "Lifeblood_Cocoon";

                if (locationPrefix == "Geo_Rock")
                    return "Rock";

                if (locationPrefix == "Soul_Totem")
                    return "Soul";
            }

            if (ShopLocations.Any(x => x == locationName))
                return "Shop";

            if (locationName == "Split_Mothwing_Cloak" || locationName == "Split_Crystal_Heart")
                return "Skill";

            if (locationName == "Start")
                return "Start";

            return "Unregistered Pool";
        }

        private List<ReferenceLocation> GetCustomImports()
        {
            return new List<ReferenceLocation>()
                //flibber-hk
                .Concat(MrMushroomLocationImport())
                .Concat(NailUpgradeLocationImport())
                .Concat(LeverLocationImport())

                //homothetyhk
                .Concat(BenchLocationImport())

                //BadMagic100
                // TRJR
                // https://github.com/BadMagic100/TheRealJournalRando

                //dplochcoder
                .Concat(MoreDoorsLocationImport())

                //Hoo-Knows
                .Concat(LostArtifactsLocationImport())

                .ToList();
        }

        private List<ReferenceLocation> MrMushroomLocationImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new ReferenceLocation { Name = "Mr_Mushroom-Fungal_Wastes", SceneName = "Fungus2_18", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Kingdom's_Edge", SceneName = "Deepnest_East_01", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Deepnest", SceneName = "Deepnest_40", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Howling_Cliffs", SceneName = "Room_nailmaster", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Ancient_Basin", SceneName = "Abyss_21", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Fog_Canyon", SceneName = "Fungus3_44", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-King's_Pass", SceneName = "Tutorial_01", Pool = MrMushroomPool },
            };

        private static List<ReferenceLocation> NailUpgradeLocationImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new ReferenceLocation { Name = "Nailsmith_Upgrade_1", SceneName = "Room_nailsmith" , Pool = "Shop" },
                new ReferenceLocation { Name = "Nailsmith_Upgrade_2", SceneName = "Room_nailsmith" , Pool = "Shop" },
                new ReferenceLocation { Name = "Nailsmith_Upgrade_3", SceneName = "Room_nailsmith" , Pool = "Shop" },
                new ReferenceLocation { Name = "Nailsmith_Upgrade_4", SceneName = "Room_nailsmith" , Pool = "Shop" },
            };

        private class LeverLocation {
            public string Name;
            public string SceneName;
        }
        private static List<ReferenceLocation> LeverLocationImport() =>
            // https://github.com/flibber-hk/HollowKnight.RandomizableLevers
            LoadDictionaryFile<LeverLocation>(ReferenceRandoLeverLocationsFilePath).Select(x =>
            new ReferenceLocation
            {
                Name = x.Name,
                SceneName = x.SceneName,
                Pool = LeverPool,
            }).ToList();

        private class BenchLocation
        {
            public string Name;
            public string SceneName;
        }
        private static List<ReferenceLocation> BenchLocationImport() =>
            // https://github.com/homothetyhk/BenchRando
            LoadDictionaryFile<BenchLocation>(ReferenceBenchRandoLocationsFilePath)
                .Select(x => new ReferenceLocation
                {
                    Name = x.Name,
                    SceneName = x.SceneName,
                    Pool = BenchPool,
                })
                .ToList();

        private static List<ReferenceLocation> MoreDoorsLocationImport() =>
            // https://github.com/dplochcoder/HollowKnight.MoreDoors
            LoadDictionaryFile<MoreDoorItem>(ReferenceMoreDoorsFilePath).Select(x =>
            new ReferenceLocation
            {
                Name = x.Key.Location.Name,
                SceneName = x.Key.Location.SceneName,
                Pool = MoreDoorsPool,
            }).ToList();

        private static List<ReferenceLocation> LostArtifactsLocationImport() =>
            new()
            {
                // https://github.com/Hoo-Knows/HollowKnight.LostArtifacts
                new ReferenceLocation { Name = "AttunedJewel", SceneName = "GG_Workshop" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "BeastShell", SceneName = "Room_Colosseum_01" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Buzzsaw", SceneName = "White_Palace_08" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "ChargedCrystal", SceneName = "Mines_18" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "CryingStatue", SceneName = "Ruins1_27" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Dreamwood", SceneName = "RestingGrounds_05" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "DungBall", SceneName = "Waterways_15" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "HiddenMemento", SceneName = "White_Palace_06" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Honeydrop", SceneName = "Hive_01" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "InfectedRock", SceneName = "Abyss_19" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "LumaflyEssence", SceneName = "Fungus3_archive_02" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "LushMoss", SceneName = "Fungus1_29" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "NoxiousShroom", SceneName = "Fungus2_30" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "PavingStone", SceneName = "Crossroads_47" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "ThornedLeaf", SceneName = "Fungus3_10" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "TotemShard", SceneName = "Ruins1_32" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "TravelersGarment", SceneName = "Town" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Tumbleweed", SceneName = "Cliffs_01" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "VoidEmblem", SceneName = "Abyss_09" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "WeaverSilk", SceneName = "Deepnest_45_v02" , Pool = LostArtifactPool },
                new ReferenceLocation { Name = "WyrmAsh", SceneName = "Deepnest_East_12" , Pool = LostArtifactPool },
            };

        private void LoadReferenceTransitions()
        {
            var transitionImports = GetTransitionImportsFromFile();
            var roomImports = GetRoomImportsFromFile();

            ReferenceTransitions = transitionImports.Join(
                roomImports,
                transition => transition.SceneName,
                room => room.SceneName,
                (transition, room) => new ReferenceTransition
                {
                    DoorName = transition.DoorName,
                    SceneName = transition.SceneName,
                    MapArea = room.MapArea,
                    TitledArea = room.TitledArea
                }).ToList();
        }

        private static List<TransitionImport> GetTransitionImportsFromFile() =>
            LoadDictionaryFile<TransitionImport>(ReferenceTransitionsFilePath);

        private static List<T> LoadDictionaryFile<T>(string filePath) =>
            File.Exists(filePath)
                ? DeserializeFile<Dictionary<string, T>>(filePath).Values.ToList()
                : new List<T>();

        private static List<T> LoadListFile<T>(string filePath) =>
            File.Exists(filePath)
                ? DeserializeFile<T[]>(filePath).ToList()
                : new List<T>();

        public Dictionary<string, Location> GetHelperLogLocations() =>
            GetDictionaryDataFromFileOrDefault<Location>(HelperLogLocationsFilename);

        public Dictionary<string, Transition> GetHelperLogTransitions() =>
            GetDictionaryDataFromFileOrDefault<Transition>(HelperLogTransitionsFilename);

        public Dictionary<string, ItemWithLocation> GetTrackerLogItems() =>
            GetDictionaryDataFromFileOrDefault<ItemWithLocation>(TrackerLogItemsFilename);

        public Dictionary<string, TransitionWithDestination> GetTrackerLogTransitions() =>
            GetDictionaryDataFromFileOrDefault<TransitionWithDestination>(TrackerLogTransitionsFilename);

        public static Dictionary<string, T> GetDictionaryDataFromFileOrDefault<T>(string filename) =>
            File.Exists(filename)
                ? DeserializeFile<Dictionary<string, T>>(filename)
                : new Dictionary<string, T>();

        public AppSettings GetAppSettings()
        {
            var appSettings = new AppSettings();

            if (File.Exists(AppSettingsFilename))
            {
                var appSettingsData = JsonConvert.DeserializeObject<JObject>(string.Join("", File.ReadAllLines(AppSettingsFilename).ToList()));

                appSettings.SelectedHelperLocationGrouping = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedHelperLocationGrouping") ?? appSettings.SelectedHelperLocationGrouping, HelperLocationGroupingOptions.Length - 1);
                appSettings.SelectedHelperLocationOrder = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedHelperLocationOrder") ?? appSettings.SelectedHelperLocationOrder, HelperLocationOrderingOptions.Length - 1);
                appSettings.SelectedHelperTransitionGrouping = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedHelperTransitionGrouping") ?? appSettings.SelectedHelperTransitionGrouping, HelperTransitionGroupingOptions.Length - 1);
                appSettings.SelectedHelperTransitionOrder = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedHelperTransitionOrder") ?? appSettings.SelectedHelperTransitionOrder, HelperTransitionOrderingOptions.Length - 1);
                appSettings.SelectedTrackerItemGrouping = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedTrackerItemGrouping") ?? appSettings.SelectedTrackerItemGrouping, TrackerItemGroupingOptions.Length - 1);
                appSettings.SelectedTrackerItemOrder = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedTrackerItemOrder") ?? appSettings.SelectedTrackerItemOrder, TrackerItemOrderingOptions.Length - 1);
                appSettings.SelectedTrackerTransitionGrouping = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedTrackerTransitionGrouping") ?? appSettings.SelectedTrackerTransitionGrouping, TrackerTransitionGroupingOptions.Length - 1);
                appSettings.SelectedTrackerTransitionOrder = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedTrackerTransitionOrder") ?? appSettings.SelectedTrackerTransitionOrder, TrackerTransitionOrderingOptions.Length - 1);
                appSettings.SelectedSpoilerItemGrouping = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedSpoilerItemGrouping") ?? appSettings.SelectedSpoilerItemGrouping, SpoilerItemGroupingOptions.Length - 1);
                appSettings.SelectedSpoilerItemOrder = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedSpoilerItemOrder") ?? appSettings.SelectedSpoilerItemOrder, SpoilerItemOrderingOptions.Length - 1);
                appSettings.SelectedSpoilerTransitionGrouping = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedSpoilerTransitionGrouping") ?? appSettings.SelectedSpoilerTransitionGrouping, SpoilerTransitionGroupingOptions.Length - 1);
                appSettings.SelectedSpoilerTransitionOrder = Math.Min(GetValueFromAppSettings(appSettingsData, "SelectedSpoilerTransitionOrder") ?? appSettings.SelectedSpoilerTransitionOrder, SpoilerTransitionOrderingOptions.Length - 1);
            }

            return appSettings;
        }

        private static int? GetValueFromAppSettings(JObject jObject, string key)
        {
            var value = jObject[key]?.Value<int>();
            return !value.HasValue || value.Value >= 0 ? value : null;
        }

        public string GetSeed() =>
            GetDictionaryDataFromFileOrDefault<string>(SeedFilename)
                .TryGetValue("Seed", out var value) 
                    ? value
                    : "";

        private static T DeserializeFile<T>(string filePath)
        {
            using var file = new StreamReader(filePath);
            return JsonConvert.DeserializeObject<T>(file.ReadToEnd());
        }

        public void SaveHelperLogLocations(Dictionary<string, Location> helperLogLocations) => 
            WriteFile(HelperLogLocationsFilename, helperLogLocations);

        public void SaveHelperLogTransitions(Dictionary<string, Transition> helperLogTransitions) => 
            WriteFile(HelperLogTransitionsFilename, helperLogTransitions);

        public void SaveTrackerLogItems(Dictionary<string, ItemWithLocation> trackerLogItems) =>
            WriteFile(TrackerLogItemsFilename, trackerLogItems);

        public void SaveTrackerLogTransitions(Dictionary<string, TransitionWithDestination> trackerLogTransitions) =>
            WriteFile(TrackerLogTransitionsFilename, trackerLogTransitions);

        public void SaveAppSettings(AppSettings settings) => 
            WriteFile(AppSettingsFilename, settings);

        public void SaveSeed(string seed) =>
            WriteFile(SeedFilename, new Dictionary<string, string> { { "Seed", seed } });

        private static void WriteFile<T>(string filename, T data)
        {
            using StreamWriter file = File.CreateText(filename);
            new JsonSerializer { Formatting = Formatting.Indented }.Serialize(file, data);
        }
    }
}
