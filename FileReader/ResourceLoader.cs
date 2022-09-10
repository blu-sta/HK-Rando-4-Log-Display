using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using Newtonsoft.Json;
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
        public Settings GetAppSettings(
            int helperGroupingsLength,
            int helperOrdersLength,
            int trackerItemGroupingsLength,
            int trackerItemOrdersLength,
            int trackerTransitionOrdersLength,
            int spoilerItemGroupingsLength,
            int spoilerItemOrdersLength,
            int spoilerTransitionOrders
        );
        public string GetSeed();
        public void SaveHelperLogLocations(Dictionary<string, LocationWithTime> helperLogLocations);
        public void SaveHelperLogTransitions(Dictionary<string, TransitionWithTime> helperLogTransitions);
        public void SaveAppSettings(Settings settings);
        public void SaveSeed(string seed);
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
            if (!File.Exists(Constants.ReferenceLocationsFilePath) || !File.Exists(Constants.ReferenceRoomsFilePath))
            {
                return;
            }

            var rooms = DeserializeFile<Dictionary<string, RoomImport>>(Constants.ReferenceRoomsFilePath).Values.ToList();
            var locations = DeserializeFile<Dictionary<string, LocationImport>>(Constants.ReferenceLocationsFilePath).Values.ToList();

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

                // https://github.com/homothetyhk/BenchRando/blob/master/BenchRando/Resources/benches.json
                new LocationImport { Name = "Bench-Dirtmouth", SceneName = "Town" },
                new LocationImport { Name = "Bench-Mato" , SceneName = "Room_nailmaster" },
                new LocationImport { Name = "Bench-Crossroads_Hot_Springs", SceneName = "Crossroads_30" },
                new LocationImport { Name = "Bench-Crossroads_Stag", SceneName = "Crossroads_47" },
                new LocationImport { Name = "Bench-Salubra", SceneName = "Crossroads_04" },
                new LocationImport { Name = "Bench-Ancestral_Mound", SceneName = "Crossroads_ShamanTemple" },
                new LocationImport { Name = "Bench-Black_Egg_Temple", SceneName = "Room_Final_Boss_Atrium" },
                new LocationImport { Name = "Bench-Waterfall", SceneName = "Fungus1_01b" },
                new LocationImport { Name = "Bench-Stone_Sanctuary", SceneName = "Fungus1_37" },
                new LocationImport { Name = "Bench-Greenpath_Toll", SceneName = "Fungus1_31" },
                new LocationImport { Name = "Bench-Greenpath_Stag", SceneName = "Fungus1_16_alt" },
                new LocationImport { Name = "Bench-Lake_of_Unn", SceneName = "Room_Slug_Shrine" },
                new LocationImport { Name = "Bench-Sheo", SceneName = "Fungus1_15" },
                new LocationImport { Name = "Bench-Archives", SceneName = "Fungus3_archive" },
                new LocationImport { Name = "Bench-Queen's_Station", SceneName = "Fungus2_02" },
                new LocationImport { Name = "Bench-Leg_Eater", SceneName = "Fungus2_26" },
                new LocationImport { Name = "Bench-Bretta", SceneName = "Fungus2_13" },
                new LocationImport { Name = "Bench-Mantis_Village", SceneName = "Fungus2_31" },
                new LocationImport { Name = "Bench-Quirrel", SceneName = "Ruins1_02" },
                new LocationImport { Name = "Bench-City_Toll", SceneName = "Ruins1_31" },
                new LocationImport { Name = "Bench-City_Storerooms", SceneName = "Ruins1_29" },
                new LocationImport { Name = "Bench-Watcher's_Spire", SceneName = "Ruins1_18" },
                new LocationImport { Name = "Bench-King's_Station", SceneName = "Ruins2_08" },
                new LocationImport { Name = "Bench-Pleasure_House", SceneName = "Ruins_Bathhouse" },
                new LocationImport { Name = "Bench-Waterways", SceneName = "Waterways_02" },
                new LocationImport { Name = "Bench-Godhome_Atrium", SceneName = "GG_Atrium" },
                new LocationImport { Name = "Bench-Godhome_Roof", SceneName = "GG_Atrium_Roof" },
                new LocationImport { Name = "Bench-Hall_of_Gods", SceneName = "GG_Workshop" },
                new LocationImport { Name = "Bench-Deepnest_Hot_Springs", SceneName = "Deepnest_30" },
                new LocationImport { Name = "Bench-Failed_Tramway", SceneName = "Deepnest_14" },
                new LocationImport { Name = "Bench-Beast's_Den", SceneName = "Deepnest_Spider_Town" },
                new LocationImport { Name = "Bench-Basin_Toll", SceneName = "Abyss_18" },
                new LocationImport { Name = "Bench-Hidden_Station", SceneName = "Abyss_22" },
                new LocationImport { Name = "Bench-Oro", SceneName = "Deepnest_East_06" },
                new LocationImport { Name = "Bench-Camp", SceneName = "Deepnest_East_13" },
                new LocationImport { Name = "Bench-Colosseum", SceneName = "Room_Colosseum_02" },
                new LocationImport { Name = "Bench-Hive", SceneName = "Hive_01" },
                new LocationImport { Name = "Bench-Peak_Dark_Room", SceneName = "Mines_29" },
                new LocationImport { Name = "Bench-Crystal_Guardian", SceneName = "Mines_18" },
                new LocationImport { Name = "Bench-Grounds_Stag", SceneName = "RestingGrounds_09" },
                new LocationImport { Name = "Bench-Grey_Mourner", SceneName = "RestingGrounds_12" },
                new LocationImport { Name = "Bench-Gardens_Cornifer", SceneName = "Fungus1_24" },
                new LocationImport { Name = "Bench-Gardens_Toll", SceneName = "Fungus3_50" },
                new LocationImport { Name = "Bench-Gardens_Stag", SceneName = "Fungus3_40" },
                new LocationImport { Name = "Bench-Palace_Entrance", SceneName = "White_Palace_01" },
                new LocationImport { Name = "Bench-Palace_Atrium", SceneName = "White_Palace_03_hub" },
                new LocationImport { Name = "Bench-Palace_Balcony", SceneName = "White_Palace_06" },
                new LocationImport { Name = "Bench-Upper_Tram", SceneName = "Room_Tram_RG" },
                new LocationImport { Name = "Bench-Lower_Tram", SceneName = "Room_Tram" },
                new LocationImport { Name = "Bench-Stag_Nest", SceneName = "Cliffs_03" },
                new LocationImport { Name = "Bench-Cliffs_Overhang", SceneName = "Cliffs_01" },
                new LocationImport { Name = "Bench-Joni's_Repose", SceneName = "Cliffs_05" },
                new LocationImport { Name = "Bench-Nightmare_Lantern", SceneName = "Cliffs_06" },
                new LocationImport { Name = "Bench-Blasted_Plains", SceneName = "Cliffs_01" },
                new LocationImport { Name = "Bench-Baldur_Cavern", SceneName = "Fungus1_28" },
                new LocationImport { Name = "Bench-Crossroads_Center", SceneName = "Crossroads_40" },
                new LocationImport { Name = "Bench-Myla", SceneName = "Crossroads_45" },
                new LocationImport { Name = "Bench-Grubfather", SceneName = "Crossroads_38" },
                new LocationImport { Name = "Bench-Crossroads_Elevator", SceneName = "Crossroads_43" },
                new LocationImport { Name = "Bench-Fungal_Road", SceneName = "Crossroads_18" },
                new LocationImport { Name = "Bench-Pilgrim's_Start", SceneName = "Crossroads_11_alt" },
                new LocationImport { Name = "Bench-Canyon_Depths", SceneName = "Fungus3_26" },
                new LocationImport { Name = "Bench-Canyon's_End", SceneName = "Fungus3_03" },
                new LocationImport { Name = "Bench-Overgrown_Atrium", SceneName = "Fungus3_44" },
                new LocationImport { Name = "Bench-Overgrown_Mound", SceneName = "Room_Fungus_Shaman" },
                new LocationImport { Name = "Bench-Millibelle", SceneName = "Fungus3_35" },
                new LocationImport { Name = "Bench-Fungal_Core", SceneName = "Fungus2_29" },
                new LocationImport { Name = "Bench-Fungal_Tower", SceneName = "Fungus2_06" },
                new LocationImport { Name = "Bench-Cloth's_Ambush", SceneName = "Fungus2_09" },
                new LocationImport { Name = "Bench-Pilgrim's_End", SceneName = "Fungus2_10" },
                new LocationImport { Name = "Bench-Mantis_Hub", SceneName = "Fungus2_15" },
                new LocationImport { Name = "Bench-Prophet's_Gate", SceneName = "Deepnest_01" },
                new LocationImport { Name = "Bench-City_Entrance", SceneName = "Ruins1_01" },
                new LocationImport { Name = "Bench-Inner_Sanctum", SceneName = "Ruins1_30" },
                new LocationImport { Name = "Bench-Outer_Sanctum", SceneName = "Ruins1_23" },
                new LocationImport { Name = "Bench-City_Fountain", SceneName = "Ruins1_27" },
                new LocationImport { Name = "Bench-Nailsmith", SceneName = "Ruins1_04" },
                new LocationImport { Name = "Bench-Flooded_Stag", SceneName = "Ruins2_07" },
                new LocationImport { Name = "Bench-Zote's_Skyway", SceneName = "Ruins1_06" },
                new LocationImport { Name = "Bench-Watcher's_Skyway", SceneName = "Ruins1_18" },
                new LocationImport { Name = "Bench-Tower_of_Love", SceneName = "Ruins2_11_b" },
                new LocationImport { Name = "Bench-Hive_Hideaway", SceneName = "Hive_03_c" },
                new LocationImport { Name = "Bench-Pure_Altar", SceneName = "Deepnest_East_14b" },
                new LocationImport { Name = "Bench-Lurker's_Overlook", SceneName = "GG_Lurker" },
                new LocationImport { Name = "Bench-Edge_Summit", SceneName = "Deepnest_East_07" },
                new LocationImport { Name = "Bench-Bardoon", SceneName = "Deepnest_East_04" },
                new LocationImport { Name = "Bench-Bardoon's_Tail", SceneName = "Deepnest_East_04" },
                new LocationImport { Name = "Bench-West_Lake_Shore", SceneName = "Crossroads_50" },
                new LocationImport { Name = "Bench-East_Lake_Shore", SceneName = "Crossroads_50" },
                new LocationImport { Name = "Bench-Spirits'_Glade", SceneName = "RestingGrounds_08" },
                new LocationImport { Name = "Bench-Crypts", SceneName = "RestingGrounds_10" },
                new LocationImport { Name = "Bench-Nosk's_Lair", SceneName = "Deepnest_32" },
                new LocationImport { Name = "Bench-Weaver's_Den", SceneName = "Deepnest_45_v02" },
                new LocationImport { Name = "Bench-Distant_Reservoir", SceneName = "Deepnest_10" },
                new LocationImport { Name = "Bench-Deepnest_Gate", SceneName = "Fungus2_25" },
                new LocationImport { Name = "Bench-Distant_Stag", SceneName = "Deepnest_09" },
                new LocationImport { Name = "Bench-Deepnest_Maze", SceneName = "Deepnest_30" },
                new LocationImport { Name = "Bench-Abyss_Workshop", SceneName = "Abyss_09" },
                new LocationImport { Name = "Bench-Far_Basin", SceneName = "Abyss_19" },
                new LocationImport { Name = "Bench-Basin_Hub", SceneName = "Abyss_04" },
                new LocationImport { Name = "Bench-Palace_Grounds", SceneName = "Abyss_05" },
                new LocationImport { Name = "Bench-Traitor's_Grave", SceneName = "Fungus3_49" },
                new LocationImport { Name = "Bench-Fort_Loodle", SceneName = "Fungus1_23" },
                new LocationImport { Name = "Bench-Far_Gardens", SceneName = "Fungus3_22" },
                new LocationImport { Name = "Bench-Dark_Gardens", SceneName = "Deepnest_43" },
                new LocationImport { Name = "Bench-Gardens_Atrium", SceneName = "Fungus3_04" },
                new LocationImport { Name = "Bench-Peak_Entrance", SceneName = "Mines_01" },
                new LocationImport { Name = "Bench-Crown_Ascent", SceneName = "Mines_34" },
                new LocationImport { Name = "Bench-Crystallized_Mound", SceneName = "Mines_35" },
                new LocationImport { Name = "Bench-Crusher_Refuge", SceneName = "Mines_19" },
                new LocationImport { Name = "Bench-Western_Peak", SceneName = "Mines_30" },
                new LocationImport { Name = "Bench-Quirrel_Peak", SceneName = "Mines_13" },
                new LocationImport { Name = "Bench-Peak_Ravine", SceneName = "Mines_28" },
                new LocationImport { Name = "Bench-Unn's_Chamber", SceneName = "Fungus1_Slug" },
                new LocationImport { Name = "Bench-Gulka_Gulley", SceneName = "Fungus1_12" },
                new LocationImport { Name = "Bench-Hunter's_Hideout", SceneName = "Fungus1_08" },
                new LocationImport { Name = "Bench-Duranda's_Trial", SceneName = "Fungus1_09" },
                new LocationImport { Name = "Bench-Greenpath_Entrance", SceneName = "Fungus1_01" },
                new LocationImport { Name = "Bench-Defender's_Repose", SceneName = "Waterways_15" },
                new LocationImport { Name = "Bench-Hermit's_Approach", SceneName = "Room_GG_Shortcut" },
                new LocationImport { Name = "Bench-Waterways_Entrance", SceneName = "Waterways_01" },
                new LocationImport { Name = "Bench-Isma's_Grove", SceneName = "Waterways_13" },
                new LocationImport { Name = "Bench-Acid_Sluice_East", SceneName = "Waterways_07" },
                new LocationImport { Name = "Bench-Acid_Sluice_West", SceneName = "Waterways_06" },
                new LocationImport { Name = "Bench-Fort_Flukefey", SceneName = "Waterways_08" },
                new LocationImport { Name = "Bench-Destroyed_Tram", SceneName = "Deepnest_26b" },
                new LocationImport { Name = "Bench-Kingsmould_Duelist", SceneName = "White_Palace_02" },
                new LocationImport { Name = "Bench-Palace_West", SceneName = "White_Palace_14" },
                new LocationImport { Name = "Bench-Sawblade_Choir", SceneName = "White_Palace_04" },
                new LocationImport { Name = "Bench-Palace_East", SceneName = "White_Palace_16" },
                new LocationImport { Name = "Bench-Thorny_Respite", SceneName = "White_Palace_05" },
                new LocationImport { Name = "Bench-Palace_Workshop", SceneName = "White_Palace_08" },
                new LocationImport { Name = "Bench-Throne_Approach", SceneName = "White_Palace_13" },
                new LocationImport { Name = "Bench-Path_Midpoint", SceneName = "White_Palace_17" },
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
            if (!File.Exists(Constants.ReferenceItemsFilePath))
            {
                return;
            }

            Items = DeserializeFile<Dictionary<string, Item>>(Constants.ReferenceItemsFilePath).Values.ToList();

            AddCustomItems();
            SetupItemPreviews();
        }

        private const string MrMushroomPool = "MrMushroom";
        private const string SkillUpgradePool = "Skill Upgrade";
        private const string LeverPool = "Lever";
        private const string TranscendencePool = "Charm - Transcendence";
        private const string EggPool = "Egg";
        private const string SkillPool = "Skill";
        private const string BenchPool = "Bench";

        private void AddCustomItems()
        {
            var itemsWithPools = new List<Item> {
                new Item { Name = "Kingsoul", Pool = "Charm" },
                new Item { Name = "Grimmchild", Pool = "Charm" },
            }
            .Concat(GetMrMushroomItems())
            .Concat(GetSkillUpgrades())
            .Concat(GetFungalCityGateKey())
            .Concat(GetLevers())
            .Concat(GetTranscendenceCharms())
            .Concat(GetRainbowEggs())
            .Concat(GetNailUpgrades())
            .Concat(GetBenches())
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
                new Item { Name = "WingsGlide", Pool = SkillUpgradePool },
            };

        private List<Item> GetFungalCityGateKey() =>
            new List<Item> { new Item { Name = "Fungal_City_Gate_Key", Pool = "Key" }, };

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

        private List<Item> GetBenches() =>
            new List<Item>
            {
                new Item { Name = "Bench-Dirtmouth", Pool = BenchPool },
                new Item { Name = "Bench-Mato" , Pool = BenchPool },
                new Item { Name = "Bench-Crossroads_Hot_Springs", Pool = BenchPool },
                new Item { Name = "Bench-Crossroads_Stag", Pool = BenchPool },
                new Item { Name = "Bench-Salubra", Pool = BenchPool },
                new Item { Name = "Bench-Ancestral_Mound", Pool = BenchPool },
                new Item { Name = "Bench-Black_Egg_Temple", Pool = BenchPool },
                new Item { Name = "Bench-Waterfall", Pool = BenchPool },
                new Item { Name = "Bench-Stone_Sanctuary", Pool = BenchPool },
                new Item { Name = "Bench-Greenpath_Toll", Pool = BenchPool },
                new Item { Name = "Bench-Greenpath_Stag", Pool = BenchPool },
                new Item { Name = "Bench-Lake_of_Unn", Pool = BenchPool },
                new Item { Name = "Bench-Sheo", Pool = BenchPool },
                new Item { Name = "Bench-Archives", Pool = BenchPool },
                new Item { Name = "Bench-Queen's_Station", Pool = BenchPool },
                new Item { Name = "Bench-Leg_Eater", Pool = BenchPool },
                new Item { Name = "Bench-Bretta", Pool = BenchPool },
                new Item { Name = "Bench-Mantis_Village", Pool = BenchPool },
                new Item { Name = "Bench-Quirrel", Pool = BenchPool },
                new Item { Name = "Bench-City_Toll", Pool = BenchPool },
                new Item { Name = "Bench-City_Storerooms", Pool = BenchPool },
                new Item { Name = "Bench-Watcher's_Spire", Pool = BenchPool },
                new Item { Name = "Bench-King's_Station", Pool = BenchPool },
                new Item { Name = "Bench-Pleasure_House", Pool = BenchPool },
                new Item { Name = "Bench-Waterways", Pool = BenchPool },
                new Item { Name = "Bench-Godhome_Atrium", Pool = BenchPool },
                new Item { Name = "Bench-Godhome_Roof", Pool = BenchPool },
                new Item { Name = "Bench-Hall_of_Gods", Pool = BenchPool },
                new Item { Name = "Bench-Deepnest_Hot_Springs", Pool = BenchPool },
                new Item { Name = "Bench-Failed_Tramway", Pool = BenchPool },
                new Item { Name = "Bench-Beast's_Den", Pool = BenchPool },
                new Item { Name = "Bench-Basin_Toll", Pool = BenchPool },
                new Item { Name = "Bench-Hidden_Station", Pool = BenchPool },
                new Item { Name = "Bench-Oro", Pool = BenchPool },
                new Item { Name = "Bench-Camp", Pool = BenchPool },
                new Item { Name = "Bench-Colosseum", Pool = BenchPool },
                new Item { Name = "Bench-Hive", Pool = BenchPool },
                new Item { Name = "Bench-Peak_Dark_Room", Pool = BenchPool },
                new Item { Name = "Bench-Crystal_Guardian", Pool = BenchPool },
                new Item { Name = "Bench-Grounds_Stag", Pool = BenchPool },
                new Item { Name = "Bench-Grey_Mourner", Pool = BenchPool },
                new Item { Name = "Bench-Gardens_Cornifer", Pool = BenchPool },
                new Item { Name = "Bench-Gardens_Toll", Pool = BenchPool },
                new Item { Name = "Bench-Gardens_Stag", Pool = BenchPool },
                new Item { Name = "Bench-Palace_Entrance", Pool = BenchPool },
                new Item { Name = "Bench-Palace_Atrium", Pool = BenchPool },
                new Item { Name = "Bench-Palace_Balcony", Pool = BenchPool },
                new Item { Name = "Bench-Upper_Tram", Pool = BenchPool },
                new Item { Name = "Bench-Lower_Tram", Pool = BenchPool },
                new Item { Name = "Bench-Stag_Nest", Pool = BenchPool },
                new Item { Name = "Bench-Cliffs_Overhang", Pool = BenchPool },
                new Item { Name = "Bench-Joni's_Repose", Pool = BenchPool },
                new Item { Name = "Bench-Nightmare_Lantern", Pool = BenchPool },
                new Item { Name = "Bench-Blasted_Plains", Pool = BenchPool },
                new Item { Name = "Bench-Baldur_Cavern", Pool = BenchPool },
                new Item { Name = "Bench-Crossroads_Center", Pool = BenchPool },
                new Item { Name = "Bench-Myla", Pool = BenchPool },
                new Item { Name = "Bench-Grubfather", Pool = BenchPool },
                new Item { Name = "Bench-Crossroads_Elevator", Pool = BenchPool },
                new Item { Name = "Bench-Fungal_Road", Pool = BenchPool },
                new Item { Name = "Bench-Pilgrim's_Start", Pool = BenchPool },
                new Item { Name = "Bench-Canyon_Depths", Pool = BenchPool },
                new Item { Name = "Bench-Canyon's_End", Pool = BenchPool },
                new Item { Name = "Bench-Overgrown_Atrium", Pool = BenchPool },
                new Item { Name = "Bench-Overgrown_Mound", Pool = BenchPool },
                new Item { Name = "Bench-Millibelle", Pool = BenchPool },
                new Item { Name = "Bench-Fungal_Core", Pool = BenchPool },
                new Item { Name = "Bench-Fungal_Tower", Pool = BenchPool },
                new Item { Name = "Bench-Cloth's_Ambush", Pool = BenchPool },
                new Item { Name = "Bench-Pilgrim's_End", Pool = BenchPool },
                new Item { Name = "Bench-Mantis_Hub", Pool = BenchPool },
                new Item { Name = "Bench-Prophet's_Gate", Pool = BenchPool },
                new Item { Name = "Bench-City_Entrance", Pool = BenchPool },
                new Item { Name = "Bench-Inner_Sanctum", Pool = BenchPool },
                new Item { Name = "Bench-Outer_Sanctum", Pool = BenchPool },
                new Item { Name = "Bench-City_Fountain", Pool = BenchPool },
                new Item { Name = "Bench-Nailsmith", Pool = BenchPool },
                new Item { Name = "Bench-Flooded_Stag", Pool = BenchPool },
                new Item { Name = "Bench-Zote's_Skyway", Pool = BenchPool },
                new Item { Name = "Bench-Watcher's_Skyway", Pool = BenchPool },
                new Item { Name = "Bench-Tower_of_Love", Pool = BenchPool },
                new Item { Name = "Bench-Hive_Hideaway", Pool = BenchPool },
                new Item { Name = "Bench-Pure_Altar", Pool = BenchPool },
                new Item { Name = "Bench-Lurker's_Overlook", Pool = BenchPool },
                new Item { Name = "Bench-Edge_Summit", Pool = BenchPool },
                new Item { Name = "Bench-Bardoon", Pool = BenchPool },
                new Item { Name = "Bench-Bardoon's_Tail", Pool = BenchPool },
                new Item { Name = "Bench-West_Lake_Shore", Pool = BenchPool },
                new Item { Name = "Bench-East_Lake_Shore", Pool = BenchPool },
                new Item { Name = "Bench-Spirits'_Glade", Pool = BenchPool },
                new Item { Name = "Bench-Crypts", Pool = BenchPool },
                new Item { Name = "Bench-Nosk's_Lair", Pool = BenchPool },
                new Item { Name = "Bench-Weaver's_Den", Pool = BenchPool },
                new Item { Name = "Bench-Distant_Reservoir", Pool = BenchPool },
                new Item { Name = "Bench-Deepnest_Gate", Pool = BenchPool },
                new Item { Name = "Bench-Distant_Stag", Pool = BenchPool },
                new Item { Name = "Bench-Deepnest_Maze", Pool = BenchPool },
                new Item { Name = "Bench-Abyss_Workshop", Pool = BenchPool },
                new Item { Name = "Bench-Far_Basin", Pool = BenchPool },
                new Item { Name = "Bench-Basin_Hub", Pool = BenchPool },
                new Item { Name = "Bench-Palace_Grounds", Pool = BenchPool },
                new Item { Name = "Bench-Traitor's_Grave", Pool = BenchPool },
                new Item { Name = "Bench-Fort_Loodle", Pool = BenchPool },
                new Item { Name = "Bench-Far_Gardens", Pool = BenchPool },
                new Item { Name = "Bench-Dark_Gardens", Pool = BenchPool },
                new Item { Name = "Bench-Gardens_Atrium", Pool = BenchPool },
                new Item { Name = "Bench-Peak_Entrance", Pool = BenchPool },
                new Item { Name = "Bench-Crown_Ascent", Pool = BenchPool },
                new Item { Name = "Bench-Crystallized_Mound", Pool = BenchPool },
                new Item { Name = "Bench-Crusher_Refuge", Pool = BenchPool },
                new Item { Name = "Bench-Western_Peak", Pool = BenchPool },
                new Item { Name = "Bench-Quirrel_Peak", Pool = BenchPool },
                new Item { Name = "Bench-Peak_Ravine", Pool = BenchPool },
                new Item { Name = "Bench-Unn's_Chamber", Pool = BenchPool },
                new Item { Name = "Bench-Gulka_Gulley", Pool = BenchPool },
                new Item { Name = "Bench-Hunter's_Hideout", Pool = BenchPool },
                new Item { Name = "Bench-Duranda's_Trial", Pool = BenchPool },
                new Item { Name = "Bench-Greenpath_Entrance", Pool = BenchPool },
                new Item { Name = "Bench-Defender's_Repose", Pool = BenchPool },
                new Item { Name = "Bench-Hermit's_Approach", Pool = BenchPool },
                new Item { Name = "Bench-Waterways_Entrance", Pool = BenchPool },
                new Item { Name = "Bench-Isma's_Grove", Pool = BenchPool },
                new Item { Name = "Bench-Acid_Sluice_East", Pool = BenchPool },
                new Item { Name = "Bench-Acid_Sluice_West", Pool = BenchPool },
                new Item { Name = "Bench-Fort_Flukefey", Pool = BenchPool },
                new Item { Name = "Bench-Destroyed_Tram", Pool = BenchPool },
                new Item { Name = "Bench-Kingsmould_Duelist", Pool = BenchPool },
                new Item { Name = "Bench-Palace_West", Pool = BenchPool },
                new Item { Name = "Bench-Sawblade_Choir", Pool = BenchPool },
                new Item { Name = "Bench-Palace_East", Pool = BenchPool },
                new Item { Name = "Bench-Thorny_Respite", Pool = BenchPool },
                new Item { Name = "Bench-Palace_Workshop", Pool = BenchPool },
                new Item { Name = "Bench-Throne_Approach", Pool = BenchPool },
                new Item { Name = "Bench-Path_Midpoint", Pool = BenchPool },
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
                BenchPool => GetBenchPreviewName(item.Name),
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

        private string GetBenchPreviewName(string benchName) =>
            benchName switch
            {
                // https://github.com/homothetyhk/BenchRando/blob/master/BenchRando/Resources/language.json
                "Bench-Dirtmouth" => "Dirtmouth Bench",
                "Bench-Mato" => "Mato Bench",
                "Bench-Crossroads_Hot_Springs" => "Crossroads Hot Springs Bench",
                "Bench-Crossroads_Stag" => "Crossroads Stag Bench",
                "Bench-Salubra" => "Salubra Bench",
                "Bench-Ancestral_Mound" => "Ancestral Mound Bench",
                "Bench-Black_Egg_Temple" => "Black Egg Temple Bench",
                "Bench-Waterfall" => "Waterfall Bench",
                "Bench-Stone_Sanctuary" => "Stone Sanctuary Bench",
                "Bench-Greenpath_Toll" => "Greenpath Toll Bench",
                "Bench-Greenpath_Stag" => "Greenpath Stag Bench",
                "Bench-Lake_of_Unn" => "Lake of Unn Bench",
                "Bench-Sheo" => "Sheo Bench",
                "Bench-Archives" => "Archives Bench",
                "Bench-Queen's_Station" => "Queen's Station Bench",
                "Bench-Leg_Eater" => "Leg Eater Bench",
                "Bench-Bretta" => "Bretta Bench",
                "Bench-Mantis_Village" => "Mantis Village Bench",
                "Bench-Quirrel" => "City Quirrel Bench",
                "Bench-City_Toll" => "City Toll Bench",
                "Bench-City_Storerooms" => "City Storerooms Bench",
                "Bench-Watcher's_Spire" => "Watcher's Spire Bench",
                "Bench-King's_Station" => "King's Station Bench",
                "Bench-Pleasure_House" => "Pleasure House Bench",
                "Bench-Waterways" => "Waterways Bench",
                "Bench-Godhome_Atrium" => "Godhome Atrium Bench",
                "Bench-Godhome_Roof" => "Godhome Roof Bench",
                "Bench-Hall_of_Gods" => "Hall of Gods Bench",
                "Bench-Deepnest_Hot_Springs" => "Deepnest Hot Springs Bench",
                "Bench-Failed_Tramway" => "Failed Tramway Bench",
                "Bench-Beast's_Den" => "Beast's Den Bench",
                "Bench-Basin_Toll" => "Basin Toll Bench",
                "Bench-Hidden_Station" => "Hidden Station Bench",
                "Bench-Oro" => "Oro Bench",
                "Bench-Camp" => "Camp Bench",
                "Bench-Colosseum" => "Colosseum Bench",
                "Bench-Hive" => "Hive Bench",
                "Bench-Peak_Dark_Room" => "Dark Room Bench",
                "Bench-Crystal_Guardian" => "Crystal Guardian Bench",
                "Bench-Grounds_Stag" => "Grounds Stag Bench",
                "Bench-Grey_Mourner" => "Grey Mourner Bench",
                "Bench-Gardens_Cornifer" => "Gardens Cornifer Bench",
                "Bench-Gardens_Toll" => "Gardens Toll Bench",
                "Bench-Gardens_Stag" => "Gardens Stag Bench",
                "Bench-Palace_Entrance" => "Palace Entrance Bench",
                "Bench-Palace_Atrium" => "Palace Atrium Bench",
                "Bench-Palace_Balcony" => "Palace Balcony Bench",
                "Bench-Upper_Tram" => "Upper Tram Bench",
                "Bench-Lower_Tram" => "Lower Tram Bench",
                "Bench-Stag_Nest" => "Stag Nest Bench",
                "Bench-Cliffs_Overhang" => "Cliff Overhang Bench",
                "Bench-Joni's_Repose" => "Joni's Repose Bench",
                "Bench-Nightmare_Lantern" => "Nightmare Lantern Bench",
                "Bench-Blasted_Plains" => "Blasted Plains Bench",
                "Bench-Baldur_Cavern" => "Baldur Cavern Bench",
                "Bench-Crossroads_Center" => "Crossroads Center Bench",
                "Bench-Myla" => "Myla Bench",
                "Bench-Grubfather" => "Grubfather Bench",
                "Bench-Crossroads_Elevator" => "Crossroads Elevator Bench",
                "Bench-Fungal_Road" => "Fungal Road Bench",
                "Bench-Pilgrim's_Start" => "Pilgrim's Start Bench",
                "Bench-Canyon_Depths" => "Canyon Depths Bench",
                "Bench-Canyon's_End" => "Canyon's End Bench",
                "Bench-Overgrown_Atrium" => "Overgrown Atrium Bench",
                "Bench-Overgrown_Mound" => "Overgrown Mound Bench",
                "Bench-Millibelle" => "Millibelle Bench",
                "Bench-Fungal_Core" => "Fungal Core Bench",
                "Bench-Fungal_Tower" => "Fungal Tower Bench",
                "Bench-Cloth's_Ambush" => "Cloth's Ambush Bench",
                "Bench-Pilgrim's_End" => "Pilgrim's End Bench",
                "Bench-Mantis_Hub" => "Mantis Hub Bench",
                "Bench-Prophet's_Gate" => "Prophet's Gate Bench",
                "Bench-City_Entrance" => "City Entrance Bench",
                "Bench-Inner_Sanctum" => "Inner Sanctum Bench",
                "Bench-Outer_Sanctum" => "Outer Sanctum Bench",
                "Bench-City_Fountain" => "City Fountain Bench",
                "Bench-Nailsmith" => "Nailsmith Bench",
                "Bench-Flooded_Stag" => "Flooded Stag Bench",
                "Bench-Zote's_Skyway" => "Zote's Skyway Bench",
                "Bench-Watcher's_Skyway" => "Watcher's Skyway Bench",
                "Bench-Tower_of_Love" => "Tower of Love Bench",
                "Bench-Hive_Hideaway" => "Hive Hideaway Bench",
                "Bench-Pure_Altar" => "Pure Altar Bench",
                "Bench-Lurker's_Overlook" => "Lurker's Overlook Bench",
                "Bench-Edge_Summit" => "Edge Summit Bench",
                "Bench-Bardoon" => "Bardoon Bench",
                "Bench-Bardoon's_Tail" => "Bardoon's Tail Bench",
                "Bench-West_Lake_Shore" => "West Lake Shore Bench",
                "Bench-East_Lake_Shore" => "East Lake Shore Bench",
                "Bench-Spirits'_Glade" => "Spirits' Glade Bench",
                "Bench-Crypts" => "Crypts Bench",
                "Bench-Nosk's_Lair" => "Nosk's Lair Bench",
                "Bench-Weaver's_Den" => "Weaver's Den Bench",
                "Bench-Distant_Reservoir" => "Distant Reservoir Bench",
                "Bench-Deepnest_Gate" => "Deepnest Gate Bench",
                "Bench-Distant_Stag" => "Distant Stag Bench",
                "Bench-Deepnest_Maze" => "Deepnest Maze Bench",
                "Bench-Abyss_Workshop" => "Abyss Workshop Bench",
                "Bench-Far_Basin" => "Far Basin Bench",
                "Bench-Basin_Hub" => "Basin Hub Bench",
                "Bench-Palace_Grounds" => "Palace Grounds Bench",
                "Bench-Traitor's_Grave" => "Traitor's Grave Bench",
                "Bench-Fort_Loodle" => "Fort Loodle Bench",
                "Bench-Far_Gardens" => "Far Gardens Bench",
                "Bench-Dark_Gardens" => "Dark Gardens Bench",
                "Bench-Gardens_Atrium" => "Gardens Atrium Bench",
                "Bench-Peak_Entrance" => "Peak Entrance Bench",
                "Bench-Crown_Ascent" => "Crown Ascent Bench",
                "Bench-Crystallized_Mound" => "Crystal Mound Bench",
                "Bench-Crusher_Refuge" => "Crusher Refuge Bench",
                "Bench-Western_Peak" => "Western Peak Bench",
                "Bench-Quirrel_Peak" => "Quirrel Peak Bench",
                "Bench-Peak_Ravine" => "Peak Ravine Bench",
                "Bench-Unn's_Chamber" => "Unn's Chamber Bench",
                "Bench-Gulka_Gulley" => "Gulka Gulley Bench",
                "Bench-Hunter's_Hideout" => "Hunter's Hideout Bench",
                "Bench-Duranda's_Trial" => "Duranda's Trial Bench",
                "Bench-Greenpath_Entrance" => "Greenpath Entrance Bench",
                "Bench-Defender's_Repose" => "Defender's Repose Bench",
                "Bench-Hermit's_Approach" => "Hermit's Approach Bench",
                "Bench-Waterways_Entrance" => "Waterways Entrance Bench",
                "Bench-Isma's_Grove" => "Isma's Grove Bench",
                "Bench-Acid_Sluice_East" => "Acid Sluice East Bench",
                "Bench-Acid_Sluice_West" => "Acid Sluice West Bench",
                "Bench-Fort_Flukefey" => "Fort Flukefey Bench",
                "Bench-Destroyed_Tram" => "Destroyed Tram Bench",
                "Bench-Kingsmould_Duelist" => "Kingsmould Duelist Bench",
                "Bench-Palace_West" => "Palace West Bench",
                "Bench-Sawblade_Choir" => "Sawblade Choir Bench",
                "Bench-Palace_East" => "Palace East Bench",
                "Bench-Thorny_Respite" => "Thorny Respite Bench",
                "Bench-Palace_Workshop" => "Palace Workshop Bench",
                "Bench-Throne_Approach" => "Throne Approach Bench",
                "Bench-Path_Midpoint" => "Path Midpoint Bench",
                _ => benchName,
            };

        private void LoadTransitions()
        {
            if (!File.Exists(Constants.ReferenceTransitionsFilePath) || !File.Exists(Constants.ReferenceRoomsFilePath))
            {
                return;
            }

            var rooms = DeserializeFile<Dictionary<string, RoomImport>>(Constants.ReferenceRoomsFilePath).Values.ToList();
            var transitions = DeserializeFile<Dictionary<string, TransitionImport>>(Constants.ReferenceTransitionsFilePath).Values.ToList();

            Transitions = new List<Transition>(transitions.Select(x =>
            {
                var roomDetails = rooms.FirstOrDefault(y => y.SceneName == x.SceneName) ?? new RoomImport { SceneName = x.SceneName, MapArea = "undefined", TitledArea = "undefined" };
                return new Transition { SceneName = x.SceneName, DoorName = x.DoorName, TitledArea = roomDetails.TitledArea, MapArea = roomDetails.MapArea };
            }));
        }

        public Dictionary<string, LocationWithTime> GetHelperLogLocations() =>
            GetDictionaryDataFromFileOrDefault<LocationWithTime>(Constants.HelperLogLocationsFilename);

        public Dictionary<string, TransitionWithTime> GetHelperLogTransitions() =>
            GetDictionaryDataFromFileOrDefault<TransitionWithTime>(Constants.HelperLogTransitionsFilename);

        public static Dictionary<string, T> GetDictionaryDataFromFileOrDefault<T>(string filename) =>
            File.Exists(filename)
                ? DeserializeFile<Dictionary<string, T>>(filename)
                : new Dictionary<string, T>();

        public Settings GetAppSettings(
            int helperGroupingsLength,
            int helperOrdersLength,
            int trackerItemGroupingsLength,
            int trackerItemOrdersLength,
            int trackerTransitionOrdersLength,
            int spoilerItemGroupingsLength,
            int spoilerItemOrdersLength,
            int spoilerTransitionOrders
        )
        {
            var settings = GetUserDefinedAppSettings();
            settings.ResetOutOfRangeValues(
                helperGroupingsLength,
                helperOrdersLength,
                trackerItemGroupingsLength,
                trackerItemOrdersLength,
                trackerTransitionOrdersLength,
                spoilerItemGroupingsLength,
                spoilerItemOrdersLength,
                spoilerTransitionOrders);
            
            // Reset 
            settings.SetDefaultValues();
            return settings;
        }

        private Settings GetUserDefinedAppSettings()
        {
            if (File.Exists(Constants.AppSettingsFilename))
            {
                return DeserializeFile<Settings>(Constants.AppSettingsFilename);
            }

            #region Update _Settings.json to latest filename
            const string OldSettingsFile = "_Settings.json";
            if (File.Exists(OldSettingsFile))
            {
                var settings = DeserializeFile<Settings>(OldSettingsFile);

                SaveAppSettings(settings);
                File.Delete(OldSettingsFile);

                return settings;
            }
            #endregion

            return new Settings();
        }

        public string GetSeed() =>
            GetDictionaryDataFromFileOrDefault<string>(Constants.SeedFilename)
                .TryGetValue("Seed", out var value) 
                    ? value
                    : "";

        private static T DeserializeFile<T>(string filePath)
        {
            using var file = new StreamReader(filePath);
            return JsonConvert.DeserializeObject<T>(file.ReadToEnd());
        }

        public void SaveHelperLogLocations(Dictionary<string, LocationWithTime> helperLogLocations) => 
            WriteFile(Constants.HelperLogLocationsFilename, helperLogLocations);

        public void SaveHelperLogTransitions(Dictionary<string, TransitionWithTime> helperLogTransitions) => 
            WriteFile(Constants.HelperLogTransitionsFilename, helperLogTransitions);

        public void SaveAppSettings(Settings settings) => 
            WriteFile(Constants.AppSettingsFilename, settings);

        public void SaveSeed(string seed) =>
            WriteFile(Constants.SeedFilename, new Dictionary<string, string> { { "Seed", seed } });

        private static void WriteFile<T>(string filename, T data)
        {
            using StreamWriter file = File.CreateText(filename);
            new JsonSerializer { Formatting = Formatting.Indented }.Serialize(file, data);
        }
    }
}
