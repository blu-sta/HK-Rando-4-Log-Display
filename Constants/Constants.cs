using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace HK_Rando_4_Log_Display.Constants
{
    public static class Constants
    {
        public static readonly string AppVersion = "v2.0.4.6";

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

        public enum SpoilerSorting
        {
            [EnumMember(Value = "Alphabetical")]
            Alpha = 0,
            [EnumMember(Value = "Seed\u00A0default")]
            SeedDefault = 1,
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
        public static readonly string[] HelperTransitionGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] HelperTransitionOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();

        public static readonly string[] TrackerItemGroupingOptions = Enum.GetValues(typeof(PoolGrouping)).Cast<PoolGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerItemOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerShowLocationRoomOptions = Enum.GetValues(typeof(ShowLocationRoom)).Cast<ShowLocationRoom>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerTransitionGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] TrackerTransitionOrderingOptions = Enum.GetValues(typeof(Sorting)).Cast<Sorting>().Select(GetEnumMemberValue).ToArray();

        public static readonly string[] SpoilerItemGroupingOptions = Enum.GetValues(typeof(PoolGrouping)).Cast<PoolGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerItemOrderingOptions = Enum.GetValues(typeof(SpoilerSorting)).Cast<SpoilerSorting>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerShowLocationRoomOptions = Enum.GetValues(typeof(ShowLocationRoom)).Cast<ShowLocationRoom>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerTransitionGroupingOptions = Enum.GetValues(typeof(RoomGrouping)).Cast<RoomGrouping>().Select(GetEnumMemberValue).ToArray();
        public static readonly string[] SpoilerTransitionOrderingOptions = Enum.GetValues(typeof(SpoilerSorting)).Cast<SpoilerSorting>().Select(GetEnumMemberValue).ToArray();

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

        public static string ReferenceMoreDoorsFilePath =>
            GetReferenceFilepath(@"MoreDoors\doors.json");

        public static string ReferenceRandoLeverItemsFilePath =>
            GetReferenceFilepath(@"RandomizableLevers\languagedata.json");
        public static string ReferenceRandoLeverLocationsFilePath =>
            GetReferenceFilepath(@"RandomizableLevers\leverlocations.json");

        public static string ReferenceBenchRandoItemsFilePath =>
            GetReferenceFilepath(@"BenchRando\language.json");
        public static string ReferenceBenchRandoLocationsFilePath =>
            GetReferenceFilepath(@"BenchRando\benches.json");

        #endregion

        private static string GetReferenceFilepath(string filename) =>
            $@".\Reference\{filename}";

        #endregion
    }
}
