using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ISettingsReader : ILogReader
    {
        public string GetSeed();

        public string GetMode();

        public JObject GetSettings();
    }


    public class SettingsReader : ISettingsReader
    {
        public bool IsFileFound { get; private set; }

        public SettingsReader()
        {
            LoadData();
        }

        public void LoadData()
        {
            var filepath = Constants.SettingsPath;
            if (!File.Exists(filepath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;
            var settingsData = File.ReadAllLines(filepath).ToList();

            LoadSettings(settingsData);
        }

        private JObject _settings;

        private void LoadSettings(List<string> trackerLogData)
        {
            var start = trackerLogData.IndexOf("{");
            if (start < 0)
            {
                return;
            }
            var end = trackerLogData.IndexOf("}", start);
            if (end < 0)
            {
                return;
            }

            var settingsString = string.Join("", trackerLogData.Where((_, i) => i >= start && i <= end));

            _settings = JsonConvert.DeserializeObject<JObject>(settingsString);
        }

        public string GetSeed() =>
            _settings?["Seed"]?.ToString();

        public string GetMode()
        {
            var transitionSettings = _settings?["TransitionSettings"];

            switch (transitionSettings?["Mode"]?.ToString())
            {
                case "None":
                    return "Item Rando";
                case "MapAreaRandomizer":
                    return "Map Rando";
                case "FullAreaRandomizer":
                    return "Area Rando";
                case "RoomRandomizer":
                    return "Room Rando";
                default:
                    return "";
            }
        }

        public JObject GetSettings() => 
            _settings;

    }
}
