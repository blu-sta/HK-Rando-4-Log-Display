using HK_Rando_4_Log_Display.DTO;
using HK_Rando_4_Log_Display.FileReader;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ISettingsReader _settingsReader;
        private readonly IItemSpoilerReader _itemSpoilerReader;
        private readonly ITransitionSpoilerReader _transitionSpoilerReader;
        private readonly IResourceLoader _resourceLoader;
        private readonly AppSettings _appSettings;
        private string _selectedParentTab;
        private string _selectedChildTab;
        private DateTime _referenceTime;

        public MainWindow(IHelperLogReader helperLogReader, ITrackerLogReader trackerLogReader, ISettingsReader settingsReader,
            IItemSpoilerReader itemSpoilerReader, ITransitionSpoilerReader transitionSpoilerReader, IResourceLoader resourceLoader)
        {
            DataContext = this;

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
            _helperLogReader.LoadData();
            _trackerLogReader.LoadData();
            _itemSpoilerReader.LoadData();
            _transitionSpoilerReader.LoadData();
            _settingsReader.LoadData();
        }

        private void UpdateHeader()
        {
            var headerStrings = new List<(bool, string)>
            {
                {(_settingsReader.IsFileFound, string.Join(": ", new[] { _settingsReader.GetMode(), _settingsReader.GetSeed() }.Where(x => !string.IsNullOrWhiteSpace(x))))},
                {(!_helperLogReader.IsFileFound, "HelperLog.txt not found" )},
                {(!_trackerLogReader.IsFileFound, "TrackerLog.txt not found" )},
                {(!_itemSpoilerReader.IsFileFound, "ItemSpoilerLog.json not found" )},
                {(!_transitionSpoilerReader.IsFileFound, "TransitionSpoilerLog.json not found" )},
                {(!_settingsReader.IsFileFound, "settings.txt not found" )},
            }.Where(x => x.Item1).Select(x => x.Item2).ToList();

            UpdateUX(() => Header.Text = string.Join("\n", headerStrings));
        }

        #region Events

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
                Dispatcher.CurrentDispatcher.BeginInvoke(() => UpdateTabs());
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = sender as ScrollViewer;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 8.0);
            e.Handled = true;
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
            _resourceLoader.SaveSeed(_settingsReader.GetSeed());
            _resourceLoader.SaveAppSettings(_appSettings);
        }

        #endregion
    }
}
