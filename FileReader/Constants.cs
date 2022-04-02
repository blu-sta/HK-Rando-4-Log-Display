using System;

namespace HK_Rando_4_Log_Display.FileReader
{
    public static class Constants
    {
        #region FilePaths

        public static string HelperLogPath =>
            $@"{BaseLogPath}\HelperLog.txt";

        public static string ItemSpoilerLogPath =>
            $@"{BaseLogPath}\ItemSpoilerLog.json";

        public static string SettingsPath =>
            $@"{BaseLogPath}\settings.txt";

        public static string TrackerLogPath =>
            $@"{BaseLogPath}\TrackerLog.txt";

        public static string TransitionSpoilerLogPath =>
            $@"{BaseLogPath}\TransitionSpoilerLog.json";

        private static string BaseLogPath =>
            @$"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\..\LocalLow\Team Cherry\Hollow Knight\Randomizer 4\Recent";

        #endregion
    }
}
