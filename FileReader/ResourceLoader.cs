﻿using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IResourceLoader
    {
        public List<ReferenceItem> ReferenceItems { get; }
        public List<ReferenceLocation> ReferenceLocations { get; }
        public List<ReferenceTransition> ReferenceTransitions { get; }
        public List<LocationImport> DeadLocations { get; }
        public List<TransitionImport> DeadTransitions { get; }
        public Dictionary<string, Location> GetHelperLogLocations();
        public Dictionary<string, Transition> GetHelperLogTransitions();
        public Dictionary<string, ItemWithLocation> GetTrackerLogItems();
        public Dictionary<string, TransitionWithDestination> GetTrackerLogTransitions();

        public AppSettings GetAppSettings();
        public string GetSeedGenerationCode();
        public void SaveHelperLogLocations(Dictionary<string, Location> helperLogLocations);
        public void SaveHelperLogTransitions(Dictionary<string, Transition> helperLogTransitions);
        public void SaveTrackerLogItems(Dictionary<string, ItemWithLocation> trackerLogItems);
        public void SaveTrackerLogTransitions(Dictionary<string, TransitionWithDestination> trackerLogTransitions);
        public void SaveAppSettings(AppSettings appSettings);
        public void SaveSeedGenerationCode(string seed);
    }

    public class ResourceLoader : IResourceLoader
    {
        public List<ReferenceItem> ReferenceItems { get; private set; } = new();
        public List<ReferenceLocation> ReferenceLocations { get; private set; } = new();
        public List<ReferenceTransition> ReferenceTransitions { get; private set; } = new();

        public List<LocationImport> DeadLocations { get; private set; } = new();
        public List<TransitionImport> DeadTransitions { get; private set; } = new();

        private const string MrMushroomPool = "Mr Mushroom";
        private const string SkillUpgradePool = "Skill Upgrade";
        private const string LeverPool = "Lever";
        private const string TranscendencePool = "Charm - Transcendence";
        private const string EggPool = "Egg";
        private const string SkillPool = "Skill";
        private const string BenchPool = "Bench";
        private const string MoreDoorsPool = "Key - MoreDoors";
        private const string LostArtifactPool = "Lost Artifact";
        private const string LoreMasterPool = "Lore Master";
        private const string BreakablePool = "Breakable Walls and Floors";
        private const string GrassPool = "Grass";
        private const string JournalEntryPool = "Journal_Entry";
        private const string HuntersNotesPool = "Hunter's_Notes";
        private const string MoreLocationsPool = "MoreLocations";
        private const string GhostPool = "DreamGhost";

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
                PreviewName = GetItemPreviewName(x),
                Pool = x.Pool
            })
                //flibber-hk
                .Concat(RandoPlusItemImport())
                .Concat(SkillUpgradeItemImport())
                .Concat(FungalCityGateKeyItemImport())
                .Concat(LeverItemImport())

                //dpinela
                .Concat(TranscendenceItemImport())
                .Concat(RainbowEggItemImport())
                .Concat(Colo3ItemImport())

                //homothetyhk
                .Concat(BenchItemImport())

                //BadMagic100
                .Concat(TRJRItemImport())
                .Concat(MoreLocationsItemImport())

                //dplochcoder
                .Concat(MoreDoorsItemImport())
                .Concat(DarknessItemImport())

                //Hoo-Knows
                .Concat(LostArtifactsItemImport())

                //Korzer420
                .Concat(LoreMasterItemImport(itemImports.Where(x => x.Pool == "Lore").ToList()))
                .Concat(ExtraRandoItemImport())

                //Bentechy66
                .Concat(BreakableWallsItemImport())

                //StormZillaa
                .Concat(GrassRandoItemImport())

                // unidentified items
                .Concat(UnidentifiedItems())

                .ToList();
        }

        private static string GetItemPreviewName(ItemImport x)
        {
            if (x.Name == "Lumafly_Escape")
                return "Nothing?";
            if (x.Name == "Dream_Gate")
                return "Dreamgate";
            if (x.Name.StartsWith("Journal_Entry-"))
                return string.Concat(x.Name.Replace("Journal_Entry-", "").Replace("-", " ").Replace("_", " "), " Journal Entry");

            return x.Name.Replace("-", " ").Replace("_", " ");
        }

        private static List<ItemImport> GetItemImportsFromFile() =>
            LoadDictionaryFileValues<ItemImport>(ReferenceItemsFilePath).Concat(new List<ItemImport>
            {
                new ItemImport { Name = "White_Fragment", Pool = "Charm" },
                new ItemImport { Name = "Kingsoul", Pool = "Charm" },
                new ItemImport { Name = "Grimmchild", Pool = "Charm" },
            }).ToList();

        private static List<ReferenceItem> RandoPlusItemImport() =>
            new()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new ReferenceItem { Name = "Mr_Mushroom_Level_Up", PreviewName = "Mr Mushroom Level Up", Pool = MrMushroomPool },
                new ReferenceItem { Name = "Nail_Upgrade", PreviewName = "Nail Upgrade", Pool = SkillPool },
                new ReferenceItem { Name = "Not_Isma's_Tear", PreviewName = "Not Isma's Tear", Pool = SkillPool },
                new ReferenceItem { Name = "Not_Lumafly_Lantern", PreviewName = "Not Lumafly Lantern", Pool = SkillPool },
                new ReferenceItem { Name = "Not_Swim", PreviewName = "Not Swim", Pool = SkillPool },
                
                new ReferenceItem { Name = "Ghost_Essence-Gravedigger", PreviewName = "Ghost_Essence-Gravedigger", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Vespa", PreviewName = "Ghost_Essence-Vespa", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Joni", PreviewName = "Ghost_Essence-Joni", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Marissa", PreviewName = "Ghost_Essence-Marissa", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Poggy_Thorax", PreviewName = "Ghost_Essence-Poggy_Thorax", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Caelif_Fera_Orthop", PreviewName = "Ghost_Essence-Caelif_Fera_Orthop", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Cloth", PreviewName = "Ghost_Essence-Cloth", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Revek", PreviewName = "Ghost_Essence-Revek", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Millybug", PreviewName = "Ghost_Essence-Millybug", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Caspian", PreviewName = "Ghost_Essence-Caspian", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Atra", PreviewName = "Ghost_Essence-Atra", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Dr_Chagax", PreviewName = "Ghost_Essence-Dr_Chagax", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Garro", PreviewName = "Ghost_Essence-Garro", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Kcin", PreviewName = "Ghost_Essence-Kcin", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Karina", PreviewName = "Ghost_Essence-Karina", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Hundred_Nail_Warrior", PreviewName = "Ghost_Essence-Hundred_Nail_Warrior", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Grohac", PreviewName = "Ghost_Essence-Grohac", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Perpetos_Noo", PreviewName = "Ghost_Essence-Perpetos_Noo", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Molten", PreviewName = "Ghost_Essence-Molten", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Magnus_Strong", PreviewName = "Ghost_Essence-Magnus_Strong", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Waldie", PreviewName = "Ghost_Essence-Waldie", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Wayner", PreviewName = "Ghost_Essence-Wayner", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Wyatt", PreviewName = "Ghost_Essence-Wyatt", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Hex", PreviewName = "Ghost_Essence-Hex", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Thistlewind", PreviewName = "Ghost_Essence-Thistlewind", Pool = GhostPool },
                new ReferenceItem { Name = "Ghost_Essence-Boss", PreviewName = "Ghost_Essence-Boss", Pool = GhostPool },
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
                new ReferenceItem { Name = "Fungal_City_Gate_Key", PreviewName = "Fungal City Gate Key", Pool = "Key" },
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
                new ReferenceItem { Name = "Marissa's_Audience", PreviewName = "Marissa's Audience", Pool = TranscendencePool },
                new ReferenceItem { Name = "Lemm's_Strength", PreviewName = "Lemm's Strength", Pool = TranscendencePool },
                new ReferenceItem { Name = "Snail_Slash", PreviewName = "Snail Slash", Pool = TranscendencePool },
                new ReferenceItem { Name = "Millibelle's_Blessing", PreviewName = "Millibelle's Blessing", Pool = TranscendencePool },
                new ReferenceItem { Name = "Disinfectant_Flask", PreviewName = "Disinfectant Flask", Pool = TranscendencePool },
                new ReferenceItem { Name = "Florist's_Blessing", PreviewName = "Florist's Blessing", Pool = TranscendencePool },
                new ReferenceItem { Name = "Greedsong", PreviewName = "Greedsong", Pool = TranscendencePool },
                new ReferenceItem { Name = "Snail_Soul", PreviewName = "Snail Soul", Pool = TranscendencePool },
                new ReferenceItem { Name = "Nitro_Crystal", PreviewName = "Nitro Crystal", Pool = TranscendencePool },
                new ReferenceItem { Name = "Shaman_Amp", PreviewName = "Shaman Amp", Pool = TranscendencePool },
                new ReferenceItem { Name = "Crystalmaster", PreviewName = "Crystalmaster", Pool = TranscendencePool },
                new ReferenceItem { Name = "Bluemoth_Wings", PreviewName = "Bluemoth Wings", Pool = TranscendencePool },
                new ReferenceItem { Name = "Chaos_Orb", PreviewName = "Chaos Orb", Pool = TranscendencePool },
                new ReferenceItem { Name = "Antigravity_Amulet", PreviewName = "Antigravity Amulet", Pool = TranscendencePool },
                new ReferenceItem { Name = "Vespa's_Vengeance", PreviewName = "Vespa's Vengeance", Pool = TranscendencePool },
            };

        private static List<ReferenceItem> RainbowEggItemImport() =>
            new()
            {
                // https://github.com/dpinela/RainbowEggs
                new ReferenceItem { Name = "Red_Egg", PreviewName = "Red Egg", Pool = EggPool },
                new ReferenceItem { Name = "Orange_Egg", PreviewName = "Orange Egg", Pool = EggPool },
                new ReferenceItem { Name = "Yellow_Egg", PreviewName = "Yellow Egg", Pool = EggPool },
                new ReferenceItem { Name = "Green_Egg", PreviewName = "Green Egg", Pool = EggPool },
                new ReferenceItem { Name = "Cyan_Egg", PreviewName = "Cyan Egg", Pool = EggPool },
                new ReferenceItem { Name = "Blue_Egg", PreviewName = "Blue Egg", Pool = EggPool },
                new ReferenceItem { Name = "Purple_Egg", PreviewName = "Purple Egg", Pool = EggPool },
                new ReferenceItem { Name = "Pink_Egg", PreviewName = "Pink Egg", Pool = EggPool },
                new ReferenceItem { Name = "Trans_Egg", PreviewName = "Trans Egg", Pool = EggPool },
                new ReferenceItem { Name = "Rainbow_Egg", PreviewName = "Rainbow Egg", Pool = EggPool },
                new ReferenceItem { Name = "Arcane_Eg", PreviewName = "Arcane Eg", Pool = EggPool },
            };

        private static List<ReferenceItem> Colo3ItemImport() =>
            new()
            {
                new ReferenceItem { Name = "The_Glory_of_Being_a_Fool", PreviewName = "TGOBAF", Pool = "Useless" },
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

        private class TRJRImport
        {
            public string IcName;
        }
        private static List<ReferenceItem> TRJRItemImport() =>
            // https://github.com/BadMagic100/TheRealJournalRando
            LoadListFile<TRJRImport>(TRJRFilePath)
                .Select(x =>
                {
                    var previewName = x.IcName.Replace("_", " ");
                    return new ReferenceItem[]
                    {
                        new ReferenceItem
                        {
                            Name = $"Journal_Entry-{x.IcName}",
                            PreviewName = $"{previewName} Journal Entry",
                            Pool = JournalEntryPool,
                        },
                        new ReferenceItem
                        {
                            Name = $"Hunter's_Notes-{x.IcName}",
                            PreviewName = $"{previewName} Hunter's Notes",
                            Pool = HuntersNotesPool,
                        },
                    };
                })
                .SelectMany(x => x)
                .Concat(
                    new List<ReferenceItem>() {
                        new ReferenceItem {
                            Name = "Hunter's_Mark",
                            PreviewName = "Hunter's Mark",
                            Pool = JournalEntryPool
                        },
                    })
                .ToList();

        private static List<ReferenceItem> MoreLocationsItemImport() => new()
        {
            new ReferenceItem {
                Name = "Wanderer's_Journal_Sale",
                PreviewName = "Wanderer's Journal Sale",
                Pool = MoreLocationsPool
            },
            new ReferenceItem {
                Name = "Hallownest_Seal_Sale",
                PreviewName = "Hallownest Seal Sale",
                Pool = MoreLocationsPool
            },
            new ReferenceItem {
                Name = "King's_Idol_Sale",
                PreviewName = "King's Idol Sale",
                Pool = MoreLocationsPool
            },
            new ReferenceItem {
                Name = "Arcane_Egg_Sale",
                PreviewName = "Arcane Egg Sale",
                Pool = MoreLocationsPool
            },
        };

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
        private class MoreDoorGate : BasicLocation { }
        private static List<ReferenceItem> MoreDoorsItemImport() =>
            // https://github.com/dplochcoder/HollowKnight.MoreDoors
            LoadDictionaryFileValues<MoreDoorItem>(ReferenceMoreDoorsFilePath).Select(x =>
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
                new ReferenceItem { Name = "TravelersGarment", PreviewName = "Travelers Garment", Pool = LostArtifactPool },
                new ReferenceItem { Name = "PavingStone", PreviewName = "Paving Stone", Pool = LostArtifactPool },
                new ReferenceItem { Name = "LushMoss", PreviewName = "Lush Moss", Pool = LostArtifactPool },
                new ReferenceItem { Name = "NoxiousShroom", PreviewName = "Noxious Shroom", Pool = LostArtifactPool },
                new ReferenceItem { Name = "CryingStatue", PreviewName = "Crying Statue", Pool = LostArtifactPool },
                new ReferenceItem { Name = "TotemShard", PreviewName = "Totem Shard", Pool = LostArtifactPool },
                new ReferenceItem { Name = "DungBall", PreviewName = "Dung Ball", Pool = LostArtifactPool },
                new ReferenceItem { Name = "Tumbleweed", PreviewName = "Tumbleweed", Pool = LostArtifactPool },
                new ReferenceItem { Name = "ChargedCrystal", PreviewName = "Charged Crystal", Pool = LostArtifactPool },
                new ReferenceItem { Name = "Dreamwood", PreviewName = "Dreamwood", Pool = LostArtifactPool },
                new ReferenceItem { Name = "LumaflyEssence", PreviewName = "Lumafly Essence", Pool = LostArtifactPool },
                new ReferenceItem { Name = "ThornedLeaf", PreviewName = "Thorned Leaf", Pool = LostArtifactPool },
                new ReferenceItem { Name = "WeaverSilk", PreviewName = "Weaver Silk", Pool = LostArtifactPool },
                new ReferenceItem { Name = "WyrmAsh", PreviewName = "Wyrm Ash", Pool = LostArtifactPool },
                new ReferenceItem { Name = "BeastShell", PreviewName = "Beast Shell", Pool = LostArtifactPool },
                new ReferenceItem { Name = "Honeydrop", PreviewName = "Honeydrop", Pool = LostArtifactPool },
                new ReferenceItem { Name = "InfectedRock", PreviewName = "Infected Rock", Pool = LostArtifactPool },
                new ReferenceItem { Name = "Buzzsaw", PreviewName = "Buzzsaw", Pool = LostArtifactPool },
                new ReferenceItem { Name = "VoidEmblem", PreviewName = "Void Emblem", Pool = LostArtifactPool },
                new ReferenceItem { Name = "AttunedJewel", PreviewName = "Attuned Jewel", Pool = LostArtifactPool },
                new ReferenceItem { Name = "HiddenMemento", PreviewName = "Hidden Memento", Pool = LostArtifactPool },
            };

        private static List<ReferenceItem> LoreMasterItemImport(List<ItemImport> defaultLorePoolItems) =>
            new List<ReferenceItem>()
            {
                // https://github.com/Korzer420/LoreMaster/
                new ReferenceItem { Name = "Magical_Key", PreviewName = "Magical Key" },
                new ReferenceItem { Name = "Dream_Medallion", PreviewName = "Dream Medallion" },
                new ReferenceItem { Name = "Silksong_Journal", PreviewName = "Silksong Journal?" },
                new ReferenceItem { Name = "Silver_Hallownest_Seal", PreviewName = "Silver Seal" },
                new ReferenceItem { Name = "Bronze_King_Idol", PreviewName = "Bronze King's Idol" },
                new ReferenceItem { Name = "Golden_Arcane_Egg", PreviewName = "Golden Arcane Egg" },
                new ReferenceItem { Name = "Lore_Tablet-Stag_Egg_Inspect", PreviewName = "Stag Adoption" },
                new ReferenceItem { Name = "Lore_Tablet-Record_Bela", PreviewName = "Lore Tablet Record Bela" },

                new ReferenceItem { Name = "Read_Ability", PreviewName = "Reading" },
                new ReferenceItem { Name = "Listen_Ability", PreviewName = "Listening" },
                new ReferenceItem { Name = "Lore_Page", PreviewName = "Lore Page" },
                new ReferenceItem { Name = "Lore_Page_Control", PreviewName = "Lore Control" },
                new ReferenceItem { Name = "Cleansing_Scroll", PreviewName = "Cleansing Scroll" },
                new ReferenceItem { Name = "Joker_Scroll", PreviewName = "Knowledge Scroll" },
                new ReferenceItem { Name = "Cleansing_Scroll_Double", PreviewName = "Cleansing Scroll Pack" },

                new ReferenceItem { Name = "Lemm_Order", PreviewName = "Lemm's Order" },
                new ReferenceItem { Name = "Traitor_Grave", PreviewName = "Traitor Grave" },

                new ReferenceItem { Name = "Dialogue-Bretta_Diary", PreviewName = "Bretta Diary" },
                new ReferenceItem { Name = "Dialogue-Bardoon", PreviewName = "Bardoon" },
                new ReferenceItem { Name = "Dialogue-Vespa", PreviewName = "Vespa" },
                new ReferenceItem { Name = "Dialogue-Mask_Maker", PreviewName = "Mask Maker" },
                new ReferenceItem { Name = "Dialogue-Midwife", PreviewName = "Midwife" },
                new ReferenceItem { Name = "Dialogue-Gravedigger", PreviewName = "Gravedigger" },
                new ReferenceItem { Name = "Dialogue-Poggy", PreviewName = "Poggy" },
                new ReferenceItem { Name = "Dialogue-Joni", PreviewName = "Joni" },
                new ReferenceItem { Name = "Dialogue-Myla", PreviewName = "Myla" },
                new ReferenceItem { Name = "Dialogue-Emilitia", PreviewName = "Emilitia" },
                new ReferenceItem { Name = "Dialogue-Willoh", PreviewName = "Willoh" },
                new ReferenceItem { Name = "Dialogue-Moss_Prophet", PreviewName = "Moss Prophet" },
                new ReferenceItem { Name = "Dialogue-Fluke_Hermit", PreviewName = "Fluke Hermit" },
                new ReferenceItem { Name = "Dialogue-Queen", PreviewName = "Queen" },
                new ReferenceItem { Name = "Dialogue-Marissa", PreviewName = "Marissa" },
                new ReferenceItem { Name = "Dialogue-Grasshopper", PreviewName = "Grasshopper" },
                new ReferenceItem { Name = "Dialogue-Dung_Defender", PreviewName = "Dung Defender" },
                new ReferenceItem { Name = "Dialogue-Menderbug_Diary", PreviewName = "Menderbug Diary" },

                new ReferenceItem { Name = "Inspect-Elder_Hu", PreviewName = "Elder Hu" },
                new ReferenceItem { Name = "Inspect-Xero", PreviewName = "Xero" },
                new ReferenceItem { Name = "Inspect-Galien", PreviewName = "Galien" },
                new ReferenceItem { Name = "Inspect-Marmu", PreviewName = "Marmu" },
                new ReferenceItem { Name = "Inspect-Gorb", PreviewName = "Gorb" },
                new ReferenceItem { Name = "Inspect-Markoth", PreviewName = "Markoth" },
                new ReferenceItem { Name = "Inspect-No_Eyes", PreviewName = "No Eyes" },

                new ReferenceItem { Name = "Dream_Dialogue-Aspid_Queen", PreviewName = "Aspid Queen" },
                new ReferenceItem { Name = "Dream_Dialogue-Mine_Golem", PreviewName = "Mine Golem" },
                new ReferenceItem { Name = "Dream_Dialogue-Hopper_Dummy", PreviewName = "Hopper Dummy" },
                new ReferenceItem { Name = "Dream_Dialogue-Ancient_Nailsmith_Golem", PreviewName = "Ancient Nailsmith Golem" },
                new ReferenceItem { Name = "Dream_Dialogue-Shriek_Statue", PreviewName = "Shriek Statue" },
                new ReferenceItem { Name = "Dream_Dialogue-Shade_Golem_Normal", PreviewName = "Shade Golem Normal" },
                new ReferenceItem { Name = "Dream_Dialogue-Shade_Golem_Void", PreviewName = "Shade Golem Void" },
                new ReferenceItem { Name = "Dream_Dialogue-Overgrown_Shaman", PreviewName = "Overgrown Shaman" },
                new ReferenceItem { Name = "Dream_Dialogue-Crystalized_Shaman", PreviewName = "Crystalized Shaman" },
                new ReferenceItem { Name = "Dream_Dialogue-Shroom_King", PreviewName = "Shroom King" },
                new ReferenceItem { Name = "Dream_Dialogue-Dryya", PreviewName = "Dryya" },
                new ReferenceItem { Name = "Dream_Dialogue-Isma", PreviewName = "Isma" },
                new ReferenceItem { Name = "Dream_Dialogue-Radiance_Statue", PreviewName = "Radiance Statue" },
                new ReferenceItem { Name = "Dream_Dialogue-Dashmaster_Statue", PreviewName = "Dashmaster Statue" },
                new ReferenceItem { Name = "Dream_Dialogue-Snail_Shaman_Tomb", PreviewName = "Snail Shaman Tomb" },
                new ReferenceItem { Name = "Dream_Dialogue-Pale_King", PreviewName = "Pale King" },
                new ReferenceItem { Name = "Dream_Dialogue-Grimm_Summoner", PreviewName = "Grimm Summoner" },
                new ReferenceItem { Name = "Dream_Dialogue-Kings_Mould", PreviewName = "Kings Mould" },
                new ReferenceItem { Name = "Dream_Dialogue-Dream_Shield_Statue", PreviewName = "Dream Shield Statue" },

                new ReferenceItem { Name = "Inscription-City_Fountain", PreviewName = "City Fountain" },
                new ReferenceItem { Name = "Inscription-Dreamer_Tablet", PreviewName = "Dreamer Tablet" },

                new ReferenceItem { Name = "Inspect-Beast_Den_Altar", PreviewName = "Beast Den Altar" },
                new ReferenceItem { Name = "Inspect-Weaver_Seal", PreviewName = "Weaver Seal" },
                new ReferenceItem { Name = "Inspect-Grimm_Machine", PreviewName = "Grimm Machine" },
                new ReferenceItem { Name = "Inspect-Garden_Golem", PreviewName = "Garden Golem" },
                new ReferenceItem { Name = "Inspect-Grub_Seal", PreviewName = "Grub Seal" },
                new ReferenceItem { Name = "Inspect-White_Palace_Nursery", PreviewName = "White Palace Nursery" },
                new ReferenceItem { Name = "Inspect-Grimm_Summoner_Corpse", PreviewName = "Grimm Summoner Corpse" },

                new ReferenceItem { Name = "Dialogue-Quirrel_Crossroads", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Greenpath", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Queen_Station", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Mantis_Village", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_City", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Deepnest", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Peaks", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Outside_Archive", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Archive", PreviewName = "Quirrel" },
                new ReferenceItem { Name = "Dialogue-Quirrel_Blue_Lake", PreviewName = "Quirrel" },

                new ReferenceItem { Name = "Dialogue-Cloth_Fungal_Wastes", PreviewName = "Cloth" },
                new ReferenceItem { Name = "Dialogue-Cloth_Basin", PreviewName = "Cloth" },
                new ReferenceItem { Name = "Dialogue-Cloth_Deepnest", PreviewName = "Cloth" },
                new ReferenceItem { Name = "Dialogue-Cloth_Garden", PreviewName = "Cloth" },
                new ReferenceItem { Name = "Dialogue-Cloth_Ghost", PreviewName = "Cloth" },

                new ReferenceItem { Name = "Dialogue-Tiso_Dirtmouth", PreviewName = "Tiso" },
                new ReferenceItem { Name = "Dialogue-Tiso_Crossroads", PreviewName = "Tiso" },
                new ReferenceItem { Name = "Dialogue-Tiso_Blue_Lake", PreviewName = "Tiso" },
                new ReferenceItem { Name = "Dialogue-Tiso_Colosseum", PreviewName = "Tiso" },
                new ReferenceItem { Name = "Dream_Dialogue-Tiso_Corpse", PreviewName = "Tiso" },

                new ReferenceItem { Name = "Dialogue-Zote_Greenpath", PreviewName = "Zote" },
                new ReferenceItem { Name = "Dialogue-Zote_Dirtmouth_Intro", PreviewName = "Zote" },
                new ReferenceItem { Name = "Dialogue-Zote_City", PreviewName = "Zote" },
                new ReferenceItem { Name = "Dialogue-Zote_Deepnest", PreviewName = "Zote" },
                new ReferenceItem { Name = "Dialogue-Zote_Colosseum", PreviewName = "Zote" },
                new ReferenceItem { Name = "Dialogue-Zote_Dirtmouth_After_Colosseum", PreviewName = "Zote" },
            }.Concat(defaultLorePoolItems.Select(x => new ReferenceItem
            {
                Name = $"{x.Name}_Empowered",
                PreviewName = $"{x.Name}_Empowered".WithoutUnderscores(),
            }))
            .Select(x => new ReferenceItem
            {
                Name = x.Name,
                PreviewName = x.PreviewName,
                Pool = $"Lore Master - {(x.Name.Contains("-") ? x.Name.Split('-')[0] : "Other")}"
            }
            ).ToList();

        private static List<ReferenceItem> ExtraRandoItemImport() =>
            new()
            {
                new ReferenceItem
                {
                    Name = "Progressive_Left_Cloak",
                    PreviewName = "Progressive Left Cloak",
                    Pool = SkillPool,
                },
                new ReferenceItem
                {
                    Name = "Progressive_Right_Cloak",
                    PreviewName = "Progressive Right Cloak",
                    Pool = SkillPool,
                },
            };

        private static List<ReferenceItem> DarknessItemImport() =>
            new()
            {
                // https://github.com/dplochcoder/HollowKnight.DarknessRandomizer
                new ReferenceItem { Name = "Lantern_Shard", PreviewName = "Lantern Shard (#0)", Pool = "Key" },
                new ReferenceItem { Name = "Final_Lantern_Shard", PreviewName = "Final Lantern Shard", Pool = "Key" },
            };

        private class BreakableWall
        {
            public string NiceName;
            public string SceneName;
        }
        private static List<ReferenceItem> BreakableWallsItemImport() =>
            // https://github.com/Bentechy66/HollowKnight.BreakableWallRandomizer
            LoadListFile<BreakableWall>(ReferenceBreakableWallsFilePath)
            .Select(x =>
            {
                var cleanedName = Regex.Replace(x.NiceName, @"[^\w\s]", "");
                return new ReferenceItem
                {
                    Name = cleanedName,
                    PreviewName = cleanedName,
                    Pool = BreakablePool
                };
            }).ToList();

        private class GrassItem
        {
            public string UsrName;
            public int Id;
            public string SceneName;
        }
        private static List<ReferenceItem> GrassRandoItemImport() =>
            // https://github.com/StormZillaa/HollowKnightGrassRando
            LoadListFile<GrassItem>(ReferenceGrassRandoFilePath)
            .Select(x =>
            {
                var cleanedName = Regex.Replace(x.UsrName, @"[^\w\s]", "");
                return new ReferenceItem
                {
                    Name = $"{cleanedName}{x.Id - 1}",
                    PreviewName = "Grass",
                    Pool = GrassPool
                };
            }).ToList();

        private static List<ReferenceItem> UnidentifiedItems() =>
            new()
            {
                new ReferenceItem
                {
                    Name = "chandelierItem",
                    PreviewName = "WK Chandelier Cut",
                    Pool = "Useless"
                },
                new ReferenceItem
                {
                    Name = "Bardoon_Butt_Smack",
                    PreviewName = "Bardoon Butt Smack",
                    Pool = "Useless"
                },
            };

        private void LoadReferenceLocations()
        {
            var locationImports = GetLocationImportsFromFile();
            var roomImports = GetRoomImportsFromFile();
            var sceneDescriptions = GetSceneDescriptionsFromFile();
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
                    Pool = GetReferenceLocationPool(location.Name),
                    SceneDescription = sceneDescriptions.TryGetValue(location.SceneName, out var sceneDescription) ? sceneDescription : location.SceneName
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
                        Pool = customLocation.Pool,
                        SceneDescription = sceneDescriptions.TryGetValue(customLocation.SceneName, out var sceneDescription) ? sceneDescription : customLocation.SceneName
                    })
                ).ToList();

            var sceneNames = roomImports.Select(x => x.SceneName).ToList();
            DeadLocations = locationImports.Concat(customImports).Where(location => !sceneNames.Contains(location.SceneName)).ToList();
        }

        private static List<LocationImport> GetLocationImportsFromFile() =>
            LoadDictionaryFileValues<LocationImport>(ReferenceLocationsFilePath);

        private static List<RoomImport> GetRoomImportsFromFile() =>
            LoadDictionaryFileValues<RoomImport>(ReferenceRoomsFilePath).Concat(new List<RoomImport>
            {
                new RoomImport { SceneName = "Room_Tram_RG", MapArea = "Tram", TitledArea = "Tram" },
                new RoomImport { SceneName = "Room_Tram", MapArea = "Tram", TitledArea = "Tram" },
                new RoomImport { SceneName = "Room_Final_Boss_Atrium", MapArea = "Black Egg Temple", TitledArea = "Black Egg Temple" },
                new RoomImport { SceneName = "GG_Atrium", MapArea = "Godhome", TitledArea = "Godhome" },
                new RoomImport { SceneName = "GG_Atrium_Roof", MapArea = "Godhome", TitledArea = "Godhome" },
                new RoomImport { SceneName = "GG_Workshop", MapArea = "Godhome", TitledArea = "Godhome" },
                new RoomImport { SceneName = "Town/Fungus3_23", MapArea = "Dirthmouth/Queen's Garden", TitledArea = "Dirthmouth/Queen's Garden" },
                new RoomImport { SceneName = "> Multiworld", MapArea = "> Multiworld", TitledArea = "> Multiworld" },
                new RoomImport { SceneName = "> Hunter's Notes", MapArea = "> Hunter's Notes", TitledArea = "> Hunter's Notes" },
                new RoomImport { SceneName = "> Journal Entry", MapArea = "> Journal Entry", TitledArea = "> Journal Entry" },
                
                new RoomImport { SceneName = "Dream_01_False_Knight", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "Dream_02_Mage_Lord", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "Dream_Backer_Shrine", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "Dream_Nailcollection", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Broken_Vessel", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Ghost_Marmu", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Ghost_Marmu_V", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Hornet_2", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Lost_Kin", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Mega_Moss_Charger", MapArea = "> Grass", TitledArea = "> Grass" },
                new RoomImport { SceneName = "GG_Traitor_Lord", MapArea = "> Grass", TitledArea = "> Grass" },
            }).ToList();

        private static Dictionary<string, string> GetSceneDescriptionsFromFile() =>
            LoadDictionaryFile<string>(ReferenceSceneDescriptionFilePath);

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
                .Concat(MultiWorldImport())

                //flibber-hk
                .Concat(RandoPlusLocationImport())
                .Concat(LeverLocationImport())

                //dpinela
                .Concat(Colo3LocationImport())

                //homothetyhk
                .Concat(BenchLocationImport())

                //BadMagic100
                .Concat(TRJRLocationImport())
                .Concat(MoreLocationsLocationImport())

                //dplochcoder
                .Concat(MoreDoorsLocationImport())

                //Hoo-Knows
                .Concat(LostArtifactsLocationImport())

                //Korzer420
                .Concat(LoreMasterLocationImport())

                //Bentechy66
                .Concat(BreakableWallsLocationImport())

                //StormZillaa
                .Concat(GrassRandoLocationImport())

                // unidentified locations
                .Concat(UnidentifiedLocationImport())

                .ToList();
        }

        private List<ReferenceLocation> MultiWorldImport() =>
            new()
            {
                new ReferenceLocation { Name = "Remote", SceneName = "> Multiworld", Pool = "> Multiworld" },
            };

        private class GhostLocation : BasicLocation { }
        private static List<ReferenceLocation> RandoPlusLocationImport() =>
            new List<ReferenceLocation>()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new ReferenceLocation { Name = "Mr_Mushroom-Fungal_Wastes", SceneName = "Fungus2_18", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Kingdom's_Edge", SceneName = "Deepnest_East_01", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Deepnest", SceneName = "Deepnest_40", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Howling_Cliffs", SceneName = "Room_nailmaster", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Ancient_Basin", SceneName = "Abyss_21", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-Fog_Canyon", SceneName = "Fungus3_44", Pool = MrMushroomPool },
                new ReferenceLocation { Name = "Mr_Mushroom-King's_Pass", SceneName = "Tutorial_01", Pool = MrMushroomPool },

                new ReferenceLocation { Name = "Nailsmith_Upgrade_1", SceneName = "Room_nailsmith", Pool = "Shop" },
                new ReferenceLocation { Name = "Nailsmith_Upgrade_2", SceneName = "Room_nailsmith", Pool = "Shop" },
                new ReferenceLocation { Name = "Nailsmith_Upgrade_3", SceneName = "Room_nailsmith", Pool = "Shop" },
                new ReferenceLocation { Name = "Nailsmith_Upgrade_4", SceneName = "Room_nailsmith", Pool = "Shop" },
            }.Concat(
                LoadListFile<GhostLocation>(ReferenceRandoPlusGhostLocationsFilePath)
                    .Select(x => new ReferenceLocation
                    {
                        Name = x.Name,
                        SceneName = x.SceneName,
                        Pool = GhostPool,
                    })
            ).ToList();

        private class LeverLocation : BasicLocation { }
        private static List<ReferenceLocation> LeverLocationImport() =>
            // https://github.com/flibber-hk/HollowKnight.RandomizableLevers
            LoadDictionaryFileValues<LeverLocation>(ReferenceRandoLeverLocationsFilePath).Select(x =>
            new ReferenceLocation
            {
                Name = x.Name,
                SceneName = x.SceneName,
                Pool = LeverPool,
            }).ToList();

        private static List<ReferenceLocation> Colo3LocationImport() =>
            new()
            {
                new ReferenceLocation
                {
                    Name = "The_Glory_of_Being_a_Fool-Colosseum",
                    SceneName = "Room_Colosseum_01",
                    Pool = "Useless",
                }
            };

        private class BenchLocation : BasicLocation { }
        private static List<ReferenceLocation> BenchLocationImport() =>
            // https://github.com/homothetyhk/BenchRando
            LoadDictionaryFileValues<BenchLocation>(ReferenceBenchRandoLocationsFilePath)
                .Select(x => new ReferenceLocation
                {
                    Name = x.Name,
                    SceneName = x.SceneName,
                    Pool = BenchPool,
                })
                .ToList();

        private static List<ReferenceLocation> TRJRLocationImport() =>
            // https://github.com/BadMagic100/TheRealJournalRando
            LoadListFile<TRJRImport>(TRJRFilePath)
                .Select(x =>
                {
                    var previewName = x.IcName.Replace("_", " ");
                    return new ReferenceLocation[]
                    {
                        new ReferenceLocation
                        {
                            Name = $"Journal_Entry-{x.IcName}",
                            SceneName = "> Journal Entry",
                            Pool = JournalEntryPool,
                        },
                        new ReferenceLocation
                        {
                            Name = $"Hunter's_Notes-{x.IcName}",
                            SceneName = "> Hunter's Notes",
                            Pool = HuntersNotesPool,
                        },
                    };
                })
                .SelectMany(x => x)
                .Concat(
                    new List<ReferenceLocation>()
                    {
                        new ReferenceLocation { Name = "Hunter's_Mark", SceneName = "Fungus1_08", Pool = "Journal_Entry"},
                    })
                .ToList();

        private static List<ReferenceLocation> MoreLocationsLocationImport() =>
            new()
            {
                new ReferenceLocation
                {
                    Name = "Swim",
                    SceneName = "Crossroads_50",
                    Pool = MoreLocationsPool,
                },
                new ReferenceLocation
                {
                    Name = "Stag_Nest_Egg",
                    SceneName = "Cliffs_03",
                    Pool = MoreLocationsPool,
                },
                new ReferenceLocation
                {
                    Name = "Geo_Chest-Above_Baldur_Shell",
                    SceneName = "Cliffs_03",
                    Pool = MoreLocationsPool,
                },
                new ReferenceLocation
                {
                    Name = "Lemm",
                    SceneName = "Ruins1_05b",
                    Pool = "Shop",
                },
                new ReferenceLocation
                {
                    Name = "Junk_Shop",
                    SceneName = "Room_GG_Shortcut",
                    Pool = "Shop",
                },
            };

        private static List<ReferenceLocation> MoreDoorsLocationImport() =>
            // https://github.com/dplochcoder/HollowKnight.MoreDoors
            LoadDictionaryFileValues<MoreDoorItem>(ReferenceMoreDoorsFilePath).Select(x =>
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
                new ReferenceLocation { Name = "AttunedJewel", SceneName = "GG_Workshop", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "BeastShell", SceneName = "Room_Colosseum_01", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Buzzsaw", SceneName = "White_Palace_08", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "ChargedCrystal", SceneName = "Mines_18", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "CryingStatue", SceneName = "Ruins1_27", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Dreamwood", SceneName = "RestingGrounds_05", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "DungBall", SceneName = "Waterways_15", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "HiddenMemento", SceneName = "White_Palace_06", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Honeydrop", SceneName = "Hive_01", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "InfectedRock", SceneName = "Abyss_19", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "LumaflyEssence", SceneName = "Fungus3_archive_02", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "LushMoss", SceneName = "Fungus1_29", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "NoxiousShroom", SceneName = "Fungus2_30", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "PavingStone", SceneName = "Crossroads_47", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "ThornedLeaf", SceneName = "Fungus3_10", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "TotemShard", SceneName = "Ruins1_32", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "TravelersGarment", SceneName = "Town", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "Tumbleweed", SceneName = "Cliffs_01", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "VoidEmblem", SceneName = "Abyss_09", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "WeaverSilk", SceneName = "Deepnest_45_v02", Pool = LostArtifactPool },
                new ReferenceLocation { Name = "WyrmAsh", SceneName = "Deepnest_East_12", Pool = LostArtifactPool },
            };

        private static List<ReferenceLocation> LoreMasterLocationImport() =>
            new()
            {
                // https://github.com/Korzer420/LoreMaster/
                new ReferenceLocation { Name = "Bretta_Diary", SceneName = "Room_Bretta", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Bardoon", SceneName = "Deepnest_East_04", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Vespa", SceneName = "Hive_05", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Mask_Maker", SceneName = "Room_Mask_Maker", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Midwife", SceneName = "Deepnest_41", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Gravedigger", SceneName = "Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Poggy", SceneName = "Ruins_Elevator", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Joni", SceneName = "Cliffs_05", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Myla", SceneName = "Crossroads_45", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Emilitia", SceneName = "Ruins_House_03", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Willoh", SceneName = "Fungus2_34", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Moss_Prophet", SceneName = "Fungus3_39", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Fluke_Hermit", SceneName = "Room_GG_Shortcut", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Queen", SceneName = "Room_Queen", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Marissa", SceneName = "Ruins_Bathhouse", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grasshopper", SceneName = "Fungus1_24", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dung_Defender", SceneName = "Waterways_05", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Menderbug_Diary", SceneName = "Room_Mender_House", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Elder_Hu_Grave", SceneName = "Fungus2_32", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Xero_Grave", SceneName = "RestingGrounds_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Gorb_Grave", SceneName = "Cliffs_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Marmu_Grave", SceneName = "Fungus3_40", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "No_Eyes_Statue", SceneName = "Fungus1_35", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Markoth_Corpse", SceneName = "Deepnest_East_10", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Galien_Corpse", SceneName = "Deepnest_40", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Aspid_Queen_Dream", SceneName = "Crossroads_22", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Mine_Golem_Dream", SceneName = "Mines_31", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Hopper_Dummy_Dream", SceneName = "Deepnest_East_16", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Ancient_Nailsmith_Golem_Dream", SceneName = "Deepnest_East_14b", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Shriek_Statue_Dream", SceneName = "Abyss_12", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Overgrown_Shaman_Dream", SceneName = "Room_Fungus_Shaman", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Shroom_King_Dream", SceneName = "Fungus2_30", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dryya_Dream", SceneName = "Fungus3_48", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Isma_Dream", SceneName = "Waterways_13", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Radiance_Statue_Dream", SceneName = "Mines_34", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dashmaster_Statue_Dream", SceneName = "Fungus2_23", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Snail_Shaman_Tomb_Dream", SceneName = "RestingGrounds_10", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Kings_Mould_Machine_Dream", SceneName = "White_Palace_08", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dream_Shield_Statue_Dream", SceneName = "RestingGrounds_17", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Shade_Golem_Dream_Normal", SceneName = "Abyss_10", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Shade_Golem_Dream_Void", SceneName = "Abyss_10", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Pale_King_Dream", SceneName = "White_Palace_09", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Crystalized_Shaman_Dream", SceneName = "Mines_35", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grimm_Summoner_Dream", SceneName = "Cliffs_06", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "City_Fountain", SceneName = "Ruins1_27", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dreamer_Tablet", SceneName = "RestingGrounds_04", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Beast_Den_Altar", SceneName = "Deepnest_Spider_Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Weaver_Seal", SceneName = "Deepnest_45_v02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grimm_Machine", SceneName = "Grimm_Main_Tent", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Garden_Golem", SceneName = "Fungus1_23", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grub_Seal", SceneName = "Ruins2_11", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Path_Of_Pain_Seal", SceneName = "White_Palace_18", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "White_Palace_Nursery", SceneName = "White_Palace_09", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grimm_Summoner_Corpse", SceneName = "Cliffs_06", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Quirrel_Crossroads", SceneName = "Room_temple", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Greenpath", SceneName = "Room_Slug_Shrine", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Queen_Station", SceneName = "Fungus2_01", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Mantis_Village", SceneName = "Fungus2_14", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_City", SceneName = "Ruins1_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Peaks", SceneName = "Mines_13", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Deepnest", SceneName = "Deepnest_30", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Outside_Archive", SceneName = "Fungus3_47", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_After_Monomon", SceneName = "Fungus3_archive_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Blue_Lake", SceneName = "Crossroads_50", Pool = LoreMasterPool },

                new ReferenceLocation { Name = "Cloth_Fungal_Wastes", SceneName = "Fungus2_09", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Basin", SceneName = "Abyss_17", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Deepnest", SceneName = "Deepnest_14", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Garden", SceneName = "Fungus3_34", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Ghost", SceneName = "Fungus3_23", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Town", SceneName = "Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_End", SceneName = "Town/Fungus3_23", Pool = LoreMasterPool }, 
                
                new ReferenceLocation { Name = "Tiso_Dirtmouth", SceneName = "Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Crossroads", SceneName = "Crossroads_47", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Blue_Lake", SceneName = "Crossroads_50", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Colosseum", SceneName = "Room_Colosseum_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Corpse", SceneName = "Deepnest_East_07", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Zote_Greenpath", SceneName = "Fungus1_20_v02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Dirtmouth_Intro", SceneName = "Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_City", SceneName = "Ruins1_06", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Deepnest", SceneName = "Deepnest_33", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Colosseum", SceneName = "Room_Colosseum_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Dirtmouth_After_Colosseum", SceneName = "Town", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Elderbug_Reward_1", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_2", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_3", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_4", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_5", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_6", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_7", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_8", SceneName="Town", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Elderbug_Reward_9", SceneName="Town", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Stag_Nest", SceneName="Cliffs_03", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Lemm_Door", SceneName="Ruins1_05b", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Lore_Tablet-Record_Bela", SceneName="Ruins1_30", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Traitor_Grave", SceneName="Fungus3_49", Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Treasure-Howling_Cliffs", SceneName="Cliffs_01", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Crossroads", SceneName="Crossroads_42", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Greenpath", SceneName="Fungus1_Slug", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Fog_Canyon", SceneName="Fungus3_archive_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Fog_Canyon", SceneName="Fungus3_archive_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Fungal_Wastes", SceneName="Fungus2_10", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-City_Of_Tears", SceneName="Ruins2_05", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Waterways", SceneName="Waterways_13", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Deepnest", SceneName="Deepnest_30", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Ancient_Basin", SceneName="Abyss_06_Core", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Kingdoms_Edge", SceneName="GG_Lurker", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Crystal_Peaks", SceneName="Mines_02", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Resting_Grounds", SceneName="RestingGrounds_08", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-Queens_Garden", SceneName="Fungus3_04", Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Treasure-White_Palace", SceneName="White_Palace_01", Pool = LoreMasterPool },
            };

        private static List<ReferenceLocation> BreakableWallsLocationImport() =>
            // https://github.com/Bentechy66/HollowKnight.BreakableWallRandomizer
            LoadListFile<BreakableWall>(ReferenceBreakableWallsFilePath)
                .Select(x => new ReferenceLocation
                {
                    Name = Regex.Replace(x.NiceName, @"[^\w\s]", ""),
                    SceneName = x.SceneName,
                    Pool = BreakablePool,
                })
                .ToList();

        private static List<ReferenceLocation> GrassRandoLocationImport() =>
            // https://github.com/StormZillaa/HollowKnightGrassRando
            LoadListFile<GrassItem>(ReferenceGrassRandoFilePath)
                .Select(x => new ReferenceLocation
                {
                    Name = $"{Regex.Replace(x.UsrName, @"[^\w\s]", "")}{x.Id - 1}",
                    SceneName = x.SceneName,
                    Pool = GrassPool,
                })
                .ToList();

        private static List<ReferenceLocation> UnidentifiedLocationImport() =>
            new()
            {
                new ReferenceLocation { Name = "chandelierLocation", SceneName = "Ruins2_03", Pool = "Useless" },
                new ReferenceLocation { Name = "Bardoon_Butt", SceneName = "Deepnest_East_04", Pool = "Useless" },
            };

        private void LoadReferenceTransitions()
        {
            var transitionImports = GetTransitionImportsFromFile();
            var roomImports = GetRoomImportsFromFile();
            var sceneDescriptions = GetSceneDescriptionsFromFile();

            ReferenceTransitions = transitionImports.Join(
                roomImports,
                transition => transition.SceneName,
                room => room.SceneName,
                (transition, room) => new ReferenceTransition
                {
                    DoorName = transition.DoorName,
                    SceneName = transition.SceneName,
                    MapArea = room.MapArea,
                    TitledArea = room.TitledArea,
                    SceneDescription = sceneDescriptions.TryGetValue(transition.SceneName, out var sceneDescription) ? sceneDescription : transition.SceneName
                }).ToList();

            var sceneNames = roomImports.Select(x => x.SceneName).ToList();
            DeadTransitions = transitionImports.Where(transition => !sceneNames.Contains(transition.SceneName)).ToList();
        }

        private static List<TransitionImport> GetTransitionImportsFromFile() =>
            LoadDictionaryFileValues<TransitionImport>(ReferenceTransitionsFilePath)
                .Concat(new[]
                {
                    new TransitionImport { SceneName = "Fungus2_08", DoorName = "right_RCD"},
                    new TransitionImport { SceneName = "Crossroads_49b", DoorName = "left_RCD"},
                })
                .ToList();

        private static List<T> LoadDictionaryFileValues<T>(string filePath) =>
            File.Exists(filePath)
                ? DeserializeFile<Dictionary<string, T>>(filePath).Values.ToList()
                : new List<T>();

        private static Dictionary<string, T> LoadDictionaryFile<T>(string filePath) =>
            File.Exists(filePath)
                ? DeserializeFile<Dictionary<string, T>>(filePath)
                : new Dictionary<string, T>();

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

        public string GetSeedGenerationCode() =>
            GetDictionaryDataFromFileOrDefault<string>(SeedFilename)
                .TryGetValue("SeedGenerationCode", out var value) 
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

        public void SaveSeedGenerationCode(string seed) =>
            WriteFile(SeedFilename, new Dictionary<string, string> { { "SeedGenerationCode", seed } });

        private static void WriteFile<T>(string filename, T data)
        {
            using StreamWriter file = File.CreateText(filename);
            new JsonSerializer { Formatting = Formatting.Indented }.Serialize(file, data);
        }
    }

    public class BasicLocation
    {
        public string Name;
        public string SceneName;
    }
}
