using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IResourceLoader
    {
        public List<Location> Locations { get; }
        public List<Item> Items { get; }
        public List<Item> PreviewItems { get; }
        public List<Transition> Transitions { get; }
        public Dictionary<string, LocationWithTime> GetHelperLogLocations();
        public Dictionary<string, TransitionWithTime> GetHelperLogTransitions();
        public Settings GetUserSettings();
        public void SaveHelperLogLocations(Dictionary<string, LocationWithTime> helperLogLocations);
        public void SaveHelperLogTransitions(Dictionary<string, TransitionWithTime> helperLogTransitions);
        public void SaveUserSettings(Settings settings);
    }

    public class ResourceLoader : IResourceLoader
    {
        public List<Location> Locations { get; private set; } = new List<Location>();
        public List<Item> Items { get; private set; } = new List<Item>();
        public List<Item> PreviewItems { get; private set; } = new List<Item>();
        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        public ResourceLoader()
        {
            LoadLocations();
            LoadItems();
            LoadTransitions();
        }

        private void LoadLocations()
        {
            if (!File.Exists(".\\Reference\\locations.json") || !File.Exists(".\\Reference\\rooms.json"))
            {
                return;
            }
            var roomsContent = ReadFile(".\\Reference\\rooms.json");
            var rooms = JsonConvert.DeserializeObject<Dictionary<string, RoomImport>>(roomsContent).Values.ToList();

            var locationsContent = ReadFile(".\\Reference\\locations.json");
            var locations = JsonConvert.DeserializeObject<Dictionary<string, LocationImport>>(locationsContent).Values.ToList();
            AddCustomLocations(locations);

            Locations = new List<Location>(locations.Select(x =>
            {
                var roomDetails = rooms.FirstOrDefault(y => y.SceneName == x.SceneName) ?? new RoomImport { SceneName = x.SceneName, MapArea = "undefined", TitledArea = "undefined" };
                return new Location { Name = x.Name, SceneName = x.SceneName, TitledArea = roomDetails.TitledArea, MapArea = roomDetails.MapArea };
            }));
        }

        private void AddCustomLocations(List<LocationImport> locationsList)
        {
            var locationsWithScenes = new List<LocationImport> {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/Resources/MrMushroom/logic.json
                new LocationImport { Name = "Mr_Mushroom-Fungal_Wastes", SceneName = "Fungus2_18" },
                new LocationImport { Name = "Mr_Mushroom-Kingdom's_Edge", SceneName = "Deepnest_East_01" },
                new LocationImport { Name = "Mr_Mushroom-Deepnest", SceneName = "Deepnest_40" },
                new LocationImport { Name = "Mr_Mushroom-Howling_Cliffs", SceneName = "Room_nailmaster" },
                new LocationImport { Name = "Mr_Mushroom-Ancient_Basin", SceneName = "Abyss_21" },
                new LocationImport { Name = "Mr_Mushroom-Fog_Canyon", SceneName = "Fungus3_44" },
                new LocationImport { Name = "Mr_Mushroom-King's_Pass", SceneName = "Tutorial_01" },

                // https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Resources/leverlocations.json
                new LocationImport { Name = "Switch-Dirtmouth_Stag", SceneName = "Room_Town_Stag_Station" },
                new LocationImport { Name = "Switch-Outside_Ancestral_Mound", SceneName = "Crossroads_06" },
                new LocationImport { Name = "Switch-Greenpath_Stag", SceneName = "Fungus1_22" },
                new LocationImport { Name = "Switch-Lower_Resting_Grounds", SceneName = "RestingGrounds_06" },
                new LocationImport { Name = "Switch-Petra_Arena", SceneName = "Fungus3_05" },
                new LocationImport { Name = "Switch-Queen's_Gardens_Stag", SceneName = "Fungus3_40" },
                new LocationImport { Name = "Switch-Crossroads_East", SceneName = "Crossroads_03" },
                new LocationImport { Name = "Lever-Dung_Defender", SceneName = "Waterways_05" },
                new LocationImport { Name = "Lever-Waterways_Hwurmp_Arena", SceneName = "Waterways_09" },
                new LocationImport { Name = "Lever-Queen's_Station_Mask_Shard", SceneName = "Fungus2_01" },
                new LocationImport { Name = "Lever-Queen's_Gardens_Ground_Block", SceneName = "Fungus3_04" },
                new LocationImport { Name = "Lever-Below_Overgrown_Mound", SceneName = "Fungus3_44" },
                new LocationImport { Name = "Lever-Tower_of_Love", SceneName = "Ruins2_11_b" },
                new LocationImport { Name = "Lever-Resting_Grounds_Stag", SceneName = "RestingGrounds_09" },
                new LocationImport { Name = "Lever-Abyss_Lighthouse", SceneName = "Abyss_Lighthouse_room" },
                new LocationImport { Name = "Lever-Failed_Tramway_Right", SceneName = "Deepnest_26" },
                new LocationImport { Name = "Lever-Failed_Tramway_Left", SceneName = "Deepnest_26b" },
                new LocationImport { Name = "Lever-Below_Spell_Twister", SceneName = "Ruins1_30" },
                new LocationImport { Name = "Lever-Sanctum_East", SceneName = "Ruins1_25" },
                new LocationImport { Name = "Lever-Sanctum_Soul_Warrior", SceneName = "Ruins1_23" },
                new LocationImport { Name = "Lever-Sanctum_Bottom", SceneName = "Ruins1_23" },
                new LocationImport { Name = "Lever-Sanctum_West_Upper", SceneName = "Ruins1_32" },
                new LocationImport { Name = "Lever-Sanctum_West_Lower", SceneName = "Ruins1_32" },
                new LocationImport { Name = "Lever-City_Fountain", SceneName = "Ruins1_27" },
                new LocationImport { Name = "Lever-City_Spire_Sentry_Lower", SceneName = "Ruins2_01" },
                new LocationImport { Name = "Lever-City_Spire_Sentry_Upper", SceneName = "Ruins2_01" },
                new LocationImport { Name = "Lever-City_Bridge_Above_Fountain", SceneName = "Ruins1_18" },
                new LocationImport { Name = "Lever-City_Storerooms", SceneName = "Ruins1_17" },
                new LocationImport { Name = "Lever-City_Lemm", SceneName = "Ruins1_05b" },
                new LocationImport { Name = "Lever-City_Above_Lemm_Right", SceneName = "Ruins1_05c" },
                new LocationImport { Name = "Lever-City_Above_Lemm_Left", SceneName = "Ruins1_05c" },
                new LocationImport { Name = "Lever-City_Above_Lemm_Upper", SceneName = "Ruins1_05" },
                new LocationImport { Name = "Lever-Shade_Soul_Exit", SceneName = "Ruins1_31b" },
                new LocationImport { Name = "Lever-Emilitia", SceneName = "Ruins_House_03" },
                new LocationImport { Name = "Lever-Mantis_Lords_Top_Left", SceneName = "Fungus2_15" },
                new LocationImport { Name = "Lever-Mantis_Lords_Middle_Left", SceneName = "Fungus2_15" },
                new LocationImport { Name = "Lever-Mantis_Lords_Bottom_Left", SceneName = "Fungus2_15" },
                new LocationImport { Name = "Lever-Mantis_Lords_Middle_Right", SceneName = "Fungus2_15" },
                new LocationImport { Name = "Lever-Mantis_Lords_Bottom_Right", SceneName = "Fungus2_15" },
                new LocationImport { Name = "Lever-Mantis_Claw", SceneName = "Fungus2_14" },
                new LocationImport { Name = "Lever-Mantis_Lords_Access", SceneName = "Fungus2_14" },
                new LocationImport { Name = "Lever-Fungal_Wastes_Thorns_Gauntlet", SceneName = "Fungus2_04" },
                new LocationImport { Name = "Lever-Fungal_Wastes_Below_Shrumal_Ogres", SceneName = "Fungus2_04" },
                new LocationImport { Name = "Lever-Fungal_Wastes_Bouncy_Grub", SceneName = "Fungus2_18" },
                new LocationImport { Name = "Lever-Dirtmouth_Elevator", SceneName = "Town" },
                new LocationImport { Name = "Lever-Crystal_Peak_Tall_Room_Upper", SceneName = "Mines_20" },
                new LocationImport { Name = "Lever-Crystal_Peak_Tall_Room_Middle", SceneName = "Mines_20" },
                new LocationImport { Name = "Lever-Crystal_Peak_Tall_Room_Lower", SceneName = "Mines_20" },
                new LocationImport { Name = "Lever-Crystal_Peak_Spike_Grub", SceneName = "Mines_03" },
                new LocationImport { Name = "Lever-Crystal_Peak_Below_Chest", SceneName = "Mines_04" },
                new LocationImport { Name = "Lever-Crystal_Peak_Above_Chest", SceneName = "Mines_37" },
                new LocationImport { Name = "Lever-Crystal_Peak_Crushers_Grub", SceneName = "Mines_19" },
                new LocationImport { Name = "Lever-Crystal_Peak_Crushers_Chest", SceneName = "Mines_37" },
                new LocationImport { Name = "Lever-Palace_Atrium", SceneName = "White_Palace_03_hub" },
                new LocationImport { Name = "Lever-Palace_Right", SceneName = "White_Palace_15" },
                new LocationImport { Name = "Lever-Palace_Final", SceneName = "White_Palace_12" },
                new LocationImport { Name = "Lever-Path_of_Pain", SceneName = "White_Palace_17" },
                new LocationImport { Name = "Lever-Palace_Entrance_Orb", SceneName = "White_Palace_02" },
                new LocationImport { Name = "Lever-Palace_Left_Orb", SceneName = "White_Palace_14" },
                new LocationImport { Name = "Lever-Palace_Right_Orb", SceneName = "White_Palace_15" },
                new LocationImport { Name = "Lever-Pilgrim's_Way_Left", SceneName = "Fungus2_21" },
                new LocationImport { Name = "Lever-Pilgrim's_Way_Right", SceneName = "Fungus2_21" },

                new LocationImport { Name = "Nailsmith_Upgrade_1", SceneName = "Room_nailsmith" },
                new LocationImport { Name = "Nailsmith_Upgrade_2", SceneName = "Room_nailsmith" },
                new LocationImport { Name = "Nailsmith_Upgrade_3", SceneName = "Room_nailsmith" },
                new LocationImport { Name = "Nailsmith_Upgrade_4", SceneName = "Room_nailsmith" },
            };
            locationsWithScenes.ForEach(x =>
            {
                if (locationsList.FirstOrDefault(y => y.Name == x.Name) == null)
                {
                    locationsList.Add(x);
                }
            });
        }

        private void LoadItems()
        {
            if (!File.Exists(".\\Reference\\items.json"))
            {
                return;
            }
            var content = ReadFile(".\\Reference\\items.json");
            Items = JsonConvert.DeserializeObject<Dictionary<string, Item>>(content).Values.ToList();

            AddCustomItems();

            SetupItemPreviews();
        }

        private const string MrMushroomPool = "MrMushroom";
        private const string SkillUpgradePool = "Skill Upgrade";
        private const string LeverPool = "Lever";
        private const string TranscendencePool = "Charm - Transcendence";
        private const string EggPool = "Egg";
        private const string SkillPool = "Skill";

        private void AddCustomItems()
        {
            var itemsWithPools = new List<Item> {
                new Item { Name = "Kingsoul", Pool = "Charm" },
            }
            .Concat(GetMrMushroomItems())
            .Concat(GetSkillUpgrades())
            .Concat(GetLevers())
            .Concat(GetTranscendenceCharms())
            .Concat(GetRainbowEggs())
            .Concat(GetNailUpgrades())
            .ToList();

            itemsWithPools.ForEach(x =>
            {
                if (Items.FirstOrDefault(y => y.Name == x.Name) == null)
                {
                    Items.Add(x);
                }
            });
        }

        private List<Item> GetMrMushroomItems() =>
            new List<Item> { new Item { Name = "Mr_Mushroom_Level_Up", Pool = MrMushroomPool }, };

        private List<Item> GetSkillUpgrades() =>
            new List<Item>
            {
                new Item { Name = "DirectionalDash", Pool = SkillUpgradePool },
                new Item { Name = "ExtraAirDash", Pool = SkillUpgradePool },
                new Item { Name = "WallClimb", Pool = SkillUpgradePool },
                new Item { Name = "VerticalSuperdash", Pool = SkillUpgradePool },
                new Item { Name = "TripleJump", Pool = SkillUpgradePool },
                new Item { Name = "DownwardFireball", Pool = SkillUpgradePool },
                new Item { Name = "HorizontalDive", Pool = SkillUpgradePool },
                new Item { Name = "SpiralScream", Pool = SkillUpgradePool },
            };

        private List<Item> GetLevers() =>
            new List<Item>
            {
                new Item { Name = "Switch-Dirtmouth_Stag", Pool = LeverPool },
                new Item { Name = "Switch-Outside_Ancestral_Mound", Pool = LeverPool },
                new Item { Name = "Switch-Greenpath_Stag", Pool = LeverPool },
                new Item { Name = "Switch-Lower_Resting_Grounds", Pool = LeverPool },
                new Item { Name = "Switch-Petra_Arena", Pool = LeverPool },
                new Item { Name = "Switch-Queen's_Gardens_Stag", Pool = LeverPool },
                new Item { Name = "Switch-Crossroads_East", Pool = LeverPool },
                new Item { Name = "Lever-Dung_Defender", Pool = LeverPool },
                new Item { Name = "Lever-Waterways_Hwurmp_Arena", Pool = LeverPool },
                new Item { Name = "Lever-Queen's_Station_Mask_Shard", Pool = LeverPool },
                new Item { Name = "Lever-Queen's_Gardens_Ground_Block", Pool = LeverPool },
                new Item { Name = "Lever-Below_Overgrown_Mound", Pool = LeverPool },
                new Item { Name = "Lever-Tower_of_Love", Pool = LeverPool },
                new Item { Name = "Lever-Resting_Grounds_Stag", Pool = LeverPool },
                new Item { Name = "Lever-Abyss_Lighthouse", Pool = LeverPool },
                new Item { Name = "Lever-Failed_Tramway_Right", Pool = LeverPool },
                new Item { Name = "Lever-Failed_Tramway_Left", Pool = LeverPool },
                new Item { Name = "Lever-Below_Spell_Twister", Pool = LeverPool },
                new Item { Name = "Lever-Sanctum_East", Pool = LeverPool },
                new Item { Name = "Lever-Sanctum_Soul_Warrior", Pool = LeverPool },
                new Item { Name = "Lever-Sanctum_Bottom", Pool = LeverPool },
                new Item { Name = "Lever-Sanctum_West_Upper", Pool = LeverPool },
                new Item { Name = "Lever-Sanctum_West_Lower", Pool = LeverPool },
                new Item { Name = "Lever-City_Fountain", Pool = LeverPool },
                new Item { Name = "Lever-City_Spire_Sentry_Lower", Pool = LeverPool },
                new Item { Name = "Lever-City_Spire_Sentry_Upper", Pool = LeverPool },
                new Item { Name = "Lever-City_Bridge_Above_Fountain", Pool = LeverPool },
                new Item { Name = "Lever-City_Storerooms", Pool = LeverPool },
                new Item { Name = "Lever-City_Lemm", Pool = LeverPool },
                new Item { Name = "Lever-City_Above_Lemm_Right", Pool = LeverPool },
                new Item { Name = "Lever-City_Above_Lemm_Left", Pool = LeverPool },
                new Item { Name = "Lever-City_Above_Lemm_Upper", Pool = LeverPool },
                new Item { Name = "Lever-Shade_Soul_Exit", Pool = LeverPool },
                new Item { Name = "Lever-Emilitia", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Lords_Top_Left", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Lords_Middle_Left", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Lords_Bottom_Left", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Lords_Middle_Right", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Lords_Bottom_Right", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Claw", Pool = LeverPool },
                new Item { Name = "Lever-Mantis_Lords_Access", Pool = LeverPool },
                new Item { Name = "Lever-Fungal_Wastes_Thorns_Gauntlet", Pool = LeverPool },
                new Item { Name = "Lever-Fungal_Wastes_Below_Shrumal_Ogres", Pool = LeverPool },
                new Item { Name = "Lever-Fungal_Wastes_Bouncy_Grub", Pool = LeverPool },
                new Item { Name = "Lever-Dirtmouth_Elevator", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Tall_Room_Upper", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Tall_Room_Middle", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Tall_Room_Lower", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Spike_Grub", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Below_Chest", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Above_Chest", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Crushers_Grub", Pool = LeverPool },
                new Item { Name = "Lever-Crystal_Peak_Crushers_Chest", Pool = LeverPool },
                new Item { Name = "Lever-Palace_Atrium", Pool = LeverPool },
                new Item { Name = "Lever-Palace_Right", Pool = LeverPool },
                new Item { Name = "Lever-Palace_Final", Pool = LeverPool },
                new Item { Name = "Lever-Path_of_Pain", Pool = LeverPool },
                new Item { Name = "Lever-Palace_Entrance_Orb", Pool = LeverPool },
                new Item { Name = "Lever-Palace_Left_Orb", Pool = LeverPool },
                new Item { Name = "Lever-Palace_Right_Orb", Pool = LeverPool },
                new Item { Name = "Lever-Pilgrim's_Way_Left", Pool = LeverPool },
                new Item { Name = "Lever-Pilgrim's_Way_Right", Pool = LeverPool },
            };

        private List<Item> GetTranscendenceCharms() =>
            new List<Item>
            {
                new Item { Name = "Marissa's_Audience", Pool = TranscendencePool },
                new Item { Name = "Lemm's_Strength", Pool = TranscendencePool },
                new Item { Name = "Snail_Slash", Pool = TranscendencePool },
                new Item { Name = "Millibelle's_Blessing", Pool = TranscendencePool },
                new Item { Name = "Disinfectant_Flask", Pool = TranscendencePool },
                new Item { Name = "Florist's_Blessing", Pool = TranscendencePool },
                new Item { Name = "Greedsong", Pool = TranscendencePool },
                new Item { Name = "Snail_Soul", Pool = TranscendencePool },
                new Item { Name = "Nitro_Crystal", Pool = TranscendencePool },
                new Item { Name = "Shaman_Amp", Pool = TranscendencePool },
                new Item { Name = "Crystalmaster", Pool = TranscendencePool },
                new Item { Name = "Bluemoth_Wings", Pool = TranscendencePool },
                new Item { Name = "Chaos_Orb", Pool = TranscendencePool },
                new Item { Name = "Antigravity_Amulet", Pool = TranscendencePool },
            };

        private List<Item> GetRainbowEggs() =>
            new List<Item>
            {
                new Item { Name = "Red_Egg", Pool = EggPool },
                new Item { Name = "Orange_Egg", Pool = EggPool },
                new Item { Name = "Yellow_Egg", Pool = EggPool },
                new Item { Name = "Green_Egg", Pool = EggPool },
                new Item { Name = "Cyan_Egg", Pool = EggPool },
                new Item { Name = "Blue_Egg", Pool = EggPool },
                new Item { Name = "Purple_Egg", Pool = EggPool },
                new Item { Name = "Pink_Egg", Pool = EggPool },
                new Item { Name = "Trans_Egg", Pool = EggPool },
                new Item { Name = "Rainbow_Egg", Pool = EggPool },
                new Item { Name = "Arcane_Eg", Pool = EggPool },
            };

        private List<Item> GetNailUpgrades() =>
            new List<Item>
            {
                new Item { Name = "Nail_Upgrade", Pool = SkillPool }
            };

        private void SetupItemPreviews()
        {
            PreviewItems = Items.Select(x => new Item
            {
                Name = GetPreviewName(x),
                Pool = x.Pool
            }).ToList();
        }

        private string GetPreviewName(Item item) =>
            item.Pool switch
            {
                LeverPool => GetLeverPreviewName(item.Name),
                SkillUpgradePool => item.Name.AddSpacesBeforeCapitals(),
                _ => item.Name.Replace("-", " ").Replace("_", " "),
            };

        private string GetLeverPreviewName(string leverName) =>
            leverName switch
            {
                // https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Resources/languagedata.json
                "Switch-Dirtmouth_Stag" => "Dirtmouth Station Door",
                "Switch-Outside_Ancestral_Mound" => "Shaman Mound Pillar",
                "Switch-Greenpath_Stag" => "Greenpath Stag Gate",
                "Switch-Lower_Resting_Grounds" => "Resting Grounds Floor",
                "Switch-Petra_Arena" => "Petra Arena Gate",
                "Switch-Queen's_Gardens_Stag" => "QG Stag Door",
                "Switch-Crossroads_East" => "East Crossroads Gate",
                "Lever-Dung_Defender" => "Waterways Acid Pool",
                "Lever-Waterways_Hwurmp_Arena" => "Waterways Exit Gate",
                "Lever-Queen's_Station_Mask_Shard" => "QS Mask Gauntlet Exit",
                "Lever-Queen's_Gardens_Ground_Block" => "QG Ground Block",
                "Lever-Below_Overgrown_Mound" => "Overgrown Mound Gate",
                "Lever-Tower_of_Love" => "Tower of Love Exit",
                "Lever-Resting_Grounds_Stag" => "RG Stag Lever",
                "Lever-Abyss_Lighthouse" => "Abyss Lighthouse",
                "Lever-Failed_Tramway_Right" => "Tramway Lower Gate",
                "Lever-Failed_Tramway_Left" => "Tramway Exit Gates",
                "Lever-Below_Spell_Twister" => "Gate Below Spell Twister",
                "Lever-Sanctum_East" => "East Sanctum Gate",
                "Lever-Sanctum_Soul_Warrior" => "Soul Warrior 1 Arena Gate",
                "Lever-Sanctum_Bottom" => "Lower Sanctum Gate",
                "Lever-Sanctum_West_Upper" => "West Sanctum Upper Gate",
                "Lever-Sanctum_West_Lower" => "West Sanctum Lower Gate",
                "Lever-City_Fountain" => "City Fountain Gate",
                "Lever-City_Spire_Sentry_Lower" => "Spire Lower Gate",
                "Lever-City_Spire_Sentry_Upper" => "Spire Upper Gate",
                "Lever-City_Bridge_Above_Fountain" => "Spire Bridge Gate",
                "Lever-City_Storerooms" => "City Storerooms Gate",
                "Lever-City_Lemm" => "Gate Opposite Lemm",
                "Lever-City_Above_Lemm_Right" => "Single Gate Above Lemm",
                "Lever-City_Above_Lemm_Left" => "Triple Gate Above Lemm",
                "Lever-City_Above_Lemm_Upper" => "City Gate Opposite Grub",
                "Lever-Shade_Soul_Exit" => "Shade Soul Exit Gate",
                "Lever-Emilitia" => "Emilitia Door",
                "Lever-Mantis_Lords_Top_Left" => "Mantis Lords Top Left",
                "Lever-Mantis_Lords_Middle_Left" => "Mantis Lords Middle Left",
                "Lever-Mantis_Lords_Bottom_Left" => "Mantis Lords Bottom Left",
                "Lever-Mantis_Lords_Middle_Right" => "Mantis Lords Middle Right",
                "Lever-Mantis_Lords_Bottom_Right" => "Mantis Lords Bottom Right",
                "Lever-Mantis_Claw" => "Mantis Claw Access",
                "Lever-Mantis_Lords_Access" => "Mantis Lords Access",
                "Lever-Fungal_Wastes_Thorns_Gauntlet" => "Fungal Thorns Gauntlet Exit",
                "Lever-Fungal_Wastes_Below_Shrumal_Ogres" => "Gate Below Shrumal Ogres",
                "Lever-Fungal_Wastes_Bouncy_Grub" => "Fungal Bouncy Grub Gate",
                "Lever-Dirtmouth_Elevator" => "Dirtmouth Elevator",
                "Lever-Crystal_Peak_Tall_Room_Upper" => "Peaks Tall Room Upper",
                "Lever-Crystal_Peak_Tall_Room_Middle" => "Peaks Tall Room Middle",
                "Lever-Crystal_Peak_Tall_Room_Lower" => "Peaks Tall Room Lower",
                "Lever-Crystal_Peak_Spike_Grub" => "Peaks Spike Grub Gate",
                "Lever-Crystal_Peak_Below_Chest" => "Peaks Gate Below Chest",
                "Lever-Crystal_Peak_Above_Chest" => "Peaks Gate Above Chest",
                "Lever-Crystal_Peak_Crushers_Grub" => "Peaks Upper Crushers",
                "Lever-Crystal_Peak_Crushers_Chest" => "Peaks Lower Crushers",
                "Lever-Palace_Atrium" => "Palace Atrium Gate",
                "Lever-Palace_Right" => "Palace Right Gates",
                "Lever-Palace_Final" => "Palace Final Gates",
                "Lever-Path_of_Pain" => "Path of Pain Gate",
                "Lever-Palace_Entrance_Orb" => "Palace Entrance Orb",
                "Lever-Palace_Left_Orb" => "Palace Left Orb",
                "Lever-Palace_Right_Orb" => "Palace Right Orb",
                "Lever-Pilgrim's_Way_Left" => "Left Pilgrim's Way Bridge",
                "Lever-Pilgrim's_Way_Right" => "Right Pilgrim's Way Bridge",
                _ => leverName,
            };

        private void LoadTransitions()
        {
            if (!File.Exists(".\\Reference\\transitions.json") || !File.Exists(".\\Reference\\rooms.json"))
            {
                return;
            }
            var roomsContent = ReadFile(".\\Reference\\rooms.json");
            var rooms = JsonConvert.DeserializeObject<Dictionary<string, RoomImport>>(roomsContent).Values.ToList();

            var transitionsContent = ReadFile(".\\Reference\\transitions.json");
            var transitions = JsonConvert.DeserializeObject<Dictionary<string, TransitionImport>>(transitionsContent).Values.ToList();

            Transitions = new List<Transition>(transitions.Select(x =>
            {
                var roomDetails = rooms.FirstOrDefault(y => y.SceneName == x.SceneName) ?? new RoomImport { SceneName = x.SceneName, MapArea = "undefined", TitledArea = "undefined" };
                return new Transition { SceneName = x.SceneName, DoorName = x.DoorName, TitledArea = roomDetails.TitledArea, MapArea = roomDetails.MapArea };
            }));
        }

        public Dictionary<string, LocationWithTime> GetHelperLogLocations()
        {
            if (!File.Exists("_HelperLogLocations.json"))
            {
                return new Dictionary<string, LocationWithTime>();
            }
            var content = ReadFile("_HelperLogLocations.json");
            return JsonConvert.DeserializeObject<Dictionary<string, LocationWithTime>>(content);
        }

        public Dictionary<string, TransitionWithTime> GetHelperLogTransitions()
        {
            if (!File.Exists("_HelperLogTransitions.json"))
            {
                return new Dictionary<string, TransitionWithTime>();
            }
            var content = ReadFile("_HelperLogTransitions.json");
            return JsonConvert.DeserializeObject<Dictionary<string, TransitionWithTime>>(content);
        }

        public Settings GetUserSettings()
        {
            if (!File.Exists("_Settings.json"))
            {
                var defaultSettings = new Settings();
                defaultSettings.SetDefaultValues();
                return defaultSettings;
            }
            var content = ReadFile("_Settings.json");
            var settings = JsonConvert.DeserializeObject<Settings>(content);
            settings.SetDefaultValues();
            return settings;
        }

        private static string ReadFile(string filename)
        {
            using var file = new StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}/{filename}");
            return file.ReadToEnd();
        }

        public void SaveHelperLogLocations(Dictionary<string, LocationWithTime> helperLogLocations)
        {
            WriteFile("_HelperLogLocations.json", helperLogLocations);
        }

        public void SaveHelperLogTransitions(Dictionary<string, TransitionWithTime> helperLogTransitions)
        {
            WriteFile("_HelperLogTransitions.json", helperLogTransitions);
        }

        public void SaveUserSettings(Settings settings)
        {
            WriteFile("_Settings.json", settings);
        }

        private static void WriteFile<T>(string filename, T data)
        {
            using StreamWriter file = File.CreateText($"{AppDomain.CurrentDomain.BaseDirectory}/{filename}");
            new JsonSerializer { Formatting = Formatting.Indented }.Serialize(file, data);
        }
    }
}
