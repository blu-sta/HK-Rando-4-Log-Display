using HK_Rando_4_Log_Display.DTO;
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
        public List<ReferenceItem> ReferenceItems { get; private set; } = new ();
        public List<ReferenceLocation> ReferenceLocations { get; private set; } = new ();
        public List<ReferenceTransition> ReferenceTransitions { get; private set; } = new ();

        public List<LocationImport> DeadLocations { get; private set; } = new ();
        public List<TransitionImport> DeadTransitions { get; private set; } = new ();

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
        private const string BreakableWFCPPool = "Breakable WFCPs";
        private const string GrassPool = "Grass";
        private const string JournalEntryPool = "Journal_Entry";
        private const string HuntersNotesPool = "Hunter's_Notes";
        private const string MoreLocationsPool = "MoreLocations";
        private const string GhostPool = "DreamGhost";
        private const string StatueMarkPool = "Statue Mark";
        private const string MilliGolfPool = "MilliGolf";

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
                // flibber-hk
                .Concat(RandoPlusItemImport())
                .Concat(SkillUpgradeItemImport())
                .Concat(FungalCityGateKeyItemImport())
                .Concat(LeverItemImport())

                // dpinela
                .Concat(TranscendenceItemImport())
                .Concat(RainbowEggItemImport())
                .Concat(Colo3ItemImport())

                // homothetyhk
                .Concat(BenchItemImport())

                // BadMagic100
                .Concat(TRJRItemImport())
                .Concat(MoreLocationsItemImport())

                // dplochcoder
                .Concat(MoreDoorsItemImport())
                .Concat(DarknessItemImport())

                // Hoo-Knows
                .Concat(LostArtifactsItemImport())

                // Korzer420
                .Concat(LoreMasterItemImport(itemImports.Where(x => x.Pool == "Lore").ToList()))
                .Concat(ExtraRandoItemImport())

                // Bentechy66
                .Concat(BreakableWallsItemImport())

                // nerthul11
                .Concat(BreakableWFCPItemImport())
                .Concat(GodhomeRandoItemImport())

                // StormZillaa
                .Concat(GrassRandoItemImport())

                // TheMathGeek314
                .Concat(MilliGolfItemImport())

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
                new () { Name = "White_Fragment", Pool = "Charm" },
                new () { Name = "Kingsoul", Pool = "Charm" },
                new () { Name = "Grimmchild", Pool = "Charm" },
            }).ToList();

        private static List<ReferenceItem> RandoPlusItemImport() =>
            new ()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new () { Name = "Mr_Mushroom_Level_Up", PreviewName = "Mr Mushroom Level Up", Pool = MrMushroomPool },
                new () { Name = "Nail_Upgrade", PreviewName = "Nail Upgrade", Pool = SkillPool },
                new () { Name = "Not_Isma's_Tear", PreviewName = "Not Isma's Tear", Pool = SkillPool },
                new () { Name = "Not_Lumafly_Lantern", PreviewName = "Not Lumafly Lantern", Pool = SkillPool },
                new () { Name = "Not_Swim", PreviewName = "Not Swim", Pool = SkillPool },
                
                new () { Name = "Ghost_Essence", PreviewName = "Ghost_Essence", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Gravedigger", PreviewName = "Ghost_Essence-Gravedigger", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Vespa", PreviewName = "Ghost_Essence-Vespa", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Joni", PreviewName = "Ghost_Essence-Joni", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Marissa", PreviewName = "Ghost_Essence-Marissa", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Poggy_Thorax", PreviewName = "Ghost_Essence-Poggy_Thorax", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Caelif_Fera_Orthop", PreviewName = "Ghost_Essence-Caelif_Fera_Orthop", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Cloth", PreviewName = "Ghost_Essence-Cloth", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Revek", PreviewName = "Ghost_Essence-Revek", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Millybug", PreviewName = "Ghost_Essence-Millybug", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Caspian", PreviewName = "Ghost_Essence-Caspian", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Atra", PreviewName = "Ghost_Essence-Atra", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Dr_Chagax", PreviewName = "Ghost_Essence-Dr_Chagax", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Garro", PreviewName = "Ghost_Essence-Garro", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Kcin", PreviewName = "Ghost_Essence-Kcin", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Karina", PreviewName = "Ghost_Essence-Karina", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Hundred_Nail_Warrior", PreviewName = "Ghost_Essence-Hundred_Nail_Warrior", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Grohac", PreviewName = "Ghost_Essence-Grohac", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Perpetos_Noo", PreviewName = "Ghost_Essence-Perpetos_Noo", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Molten", PreviewName = "Ghost_Essence-Molten", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Magnus_Strong", PreviewName = "Ghost_Essence-Magnus_Strong", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Waldie", PreviewName = "Ghost_Essence-Waldie", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Wayner", PreviewName = "Ghost_Essence-Wayner", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Wyatt", PreviewName = "Ghost_Essence-Wyatt", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Hex", PreviewName = "Ghost_Essence-Hex", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Thistlewind", PreviewName = "Ghost_Essence-Thistlewind", Pool = GhostPool },
                new () { Name = "Ghost_Essence-Boss", PreviewName = "Ghost_Essence-Boss", Pool = GhostPool },
            };

        private static List<ReferenceItem> SkillUpgradeItemImport() =>
            new ()
            {
                // https://github.com/flibber-hk/HollowKnight.SkillUpgrades
                new () { Name = "DirectionalDash", PreviewName = "Directional Dash", Pool = SkillUpgradePool },
                new () { Name = "DownwardFireball", PreviewName = "Downward Fireball", Pool = SkillUpgradePool },
                new () { Name = "ExtraAirDash", PreviewName = "Extra Air Dash", Pool = SkillUpgradePool },
                new () { Name = "GreatSlashShockwave", PreviewName = "Great Slash Shockwave", Pool = SkillUpgradePool },
                new () { Name = "HorizontalDive", PreviewName = "Horizontal Dive", Pool = SkillUpgradePool },
                new () { Name = "SpiralScream", PreviewName = "Spiral Scream", Pool = SkillUpgradePool },
                new () { Name = "TripleJump", PreviewName = "Triple Jump", Pool = SkillUpgradePool },
                new () { Name = "VerticalSuperdash", PreviewName = "Vertical Superdash", Pool = SkillUpgradePool },
                new () { Name = "WallClimb", PreviewName = "Wall Climb", Pool = SkillUpgradePool },
                new () { Name = "WingsGlide", PreviewName = "Wings Glide", Pool = SkillUpgradePool },
            };

        private static List<ReferenceItem> FungalCityGateKeyItemImport() =>
            new ()
            {
                // https://github.com/flibber-hk/HollowKnight.ReopenCityDoor
                new () { Name = "Fungal_City_Gate_Key", PreviewName = "Fungal City Gate Key", Pool = "Key" },
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
            new ()
            {
                // https://github.com/dpinela/Transcendence
                new () { Name = "Marissa's_Audience", PreviewName = "Marissa's Audience", Pool = TranscendencePool },
                new () { Name = "Lemm's_Strength", PreviewName = "Lemm's Strength", Pool = TranscendencePool },
                new () { Name = "Snail_Slash", PreviewName = "Snail Slash", Pool = TranscendencePool },
                new () { Name = "Millibelle's_Blessing", PreviewName = "Millibelle's Blessing", Pool = TranscendencePool },
                new () { Name = "Disinfectant_Flask", PreviewName = "Disinfectant Flask", Pool = TranscendencePool },
                new () { Name = "Florist's_Blessing", PreviewName = "Florist's Blessing", Pool = TranscendencePool },
                new () { Name = "Greedsong", PreviewName = "Greedsong", Pool = TranscendencePool },
                new () { Name = "Snail_Soul", PreviewName = "Snail Soul", Pool = TranscendencePool },
                new () { Name = "Nitro_Crystal", PreviewName = "Nitro Crystal", Pool = TranscendencePool },
                new () { Name = "Shaman_Amp", PreviewName = "Shaman Amp", Pool = TranscendencePool },
                new () { Name = "Crystalmaster", PreviewName = "Crystalmaster", Pool = TranscendencePool },
                new () { Name = "Bluemoth_Wings", PreviewName = "Bluemoth Wings", Pool = TranscendencePool },
                new () { Name = "Chaos_Orb", PreviewName = "Chaos Orb", Pool = TranscendencePool },
                new () { Name = "Antigravity_Amulet", PreviewName = "Antigravity Amulet", Pool = TranscendencePool },
                new () { Name = "Vespa's_Vengeance", PreviewName = "Vespa's Vengeance", Pool = TranscendencePool },
            };

        private static List<ReferenceItem> RainbowEggItemImport() =>
            new ()
            {
                // https://github.com/dpinela/RainbowEggs
                new () { Name = "Red_Egg", PreviewName = "Red Egg", Pool = EggPool },
                new () { Name = "Orange_Egg", PreviewName = "Orange Egg", Pool = EggPool },
                new () { Name = "Yellow_Egg", PreviewName = "Yellow Egg", Pool = EggPool },
                new () { Name = "Green_Egg", PreviewName = "Green Egg", Pool = EggPool },
                new () { Name = "Cyan_Egg", PreviewName = "Cyan Egg", Pool = EggPool },
                new () { Name = "Blue_Egg", PreviewName = "Blue Egg", Pool = EggPool },
                new () { Name = "Purple_Egg", PreviewName = "Purple Egg", Pool = EggPool },
                new () { Name = "Pink_Egg", PreviewName = "Pink Egg", Pool = EggPool },
                new () { Name = "Trans_Egg", PreviewName = "Trans Egg", Pool = EggPool },
                new () { Name = "Rainbow_Egg", PreviewName = "Rainbow Egg", Pool = EggPool },
                new () { Name = "Arcane_Eg", PreviewName = "Arcane Eg", Pool = EggPool },
            };

        private static List<ReferenceItem> Colo3ItemImport() =>
            new ()
            {
                new () { Name = "The_Glory_of_Being_a_Fool", PreviewName = "The Glory of Being a Fool", Pool = "Useless" },
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
                .SelectMany(x =>
                {
                    var previewName = x.IcName.Replace("_", " ");
                    return new List<ReferenceItem>
                    {
                        new () {
                            Name = $"Journal_Entry-{x.IcName}",
                            PreviewName = $"{previewName} Journal Entry",
                            Pool = JournalEntryPool,
                        },
                        new () {
                            Name = $"Hunter's_Notes-{x.IcName}",
                            PreviewName = $"{previewName} Hunter's Notes",
                            Pool = HuntersNotesPool,
                        },
                    };
                })
                .Append(new () { Name = "Hunter's_Mark", PreviewName = "Hunter's Mark", Pool = JournalEntryPool })
                .ToList();

        private static List<ReferenceItem> MoreLocationsItemImport() => new ()
        {
            new () {
                Name = "Wanderer's_Journal_Sale",
                PreviewName = "Wanderer's Journal Sale",
                Pool = MoreLocationsPool
            },
            new () {
                Name = "Hallownest_Seal_Sale",
                PreviewName = "Hallownest Seal Sale",
                Pool = MoreLocationsPool
            },
            new () {
                Name = "King's_Idol_Sale",
                PreviewName = "King's Idol Sale",
                Pool = MoreLocationsPool
            },
            new () {
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
            new ()
            {
                // https://github.com/Hoo-Knows/HollowKnight.LostArtifacts
                new () { Name = "TravelersGarment", PreviewName = "Travelers Garment", Pool = LostArtifactPool },
                new () { Name = "PavingStone", PreviewName = "Paving Stone", Pool = LostArtifactPool },
                new () { Name = "LushMoss", PreviewName = "Lush Moss", Pool = LostArtifactPool },
                new () { Name = "NoxiousShroom", PreviewName = "Noxious Shroom", Pool = LostArtifactPool },
                new () { Name = "CryingStatue", PreviewName = "Crying Statue", Pool = LostArtifactPool },
                new () { Name = "TotemShard", PreviewName = "Totem Shard", Pool = LostArtifactPool },
                new () { Name = "DungBall", PreviewName = "Dung Ball", Pool = LostArtifactPool },
                new () { Name = "Tumbleweed", PreviewName = "Tumbleweed", Pool = LostArtifactPool },
                new () { Name = "ChargedCrystal", PreviewName = "Charged Crystal", Pool = LostArtifactPool },
                new () { Name = "Dreamwood", PreviewName = "Dreamwood", Pool = LostArtifactPool },
                new () { Name = "LumaflyEssence", PreviewName = "Lumafly Essence", Pool = LostArtifactPool },
                new () { Name = "ThornedLeaf", PreviewName = "Thorned Leaf", Pool = LostArtifactPool },
                new () { Name = "WeaverSilk", PreviewName = "Weaver Silk", Pool = LostArtifactPool },
                new () { Name = "WyrmAsh", PreviewName = "Wyrm Ash", Pool = LostArtifactPool },
                new () { Name = "BeastShell", PreviewName = "Beast Shell", Pool = LostArtifactPool },
                new () { Name = "Honeydrop", PreviewName = "Honeydrop", Pool = LostArtifactPool },
                new () { Name = "InfectedRock", PreviewName = "Infected Rock", Pool = LostArtifactPool },
                new () { Name = "Buzzsaw", PreviewName = "Buzzsaw", Pool = LostArtifactPool },
                new () { Name = "VoidEmblem", PreviewName = "Void Emblem", Pool = LostArtifactPool },
                new () { Name = "AttunedJewel", PreviewName = "Attuned Jewel", Pool = LostArtifactPool },
                new () { Name = "HiddenMemento", PreviewName = "Hidden Memento", Pool = LostArtifactPool },
            };

        private static List<ReferenceItem> LoreMasterItemImport(List<ItemImport> defaultLorePoolItems) =>
            new List<ReferenceItem>()
            {
                // https://github.com/Korzer420/LoreMaster/
                new () { Name = "Magical_Key", PreviewName = "Magical Key" },
                new () { Name = "Dream_Medallion", PreviewName = "Dream Medallion" },
                new () { Name = "Silksong_Journal", PreviewName = "Silksong Journal?" },
                new () { Name = "Silver_Hallownest_Seal", PreviewName = "Silver Seal" },
                new () { Name = "Bronze_King_Idol", PreviewName = "Bronze King's Idol" },
                new () { Name = "Golden_Arcane_Egg", PreviewName = "Golden Arcane Egg" },
                new () { Name = "Lore_Tablet-Stag_Egg_Inspect", PreviewName = "Stag Adoption" },
                new () { Name = "Lore_Tablet-Record_Bela", PreviewName = "Lore Tablet Record Bela" },

                new () { Name = "Read_Ability", PreviewName = "Reading" },
                new () { Name = "Listen_Ability", PreviewName = "Listening" },
                new () { Name = "Lore_Page", PreviewName = "Lore Page" },
                new () { Name = "Lore_Page_Control", PreviewName = "Lore Control" },
                new () { Name = "Cleansing_Scroll", PreviewName = "Cleansing Scroll" },
                new () { Name = "Joker_Scroll", PreviewName = "Knowledge Scroll" },
                new () { Name = "Cleansing_Scroll_Double", PreviewName = "Cleansing Scroll Pack" },

                new () { Name = "Lemm_Order", PreviewName = "Lemm's Order" },
                new () { Name = "Traitor_Grave", PreviewName = "Traitor Grave" },

                new () { Name = "Dialogue-Bretta_Diary", PreviewName = "Bretta Diary" },
                new () { Name = "Dialogue-Bardoon", PreviewName = "Bardoon" },
                new () { Name = "Dialogue-Vespa", PreviewName = "Vespa" },
                new () { Name = "Dialogue-Mask_Maker", PreviewName = "Mask Maker" },
                new () { Name = "Dialogue-Midwife", PreviewName = "Midwife" },
                new () { Name = "Dialogue-Gravedigger", PreviewName = "Gravedigger" },
                new () { Name = "Dialogue-Poggy", PreviewName = "Poggy" },
                new () { Name = "Dialogue-Joni", PreviewName = "Joni" },
                new () { Name = "Dialogue-Myla", PreviewName = "Myla" },
                new () { Name = "Dialogue-Emilitia", PreviewName = "Emilitia" },
                new () { Name = "Dialogue-Willoh", PreviewName = "Willoh" },
                new () { Name = "Dialogue-Moss_Prophet", PreviewName = "Moss Prophet" },
                new () { Name = "Dialogue-Fluke_Hermit", PreviewName = "Fluke Hermit" },
                new () { Name = "Dialogue-Queen", PreviewName = "Queen" },
                new () { Name = "Dialogue-Marissa", PreviewName = "Marissa" },
                new () { Name = "Dialogue-Grasshopper", PreviewName = "Grasshopper" },
                new () { Name = "Dialogue-Dung_Defender", PreviewName = "Dung Defender" },
                new () { Name = "Dialogue-Menderbug_Diary", PreviewName = "Menderbug Diary" },

                new () { Name = "Inspect-Elder_Hu", PreviewName = "Elder Hu" },
                new () { Name = "Inspect-Xero", PreviewName = "Xero" },
                new () { Name = "Inspect-Galien", PreviewName = "Galien" },
                new () { Name = "Inspect-Marmu", PreviewName = "Marmu" },
                new () { Name = "Inspect-Gorb", PreviewName = "Gorb" },
                new () { Name = "Inspect-Markoth", PreviewName = "Markoth" },
                new () { Name = "Inspect-No_Eyes", PreviewName = "No Eyes" },

                new () { Name = "Dream_Dialogue-Aspid_Queen", PreviewName = "Aspid Queen" },
                new () { Name = "Dream_Dialogue-Mine_Golem", PreviewName = "Mine Golem" },
                new () { Name = "Dream_Dialogue-Hopper_Dummy", PreviewName = "Hopper Dummy" },
                new () { Name = "Dream_Dialogue-Ancient_Nailsmith_Golem", PreviewName = "Ancient Nailsmith Golem" },
                new () { Name = "Dream_Dialogue-Shriek_Statue", PreviewName = "Shriek Statue" },
                new () { Name = "Dream_Dialogue-Shade_Golem_Normal", PreviewName = "Shade Golem Normal" },
                new () { Name = "Dream_Dialogue-Shade_Golem_Void", PreviewName = "Shade Golem Void" },
                new () { Name = "Dream_Dialogue-Overgrown_Shaman", PreviewName = "Overgrown Shaman" },
                new () { Name = "Dream_Dialogue-Crystalized_Shaman", PreviewName = "Crystalized Shaman" },
                new () { Name = "Dream_Dialogue-Shroom_King", PreviewName = "Shroom King" },
                new () { Name = "Dream_Dialogue-Dryya", PreviewName = "Dryya" },
                new () { Name = "Dream_Dialogue-Isma", PreviewName = "Isma" },
                new () { Name = "Dream_Dialogue-Radiance_Statue", PreviewName = "Radiance Statue" },
                new () { Name = "Dream_Dialogue-Dashmaster_Statue", PreviewName = "Dashmaster Statue" },
                new () { Name = "Dream_Dialogue-Snail_Shaman_Tomb", PreviewName = "Snail Shaman Tomb" },
                new () { Name = "Dream_Dialogue-Pale_King", PreviewName = "Pale King" },
                new () { Name = "Dream_Dialogue-Grimm_Summoner", PreviewName = "Grimm Summoner" },
                new () { Name = "Dream_Dialogue-Kings_Mould", PreviewName = "Kings Mould" },
                new () { Name = "Dream_Dialogue-Dream_Shield_Statue", PreviewName = "Dream Shield Statue" },

                new () { Name = "Inscription-City_Fountain", PreviewName = "City Fountain" },
                new () { Name = "Inscription-Dreamer_Tablet", PreviewName = "Dreamer Tablet" },

                new () { Name = "Inspect-Beast_Den_Altar", PreviewName = "Beast Den Altar" },
                new () { Name = "Inspect-Weaver_Seal", PreviewName = "Weaver Seal" },
                new () { Name = "Inspect-Grimm_Machine", PreviewName = "Grimm Machine" },
                new () { Name = "Inspect-Garden_Golem", PreviewName = "Garden Golem" },
                new () { Name = "Inspect-Grub_Seal", PreviewName = "Grub Seal" },
                new () { Name = "Inspect-White_Palace_Nursery", PreviewName = "White Palace Nursery" },
                new () { Name = "Inspect-Grimm_Summoner_Corpse", PreviewName = "Grimm Summoner Corpse" },

                new () { Name = "Dialogue-Quirrel_Crossroads", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Greenpath", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Queen_Station", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Mantis_Village", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_City", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Deepnest", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Peaks", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Outside_Archive", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Archive", PreviewName = "Quirrel" },
                new () { Name = "Dialogue-Quirrel_Blue_Lake", PreviewName = "Quirrel" },

                new () { Name = "Dialogue-Cloth_Fungal_Wastes", PreviewName = "Cloth" },
                new () { Name = "Dialogue-Cloth_Basin", PreviewName = "Cloth" },
                new () { Name = "Dialogue-Cloth_Deepnest", PreviewName = "Cloth" },
                new () { Name = "Dialogue-Cloth_Garden", PreviewName = "Cloth" },
                new () { Name = "Dialogue-Cloth_Ghost", PreviewName = "Cloth" },

                new () { Name = "Dialogue-Tiso_Dirtmouth", PreviewName = "Tiso" },
                new () { Name = "Dialogue-Tiso_Crossroads", PreviewName = "Tiso" },
                new () { Name = "Dialogue-Tiso_Blue_Lake", PreviewName = "Tiso" },
                new () { Name = "Dialogue-Tiso_Colosseum", PreviewName = "Tiso" },
                new () { Name = "Dream_Dialogue-Tiso_Corpse", PreviewName = "Tiso" },

                new () { Name = "Dialogue-Zote_Greenpath", PreviewName = "Zote" },
                new () { Name = "Dialogue-Zote_Dirtmouth_Intro", PreviewName = "Zote" },
                new () { Name = "Dialogue-Zote_City", PreviewName = "Zote" },
                new () { Name = "Dialogue-Zote_Deepnest", PreviewName = "Zote" },
                new () { Name = "Dialogue-Zote_Colosseum", PreviewName = "Zote" },
                new () { Name = "Dialogue-Zote_Dirtmouth_After_Colosseum", PreviewName = "Zote" },
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
            // https://github.com/Korzer420/ExtraRando
            new()
            {
                // Split Shade Cloak
                new () { Name = "Progressive_Left_Cloak", PreviewName = "Progressive Left Cloak", Pool = SkillPool },
                new () { Name = "Progressive_Right_Cloak", PreviewName = "Progressive Right Cloak", Pool = SkillPool },

                // Bardoons Butt
                new () { Name = "Bardoon_Butt_Smack", PreviewName = "Bardoon Butt Smack", Pool = "Useless" },

                // Hot Springs
                new () { Name = "Hot_Spring_Water", PreviewName = "Hot Spring Water", Pool = "Hot Spring" },

                // Colo Access
                new () { Name = "Colo_Ticket-Bronze", PreviewName = "Colo Ticket - Bronze", Pool = "Colo" },
                new () { Name = "Colo_Ticket-Silver", PreviewName = "Colo Ticket - Silver", Pool = "Colo" },
                new () { Name = "Colo_Ticket-Gold", PreviewName = "Colo Ticket - Gold", Pool = "Colo" },
            };

        private static List<ReferenceItem> DarknessItemImport() =>
            new ()
            {
                // https://github.com/dplochcoder/HollowKnight.DarknessRandomizer
                new () { Name = "Lantern_Shard", PreviewName = "Lantern Shard (#0)", Pool = "Key" },
                new () { Name = "Final_Lantern_Shard", PreviewName = "Final Lantern Shard", Pool = "Key" },
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

        private class BreakableWFCP : BasicLocation { }
        private static List<ReferenceItem> BreakableWFCPItemImport() =>
            // https://github.com/nerthul11/BreakableWallRandomizer
            LoadListFile<BreakableWFCP>(ReferenceBreakableWFCPFilePath)
            .Select(x => new ReferenceItem
            {
                Name = x.Name,
                PreviewName = x.Name.Replace("-", " - ").WithoutUnderscores(),
                Pool = BreakableWFCPPool
            }).ToList();

        private class StatueItem
        {
            public string Name;
        }
        private static List<ReferenceItem> GodhomeRandoItemImport() =>
            // https://github.com/nerthul11/GodhomeRandomizer
            LoadListFile<StatueItem>(ReferenceStatueItemsFilePath)
            .Select(x => new ReferenceItem
            {
                Name = x.Name,
                PreviewName = x.Name.Replace("-", " - ").WithoutUnderscores(),
                Pool = StatueMarkPool
            })
            .ToList();

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

        private static readonly List<BasicLocation> MilliGolfLocationList = new ()
        {
            new BasicLocation { Name = "Dirtmouth", SceneName = "Town"},
            new BasicLocation { Name = "Crossroads", SceneName = "Crossroads_07"},
            new BasicLocation { Name = "Grounds", SceneName = "RestingGrounds_05"},
            new BasicLocation { Name = "Hive", SceneName = "Hive_03"},
            new BasicLocation { Name = "Greenpath", SceneName = "Fungus1_31"},
            new BasicLocation { Name = "Canyon", SceneName = "Fungus3_02"},
            new BasicLocation { Name = "Edge", SceneName = "Deepnest_East_11"},
            new BasicLocation { Name = "Waterways", SceneName = "Waterways_02"},
            new BasicLocation { Name = "Cliffs", SceneName = "Cliffs_01"},
            new BasicLocation { Name = "Abyss", SceneName = "Abyss_06_Core"},
            new BasicLocation { Name = "Fungal", SceneName = "Fungus2_12"},
            new BasicLocation { Name = "Sanctum", SceneName = "Ruins1_30"},
            new BasicLocation { Name = "Basin", SceneName = "Abyss_04"},
            new BasicLocation { Name = "Gardens", SceneName = "Fungus3_04"},
            new BasicLocation { Name = "City", SceneName = "Ruins1_03"},
            new BasicLocation { Name = "Deepnest", SceneName = "Deepnest_35"},
            new BasicLocation { Name = "Peak", SceneName = "Mines_23"},
            new BasicLocation { Name = "Palace", SceneName = "White_Palace_19"},
        };

        private static List<ReferenceItem> MilliGolfItemImport() =>
            // https://github.com/TheMathGeek314/MilliGolf
            MilliGolfLocationList.SelectMany(x => new List<ReferenceItem>
            {
                new () { Name = $"Course_Access-{x.Name}", PreviewName = $"Course Access - {x.Name}", Pool = MilliGolfPool },
                new () { Name = $"Course_Completion-{x.Name}", PreviewName = $"Course Completion - {x.Name}", Pool = MilliGolfPool },
            })
            .ToList();

        private static List<ReferenceItem> UnidentifiedItems() =>
            new ()
            {
                new () { Name = "Chandelier-Watcher_Knights", PreviewName = "Chandelier-Watcher Knights", Pool = "Useless" },
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
                new () { SceneName = "Room_Tram_RG", MapArea = "Tram", TitledArea = "Tram" },
                new () { SceneName = "Room_Tram", MapArea = "Tram", TitledArea = "Tram" },
                new () { SceneName = "Room_Final_Boss_Atrium", MapArea = "Black Egg Temple", TitledArea = "Black Egg Temple" },
                new () { SceneName = "GG_Atrium", MapArea = "Godhome", TitledArea = "Godhome" },
                new () { SceneName = "GG_Atrium_Roof", MapArea = "Godhome", TitledArea = "Godhome" },
                new () { SceneName = "GG_Workshop", MapArea = "Godhome", TitledArea = "Godhome" },
                new () { SceneName = "GG_Unlock_Wastes", MapArea = "Godhome", TitledArea = "Godhome" },
                new () { SceneName = "Town/Fungus3_23", MapArea = "Dirthmouth/Queen's Garden", TitledArea = "Dirthmouth/Queen's Garden" },
                new () { SceneName = "> Multiworld", MapArea = "> Multiworld", TitledArea = "> Multiworld" },
                new () { SceneName = "> Hunter's Notes", MapArea = "> Hunter's Notes", TitledArea = "> Hunter's Notes" },
                new () { SceneName = "> Journal Entry", MapArea = "> Journal Entry", TitledArea = "> Journal Entry" },
                
                new () { SceneName = "Dream_01_False_Knight", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "Dream_02_Mage_Lord", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "Dream_Backer_Shrine", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "Dream_Nailcollection", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Broken_Vessel", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Ghost_Marmu", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Ghost_Marmu_V", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Hornet_2", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Lost_Kin", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Mega_Moss_Charger", MapArea = "> Grass", TitledArea = "> Grass" },
                new () { SceneName = "GG_Traitor_Lord", MapArea = "> Grass", TitledArea = "> Grass" },
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

                // flibber-hk
                .Concat(RandoPlusLocationImport())
                .Concat(LeverLocationImport())

                // dpinela
                .Concat(Colo3LocationImport())
                .Concat(FishingLocationImport())

                // homothetyhk
                .Concat(BenchLocationImport())

                // BadMagic100
                .Concat(TRJRLocationImport())
                .Concat(MoreLocationsLocationImport())

                // dplochcoder
                .Concat(MoreDoorsLocationImport())

                // Hoo-Knows
                .Concat(LostArtifactsLocationImport())

                // Korzer420
                .Concat(LoreMasterLocationImport())
                .Concat(ExtraRandoLocationImport())

                // Bentechy66
                .Concat(BreakableWallsLocationImport())
                
                // nerthul11
                .Concat(BreakableWFCPLocationImport())
                .Concat(GodhomeRandoLocationImport())
                .Concat (FlowerQuestLocationImport())

                // StormZillaa
                .Concat(GrassRandoLocationImport())

                // ManicJamie
                .Concat(GrassRandoForkLocationImport())

                // TheMathGeek314
                .Concat(MilliGolfLocationImport())

                // IronLucario2012
                .Concat(MaskMakerNotchLocationImport())

                // unidentified locations
                .Concat(UnidentifiedLocationImport())

                .ToList();
        }

        private static List<ReferenceLocation> MultiWorldImport() =>
            new ()
            {
                new () { Name = "Remote", SceneName = "> Multiworld", Pool = "> Multiworld" },
            };

        private class GhostLocation : BasicLocation { }
        private static List<ReferenceLocation> RandoPlusLocationImport() =>
            new List<ReferenceLocation>()
            {
                // https://github.com/flibber-hk/HollowKnight.RandoPlus
                new () { Name = "Mr_Mushroom-Fungal_Wastes", SceneName = "Fungus2_18", Pool = MrMushroomPool },
                new () { Name = "Mr_Mushroom-Kingdom's_Edge", SceneName = "Deepnest_East_01", Pool = MrMushroomPool },
                new () { Name = "Mr_Mushroom-Deepnest", SceneName = "Deepnest_40", Pool = MrMushroomPool },
                new () { Name = "Mr_Mushroom-Howling_Cliffs", SceneName = "Room_nailmaster", Pool = MrMushroomPool },
                new () { Name = "Mr_Mushroom-Ancient_Basin", SceneName = "Abyss_21", Pool = MrMushroomPool },
                new () { Name = "Mr_Mushroom-Fog_Canyon", SceneName = "Fungus3_44", Pool = MrMushroomPool },
                new () { Name = "Mr_Mushroom-King's_Pass", SceneName = "Tutorial_01", Pool = MrMushroomPool },

                new () { Name = "Nailsmith_Upgrade_1", SceneName = "Room_nailsmith", Pool = "Shop" },
                new () { Name = "Nailsmith_Upgrade_2", SceneName = "Room_nailsmith", Pool = "Shop" },
                new () { Name = "Nailsmith_Upgrade_3", SceneName = "Room_nailsmith", Pool = "Shop" },
                new () { Name = "Nailsmith_Upgrade_4", SceneName = "Room_nailsmith", Pool = "Shop" },
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
            new ()
            {
                new ()
                {
                    Name = "The_Glory_of_Being_a_Fool-Colosseum",
                    SceneName = "Room_Colosseum_01",
                    Pool = "Colo",
                }
            };

        private static List<ReferenceLocation> FishingLocationImport() =>
            // https://github.com/dpinela/Fishing
            new()
            {
                new () { Name = "Fishing_Spot-Lake_of_Unn", SceneName = "Fungus1_26", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Distant_Village", SceneName = "Deepnest_10", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-West_Lake_Shore", SceneName = "Crossroads_50", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-East_Lake_Shore", SceneName = "Crossroads_50", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Waterways_Central_Pool", SceneName = "Waterways_04", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Waterways_Mask_Shard_Pool", SceneName = "Waterways_04b", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Waterways_Long_Acid_Pool", SceneName = "Waterways_14", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Abyss_Lighthouse", SceneName = "Abyss_09", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Godhome_Atrium", SceneName = "GG_Atrium", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Junk_Pit", SceneName = "GG_Waterways", Pool = "Fishing" },
                new () { Name = "Fishing_Spot-Pale_Lurker", SceneName = "GG_Lurker", Pool = "Fishing" },
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
                .SelectMany(x => new List<ReferenceLocation>
                {
                    new () {
                        Name = $"Journal_Entry-{x.IcName}",
                        SceneName = "> Journal Entry",
                        Pool = JournalEntryPool,
                    },
                    new () {
                        Name = $"Hunter's_Notes-{x.IcName}",
                        SceneName = "> Hunter's Notes",
                        Pool = HuntersNotesPool,
                    },
                })
                .Append(new () { Name = "Hunter's_Mark", SceneName = "Fungus1_08", Pool = "Journal_Entry" })
                .ToList();

        private static List<ReferenceLocation> MoreLocationsLocationImport() =>
            new ()
            {
                new ()
                {
                    Name = "Swim",
                    SceneName = "Crossroads_50",
                    Pool = MoreLocationsPool,
                },
                new ()
                {
                    Name = "Stag_Nest_Egg",
                    SceneName = "Cliffs_03",
                    Pool = MoreLocationsPool,
                },
                new ()
                {
                    Name = "Geo_Chest-Above_Baldur_Shell",
                    SceneName = "Cliffs_03",
                    Pool = MoreLocationsPool,
                },
                new ()
                {
                    Name = "Lemm",
                    SceneName = "Ruins1_05b",
                    Pool = "Shop",
                },
                new ()
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
            new ()
            {
                // https://github.com/Hoo-Knows/HollowKnight.LostArtifacts
                new () { Name = "AttunedJewel", SceneName = "GG_Workshop", Pool = LostArtifactPool },
                new () { Name = "BeastShell", SceneName = "Room_Colosseum_01", Pool = LostArtifactPool },
                new () { Name = "Buzzsaw", SceneName = "White_Palace_08", Pool = LostArtifactPool },
                new () { Name = "ChargedCrystal", SceneName = "Mines_18", Pool = LostArtifactPool },
                new () { Name = "CryingStatue", SceneName = "Ruins1_27", Pool = LostArtifactPool },
                new () { Name = "Dreamwood", SceneName = "RestingGrounds_05", Pool = LostArtifactPool },
                new () { Name = "DungBall", SceneName = "Waterways_15", Pool = LostArtifactPool },
                new () { Name = "HiddenMemento", SceneName = "White_Palace_06", Pool = LostArtifactPool },
                new () { Name = "Honeydrop", SceneName = "Hive_01", Pool = LostArtifactPool },
                new () { Name = "InfectedRock", SceneName = "Abyss_19", Pool = LostArtifactPool },
                new () { Name = "LumaflyEssence", SceneName = "Fungus3_archive_02", Pool = LostArtifactPool },
                new () { Name = "LushMoss", SceneName = "Fungus1_29", Pool = LostArtifactPool },
                new () { Name = "NoxiousShroom", SceneName = "Fungus2_30", Pool = LostArtifactPool },
                new () { Name = "PavingStone", SceneName = "Crossroads_47", Pool = LostArtifactPool },
                new () { Name = "ThornedLeaf", SceneName = "Fungus3_10", Pool = LostArtifactPool },
                new () { Name = "TotemShard", SceneName = "Ruins1_32", Pool = LostArtifactPool },
                new () { Name = "TravelersGarment", SceneName = "Town", Pool = LostArtifactPool },
                new () { Name = "Tumbleweed", SceneName = "Cliffs_01", Pool = LostArtifactPool },
                new () { Name = "VoidEmblem", SceneName = "Abyss_09", Pool = LostArtifactPool },
                new () { Name = "WeaverSilk", SceneName = "Deepnest_45_v02", Pool = LostArtifactPool },
                new () { Name = "WyrmAsh", SceneName = "Deepnest_East_12", Pool = LostArtifactPool },
            };

        private static List<ReferenceLocation> LoreMasterLocationImport() =>
            new ()
            {
                // https://github.com/Korzer420/LoreMaster/
                new () { Name = "Bretta_Diary", SceneName = "Room_Bretta", Pool = LoreMasterPool },
                new () { Name = "Bardoon", SceneName = "Deepnest_East_04", Pool = LoreMasterPool },
                new () { Name = "Vespa", SceneName = "Hive_05", Pool = LoreMasterPool },
                new () { Name = "Mask_Maker", SceneName = "Room_Mask_Maker", Pool = LoreMasterPool },
                new () { Name = "Midwife", SceneName = "Deepnest_41", Pool = LoreMasterPool },
                new () { Name = "Gravedigger", SceneName = "Town", Pool = LoreMasterPool },
                new () { Name = "Poggy", SceneName = "Ruins_Elevator", Pool = LoreMasterPool },
                new () { Name = "Joni", SceneName = "Cliffs_05", Pool = LoreMasterPool },
                new () { Name = "Myla", SceneName = "Crossroads_45", Pool = LoreMasterPool },
                new () { Name = "Emilitia", SceneName = "Ruins_House_03", Pool = LoreMasterPool },
                new () { Name = "Willoh", SceneName = "Fungus2_34", Pool = LoreMasterPool },
                new () { Name = "Moss_Prophet", SceneName = "Fungus3_39", Pool = LoreMasterPool },
                new () { Name = "Fluke_Hermit", SceneName = "Room_GG_Shortcut", Pool = LoreMasterPool },
                new () { Name = "Queen", SceneName = "Room_Queen", Pool = LoreMasterPool },
                new () { Name = "Marissa", SceneName = "Ruins_Bathhouse", Pool = LoreMasterPool },
                new () { Name = "Grasshopper", SceneName = "Fungus1_24", Pool = LoreMasterPool },
                new () { Name = "Dung_Defender", SceneName = "Waterways_05", Pool = LoreMasterPool },
                new () { Name = "Menderbug_Diary", SceneName = "Room_Mender_House", Pool = LoreMasterPool },
                
                new () { Name = "Elder_Hu_Grave", SceneName = "Fungus2_32", Pool = LoreMasterPool },
                new () { Name = "Xero_Grave", SceneName = "RestingGrounds_02", Pool = LoreMasterPool },
                new () { Name = "Gorb_Grave", SceneName = "Cliffs_02", Pool = LoreMasterPool },
                new () { Name = "Marmu_Grave", SceneName = "Fungus3_40", Pool = LoreMasterPool },
                new () { Name = "No_Eyes_Statue", SceneName = "Fungus1_35", Pool = LoreMasterPool },
                new () { Name = "Markoth_Corpse", SceneName = "Deepnest_East_10", Pool = LoreMasterPool },
                new () { Name = "Galien_Corpse", SceneName = "Deepnest_40", Pool = LoreMasterPool },
                
                new () { Name = "Aspid_Queen_Dream", SceneName = "Crossroads_22", Pool = LoreMasterPool },
                new () { Name = "Mine_Golem_Dream", SceneName = "Mines_31", Pool = LoreMasterPool },
                new () { Name = "Hopper_Dummy_Dream", SceneName = "Deepnest_East_16", Pool = LoreMasterPool },
                new () { Name = "Ancient_Nailsmith_Golem_Dream", SceneName = "Deepnest_East_14b", Pool = LoreMasterPool },
                new () { Name = "Shriek_Statue_Dream", SceneName = "Abyss_12", Pool = LoreMasterPool },
                new () { Name = "Overgrown_Shaman_Dream", SceneName = "Room_Fungus_Shaman", Pool = LoreMasterPool },
                new () { Name = "Shroom_King_Dream", SceneName = "Fungus2_30", Pool = LoreMasterPool },
                new () { Name = "Dryya_Dream", SceneName = "Fungus3_48", Pool = LoreMasterPool },
                new () { Name = "Isma_Dream", SceneName = "Waterways_13", Pool = LoreMasterPool },
                new () { Name = "Radiance_Statue_Dream", SceneName = "Mines_34", Pool = LoreMasterPool },
                new () { Name = "Dashmaster_Statue_Dream", SceneName = "Fungus2_23", Pool = LoreMasterPool },
                new () { Name = "Snail_Shaman_Tomb_Dream", SceneName = "RestingGrounds_10", Pool = LoreMasterPool },
                new () { Name = "Kings_Mould_Machine_Dream", SceneName = "White_Palace_08", Pool = LoreMasterPool },
                new () { Name = "Dream_Shield_Statue_Dream", SceneName = "RestingGrounds_17", Pool = LoreMasterPool },
                
                new () { Name = "Shade_Golem_Dream_Normal", SceneName = "Abyss_10", Pool = LoreMasterPool },
                new () { Name = "Shade_Golem_Dream_Void", SceneName = "Abyss_10", Pool = LoreMasterPool },
                new () { Name = "Pale_King_Dream", SceneName = "White_Palace_09", Pool = LoreMasterPool },
                new () { Name = "Crystalized_Shaman_Dream", SceneName = "Mines_35", Pool = LoreMasterPool },
                new () { Name = "Grimm_Summoner_Dream", SceneName = "Cliffs_06", Pool = LoreMasterPool },
                
                new () { Name = "City_Fountain", SceneName = "Ruins1_27", Pool = LoreMasterPool },
                new () { Name = "Dreamer_Tablet", SceneName = "RestingGrounds_04", Pool = LoreMasterPool },
                new () { Name = "Beast_Den_Altar", SceneName = "Deepnest_Spider_Town", Pool = LoreMasterPool },
                new () { Name = "Weaver_Seal", SceneName = "Deepnest_45_v02", Pool = LoreMasterPool },
                new () { Name = "Grimm_Machine", SceneName = "Grimm_Main_Tent", Pool = LoreMasterPool },
                new () { Name = "Garden_Golem", SceneName = "Fungus1_23", Pool = LoreMasterPool },
                new () { Name = "Grub_Seal", SceneName = "Ruins2_11", Pool = LoreMasterPool },
                new () { Name = "Path_Of_Pain_Seal", SceneName = "White_Palace_18", Pool = LoreMasterPool },
                new () { Name = "White_Palace_Nursery", SceneName = "White_Palace_09", Pool = LoreMasterPool },
                new () { Name = "Grimm_Summoner_Corpse", SceneName = "Cliffs_06", Pool = LoreMasterPool },
                
                new () { Name = "Quirrel_Crossroads", SceneName = "Room_temple", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Greenpath", SceneName = "Room_Slug_Shrine", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Queen_Station", SceneName = "Fungus2_01", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Mantis_Village", SceneName = "Fungus2_14", Pool = LoreMasterPool },
                new () { Name = "Quirrel_City", SceneName = "Ruins1_02", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Peaks", SceneName = "Mines_13", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Deepnest", SceneName = "Deepnest_30", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Outside_Archive", SceneName = "Fungus3_47", Pool = LoreMasterPool },
                new () { Name = "Quirrel_After_Monomon", SceneName = "Fungus3_archive_02", Pool = LoreMasterPool },
                new () { Name = "Quirrel_Blue_Lake", SceneName = "Crossroads_50", Pool = LoreMasterPool },

                new () { Name = "Cloth_Fungal_Wastes", SceneName = "Fungus2_09", Pool = LoreMasterPool },
                new () { Name = "Cloth_Basin", SceneName = "Abyss_17", Pool = LoreMasterPool },
                new () { Name = "Cloth_Deepnest", SceneName = "Deepnest_14", Pool = LoreMasterPool },
                new () { Name = "Cloth_Garden", SceneName = "Fungus3_34", Pool = LoreMasterPool },
                new () { Name = "Cloth_Ghost", SceneName = "Fungus3_23", Pool = LoreMasterPool },
                new () { Name = "Cloth_Town", SceneName = "Town", Pool = LoreMasterPool },
                new () { Name = "Cloth_End", SceneName = "Town/Fungus3_23", Pool = LoreMasterPool }, 
                
                new () { Name = "Tiso_Dirtmouth", SceneName = "Town", Pool = LoreMasterPool },
                new () { Name = "Tiso_Crossroads", SceneName = "Crossroads_47", Pool = LoreMasterPool },
                new () { Name = "Tiso_Blue_Lake", SceneName = "Crossroads_50", Pool = LoreMasterPool },
                new () { Name = "Tiso_Colosseum", SceneName = "Room_Colosseum_02", Pool = LoreMasterPool },
                new () { Name = "Tiso_Corpse", SceneName = "Deepnest_East_07", Pool = LoreMasterPool },
                
                new () { Name = "Zote_Greenpath", SceneName = "Fungus1_20_v02", Pool = LoreMasterPool },
                new () { Name = "Zote_Dirtmouth_Intro", SceneName = "Town", Pool = LoreMasterPool },
                new () { Name = "Zote_City", SceneName = "Ruins1_06", Pool = LoreMasterPool },
                new () { Name = "Zote_Deepnest", SceneName = "Deepnest_33", Pool = LoreMasterPool },
                new () { Name = "Zote_Colosseum", SceneName = "Room_Colosseum_02", Pool = LoreMasterPool },
                new () { Name = "Zote_Dirtmouth_After_Colosseum", SceneName = "Town", Pool = LoreMasterPool },
                
                new () { Name = "Elderbug_Reward_1", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_2", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_3", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_4", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_5", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_6", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_7", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_8", SceneName="Town", Pool = LoreMasterPool },
                new () { Name = "Elderbug_Reward_9", SceneName="Town", Pool = LoreMasterPool },
                
                new () { Name = "Stag_Nest", SceneName="Cliffs_03", Pool = LoreMasterPool },
                new () { Name = "Lemm_Door", SceneName="Ruins1_05b", Pool = LoreMasterPool },
                new () { Name = "Lore_Tablet-Record_Bela", SceneName="Ruins1_30", Pool = LoreMasterPool },
                new () { Name = "Traitor_Grave", SceneName="Fungus3_49", Pool = LoreMasterPool },
                
                new () { Name = "Treasure-Howling_Cliffs", SceneName="Cliffs_01", Pool = LoreMasterPool },
                new () { Name = "Treasure-Crossroads", SceneName="Crossroads_42", Pool = LoreMasterPool },
                new () { Name = "Treasure-Greenpath", SceneName="Fungus1_Slug", Pool = LoreMasterPool },
                new () { Name = "Treasure-Fog_Canyon", SceneName="Fungus3_archive_02", Pool = LoreMasterPool },
                new () { Name = "Treasure-Fog_Canyon", SceneName="Fungus3_archive_02", Pool = LoreMasterPool },
                new () { Name = "Treasure-Fungal_Wastes", SceneName="Fungus2_10", Pool = LoreMasterPool },
                new () { Name = "Treasure-City_Of_Tears", SceneName="Ruins2_05", Pool = LoreMasterPool },
                new () { Name = "Treasure-Waterways", SceneName="Waterways_13", Pool = LoreMasterPool },
                new () { Name = "Treasure-Deepnest", SceneName="Deepnest_30", Pool = LoreMasterPool },
                new () { Name = "Treasure-Ancient_Basin", SceneName="Abyss_06_Core", Pool = LoreMasterPool },
                new () { Name = "Treasure-Kingdoms_Edge", SceneName="GG_Lurker", Pool = LoreMasterPool },
                new () { Name = "Treasure-Crystal_Peaks", SceneName="Mines_02", Pool = LoreMasterPool },
                new () { Name = "Treasure-Resting_Grounds", SceneName="RestingGrounds_08", Pool = LoreMasterPool },
                new () { Name = "Treasure-Queens_Garden", SceneName="Fungus3_04", Pool = LoreMasterPool },
                new () { Name = "Treasure-White_Palace", SceneName="White_Palace_01", Pool = LoreMasterPool },
            };

        private static List<ReferenceLocation> ExtraRandoLocationImport() =>
            // https://github.com/Korzer420/ExtraRando
            new()
            {
                // Bardoons Butt
                new () { Name = "Bardoon_Butt", SceneName = "Deepnest_East_04", Pool = "Useless" },
                
                // Hotsprings
                new () { Name = "Bathhouse-Hot_Spring", SceneName = "Ruins_Bathhouse", Pool = "Hot Spring" },
                new () { Name = "Colosseum-Hot_Spring", SceneName = "Room_Colosseum_02", Pool = "Hot Spring" },
                new () { Name = "Deepnest-Hot_Spring", SceneName = "Deepnest_30", Pool = "Hot Spring" },
                new () { Name = "Lower_Godhome-Hot_Spring", SceneName = "GG_Atrium", Pool = "Hot Spring" },
                new () { Name = "Upper_Godhome-Hot_Spring", SceneName = "GG_Atrium_Roof", Pool = "Hot Spring" },
                new () { Name = "Crossroads-Hot_Spring", SceneName = "Crossroads_30", Pool = "Hot Spring" },
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

        private static List<ReferenceLocation> BreakableWFCPLocationImport() =>
            // https://github.com/nerthul11/BreakableWallRandomizer
            LoadListFile<BreakableWFCP>(ReferenceBreakableWFCPFilePath)
                .Select(x => new ReferenceLocation
                {
                    Name = x.Name,
                    SceneName = x.SceneName,
                    Pool = BreakableWFCPPool,
                })
                .Append(new () { Name = "Myla_Shop", SceneName = "Crossroads_45", Pool = "Shop" })
                .ToList();

        private class StatueLocation : BasicLocation { }
        private static List<ReferenceLocation> GodhomeRandoLocationImport() =>
            // https://github.com/nerthul11/GodhomeRandomizer
            LoadListFile<StatueLocation>(ReferenceStatueLocationsFilePath)
                .Select(x => new ReferenceLocation
                {
                    Name = x.Name,
                    SceneName = x.SceneName,
                    Pool = $"{StatueMarkPool} [{x.Name.Split("-")[0].Split("_")[0]}]",
                })
                .Append(new () { Name = "Godhome_Shop", SceneName = "GG_Unlock_Wastes", Pool = "Shop" })
            .ToList();

        private static List<ReferenceLocation> FlowerQuestLocationImport() =>
            // https://github.com/nerthul11/FlowerRandomizer
            new()
            {
                // Vanilla NPCs
                new () { Name = "Flower_Quest-Elderbug", SceneName = "Town", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Oro", SceneName = "Room_nailmaster_03", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Godseeker", SceneName = "GG_Waterways", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Emilitia", SceneName = "Ruins_House_03", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-White_Lady", SceneName = "Room_Queen", Pool = "Flower Quest"},

                // Custom NPCs
                new () { Name = "Flower_Quest-Mato", SceneName = "Room_nailmaster", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Sheo", SceneName = "Room_nailmaster_02", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Marissa", SceneName = "Ruins_Bathhouse", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Midwife", SceneName = "Deepnest_41", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Isma", SceneName = "Waterways_13", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Radiance", SceneName = "Mines_34", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Pale_King", SceneName = "White_Palace_09", Pool = "Flower Quest"},
                new () { Name = "Flower_Quest-Pain", SceneName = "White_Palace_20", Pool = "Flower Quest"},
            };

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

        private static List<ReferenceLocation> GrassRandoForkLocationImport() =>
            // https://github.com/ManicJamie/HollowKnightGrassRando
            new()
            {
                new () { Name = "Grass_Shop", SceneName = "Room_Slug_Shrine", Pool = "Shop" }
            };

        private static List<ReferenceLocation> MilliGolfLocationImport() =>
            // https://github.com/TheMathGeek314/MilliGolf
            MilliGolfLocationList.Select(x => new ReferenceLocation
            {
                Name = $"Milligolf_Course-{x.Name}",
                SceneName = x.SceneName,
                Pool = MilliGolfPool,
            })
            .ToList();

        private static List<ReferenceLocation> MaskMakerNotchLocationImport() =>
            // https://github.com/IronLucario2012/MaskMakerNotchesMod
            Enumerable.Range(0, 6).Select(x => new ReferenceLocation { Name = $"MMNotchLoc{x}", SceneName = "Room_Mask_Maker", Pool = "MMNotch" })
                .ToList();

        private static List<ReferenceLocation> UnidentifiedLocationImport() =>
            new ()
            {
                new () { Name = "Chandelier-Watcher_Knights", SceneName = "Ruins2_03", Pool = "Useless" },
                new () { Name = "chandelierLocation", SceneName = "Ruins2_03", Pool = "Useless" },
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
