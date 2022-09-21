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
        private readonly Settings _settings;
        private string _selectedTab;
        private DateTime _referenceTime;

        public MainWindow(IHelperLogReader helperLogReader, ITrackerLogReader trackerLogReader, ISettingsReader settingsReader,
            IItemSpoilerReader itemSpoilerReader, ITransitionSpoilerReader transitionSpoilerReader, IResourceLoader resourceLoader)
        {
            _helperLogReader = helperLogReader;
            _trackerLogReader = trackerLogReader;
            _settingsReader = settingsReader;
            _itemSpoilerReader = itemSpoilerReader;
            _transitionSpoilerReader = transitionSpoilerReader;
            _resourceLoader = resourceLoader;

            _settings = _resourceLoader.GetAppSettings(
                _helperGroupings.Length,
                _helperOrders.Length,
                _trackerItemGroupings.Length,
                _trackerItemOrders.Length,
                _trackerTransitionOrders.Length,
                _spoilerItemGroupings.Length,
                _spoilerItemOrders.Length,
                _spoilerTransitionOrders.Length
            );

            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void UpdateUX(Action action) => Dispatcher.Invoke(new Action(action), DispatcherPriority.ContextIdle);

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitialiseButtons();
#if DEBUG
            dataExtractor.Interval = 60000;
#endif
            dataExtractor.Elapsed += UpdateTabs;
            dataExtractor.AutoReset = true;
            dataExtractor.Enabled = true;
            
            UpdateUX(() => Footer.Text = "v1.1.0-beta-2 blu.sta");
            UpdateTabs();
        }

        private void InitialiseButtons()
        {
            HelperLocationGrouping.Content = _helperGroupings[(int)_settings.SelectedHelperLocationGrouping];
            HelperLocationOrder.Content = _helperOrders[(int)_settings.SelectedHelperLocationOrder];
            HelperTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedHelperTransitionGrouping];
            HelperTransitionOrder.Content = _helperOrders[(int)_settings.SelectedHelperTransitionOrder];
            TrackerItemGrouping.Content = _trackerItemGroupings[(int)_settings.SelectedTrackerItemGrouping];
            TrackerItemOrder.Content = _trackerItemOrders[(int)_settings.SelectedTrackerItemOrder];
            TrackerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedTrackerTransitionGrouping];
            TrackerTransitionOrder.Content = _trackerTransitionOrders[(int)_settings.SelectedTrackerTransitionOrder];
            SpoilerItemGrouping.Content = _spoilerItemGroupings[(int)_settings.SelectedSpoilerItemGrouping];
            SpoilerItemOrder.Content = _spoilerItemOrders[(int)_settings.SelectedSpoilerItemOrder];
            SpoilerTransitionGrouping.Content = _helperGroupings[(int)_settings.SelectedSpoilerTransitionGrouping];
            SpoilerTransitionOrder.Content = _spoilerTransitionOrders[(int)_settings.SelectedSpoilerTransitionOrder];
        }

        private void UpdateTabs(object _, ElapsedEventArgs __) => UpdateTabs();

        private bool _firstRun = true;

        private void UpdateTabs()
        {
            lock(_lock)
            {
                SetReferenceTime();

                if (_firstRun)
                {
                    UpdateHelperLocationTab();
                    UpdateHelperTransitionTab();
                    UpdateTrackerItemTab();
                    UpdateTrackerTransitionTab();
                    UpdateSettingsTab();
                    UpdateSpoilerItemTab();
                    UpdateSpoilerTransitionTab();
                    UpdateHeader();
                    _firstRun = false;
                    return;
                }

                LoadFiles();
                UpdateHelperLocationTab();
                UpdateHelperTransitionTab();

                switch (_selectedTab)
                {
                    case "Item Helper":
                    case "Transition Helper":
                        // Always update the helper tabs
                        break;
                    case "Item Tracker":
                        UpdateTrackerItemTab();
                        break;
                    case "Transition Tracker":
                        UpdateTrackerTransitionTab();
                        break;
                    case "Settings":
                        UpdateSettingsTab();
                        break;
                    case "Item Spoiler":
                        UpdateSpoilerItemTab();
                        break;
                    case "Transition Spoiler":
                        UpdateSpoilerTransitionTab();
                        break;
                }

                UpdateHeader();
            }
        }

        private void SetReferenceTime() => _referenceTime = DateTime.Now;

        private void LoadFiles()
        {
            _helperLogReader.LoadData();
            _trackerLogReader.LoadData();
            _settingsReader.LoadData();
            _itemSpoilerReader.LoadData();
            _transitionSpoilerReader.LoadData();
        }

        private void UpdateHeader()
        {
            var headerStrings = new List<string>();

            var fileStrings = new List<(bool, string)>
            {
                {(!_helperLogReader.IsFileFound, "HelperLog.txt not found" )},
                {(!_trackerLogReader.IsFileFound, "TrackerLog.txt not found" )},
                {(!_settingsReader.IsFileFound, "settings.txt not found" )},
                {(!_itemSpoilerReader.IsFileFound, "ItemSpoilerLog.json not found" )},
                {(!_transitionSpoilerReader.IsFileFound, "TransitionSpoilerLog.json not found" )},
            };
            
            if (fileStrings.Any(x => x.Item1))
            {
                headerStrings.AddRange(fileStrings.Where(x => x.Item1).Select(x => x.Item2));
            }

            if (_settingsReader.IsFileFound)
            {
                headerStrings.Add(string.Join(": ", new List<string> { _settingsReader.GetMode(), _settingsReader.GetSeed() }.Where(x => !string.IsNullOrWhiteSpace(x))));
            }

            UpdateUX(() => Header.Text = string.Join("\n", headerStrings));
        }

        private readonly string[] _helperGroupings = new[] { "Group by map area", "Group by titled area", "Group by map area by room", "Group by titled area by room", "Group by room", "No grouping" };
        private readonly string[] _helperOrders = new[] { "Order alphabetically", "Order alphabetically (show time)", "Order by time" };
        private readonly string[] _trackerItemGroupings = new[] { "Group by pool", "No grouping" };
        private readonly string[] _trackerItemOrders = new[] { "Order alphabetically", "Order by time" };
        private readonly string[] _trackerTransitionOrders = new[] { "Order alphabetically", "Order by time" };
        private readonly string[] _spoilerItemGroupings = new[] { "Group by pool", "No grouping" };
        private readonly string[] _spoilerItemOrders = new[] { "Order alphabetically", "Order by seed default" };
        private readonly string[] _spoilerTransitionOrders = new[] { "Order alphabetically", "Order by seed default" };

        #region Events

        private void Body_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedTab = (Body.SelectedItem as TabItem)?.Header as string;
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = sender as ScrollViewer;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 8.0);
            e.Handled = true;
        }

        private static void ExpandExpanders(ListBox listBox) => 
            listBox.Items.OfType<Expander>().ToList().ForEach(x =>
                {
                    x.IsExpanded = true;
                    (x.Content as StackPanel)?.Children.OfType<Expander>().ToList()
                        .ForEach(x => x.IsExpanded = true);
                });

        private static void CollapseExpanders(ListBox listBox) => 
            listBox.Items.OfType<Expander>().ToList().ForEach(x =>
            {
                (x.Content as StackPanel)?.Children.OfType<Expander>().ToList()
                    .ForEach(x => x.IsExpanded = false);
                x.IsExpanded = false;
            });

        protected override void OnClosed(EventArgs e)
        {
            SaveCurrentState();
            dataExtractor.Dispose();
            base.OnClosed(e);
        }

        private void SaveCurrentState()
        {
            if (_helperLogReader.IsFileFound)
                _helperLogReader.SaveState();
            if (_settingsReader.IsFileFound)
                _resourceLoader.SaveSeed(_settingsReader.GetSeed());
            _resourceLoader.SaveAppSettings(_settings);
        }

        #endregion
    }
}
