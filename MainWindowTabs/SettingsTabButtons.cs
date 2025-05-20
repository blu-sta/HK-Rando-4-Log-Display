using System;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
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
            CopyFileIfExists(TransitionSpoilerLogPath, $@"{tempFolderPath}\TransitionSpoilerLog.json");
            CopyFileIfExists(SeedSettingsPath, $@"{tempFolderPath}\settings.txt");

            new[] {
                HelperLogTransitionsFilename,
                HelperLogLocationsFilename,
                TrackerLogTransitionsFilename,
                TrackerLogItemsFilename,
            }
            .Concat(Directory.GetFiles(".", $"error_{now:yyyy_MM}*.log"))
            .ToList()
            .ForEach(filename => CopyFileIfExists(filename, $@"{tempFolderPath}\{filename}"));
            

            ZipFile.CreateFromDirectory(tempFolderPath, zipFileName);

            Directory.Delete(tempFolderPath, true);

            Process.Start(new ProcessStartInfo(zipFolderPath) { UseShellExecute = true });
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
