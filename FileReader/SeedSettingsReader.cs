using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ISeedSettingsReader : ILogReader
    {
        public JObject GetSettings();
        public string GetSeed();
        public string GetMode();
        public string GetGenerationCode();
    }


    public class SeedSettingsReader : ISeedSettingsReader
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private JObject _seedSettings;
        private string _generationCode;

        public bool IsFileFound { get; private set; }
        public bool IsFileLoaded { get; private set; }

        public SeedSettingsReader()
        {
            LoadData(Array.Empty<string>());
        }

        public void LoadData(string[] _)
        {
            IsFileFound = File.Exists(SeedSettingsPath);
            if (!IsFileFound)
            {
                return;
            }
            var settingsData = File.ReadAllLines(SeedSettingsPath).ToList();

            try
            {
                LoadSeedSettings(settingsData);
                LoadGenerationCode(settingsData);
                IsFileLoaded = true;
            }
            catch (Exception e)
            {
                _logger.Error(e, "SeedSettingsReader LoadData Error");
                IsFileLoaded = false;
            }
        }

        public void OpenFile()
        {
            if (File.Exists(SeedSettingsPath)) Process.Start(new ProcessStartInfo(SeedSettingsPath) { UseShellExecute = true });
        }

        private void LoadSeedSettings(List<string> trackerLogData)
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

            _seedSettings = JsonConvert.DeserializeObject<JObject>(settingsString);
        }

        private void LoadGenerationCode(List<string> trackerLogData)
        {
            var generationSettings = trackerLogData.IndexOf("Logging menu GenerationSettings code:");
            if (generationSettings < 0 || trackerLogData.Count < generationSettings + 2)
            {
                return;
            }
            _generationCode = trackerLogData[generationSettings + 1];
        }

        public JObject GetSettings() => _seedSettings;

        public string GetSeed() =>
            _seedSettings?["Seed"]?.ToString() ?? string.Empty;

        public string GetMode()
        {
            var mode = _seedSettings?["TransitionSettings"]?["Mode"]?.ToString() ?? string.Empty;
            return mode switch
            {
                "None" => "Item Rando",
                "MapAreaRandomizer" => "Map Area Rando",
                "FullAreaRandomizer" => "Area Rando",
                "RoomRandomizer" => "Room Rando",
                _ => mode,
            };
        }

        public string GetGenerationCode() => _generationCode;
    }
}
