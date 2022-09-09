using System;

namespace HK_Rando_4_Log_Display.FileReader
{
    public static class Constants
    {
        #region Randomizer FilePaths

        public static string HelperLogPath =>
            GetRandoFilePath("HelperLog.txt");

        public static string ItemSpoilerLogPath =>
            GetRandoFilePath("ItemSpoilerLog.json");

        public static string SettingsPath =>
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
        public static string AppSettingsFilename => "_AppSettings.json";
        public static string SeedFilename => "_CurrentSeed.json";

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
        private static string GetReferenceFilepath(string filename) =>
            $@".\Reference\{filename}";

        #endregion
    }
}
