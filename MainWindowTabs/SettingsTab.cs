using HK_Rando_4_Log_Display.Extensions;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        #region Settings

        private void UpdateSettingsTab()
        {
            var settings = _settingsReader.GetSettings();
            UpdateUX(() => UpdateSettingsList(settings));
        }

        private void UpdateSettingsList(JObject settings)
        {
            SettingsList.Items.Clear();
            if (settings == null)
            {
                return;
            }
            foreach (var setting in settings)
            {
                var settingName = setting.Key.WithoutUnderscores();
                var settingExpanderName = settingName.AsObjectName();
                var internalSettingValue = setting.Value;

                var expander = new Expander
                {
                    Name = settingExpanderName,
                    Header = settingName,
                    Content = GetSettingObject(internalSettingValue),
                    IsExpanded = ExpandedSettings.Contains(settingExpanderName)
                };
                expander.Expanded += (object _, RoutedEventArgs e) => ExpandedSettings.Add((e.Source as Expander).Name);
                expander.Collapsed += (object _, RoutedEventArgs e) => ExpandedSettings.Remove((e.Source as Expander).Name);
                SettingsList.Items.Add(expander);
            }
        }

        private object GetSettingObject(JToken internalSettingValue)
        {
            switch (internalSettingValue.Type)
            {
                case JTokenType.Object:
                    var internalSettings = internalSettingValue.ToObject<Dictionary<string, string>>().ToList();
                    return GenerateAutoStarGrid(internalSettings);
                case JTokenType.Integer:
                case JTokenType.String:
                    var stringStacker = new StackPanel
                    {
                        Margin = new Thickness(20, 0, 0, 0)
                    };
                    stringStacker.Children.Add(new TextBlock { Text = $"{internalSettingValue.Value<string>()}" });
                    return stringStacker;
                default:
                    var defaultStacker = new StackPanel
                    {
                        Margin = new Thickness(20, 0, 0, 0)
                    };
                    defaultStacker.Children.Add(new TextBlock { Text = $"Failed to read settings correctly" });
                    return defaultStacker;
            }
        }

        private HashSet<string> ExpandedSettings = new HashSet<string>();

        #endregion
    }
}
