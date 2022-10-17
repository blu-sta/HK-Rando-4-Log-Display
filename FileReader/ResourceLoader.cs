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
        public List<LocationImport> DeadLocations { get; }
        public List<TransitionImport> DeadTransitions { get; }
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
        private const string LoreMasterPool = "Lore - Lore Master";

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

                //Korzer420
                .Concat(LoreMasterItemImport())

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

        private static List<ReferenceItem> LoreMasterItemImport() =>
            new()
            {
                // https://github.com/Korzer420/LoreMaster/
                new ReferenceItem { Name = "Magical_Key", PreviewName = "Magical Key" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Medallion", PreviewName = "Dream Medallion" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Silksong_Journal", PreviewName = "Silksong Journal?" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Silver_Hallownest_Seal", PreviewName = "Silver Seal" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Bronze_King_Idol", PreviewName = "Bronze King's Idol" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Golden_Arcane_Egg", PreviewName = "Golden Arcane Egg" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Lore_Tablet-Stag_Egg_Inspect", PreviewName = "Stag Adoption" , Pool = LoreMasterPool },
                
                new ReferenceItem { Name = "Read_Ability", PreviewName = "Reading" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Listen_Ability", PreviewName = "Listening" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Lore_Page", PreviewName = "Lore Page" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Lore_Page_Control", PreviewName = "Lore Control" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Cleansing_Scroll", PreviewName = "Cleansing Scroll" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Joker_Scroll", PreviewName = "Knowledge Scroll" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Cleansing_Scroll_Double", PreviewName = "Cleansing Scroll Pack" , Pool = LoreMasterPool },
                
                new ReferenceItem { Name = "Lemm_Order", PreviewName = "Lemm's Order" , Pool = LoreMasterPool },
                
                new ReferenceItem { Name = "Dialogue-Bretta_Diary", PreviewName = "Bretta Diary" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Bardoon", PreviewName = "Bardoon" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Vespa", PreviewName = "Vespa" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Mask_Maker", PreviewName = "Mask Maker" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Midwife", PreviewName = "Midwife" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Gravedigger", PreviewName = "Gravedigger" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Poggy", PreviewName = "Poggy" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Joni", PreviewName = "Joni" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Myla", PreviewName = "Myla" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Emilitia", PreviewName = "Emilitia" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Willoh", PreviewName = "Willoh" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Moss_Prophet", PreviewName = "Moss Prophet" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Fluke_Hermit", PreviewName = "Fluke Hermit" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Queen", PreviewName = "Queen" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Marissa", PreviewName = "Marissa" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Grasshopper", PreviewName = "Grasshopper" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Dung_Defender", PreviewName = "Dung Defender" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Menderbug_Diary", PreviewName = "Menderbug Diary" , Pool = LoreMasterPool },
                
                new ReferenceItem { Name = "Inspect-Elder_Hu", PreviewName = "Elder Hu" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Xero", PreviewName = "Xero" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Galien", PreviewName = "Galien" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Marmu", PreviewName = "Marmu" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Gorb", PreviewName = "Gorb" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Markoth", PreviewName = "Markoth" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-No_Eyes", PreviewName = "No Eyes" , Pool = LoreMasterPool },

                new ReferenceItem { Name = "Dream_Dialogue-Aspid_Queen", PreviewName = "Aspid Queen" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Mine_Golem", PreviewName = "Mine Golem" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Hopper_Dummy", PreviewName = "Hopper Dummy" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Ancient_Nailsmith_Golem", PreviewName = "Ancient Nailsmith Golem" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Shriek_Statue", PreviewName = "Shriek Statue" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Shade_Golem_Normal", PreviewName = "Shade Golem Normal" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Shade_Golem_Void", PreviewName = "Shade Golem Void" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Overgrown_Shaman", PreviewName = "Overgrown Shaman" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Crystalized_Shaman", PreviewName = "Crystalized Shaman" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Shroom_King", PreviewName = "Shroom King" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Dryya", PreviewName = "Dryya" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Isma", PreviewName = "Isma" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Radiance_Statue", PreviewName = "Radiance Statue" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Dashmaster_Statue", PreviewName = "Dashmaster Statue" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Snail_Shaman_Tomb", PreviewName = "Snail Shaman Tomb" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Pale_King", PreviewName = "Pale King" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Grimm_Summoner", PreviewName = "Grimm Summoner" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Kings_Mould", PreviewName = "Kings Mould" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Dream_Shield_Statue", PreviewName = "Dream Shield Statue" , Pool = LoreMasterPool },

                new ReferenceItem { Name = "Inscription-City_Fountain", PreviewName = "City Fountain" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inscription-Dreamer_Tablet", PreviewName = "Dreamer Tablet" , Pool = LoreMasterPool },

                new ReferenceItem { Name = "Inspect-Beast_Den_Altar", PreviewName = "Beast Den Altar" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Weaver_Seal", PreviewName = "Weaver Seal" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Grimm_Machine", PreviewName = "Grimm Machine" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Garden_Golem", PreviewName = "Garden Golem" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Grub_Seal", PreviewName = "Grub Seal" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Path_Of_Pain_Seal", PreviewName = "Path Of Pain Seal" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-White_Palace_Nursery", PreviewName = "White Palace Nursery" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Inspect-Grimm_Summoner_Corpse", PreviewName = "Grimm Summoner Corpse" , Pool = LoreMasterPool },
                
                new ReferenceItem { Name = "Dialogue-Quirrel", PreviewName = "Quirrel" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Crossroads", PreviewName = "Quirrel_Crossroads" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Greenpath", PreviewName = "Quirrel_Greenpath" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Queen_Station", PreviewName = "Quirrel_Queen_Station" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Mantis_Village", PreviewName = "Quirrel_Mantis_Village" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_City", PreviewName = "Quirrel_City" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Deepnest", PreviewName = "Quirrel_Deepnest" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Peaks", PreviewName = "Quirrel_Peaks" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Outside_Archive", PreviewName = "Quirrel_Outside_Archive" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Archive", PreviewName = "Quirrel_Archive" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Quirrel_Blue_Lake", PreviewName = "Quirrel_Blue_Lake" , Pool = LoreMasterPool },

                new ReferenceItem { Name = "Dialogue-Cloth", PreviewName = "Cloth" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Cloth_Fungal_Wastes", PreviewName = "Cloth_Fungal_Wastes" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Cloth_Basin", PreviewName = "Cloth_Basin" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Cloth_Deepnest", PreviewName = "Cloth_Deepnest" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Cloth_Garden", PreviewName = "Cloth_Garden" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Cloth_Ghost", PreviewName = "Cloth_Ghost" , Pool = LoreMasterPool },

                new ReferenceItem { Name = "Dialogue-Tiso", PreviewName = "Tiso" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Tiso_Dirtmouth", PreviewName = "Tiso_Dirtmouth" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Tiso_Crossroads", PreviewName = "Tiso_Crossroads" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Tiso_Blue_Lake", PreviewName = "Tiso_Blue_Lake" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Tiso_Colosseum", PreviewName = "Tiso_Colosseum" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dream_Dialogue-Tiso_Corpse", PreviewName = "Tiso_Corpse" , Pool = LoreMasterPool },

                new ReferenceItem { Name = "Dialogue-Zote", PreviewName = "Zote" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Zote_Greenpath", PreviewName = "Zote_Greenpath" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Zote_Dirtmouth_Intro", PreviewName = "Zote_Dirtmouth_Intro" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Zote_City", PreviewName = "Zote_City" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Zote_Deepnest", PreviewName = "Zote_Deepnest" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Zote_Colosseum", PreviewName = "Zote_Colosseum" , Pool = LoreMasterPool },
                new ReferenceItem { Name = "Dialogue-Zote_Dirtmouth_After_Colosseum", PreviewName = "Zote_Dirtmouth_After_Colosseum" , Pool = LoreMasterPool },
            };

        private static List<ReferenceItem> DarknessItemImport() =>
            new()
            {
                // https://github.com/dplochcoder/HollowKnight.DarknessRandomizer
                new ReferenceItem { Name = "Lantern_Shard", PreviewName = "Lantern Shard (#0)" , Pool = "Key" },
                new ReferenceItem { Name = "Final_Lantern_Shard", PreviewName = "Final Lantern Shard" , Pool = "Key" },
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

            var sceneNames = roomImports.Select(x => x.SceneName).ToList();
            DeadLocations = locationImports.Concat(customImports).Where(location => !sceneNames.Contains(location.SceneName)).ToList();
        }

        private static List<LocationImport> GetLocationImportsFromFile() =>
            LoadDictionaryFile<LocationImport>(ReferenceLocationsFilePath);

        private static List<RoomImport> GetRoomImportsFromFile() =>
            LoadDictionaryFile<RoomImport>(ReferenceRoomsFilePath).Concat(new List<RoomImport>
            {
                new RoomImport { SceneName = "Room_Tram_RG", MapArea = "Tram", TitledArea = "Tram" },
                new RoomImport { SceneName = "Room_Tram", MapArea = "Tram", TitledArea = "Tram" },
                new RoomImport { SceneName = "Room_Final_Boss_Atrium", MapArea = "Black Egg Temple", TitledArea = "Black Egg Temple" },
                new RoomImport { SceneName = "GG_Atrium", MapArea = "Godhome", TitledArea = "Godhome" },
                new RoomImport { SceneName = "GG_Atrium_Roof", MapArea = "Godhome", TitledArea = "Godhome" },
                new RoomImport { SceneName = "GG_Workshop", MapArea = "Godhome", TitledArea = "Godhome" },
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

                //Korzer420
                .Concat(LoreMasterLocationImport())

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

        private static List<ReferenceLocation> LoreMasterLocationImport() =>
            new()
            {
                // https://github.com/Korzer420/LoreMaster/
                new ReferenceLocation { Name = "Bretta_Diary", SceneName = "Room_Bretta" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Bardoon", SceneName = "Deepnest_East_04" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Vespa", SceneName = "Hive_05" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Mask_Maker", SceneName = "Room_Mask_Maker" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Midwife", SceneName = "Deepnest_41" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Gravedigger", SceneName = "Town" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Poggy", SceneName = "Ruins_Elevator" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Joni", SceneName = "Cliffs_05" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Myla", SceneName = "Crossroads_45" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Emilitia", SceneName = "Ruins_House_03" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Willoh", SceneName = "Fungus2_34" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Moss_Prophet", SceneName = "Fungus3_39" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Fluke_Hermit", SceneName = "Room_GG_Shortcut" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Queen", SceneName = "Room_Queen" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Marissa", SceneName = "Ruins_Bathhouse" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grasshopper", SceneName = "Fungus1_24" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dung_Defender", SceneName = "Waterways_05" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Menderbug_Diary", SceneName = "Room_Mender_House" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Elder_Hu_Grave", SceneName = "Fungus2_32" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Xero_Grave", SceneName = "RestingGrounds_02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Gorb_Grave", SceneName = "Cliffs_02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Marmu_Grave", SceneName = "Fungus3_40" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "No_Eyes_Statue", SceneName = "Fungus1_35" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Markoth_Corpse", SceneName = "Deepnest_East_10" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Galien_Corpse", SceneName = "Deepnest_40" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Aspid_Queen_Dream", SceneName = "Crossroads_22" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Mine_Golem_Dream", SceneName = "Mines_31" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Hopper_Dummy_Dream", SceneName = "Deepnest_East_16" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Ancient_Nailsmith_Golem_Dream", SceneName = "Deepnest_East_14b" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Shriek_Statue_Dream", SceneName = "Abyss_12" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Overgrown_Shaman_Dream", SceneName = "Room_Fungus_Shaman" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Shroom_King_Dream", SceneName = "Fungus2_30" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dryya_Dream", SceneName = "Fungus3_48" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Isma_Dream", SceneName = "Waterways_13" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Radiance_Statue_Dream", SceneName = "Mines_34" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dashmaster_Statue_Dream", SceneName = "Fungus2_23" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Snail_Shaman_Tomb_Dream", SceneName = "RestingGrounds_10" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Kings_Mould_Machine_Dream", SceneName = "White_Palace_08" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dream_Shield_Statue_Dream", SceneName = "RestingGrounds_17" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Shade_Golem_Dream_Normal", SceneName = "Abyss_10" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Shade_Golem_Dream_Void", SceneName = "Abyss_10" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Pale_King_Dream", SceneName = "White_Palace_09" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Crystalized_Shaman_Dream", SceneName = "Mines_35" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grimm_Summoner_Dream", SceneName = "Cliffs_06" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "City_Fountain", SceneName = "Ruins1_27" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Dreamer_Tablet", SceneName = "RestingGrounds_04" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Beast_Den_Altar", SceneName = "Deepnest_Spider_Town" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Weaver_Seal", SceneName = "Deepnest_45_v02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grimm_Machine", SceneName = "Grimm_Main_Tent" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Garden_Golem", SceneName = "Fungus1_23" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grub_Seal", SceneName = "Ruins2_11" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Path_Of_Pain_Seal", SceneName = "White_Palace_18" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "White_Palace_Nursery", SceneName = "White_Palace_09" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Grimm_Summoner_Corpse", SceneName = "Cliffs_06" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Quirrel_Crossroads", SceneName = "Room_temple" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Greenpath", SceneName = "Room_Slug_Shrine" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Queen_Station", SceneName = "Fungus2_01" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Mantis_Village", SceneName = "Fungus2_14" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_City", SceneName = "Ruins1_02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Peaks", SceneName = "Mines_13" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Deepnest", SceneName = "Deepnest_30" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Outside_Archive", SceneName = "Fungus3_47" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_After_Monomon", SceneName = "Fungus3_archive_02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Quirrel_Blue_Lake", SceneName = "Crossroads_50" , Pool = LoreMasterPool },

                new ReferenceLocation { Name = "Cloth_Fungal_Wastes", SceneName = "Fungus2_09" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Basin", SceneName = "Abyss_17" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Deepnest", SceneName = "Deepnest_14" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Garden", SceneName = "Fungus3_34" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Ghost", SceneName = "Fungus3_23" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_Town", SceneName = "Town" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Cloth_End", SceneName = "Town" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Tiso_Dirtmouth", SceneName = "Town" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Crossroads", SceneName = "Crossroads_47" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Blue_Lake", SceneName = "Crossroads_50" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Colosseum", SceneName = "Room_Colosseum_02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Tiso_Corpse", SceneName = "Deepnest_East_07" , Pool = LoreMasterPool },
                
                new ReferenceLocation { Name = "Zote_Greenpath", SceneName = "Fungus1_20_v02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Dirtmouth_Intro", SceneName = "Town" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_City", SceneName = "Ruins1_06" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Deepnest", SceneName = "Deepnest_33" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Colosseum", SceneName = "Room_Colosseum_02" , Pool = LoreMasterPool },
                new ReferenceLocation { Name = "Zote_Dirtmouth_After_Colosseum", SceneName = "Town" , Pool = LoreMasterPool },
                
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

            var sceneNames = roomImports.Select(x => x.SceneName).ToList();
            DeadTransitions = transitionImports.Where(transition => !sceneNames.Contains(transition.SceneName)).ToList();
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
