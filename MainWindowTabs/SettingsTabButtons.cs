using System;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using static HK_Rando_4_Log_Display.Constants.Constants;
using System.Text;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private void AppSettingsButton_Click(object sender, RoutedEventArgs _)
        {
            var button = sender as Button;
            var buttonText = (button?.Content as TextBlock)?.Text;
            var parentGridName = (button?.Parent as Grid)?.Name;
            switch (parentGridName)
            {
                case nameof(HelperLocationGroupOptions):
                    _appSettings.SelectedHelperLocationGrouping = Array.FindIndex(HelperLocationGroupingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(HelperLocationGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationGrouping));
                    break;
                case nameof(HelperLocationOrderOptions):
                    _appSettings.SelectedHelperLocationOrder = Array.FindIndex(HelperLocationOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(HelperLocationOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationOrder));
                    break;
                case nameof(HelperTransitionGroupOptions):
                    _appSettings.SelectedHelperTransitionGrouping = Array.FindIndex(HelperTransitionGroupingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(HelperTransitionGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperTransitionGrouping));
                    break;
                case nameof(HelperTransitionOrderOptions):
                    _appSettings.SelectedHelperTransitionOrder = Array.FindIndex(HelperTransitionOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(HelperTransitionOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperTransitionOrder));
                    break;

                case nameof(TrackerItemGroupOptions):
                    _appSettings.SelectedTrackerItemGrouping = Array.FindIndex(TrackerItemGroupingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(TrackerItemGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerItemGrouping));
                    break;
                case nameof(TrackerItemOrderOptions):
                    _appSettings.SelectedTrackerItemOrder = Array.FindIndex(TrackerItemOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(TrackerItemOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerItemOrder));
                    break;
                case nameof(TrackerTransitionGroupOptions):
                    _appSettings.SelectedTrackerTransitionGrouping = Array.FindIndex(TrackerTransitionGroupingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(TrackerTransitionGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerTransitionGrouping));
                    break;
                case nameof(TrackerTransitionOrderOptions):
                    _appSettings.SelectedTrackerTransitionOrder = Array.FindIndex(TrackerTransitionOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(TrackerTransitionOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerTransitionOrder));
                    break;

                case nameof(SpoilerItemGroupOptions):
                    _appSettings.SelectedSpoilerItemGrouping = Array.FindIndex(SpoilerItemGroupingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(SpoilerItemGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerItemGrouping));
                    break;
                case nameof(SpoilerItemOrderOptions):
                    _appSettings.SelectedSpoilerItemOrder = Array.FindIndex(SpoilerItemOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(SpoilerItemOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerItemOrder));
                    break;
                case nameof(SpoilerTransitionGroupOptions):
                    _appSettings.SelectedSpoilerTransitionGrouping = Array.FindIndex(SpoilerTransitionGroupingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(SpoilerTransitionGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerTransitionGrouping));
                    break;
                case nameof(SpoilerTransitionOrderOptions):
                    _appSettings.SelectedSpoilerTransitionOrder = Array.FindIndex(SpoilerTransitionOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(SpoilerTransitionOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerTransitionOrder));
                    break;
            }
        }

        private static void SetActiveButton(Button[] buttons, int activeIndex)
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].Background = i == activeIndex ? Brushes.LightBlue : Brushes.LightGray;
            }
        }

        private void ZipFiles_Click(object __, RoutedEventArgs _)
        {
            SaveCurrentState();
            ZipFiles();
        }

        private static void ZipFiles()
        {
            var now = DateTime.Now;
            var nowString = now.ToString("yy_MM_dd_HH_mm_ss");
            var zipFolderPath = @".\ZipFiles";
            var zipFileName = $@"{zipFolderPath}\HKLogReader_{AppVersion}_{nowString}.zip";
            var tempFolderPath = $@"{zipFolderPath}\Temp_{nowString}";
            Directory.CreateDirectory(tempFolderPath);

            CopyFileIfExists(HelperLogPath, $@"{tempFolderPath}\HelperLog.txt");
            CopyFileIfExists(TrackerLogPath, $@"{tempFolderPath}\TrackerLog.txt");
            CopyFileIfExists(ItemSpoilerLogPath, $@"{tempFolderPath}\ItemSpoilerLog.json");
            CopyFileIfExists(TransitionSpoilerLogPath, $@"{tempFolderPath}\TransitionSpoilerLog.txt");
            CopyFileIfExists(SeedSettingsPath, $@"{tempFolderPath}\settings.txt");

            CopyFileIfExists(HelperLogTransitionsFilename, $@"{tempFolderPath}\{HelperLogTransitionsFilename}");
            CopyFileIfExists(HelperLogLocationsFilename, $@"{tempFolderPath}\{HelperLogLocationsFilename}");
            CopyFileIfExists(TrackerLogTransitionsFilename, $@"{tempFolderPath}\{TrackerLogTransitionsFilename}");
            CopyFileIfExists(TrackerLogItemsFilename, $@"{tempFolderPath}\{TrackerLogItemsFilename}");

            ZipFile.CreateFromDirectory(tempFolderPath, zipFileName);

            Directory.Delete(tempFolderPath, true);

            Process.Start("explorer.exe", zipFolderPath);
        }

        private static void CopyFileIfExists(string fromPath, string toPath)
        {
            if (File.Exists(fromPath)) File.Copy(fromPath, toPath);
        }

        private void GenerationCodeCopy_Click(object sender, RoutedEventArgs e)
        {
            var code = _settingsReader.GetGenerationCode();
            if (string.IsNullOrWhiteSpace(code))
            {
                ShowInPopup($"Failed to find shareable settings code");
                return;
            }
            Clipboard.SetText(code);

            var s = new StringBuilder();
            var s2 = new StringBuilder();
            var codeLines = code.Split(";");
            for (var i = 0; i < codeLines.Length - 1; i++)
            {
                var line = codeLines[i];
                if (s2.Length + line.Length >= 40)
                {
                    s.Append(s2);
                    s.Append('\n');
                    s2.Clear();
                }
                s2.Append(line);
                s2.Append(';');
            }
            s.Append(s2);
            s.Append('\n');
            s.Append(codeLines[^1]);

            ShowInPopup($"Copied:\n{s}");
        }

        private void CopySeed_Click(object __, RoutedEventArgs _)
        {
            var seed = _settingsReader.GetSeed();
            if (string.IsNullOrWhiteSpace(seed))
            {
                ShowInPopup($"Failed to find seed");
                return;
            }
            Clipboard.SetText(seed);
            ShowInPopup($"Copied:\n{seed}");
        }
    }
}
