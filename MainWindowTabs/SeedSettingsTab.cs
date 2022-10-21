using HK_Rando_4_Log_Display.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private readonly HashSet<string> ExpandedSettings = new();

        private void UpdateSeedSettingsTab()
        {
            var settings = _settingsReader.GetSettings();
            UpdateUX(() => UpdateSeedSettingsList(settings));
        }

        private void UpdateSeedSettingsList(JObject settings)
        {
            SeedSettingsList.Items.Clear();
            if (settings == null)
            {
                return;
            }
            foreach (var setting in settings)
            {
                var settingName = setting.Key.WithoutUnderscores();
                var internalSettingValue = setting.Value;
                var expander = GenerateExpanderWithContent(settingName, GetSettingObject(internalSettingValue), ExpandedSettings);
                SeedSettingsList.Items.Add(expander);
            }
        }

        private static object GetSettingObject(JToken internalSettingValue)
        {
            switch (internalSettingValue.Type)
            {
                case JTokenType.Object:
                    var internalSettings = internalSettingValue.ToObject<Dictionary<string, string>>().ToList();
                    return GenerateAutoStarGrid(internalSettings);
                case JTokenType.Integer:
                case JTokenType.String:
                    var stringStacker = GenerateStackPanel();
                    stringStacker.Children.Add(new TextBlock { Text = $"{internalSettingValue.Value<string>()}" });
                    return stringStacker;
                default:
                    var defaultStacker = GenerateStackPanel();
                    defaultStacker.Children.Add(new TextBlock { Text = $"Failed to read settings correctly" });
                    return defaultStacker;
            }
        }
    }
}
