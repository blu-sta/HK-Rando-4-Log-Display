using HK_Rando_4_Log_Display.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

        private void AddCustomItems()
        {
            var itemsWithPools = new List<Item> {
                new Item { Name = "Kingsoul", Pool = "Charm" },
            }
            .Concat(GetMrMushroomItems())
            .Concat(GetSkillUpgrades())
            .Concat(GetLevers())
            .Concat(GetTranscendenceCharms())
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
            new List<Item> { new Item { Name = "Mr_Mushroom_Level_Up", Pool = "MrMushroom" }, };

        private List<Item> GetSkillUpgrades() =>
            new List<Item>
            {
                new Item { Name = "DirectionalDash", Pool = "Skill Upgrade" },
                new Item { Name = "ExtraAirDash", Pool = "Skill Upgrade" },
                new Item { Name = "WallClimb", Pool = "Skill Upgrade" },
                new Item { Name = "VerticalSuperdash", Pool = "Skill Upgrade" },
                new Item { Name = "TripleJump", Pool = "Skill Upgrade" },
                new Item { Name = "DownwardFireball", Pool = "Skill Upgrade" },
                new Item { Name = "HorizontalDive", Pool = "Skill Upgrade" },
                new Item { Name = "SpiralScream", Pool = "Skill Upgrade" },
            };

        private List<Item> GetLevers() =>
            new List<Item>
            {
                new Item { Name = "Switch-Dirtmouth_Stag", Pool = "Lever" },
                new Item { Name = "Switch-Outside_Ancestral_Mound", Pool = "Lever" },
                new Item { Name = "Switch-Greenpath_Stag", Pool = "Lever" },
                new Item { Name = "Switch-Lower_Resting_Grounds", Pool = "Lever" },
                new Item { Name = "Switch-Petra_Arena", Pool = "Lever" },
                new Item { Name = "Switch-Queen's_Gardens_Stag", Pool = "Lever" },
                new Item { Name = "Switch-Crossroads_East", Pool = "Lever" },
                new Item { Name = "Lever-Dung_Defender", Pool = "Lever" },
                new Item { Name = "Lever-Waterways_Hwurmp_Arena", Pool = "Lever" },
                new Item { Name = "Lever-Queen's_Station_Mask_Shard", Pool = "Lever" },
                new Item { Name = "Lever-Queen's_Gardens_Ground_Block", Pool = "Lever" },
                new Item { Name = "Lever-Below_Overgrown_Mound", Pool = "Lever" },
                new Item { Name = "Lever-Tower_of_Love", Pool = "Lever" },
                new Item { Name = "Lever-Resting_Grounds_Stag", Pool = "Lever" },
                new Item { Name = "Lever-Abyss_Lighthouse", Pool = "Lever" },
                new Item { Name = "Lever-Failed_Tramway_Right", Pool = "Lever" },
                new Item { Name = "Lever-Failed_Tramway_Left", Pool = "Lever" },
                new Item { Name = "Lever-Below_Spell_Twister", Pool = "Lever" },
                new Item { Name = "Lever-Sanctum_East", Pool = "Lever" },
                new Item { Name = "Lever-Sanctum_Soul_Warrior", Pool = "Lever" },
                new Item { Name = "Lever-Sanctum_Bottom", Pool = "Lever" },
                new Item { Name = "Lever-Sanctum_West_Upper", Pool = "Lever" },
                new Item { Name = "Lever-Sanctum_West_Lower", Pool = "Lever" },
                new Item { Name = "Lever-City_Fountain", Pool = "Lever" },
                new Item { Name = "Lever-City_Spire_Sentry_Lower", Pool = "Lever" },
                new Item { Name = "Lever-City_Spire_Sentry_Upper", Pool = "Lever" },
                new Item { Name = "Lever-City_Bridge_Above_Fountain", Pool = "Lever" },
                new Item { Name = "Lever-City_Storerooms", Pool = "Lever" },
                new Item { Name = "Lever-City_Lemm", Pool = "Lever" },
                new Item { Name = "Lever-City_Above_Lemm_Right", Pool = "Lever" },
                new Item { Name = "Lever-City_Above_Lemm_Left", Pool = "Lever" },
                new Item { Name = "Lever-City_Above_Lemm_Upper", Pool = "Lever" },
                new Item { Name = "Lever-Shade_Soul_Exit", Pool = "Lever" },
                new Item { Name = "Lever-Emilitia", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Lords_Top_Left", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Lords_Middle_Left", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Lords_Bottom_Left", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Lords_Middle_Right", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Lords_Bottom_Right", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Claw", Pool = "Lever" },
                new Item { Name = "Lever-Mantis_Lords_Access", Pool = "Lever" },
                new Item { Name = "Lever-Fungal_Wastes_Thorns_Gauntlet", Pool = "Lever" },
                new Item { Name = "Lever-Fungal_Wastes_Below_Shrumal_Ogres", Pool = "Lever" },
                new Item { Name = "Lever-Fungal_Wastes_Bouncy_Grub", Pool = "Lever" },
                new Item { Name = "Lever-Dirtmouth_Elevator", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Tall_Room_Upper", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Tall_Room_Middle", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Tall_Room_Lower", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Spike_Grub", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Below_Chest", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Above_Chest", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Crushers_Grub", Pool = "Lever" },
                new Item { Name = "Lever-Crystal_Peak_Crushers_Chest", Pool = "Lever" },
                new Item { Name = "Lever-Palace_Atrium", Pool = "Lever" },
                new Item { Name = "Lever-Palace_Right", Pool = "Lever" },
                new Item { Name = "Lever-Palace_Final", Pool = "Lever" },
                new Item { Name = "Lever-Path_of_Pain", Pool = "Lever" },
                new Item { Name = "Lever-Palace_Entrance_Orb", Pool = "Lever" },
                new Item { Name = "Lever-Palace_Left_Orb", Pool = "Lever" },
                new Item { Name = "Lever-Palace_Right_Orb", Pool = "Lever" },
                new Item { Name = "Lever-Pilgrim's_Way_Left", Pool = "Lever" },
                new Item { Name = "Lever-Pilgrim's_Way_Right", Pool = "Lever" },
            };

        private List<Item> GetTranscendenceCharms() =>
            new List<Item>
            {
                new Item { Name = "Marissa's_Audience", Pool = "Charm" },
                new Item { Name = "Lemm's_Strength", Pool = "Charm" },
                new Item { Name = "Snail_Slash", Pool = "Charm" },
                new Item { Name = "Millibelle's_Blessing", Pool = "Charm" },
                new Item { Name = "Disinfectant_Flask", Pool = "Charm" },
                new Item { Name = "Florist's_Blessing", Pool = "Charm" },
                new Item { Name = "Greedsong", Pool = "Charm" },
                new Item { Name = "Snail_Soul", Pool = "Charm" },
                new Item { Name = "Nitro_Crystal", Pool = "Charm" },
                new Item { Name = "Shaman_Amp", Pool = "Charm" },
                new Item { Name = "Crystalmaster", Pool = "Charm" },
                new Item { Name = "Bluemoth_Wings", Pool = "Charm" },
                new Item { Name = "Chaos_Orb", Pool = "Charm" },
                new Item { Name = "Antigravity_Amulet", Pool = "Charm" },
            };

        private void SetupItemPreviews()
        {
            PreviewItems = Items.Select(x => new Item 
            {
                Name = x.Name.Replace("-"," ").Replace("_"," "),
                Pool = x.Pool 
            }).ToList();
        }

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
