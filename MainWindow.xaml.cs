using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.FileReader;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow : Window
    {
        private readonly object _lock = new(); 
        private readonly Timer dataExtractor = new(5000);
        private readonly IHelperLogReader _helperLogReader;
        private readonly ITrackerLogReader _trackerLogReader;
        private readonly ISeedSettingsReader _settingsReader;
        private readonly IItemSpoilerReader _itemSpoilerReader;
        private readonly ITransitionSpoilerReader _transitionSpoilerReader;
        private readonly IResourceLoader _resourceLoader;
        private readonly IVersionChecker _versionChecker;
        private readonly AppSettings _appSettings;
        private string _selectedParentTab;
        private string _selectedChildTab;
        private DateTime _referenceTime;
        private string[] _multiWorldPlayerNames = Array.Empty<string>();

        public MainWindow(IHelperLogReader helperLogReader, ITrackerLogReader trackerLogReader, ISeedSettingsReader settingsReader,
            IItemSpoilerReader itemSpoilerReader, ITransitionSpoilerReader transitionSpoilerReader, IResourceLoader resourceLoader,
            IVersionChecker versionChecker)
        {
            DataContext = this;

            _versionChecker = versionChecker;
            _helperLogReader = helperLogReader;
            _trackerLogReader = trackerLogReader;
            _settingsReader = settingsReader;
            _itemSpoilerReader = itemSpoilerReader;
            _transitionSpoilerReader = transitionSpoilerReader;
            _resourceLoader = resourceLoader;
            _appSettings = _resourceLoader.GetAppSettings();

            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void UpdateUX(Action action) => Dispatcher.BeginInvoke(new Action(action), DispatcherPriority.ContextIdle);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            AddButtonsToAppSettingsTab();
#if DEBUG
            InitialiseDebugMode();
#endif
            dataExtractor.Elapsed += UpdateTabs;
            dataExtractor.AutoReset = true;
            dataExtractor.Enabled = true;
            UpdateTabsFirstRun();
            Dispatcher.BeginInvoke(async () => await ShowUpdateAvailability());
        }

        private async Task ShowUpdateAvailability()
        {
            var updatedVersion = await _versionChecker.GetNewVersionOrDefault();
            if (!string.IsNullOrWhiteSpace(updatedVersion))
            {
                UpdateUX(() =>
                {
                    Update_Button.Content = $"Update Available: {updatedVersion}";
                    Update_Button.Visibility = Visibility.Visible;
                });
            }
        }

        private void UpdateTabs(object _, ElapsedEventArgs __) => UpdateTabs();

        private void UpdateTabsFirstRun()
        {
            lock (_lock)
            {
                UpdateUX(() => UpdateButtonsFromAppSettings());
                SetReferenceTime();
                LoadFiles();

                UpdateHelperLocationsTab();
                UpdateHelperTransitionsTab();
                UpdateTrackerItemsTab();
                UpdateTrackerTransitionsTab();
                UpdateSpoilerItemsTab();
                UpdateSpoilerTransitionsTab();
                UpdateSeedSettingsTab();
                
                UpdateHeader();
                UpdateUX(() => Footer.Text = $"{AppVersion} blu.sta");
            }
        }

        private void UpdateTabs()
        {
            lock(_lock)
            {
                UpdateUX(() => UpdateButtonsFromAppSettings());
                SetReferenceTime();
                LoadFiles();

                switch (_selectedParentTab)
                {
                    case "Helper":
                        switch(_selectedChildTab)
                        {
                            case "Locations":
                                UpdateHelperLocationsTab();
#if DEBUG
                                if (_showDeadImports)
                                    ShowDeadImports();
#endif
                                break;
                            case "Transitions":
                                UpdateHelperTransitionsTab();
                                break;
                        }
                        break;
                    case "Tracker":
                        switch (_selectedChildTab)
                        {
                            case "Items":
                                UpdateTrackerItemsTab();
                                break;
                            case "Transitions":
                                UpdateTrackerTransitionsTab();
                                break;
                        }
                        break;
                    case "Spoiler":
                        switch (_selectedChildTab)
                        {
                            case "Items":
                                UpdateSpoilerItemsTab();
                                break;
                            case "Transitions":
                                UpdateSpoilerTransitionsTab();
                                break;
                        }
                        break;
                    case "Settings":
                        switch (_selectedChildTab)
                        {
                            case "Seed Settings":
                                UpdateSeedSettingsTab();
                                break;
                            case "App Settings":
                                // Nothing to do here regularly
                                // Button states updated on tab change
                                break;
                        }
                        break;
                }

                UpdateHeader();
            }
        }

        private void UpdateButtonsFromAppSettings()
        {
            SetHelperLocationsTabButtonContent();
            SetHelperTransitionsTabButtonContent();
            SetTrackerItemsTabButtonContent();
            SetTrackerTransitionsTabButtonContent();
            SetSpoilerItemsTabButtonContent();
            SetSpoilerTransitionsTabButtonContent();
            SetSettingAppSettingsActiveButtons();
        }

        private void SetReferenceTime() => _referenceTime = DateTime.Now;

        private void LoadFiles()
        {
            _settingsReader.LoadData(_multiWorldPlayerNames);
            _helperLogReader.LoadData(_multiWorldPlayerNames);
            _trackerLogReader.LoadData(_multiWorldPlayerNames);
            _itemSpoilerReader.LoadData(_multiWorldPlayerNames);
            _transitionSpoilerReader.LoadData(_multiWorldPlayerNames);
        }

        private void UpdateHeader()
        {
            var headerStrings = new List<string>();

            if (_settingsReader.IsFileFound)
            {
                headerStrings.Add(string.Join(": ", new[] { _settingsReader.GetMode(), _settingsReader.GetSeed() }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }
            new (ILogReader, string)[] {
                (_helperLogReader, "HelperLog.txt"),
                (_trackerLogReader, "TrackerLog.txt"),
                (_itemSpoilerReader, "ItemSpoilerLog.json"),
                (_transitionSpoilerReader, "TransitionSpoilerLog.json"),
                (_settingsReader, "settings.txt")
            }.ToList().ForEach((x) =>
            {
                var logReader = x.Item1;
                var filename = x.Item2;

                if (!logReader.IsFileFound)
                {
                    headerStrings.Add($"{filename} not found");
                }
                else if (!logReader.IsFileLoaded)
                {
                    headerStrings.Add($"{filename} not loaded correctly");
                }
            });
            
            UpdateUX(() => Header.Text = string.Join("\n", headerStrings));
        }

        #region Events

        private void MultiWorld_Click(object sender, RoutedEventArgs e)
        {
            var multiWorldWindow = new MultiWorldWindow(_multiWorldPlayerNames)
            {
                Owner = this
            };
            multiWorldWindow.ShowDialog();
            _multiWorldPlayerNames = MultiWorldWindow.PlayerNames;
        }

        private void Update_Click(object sender, RoutedEventArgs e) => 
            Process.Start(new ProcessStartInfo("https://github.com/blu-sta/HK-Rando-4-Log-Display/releases/latest") { UseShellExecute = true });

        private void Tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedParentTab = (ParentTabs.SelectedItem as TabItem)?.Header as string;
            _selectedChildTab = _selectedParentTab switch
            {
                "Helper" => (HelperTabs.SelectedItem as TabItem)?.Header as string,
                "Tracker" => (TrackerTabs.SelectedItem as TabItem)?.Header as string,
                "Spoiler" => (SpoilerTabs.SelectedItem as TabItem)?.Header as string,
                "Settings" => (SettingsTabs.SelectedItem as TabItem)?.Header as string,
                _ => ""
            };

            if (IsLoaded)
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(() => UpdateTabs());
                if (_selectedParentTab == "Settings" && _selectedChildTab == "App Settings")
                    Dispatcher.CurrentDispatcher.BeginInvoke(() => OpenLogFile_Button.Visibility = Visibility.Hidden);
                else if (OpenLogFile_Button.Visibility == Visibility.Hidden)
                    Dispatcher.CurrentDispatcher.BeginInvoke(() => OpenLogFile_Button.Visibility = Visibility.Visible);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = sender as ScrollViewer;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 8.0);
            e.Handled = true;
        }

        private void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            switch (_selectedParentTab)
            {
                case "Helper":
                    _helperLogReader.OpenFile();
                    break;
                case "Tracker":
                    _trackerLogReader.OpenFile();
                    break;
                case "Spoiler":
                    switch (_selectedChildTab)
                    {
                        case "Items":
                            _itemSpoilerReader.OpenFile();
                            break;
                        case "Transitions":
                            _transitionSpoilerReader.OpenFile();
                            break;
                    }
                    break;
                case "Settings":
                    switch (_selectedChildTab)
                    {
                        case "Seed Settings":
                            _settingsReader.OpenFile();
                            break;
                        case "App Settings":
                            break;
                    }
                    break;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveCurrentState();
            dataExtractor.Dispose();
            base.OnClosed(e);
        }

        private void SaveCurrentState()
        {
            _helperLogReader.SaveState();
            _trackerLogReader.SaveState();
            _resourceLoader.SaveSeedGenerationCode(_settingsReader.GetGenerationCode());
            _resourceLoader.SaveAppSettings(_appSettings);
        }

        #endregion
    }
}
