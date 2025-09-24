using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace HK_Rando_4_Log_Display.Constants
{
    public static class Constants
    {
        public static readonly string AppVersion = "v2.1.0.3";

        public enum RoomGrouping
        {
            [EnumMember(Value = "Map\u00A0Areas")]
            MapArea = 0,
            [EnumMember(Value = "Titled\u00A0Areas")]
            TitleArea = 1,
            [EnumMember(Value = "Rooms in Map\u00A0Areas")]
            RoomMapArea = 2,
            [EnumMember(Value = "Rooms in Titled\u00A0Areas")]
            RoomTitleArea = 3,
            [EnumMember(Value = "Rooms")]
            Room = 4,
            [EnumMember(Value = "None")]
            None = 5,
        }

        public enum PoolGrouping
        {
            [EnumMember(Value = "Curated Item\u00A0Pools")]
            CuratedItems = 0,
            [EnumMember(Value = "All Item\u00A0Pools")]
            AllItems = 1,
            [EnumMember(Value = "All Location\u00A0Pools")]
            AllLocations = 2,
            [EnumMember(Value = "None")]
            None = 3,
        }

        public enum Sorting
        {
            [EnumMember(Value = "Alphabetical")]
            Alpha = 0,
            [EnumMember(Value = "Time")]
            Time = 1,
        }

        public enum OutOfLogicSorting
        {
            [EnumMember(Value = "Show")]
            Show = 0,
            [EnumMember(Value = "Split")]
            Split = 1,
            [EnumMember(Value = "Hide")]
            Hide = 2,
        }

        public enum SpoilerSorting
        {
            [EnumMember(Value = "Alphabetical")]
            Alpha = 0,
            [EnumMember(Value = "Seed\u00A0default")]
            SeedDefault = 1,
        }

        public enum SpoilerObtainedOrTraversedDisplay
        {
            [EnumMember(Value = "Ignore")]
            Ignore = 0,
            [EnumMember(Value = "Mark")]
            Mark = 1,
            [EnumMember(Value = "Hide")]
            Hide = 2,
        }

        public enum ShowLocationRoom
        {
            [EnumMember(Value = "None")]
            None = 0,
            [EnumMember(Value = "Code")]
            RoomCode = 1,
            [EnumMember(Value = "Desc.")]
            RoomDescription = 2,
        }

        public static readonly string[] HelperLocationGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] HelperLocationOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] HelperLocationOutOfLogicOrderingOptions = Enum.GetValues(typeof(OutOfLogicSorting)).Cast<OutOfLogicSorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] HelperTransitionGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] HelperTransitionOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] HelperTransitionOutOfLogicOrderingOptions = Enum.GetValues(typeof(OutOfLogicSorting)).Cast<OutOfLogicSorting>().Select(GetEnumMemberValue).ToArray();

        public static readonly string[] TrackerItemGroupingOptions = Enum.GetValues(typeof(PoolGrouping)).Cast<PoolGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerItemOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerShowLocationRoomOptions = Enum.GetValues(typeof(ShowLocationRoom)).Cast<ShowLocationRoom>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerTransitionGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerTransitionOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();

        public static readonly string[] SpoilerItemGroupingOptions = Enum.GetValues(typeof(PoolGrouping)).Cast<PoolGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerItemOrderingOptions = Enum.GetValues(typeof(SpoilerSorting)).Cast<SpoilerSorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerObtainedDisplayOptions = Enum.GetValues(typeof(SpoilerObtainedOrTraversedDisplay)).Cast<SpoilerObtainedOrTraversedDisplay>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerShowLocationRoomOptions = Enum.GetValues(typeof(ShowLocationRoom)).Cast<ShowLocationRoom>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerTransitionGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerTransitionOrderingOptions = Enum.GetValues(typeof(SpoilerSorting)).Cast<SpoilerSorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerTraversedDisplayOptions = Enum.GetValues(typeof(SpoilerObtainedOrTraversedDisplay)).Cast<SpoilerObtainedOrTraversedDisplay>().Select(GetEnumMemberValue).ToArray();

        private static string GetEnumMemberValue<T>(T value) where T : struct, IConvertible =>
            typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value;

        #region Randomizer FilePaths

        public static string HelperLogPath =>
            GetRandoFilePath("HelperLog.txt");

        public static string ItemSpoilerLogPath =>
            GetRandoFilePath("ItemSpoilerLog.json");

        public static string SeedSettingsPath =>
            GetRandoFilePath("settings.txt");

        public static string TrackerLogPath =>
            GetRandoFilePath("TrackerLog.txt");

        public static string TransitionSpoilerLogPath =>
            GetRandoFilePath("TransitionSpoilerLog.json");

        private static string GetRandoFilePath(string filename) =>
            @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\..\LocalLow\Team Cherry\Hollow Knight\Randomizer 4\Recent\{filename}";

        #endregion

        #region App FilePaths

        public static string HelperLogTransitionsFilename => "_HelperLogTransitions.json";
        public static string HelperLogLocationsFilename => "_HelperLogLocations.json";
        public static string TrackerLogTransitionsFilename => "_TrackerLogTransitions.json";
        public static string TrackerLogItemsFilename => "_TrackerLogItems.json";
        public static string AppSettingsFilename => "_AppSettings.json";
        public static string SeedFilename => "_Seed.json";

        #endregion

        #region Reference FilePaths

        public static string ReferenceTransitionsFilePath =>
            GetReferenceFilepath("transitions.json");
        public static string ReferenceLocationsFilePath =>
            GetReferenceFilepath("locations.json");
        public static string ReferenceRoomsFilePath =>
            GetReferenceFilepath("rooms.json");
        public static string ReferenceItemsFilePath =>
            GetReferenceFilepath("items.json");
        public static string ReferenceSceneDescriptionFilePath =>
            GetReferenceFilepath("_sceneDescriptions.json");

        #region Mod References

        // BadMagic100
        public static string TRJRFilePath =>
            GetReferenceFilepath(@"TRJR\journalData.json");

        // Bentechy66
        public static string BreakableWallsFilePath =>
            GetReferenceFilepath(@"BreakableWalls\breakable_walls.json");

        // dplochcoder
        public static string MoreDoorsFilePath =>
            GetReferenceFilepath(@"MoreDoors\doors.json");

        // flibber-hk
        public static string RandoPlusGhostLocationsFilePath =>
            GetReferenceFilepath(@"RandoPlus\ghostdata.json");
        public static string RandoLeverItemsFilePath =>
            GetReferenceFilepath(@"RandomizableLevers\languagedata.json");
        public static string RandoLeverLocationsFilePath =>
            GetReferenceFilepath(@"RandomizableLevers\leverlocations.json");

        // homothetyhk
        public static string BenchRandoItemsFilePath =>
            GetReferenceFilepath(@"BenchRando\language.json");
        public static string BenchRandoLocationsFilePath =>
            GetReferenceFilepath(@"BenchRando\benches.json");

        // Korzer420
        public static string LoreRandoShrineLocationsFilePath =>
            GetReferenceFilepath(@"LoreRando\ShrineLogic.json");

        // ManicJamie
        public static string GrassRandoV2FilePath =>
            GetReferenceFilepath(@"GrassRandoV2\Locations.json");

        // nerthul11
        public static string BreakableWFCPFilePath =>
            GetReferenceFilepath(@"BreakableWFCP\BreakableWallObjects.json");
        public static string StatueItemsFilePath =>
            GetReferenceFilepath(@"GodhomeRando\StatueItems.json");
        public static string StatueLocationsFilePath =>
            GetReferenceFilepath(@"GodhomeRando\StatueLocations.json");
        public static string BindingItemsFilePath =>
            GetReferenceFilepath(@"GodhomeRando\BindingItems.json"); // TODO: Bindings not used
        public static string BindingLocationsFilePath =>
            GetReferenceFilepath(@"GodhomeRando\BindingLocations.json"); // TODO: Bindings not used
        public static string AccessRandoKeyItemsFilePath =>
            GetReferenceFilepath(@"AccessRando\KeyItems.json");
        public static string AccessRandoPassItemsFilePath =>
            GetReferenceFilepath(@"AccessRando\PassItems.json");

        // StormZillaa
        public static string GrassRandoFilePath =>
            GetReferenceFilepath(@"GrassRando\GrassLog.json");

        // TheMathGeek314
        public static string YARCHivePlatFilePath =>
            GetReferenceFilepath(@"YARC\HivePlatCoords.json");
        public static string YARCVineFilePath =>
            GetReferenceFilepath(@"YARC\VineCoords.json");
        public static string YARCDreamOrbsFilePath =>
            GetReferenceFilepath(@"YARC\DreamOrbCoords.json");
        public static string YARCJarFilePath =>
            GetReferenceFilepath(@"YARC\JarCoords.json");
        public static string YARCEggBombFilePath =>
            GetReferenceFilepath(@"YARC\EggBombCoords.json");

        #endregion

        private static string GetReferenceFilepath(string filename) =>
            $@".\Reference\{filename}";

        #endregion
    }
}
