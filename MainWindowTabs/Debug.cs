using System.Linq;
using System.Timers;
using System.Windows.Controls;
using System.Windows;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {

        private const string DebugInterfaceName = "DebugInterface";

        private void InitialiseDebugMode()
        {
            dataExtractor.Interval = RefreshIntervalOptions[SelectedRefreshInterval];

            var border = new Border
            {
                Name = DebugInterfaceName,
                Style = FindResource("WrapPanelWithBorder") as Style,
            };
            Grid.SetRow(border, 3);
            Main.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            Main.Children.Add(border);

            var wrapPanel = new WrapPanel();
            border.Child = wrapPanel;

            var refreshIntervalButton = new Button
            {
                Name = RefreshIntervalButtonName,
                Content = GenerateButtonTextBlock(RefreshIntervalButtonContent()),
            };
            refreshIntervalButton.Click += RefreshIntervalButton_Click;
            wrapPanel.Children.Add(refreshIntervalButton);

            var cycleIntervalButton = new Button
            {
                Name = CycleIntervalButtonName,
                Content = GenerateButtonTextBlock(CycleIntervalButtonContent()),

                IsEnabled = false
            };
            cycleIntervalButton.Click += CycleIntervalButton_Click;
            wrapPanel.Children.Add(cycleIntervalButton);

            tabCycleTimer.Elapsed += CycleTabs;

            var memoryPurgeButton = new Button
            {
                Name = MemoryPurgeButtonName,
                Content = GenerateButtonTextBlock("Purge memory"),
            };
            memoryPurgeButton.Click += MemoryPurgeButton_Click;
            wrapPanel.Children.Add(memoryPurgeButton);

            var deadImportButton = new Button
            {
                Name = DeadImportButton,
                Content = GenerateButtonTextBlock(DeadImportButtonContent())
            };
            deadImportButton.Click += DeadImportButton_Click;
            wrapPanel.Children.Add(deadImportButton);
        }

        #region TestModeButton

        private const string DeadImportButton = "DeadImportButton";
        private bool _showDeadImports = false;

        private string DeadImportButtonContent() =>
            _showDeadImports ? "Dead Imports: Show" : "Dead Imports: Hide";

        private void DeadImportButton_Click(object __, RoutedEventArgs _)
        {
            _showDeadImports = !_showDeadImports;
            UpdateUX(() => GetDebugInterfaceTextBlock(DeadImportButton).Text = DeadImportButtonContent());
            Dispatcher.Invoke(() => UpdateTabs());
        }

        #endregion

        #region RefreshIntervalButton

        private const string RefreshIntervalButtonName = "RefreshIntervalButton";
        private int SelectedRefreshInterval = 0;
        private readonly int[] RefreshIntervalOptions = new[] { 60000, 15000, 5000 };

        private string RefreshIntervalButtonContent() =>
            $"Refresh: {dataExtractor.Interval}";

        private void RefreshIntervalButton_Click(object __, RoutedEventArgs _)
        {
            SelectedRefreshInterval = (SelectedRefreshInterval + 1) % RefreshIntervalOptions.Length;
            dataExtractor.Interval = RefreshIntervalOptions[SelectedRefreshInterval];
            UpdateUX(() => GetDebugInterfaceTextBlock(RefreshIntervalButtonName).Text = RefreshIntervalButtonContent());
        }

        #endregion

        #region CycleIntervalButton

        private const string CycleIntervalButtonName = "CycleIntervalButton";
        private int SelectedCycleInterval = 0;
        private readonly int[] CycleIntervalOptions = new[] { 0, 15000, 30000, 60000 };
        private readonly Timer tabCycleTimer = new() { Enabled = false, AutoReset = true };

        private string CycleIntervalButtonContent() =>
            $"Tab cycle: {(tabCycleTimer.Enabled ? tabCycleTimer.Interval : "Off")}";

        private void CycleTabs(object _, ElapsedEventArgs __)
        {
            // TODO: Cycle through tabs
            // TODO: Add tab selector to only cycle specific tabs
        }

        private void CycleIntervalButton_Click(object __, RoutedEventArgs _)
        {
            SelectedCycleInterval = (SelectedCycleInterval + 1) % CycleIntervalOptions.Length;
            var interval = CycleIntervalOptions[SelectedCycleInterval];
            if (interval == 0)
            {
                tabCycleTimer.Enabled = false;
            }
            else
            {
                tabCycleTimer.Interval = interval;
                tabCycleTimer.Enabled = true;
            }
            UpdateUX(() => GetDebugInterfaceTextBlock(CycleIntervalButtonName).Text = CycleIntervalButtonContent());
        }

        #endregion


        #region MemoryPurgeButton

        private const string MemoryPurgeButtonName = "MemoryPurgeButton";

        private void MemoryPurgeButton_Click(object __, RoutedEventArgs _)
        {
            _helperLogReader.PurgeMemory();
            _trackerLogReader.PurgeMemory();
            Dispatcher.Invoke(() => UpdateTabs());
        }

        #endregion

        private TextBlock GetDebugInterfaceTextBlock(string buttonName) =>
            (Main.Children.OfType<Border>().First(x => x.Name == DebugInterfaceName).Child as WrapPanel)
                .Children.OfType<Button>().First(x => x.Name == buttonName)
                    .Content as TextBlock;

    }
}
