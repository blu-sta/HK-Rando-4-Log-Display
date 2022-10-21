﻿using System.Collections.Generic;

namespace HK_Rando_4_Log_Display.Reference
{
    public class SceneNameDictionary
    {
        private readonly Dictionary<string, string> _sceneNameDictionary = new()
        {
            // Dirtmouth
            { "Tutorial_01", "King's Pass" },
            { "Town", "Dirtmouth" },
            { "Room_shop", "Sly" },
            { "Room_Sly_Storeroom", "Sly Basement" },
            { "Room_Town_Stag_Station", "Dirtmouth Stag" },
            { "Room_Mapper", "Iselda" },
            { "Room_Bretta", "Bretta" },
            { "Room_Bretta_Basement", "Bretta's Basement" },
            { "Room_Ouiji", "Jiji" },
            { "Room_Jinn", "Jinn" },

            // Trams
            { "Room_Tram_RG", "Upper Tram" },
            { "Room_Tram", "Lower Tram" },
            
            // Crossroads
            { "Crossroads_01", "CR Well" },
            { "Crossroads_02", "CR Outside Temple" },
            { "Crossroads_03", "CR Outside Stag" },
            { "Crossroads_04", "CR Gruz Mother" },
            { "Crossroads_05", "CR Grub under Well" },
            { "Crossroads_06", "CR Outside Anc.Mound" },
            { "Crossroads_07", "CR Outside Grubfather" },
            { "Crossroads_08", "CR Aspid Arena" },
            { "Crossroads_09", "CR Brooding Mawlek" },
            { "Crossroads_10", "CR False Knight" },
            { "Crossroads_11_alt", "CR GP Baldur" },
            { "Crossroads_12", "CR Acid Grub Corridor" },
            { "Crossroads_13", "CR Goam Mask Shard" },
            { "Crossroads_14", "CR Outside Myla" },
            { "Crossroads_15", "CR Stag to Tram Corridor" },
            { "Crossroads_16", "CR Above Stag Lever" },
            { "Crossroads_18", "CR Fungal Entrance" },
            { "Crossroads_19", "CR Outside Gruz Mother" },
            { "Crossroads_21", "CR Outside False Knight" },
            { "Crossroads_22", "CR Glowing Womb" },
            { "Crossroads_25", "CR Mawlek Corridor" },
            { "Crossroads_27", "CR Outside Tram" },
            { "Crossroads_30", "CR Hot Spring" },
            { "Crossroads_31", "CR Spike Grub" },
            { "Crossroads_33", "CR Cornifer" },
            { "Crossroads_35", "CR Acid Grub" },
            { "Crossroads_36", "CR Outside Mawlek" },
            { "Crossroads_37", "CR Vessel Fragment" },
            { "Crossroads_38", "CR Grubfather" },
            { "Crossroads_39", "CR Corridor Right of Temple" },
            { "Crossroads_40", "CR Corridor Right of Grub" },
            { "Crossroads_42", "CR Goam Corridor" },
            { "Crossroads_43", "CR Elevator Corridor" },
            { "Crossroads_45", "CR Myla" },
            { "Crossroads_46", "CR Tram" },
            { "Crossroads_47", "CR Stag" },
            { "Crossroads_48", "CR Guarded Grub" },
            { "Crossroads_49", "CR East Elevator Top" },
            { "Crossroads_52", "CR Goam Journal" },
            { "Crossroads_ShamanTemple", "CR Ancestral Mound" },
            { "Mines_33", "CR Dark Toll" },
            { "Room_Mender_House", "CR Menderbug" },
            { "Room_Charm_Shop", "CR Salubra" },
            { "Room_ruinhouse", "CR Sly (lost)" },
            { "Room_temple", "CR BET Entrance" },
            
            // Greenpath
            { "Fungus1_01", "GP Entrance" },
            { "Fungus1_01b", "GP Waterfall" },
            { "Fungus1_02", "GP Hornet on Ledge" },
            { "Fungus1_03", "GP Storeroom" },
            { "Fungus1_04", "GP Hornet" },
            { "Fungus1_05", "GP Outside Thorns" },
            { "Fungus1_06", "GP Cornifer" },
            { "Fungus1_07", "GP Outside Hunter" },
            { "Fungus1_08", "GP Hunter" },
            { "Fungus1_09", "GP Sheo Gauntlet" },
            { "Fungus1_10", "GP Acid Bridge" },
            { "Fungus1_11", "GP Above Fog Canyon" },
            { "Fungus1_12", "GP Gulka Gulley" },
            { "Fungus1_13", "GP Whispering Root Acid" },
            { "Fungus1_14", "GP Thorns of Agony" },
            { "Fungus1_15", "GP Outside Sheo" },
            { "Fungus1_16_alt", "GP Stag" },
            { "Fungus1_17", "GP Charger Corridor" },
            { "Fungus1_19", "GP Obble Corridor" },
            { "Fungus1_20_v02", "GP Vengefly King" },
            { "Fungus1_21", "GP Outside Hornet" },
            { "Fungus1_22", "GP Outside Stag" },
            { "Fungus1_25", "GP Outside Stag" },
            { "Fungus1_26", "GP Lake of Unn" },
            { "Fungus1_29", "GP Massive Moss Charger" },
            { "Fungus1_30", "GP Below Toll Bench" },
            { "Fungus1_31", "GP Toll Bench" },
            { "Fungus1_32", "GP Moss Knight Arena" },
            { "Fungus1_34", "GP Stone Sanc. Entrance" },
            { "Fungus1_35", "GP Stone Sanctuary" },
            { "Fungus1_36", "GP Stone Sanc. Mask Shard" },
            { "Fungus1_37", "GP Stone Sanc. Bench" },
            { "Fungus1_Slug", "GP Unn Chamber" },
            { "Room_nailmaster_02", "GP Sheo" },
            { "Room_Slug_Shrine", "GP Unn Bench" },
            
            // Fungal Wastes
            { "Deepnest_01", "Fungal Deepnest Fall" },
            { "Fungus2_01", "Fungal Queen's Station" },
            { "Fungus2_02", "Fungal Queen's Stag" },
            { "Fungus2_03", "Fungal Queen's Sporgs" },
            { "Fungus2_04", "Fungal Below Shrumals" },
            { "Fungus2_05", "Fungal Shrumal Ogres" },
            { "Fungus2_06", "Fungal Outside Leg Eater" },
            { "Fungus2_07", "Fungal Shurmal Warrior Bridge" },
            { "Fungus2_08", "Fungal Outside Elder Hu" },
            { "Fungus2_09", "Fungal Cloth Corridor" },
            { "Fungus2_10", "Fungal Outside City Bridge" },
            { "Fungus2_11", "Fungal Epogo Room" },
            { "Fungus2_12", "Fungal Mantis Corridor" },
            { "Fungus2_13", "Fungal Bretta Bench" },
            { "Fungus2_14", "Fungal Mantis Village" },
            { "Fungus2_15", "Fungal Mantis Lords" },
            { "Fungus2_17", "Fungal Above Mantis Village" },
            { "Fungus2_18", "Fungal Cornifer" },
            { "Fungus2_19", "Fungal Fungling Acid" },
            { "Fungus2_20", "Fungal Spore Shroom" },
            { "Fungus2_21", "Fungal City Acid Bridge" },
            { "Fungus2_23", "Fungal Dashmaster" },
            { "Fungus2_26", "Fungal Leg Eater" },
            { "Fungus2_28", "Fungal Shrumal Warrior Loop" },
            { "Fungus2_29", "Fungal Upper Core" },
            { "Fungus2_30", "Fungal Lower Core" },
            { "Fungus2_31", "Fungal Mantis Rewards" },
            { "Fungus2_32", "Fungal Elder Hu" },
            { "Fungus2_33", "Fungal Whispering Root" },
            { "Fungus2_34", "Fungal Willoh" },
            
            // Fog Canyon
            { "Fungus3_01", "FogC Upper West Column" },
            { "Fungus3_02", "FogC Lower West Column" },
            { "Fungus3_03", "FogC QG Acid" },
            { "Fungus3_24", "FogC Autopilot Acid" },
            { "Fungus3_25", "FogC Cornifer" },
            { "Fungus3_25b", "FogC Corridor Right of Cornifer" },
            { "Fungus3_26", "FogC East Column" },
            { "Fungus3_27", "FogC Archive Corridor" },
            { "Fungus3_28", "FogC Charm Notch" },
            { "Fungus3_30", "FogC Autopilot Lifeblood" },
            { "Fungus3_35", "FogC Millibelle" },
            { "Fungus3_44", "FogC Outside Over.Mound" },
            { "Fungus3_47", "FogC Outside Archives" },
            { "Fungus3_archive", "FogC Archives Bench" },
            { "Fungus3_archive_02", "FogC Uumuu" },
            { "Room_Fungus_Shaman", "FogC Overgrown Mound" },
            
            // Howling Cliffs
            { "Cliffs_01", "HC Wastelands Edge" },
            { "Cliffs_02", "HC Gorb" },
            { "Cliffs_03", "HC Stag Nest" },
            { "Cliffs_04", "HC Outside Joni" },
            { "Cliffs_05", "HC Joni" },
            { "Cliffs_06", "HC Grimm Lantern" },
            { "Fungus1_28", "HC Baldurs" },
            { "Room_nailmaster", "HC Mato" },

            // Crystal Peak
            { "Mines_01", "CPeak Dive Entrance" },
            { "Mines_02", "CPeak Dark Entrance" },
            { "Mines_03", "CPeak Spike Tunnel Grub" },
            { "Mines_04", "CPeak Hunter Conveyors" },
            { "Mines_05", "CPeak Above Spike Grub" },
            { "Mines_06", "CPeak Deep Focus Gauntlet" },
            { "Mines_07", "CPeak Dark Corridor" },
            { "Mines_10", "CPeak Elevator Entrance" },
            { "Mines_11", "CPeak Shop Key" },
            { "Mines_13", "CPeak Quirrel Corridor" },
            { "Mines_16", "CPeak Mimic Room" },
            { "Mines_17", "CPeak Above Dark Bench" },
            { "Mines_18", "CPeak Crystal Guardian" },
            { "Mines_19", "CPeak Grub Crushers" },
            { "Mines_20", "CPeak East Column" },
            { "Mines_23", "CPeak Crown Root" },
            { "Mines_24", "CPeak Crown Grub" },
            { "Mines_25", "CPeak Crown Climb" },
            { "Mines_28", "CPeak Outside Crys.Mound" },
            { "Mines_29", "CPeak Dark Bench" },
            { "Mines_30", "CPeak Cornifer" },
            { "Mines_31", "CPeak Crystal Heart" },
            { "Mines_32", "CPeak Enraged Guardian" },
            { "Mines_34", "CPeak Crown Peak" },
            { "Mines_35", "CPeak Crystallised Mound" },
            { "Mines_36", "CPeak Deep Focus" },
            { "Mines_37", "CPeak Chest Crushers" },
            
            // Ancient Basin
            { "Abyss_03", "AB Tram" },
            { "Abyss_04", "AB Fountain" },
            { "Abyss_05", "AB Palace Kings Mould" },
            { "Abyss_17", "AB Cloth Pale Ore" },
            { "Abyss_18", "AB Toll Bench" },
            { "Abyss_19", "AB Broken Vessel" },
            { "Abyss_20", "AB Simple Key" },
            { "Abyss_21", "AB Monarch Wings" },
            { "Abyss_22", "AB Hidden Station" },
            
            // Abyss
            { "Abyss_06_Core", "Abyss Core" },
            { "Abyss_08", "Abyss Lifeblood Core" },
            { "Abyss_09", "Abyss Lighthouse Climb" },
            { "Abyss_10", "Abyss Shade Cloak" },
            { "Abyss_12", "Abyss Shriek" },
            { "Abyss_15", "Abyss Birthplace" },
            { "Abyss_16", "Abyss Lighthouse Corridor" },
            { "Abyss_Lighthouse_room", "Abyss Lighthouse Room" },

            // Resting Grounds
            { "Crossroads_46b", "RG Tram" },
            { "Crossroads_50", "RG Blue Lake" },
            { "RestingGrounds_02", "RG Xero" },
            { "RestingGrounds_04", "RG Dreamers" },
            { "RestingGrounds_05", "RG Whispering Root" },
            { "RestingGrounds_06", "RG Below Xero" },
            { "RestingGrounds_07", "RG Seer" },
            { "RestingGrounds_08", "RG Spirits' Glade" },
            { "RestingGrounds_09", "RG Stag" },
            { "RestingGrounds_10", "RG Crypts" },
            { "RestingGrounds_12", "RG Outside Grey Mourner" },
            { "RestingGrounds_17", "RG Dreamshield" },
            { "Room_Mansion", "RG Grey Mourner" },
            { "Ruins2_10", "RG West Elevator Top" },
            
            // Kingdom's Edge
            { "Abyss_03_c", "KE Tram" },
            { "Deepnest_East_01", "KE Left of Hive" },
            { "Deepnest_East_02", "KE Above Hive" },
            { "Deepnest_East_03", "KE Cornifer" },
            { "Deepnest_East_04", "KE Bardoon" },
            { "Deepnest_East_06", "KE Outside Oro" },
            { "Deepnest_East_07", "KE Whispering Root" },
            { "Deepnest_East_08", "KE Great Hopper Idol" },
            { "Deepnest_East_09", "KE Outside Colo" },
            { "Deepnest_East_10", "KE Markoth" },
            { "Deepnest_East_11", "KE Below Camp Bench" },
            { "Deepnest_East_12", "KE Outside Hornet" },
            { "Deepnest_East_13", "KE Camp Bench" },
            { "Deepnest_East_14", "KE Below Oro" },
            { "Deepnest_East_14b", "KE Quickslash" },
            { "Deepnest_East_15", "KE Bardoon Lifeblood" },
            { "Deepnest_East_16", "KE Oro Scarecrow" },
            { "Deepnest_East_17", "KE 420 Rock" },
            { "Deepnest_East_18", "KE Outside Markoth" },
            { "Deepnest_East_Hornet", "KE Hornet" },
            { "GG_Lurker", "KE Pale Lurker" },
            { "Room_Colosseum_01", "KE Colo Entrance" },
            { "Room_Colosseum_02", "KE Colo Bench" },
            { "Room_Colosseum_Spectate", "KE Colo Backdoor" },
            { "Room_Colosseum_Bronze", "KE Colo 1" },
            { "Room_Colosseum_Silver", "KE Colo 2" },
            { "Room_Colosseum_Gold", "KE Colo 3" },
            { "Room_nailmaster_03", "KE Oro" },
            { "Room_Wyrm", "KE Cast Off Shell" },

            // City of Tears
            { "Crossroads_49b", "City East Elevator Bottom" },
            { "Room_nailsmith", "City Nailsmith" },
            { "Ruins_Bathhouse", "City Pleasure House" },
            { "Ruins_Elevator", "City Pleasure Elevator" },
            { "Ruins_House_01", "City Guarded Grub" },
            { "Ruins_House_02", "City Gorgeous Husk" },
            { "Ruins_House_03", "City Emilitia" },
            { "Ruins1_01", "City Pilgrim's Way Entrance" },
            { "Ruins1_02", "City Quirrel Bench" },
            { "Ruins1_03", "City Rafters" },
            { "Ruins1_04", "City Outside Nailsmith" },
            { "Ruins1_05", "City Under City Toll Bench" },
            { "Ruins1_05b", "City Lemm" },
            { "Ruins1_05c", "City Above Lemm" },
            { "Ruins1_06", "City Storerooms Corridor" },
            { "Ruins1_09", "City Soul Twister Outside Sanctum" },
            { "Ruins1_17", "City Storerooms Platforms" },
            { "Ruins1_18", "City Spire Bridge" },
            { "Ruins1_23", "City Sanctum Entrance" },
            { "Ruins1_24", "City Soul Master" },
            { "Ruins1_25", "City Sanctum East Elevators" },
            { "Ruins1_27", "City HK Fountain" },
            { "Ruins1_28", "City Outside Storerooms Stag" },
            { "Ruins1_29", "City Storerooms Stag" },
            { "Ruins1_30", "City Sanctum Spell Twister" },
            { "Ruins1_31", "City Toll Bench" },
            { "Ruins1_31b", "City Shade Soul" },
            { "Ruins1_32", "City Soul Master Rewards" },
            { "Ruins2_01", "City Spire Great Husk" },
            { "Ruins2_01_b", "City Below Spire" },
            { "Ruins2_03", "City Spire Watchers" },
            { "Ruins2_03b", "City Spire Below Watchers" },
            { "Ruins2_04", "City Central Hub" },
            { "Ruins2_05", "City Above King's Station" },
            { "Ruins2_06", "City Outside King's Station" },
            { "Ruins2_07", "City Below Tower of Love" },
            { "Ruins2_08", "City King's Station" },
            { "Ruins2_09", "City King's Vessel Fragment" },
            { "Ruins2_10b", "City East Elevator Bottom" },
            { "Ruins2_11", "City Collector" },
            { "Ruins2_11_b", "City Tower of Love" },
            { "Ruins2_Watcher_Room", "City Lurien" },

            // Hive
            { "Hive_01", "Hive Entrance" },
            { "Hive_02", "Hive Whispering Root" },
            { "Hive_03", "Hive Grimm Nightmare Grub" },
            { "Hive_03_c", "Hive Shortcuts" },
            { "Hive_04", "Hive Mask Shard" },
            { "Hive_05", "Hive Knight" },

            // Royal Waterways
            { "Abyss_01", "WW Broken Elevator" },
            { "Abyss_02", "WW Basin Bridge" },
            { "GG_Pipeway", "WW Flukemungas" },
            { "GG_Waterways", "WW Junk Pit" },
            { "Room_GG_Shortcut", "WW Fluke Hermit" },
            { "Waterways_01", "WW City Entrance" },
            { "Waterways_02", "WW Bench" },
            { "Waterways_03", "WW Tuk" },
            { "Waterways_04", "WW West Grub" },
            { "Waterways_04b", "WW Mask Shard" },
            { "Waterways_05", "WW Dung Defender" },
            { "Waterways_06", "WW Broken Elevator Corridor" },
            { "Waterways_07", "WW Outside Isma's Grove" },
            { "Waterways_08", "WW Outside Flukemarm" },
            { "Waterways_09", "WW Cornifer Hwurmps" },
            { "Waterways_12", "WW Flukemarm" },
            { "Waterways_13", "WW Isma's Grove" },
            { "Waterways_14", "WW KE Acid Corridor" },
            { "Waterways_15", "WW Dung Defender Cave" },

            // Deepnest
            { "Abyss_03_b", "DN Tram" },
            { "Deepnest_01b", "DN Upper Cornifer" },
            { "Deepnest_02", "DN Outside Mimics" },
            { "Deepnest_03", "DN Left of Hot Spring" },
            { "Deepnest_09", "DN Distant Village Stag" },
            { "Deepnest_10", "DN Distant Village" },
            { "Deepnest_14", "DN Failed Tram Bench" },
            { "Deepnest_16", "DN Super Secret Seal" },
            { "Deepnest_17", "DN Garpedes under Cornifer" },
            { "Deepnest_26", "DN Failed Tramway Lifeblood" },
            { "Deepnest_26b", "DN Tram Pass" },
            { "Deepnest_30", "DN Hot Spring" },
            { "Deepnest_31", "DN Outside Nosk" },
            { "Deepnest_32", "DN Nosk" },
            { "Deepnest_33", "DN Zote Creepers" },
            { "Deepnest_34", "DN Darknest from Hot Spring" },
            { "Deepnest_35", "DN Darknest Outside Galien" },
            { "Deepnest_36", "DN Mimics" },
            { "Deepnest_37", "DN Tram Corridor" },
            { "Deepnest_38", "DN Vessel Fragment" },
            { "Deepnest_39", "DN Darknest Root" },
            { "Deepnest_40", "DN Galien" },
            { "Deepnest_41", "DN Midwife" },
            { "Deepnest_42", "DN Outside Mask Maker" },
            { "Deepnest_44", "DN Sharp Shadow" },
            { "Deepnest_45_v02", "DN Weaver's Den" },
            { "Deepnest_Spider_Town", "DN Beast's Den" },
            { "Fungus2_25", "DN Lower Cornifer" },
            { "Room_Mask_Maker", "DN Mask Maker" },
            { "Room_spider_small", "DN Brumm" },

            // Queen's Gardens
            { "Deepnest_43", "QG Thorns above Deepnest" },
            { "Fungus1_23", "QG Loodle Beast Corridor" },
            { "Fungus1_24", "QG Cornifer" },
            { "Fungus3_04", "QG Big East Room" },
            { "Fungus3_05", "QG Petra Arena" },
            { "Fungus3_08", "QG Lower Petra Corridor" },
            { "Fungus3_10", "QG Arena under Stag" },
            { "Fungus3_11", "QG Whispering Root" },
            { "Fungus3_13", "QG Outside Stag" },
            { "Fungus3_21", "QG Thorns to Traitor Lord" },
            { "Fungus3_22", "QG Upper Grub" },
            { "Fungus3_23", "QG Traitor Lord" },
            { "Fungus3_34", "QG Fog Canyon Entrance" },
            { "Fungus3_39", "QG Moss Prophet" },
            { "Fungus3_40", "QG Stag" },
            { "Fungus3_48", "QG Big West Room" },
            { "Fungus3_49", "QG Flower Grave" },
            { "Fungus3_50", "QG Toll Bench" },
            { "Room_Queen", "QG White Lady" },

            // White Palace
            { "White_Palace_01", "WP Entrance" },
            { "White_Palace_02", "WP Lever KingsMould" },
            { "White_Palace_03_hub", "WP Atrium" },
            { "White_Palace_04", "WP Atrium Left" },
            { "White_Palace_05", "WP Saw Thorn Corridor" },
            { "White_Palace_06", "WP Balcony" },
            { "White_Palace_07", "WP Above Balcony" },
            { "White_Palace_08", "WP Workshop" },
            { "White_Palace_09", "WP Throne" },
            { "White_Palace_11", "WP Outside KingsMould" },
            { "White_Palace_12", "WP Spike Drop" },
            { "White_Palace_13", "WP Thorn Climb" },
            { "White_Palace_14", "WP Left Orb" },
            { "White_Palace_15", "WP Right Orb" },
            { "White_Palace_16", "WP Saw Climb" },

            // Path of Pain
            { "White_Palace_17", "POP 2 Lever Room" },
            { "White_Palace_18", "POP 1 Entrance" },
            { "White_Palace_19", "POP 3 Climb" },
            { "White_Palace_20", "POP 4 Finale" },

            // Black Egg Temple
            { "Room_Final_Boss_Atrium", "Black Egg Temple" },
            { "Room_Final_Boss_Core", "Hollow Knight" },
            { "Dream_Final_Boss", "Radiance" },

            // Grimm
            { "Grimm_Divine", "Grimm Divine" },
            { "Grimm_Main_Tent", "Grimm Main Tent" },
            { "Grimm_Nightmare", "Grimm NKG" },

            // Dream
            { "Dream_01_False_Knight", "Dream Failed Champ" },
            { "Dream_02_Mage_Lord", "Dream Soul Tyrant" },
            { "Dream_03_Infected_Knight", "Dream Lost Kin" },
            { "Dream_04_White_Defender", "Dream White Defender" },
            { "Dream_Abyss", "Dream Abyss Climb" },
            { "Dream_Backer_Shrine", "Dream Outside Shrine" },
            { "Dream_Guardian_Hegemol", "Dream Herrah" },
            { "Dream_Guardian_Lurien", "Dream Lurien" },
            { "Dream_Guardian_Monomon", "Dream Monomon" },
            { "Dream_Mighty_Zote", "Dream Grey Prince Zote" },
            { "Dream_Nailcollection", "Dream Nail" },
            { "Dream_Room_Believer_Shrine", "Dream Shrine of Believers" },

            // Godhome
            { "GG_Atrium", "GG Pantheons" },
            { "GG_Atrium_Roof", "GG Patheon Roof" },
            { "GG_Workshop", "GG Hall of Gods" },
            { "GG_Blue_Room", "GG Lifeblood" },
            { "GG_Land_Of_Storms", "GG Land of Storms" },
            { "GG_Mighty_Zote", "GG Eternal Ordeal" },
            { "GG_Unlock_Wastes", "GG Godtuner" },
        };

        public string GetAltSceneName(string sceneName) =>
            _sceneNameDictionary.TryGetValue(sceneName, out var altSceneName)
                ? altSceneName
                : sceneName;
    }
}
