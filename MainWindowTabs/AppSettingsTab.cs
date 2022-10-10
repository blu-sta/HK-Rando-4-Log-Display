using System.Linq;
using System.Windows;
using System.Windows.Controls;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private void AddButtonsToAppSettingsTab()
        {
            AddButtonsToGrid(HelperLocationGroupingOptions, HelperLocationGroupOptions);
            AddButtonsToGrid(HelperLocationOrderingOptions, HelperLocationOrderOptions);
            AddButtonsToGrid(HelperTransitionGroupingOptions, HelperTransitionGroupOptions);
            AddButtonsToGrid(HelperTransitionOrderingOptions, HelperTransitionOrderOptions);

            AddButtonsToGrid(TrackerItemGroupingOptions, TrackerItemGroupOptions);
            AddButtonsToGrid(TrackerItemOrderingOptions, TrackerItemOrderOptions);
            AddButtonsToGrid(TrackerTransitionGroupingOptions, TrackerTransitionGroupOptions);
            AddButtonsToGrid(TrackerTransitionOrderingOptions, TrackerTransitionOrderOptions);

            AddButtonsToGrid(SpoilerItemGroupingOptions, SpoilerItemGroupOptions);
            AddButtonsToGrid(SpoilerItemOrderingOptions, SpoilerItemOrderOptions);
            AddButtonsToGrid(SpoilerTransitionGroupingOptions, SpoilerTransitionGroupOptions);
            AddButtonsToGrid(SpoilerTransitionOrderingOptions, SpoilerTransitionOrderOptions);
        }

        private void AddButtonsToGrid(string[] options, Grid buttonGrid)
        {
            for (var i = 0; i < options.Length; i++)
            {
                var button = new Button
                {
                    Content = new TextBlock
                    {
                        Text = options[i],
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center
                    },
                    Style = FindResource("RoundButton") as Style,
                    Margin = new Thickness(2),
                };
                button.Click += AppSettingsButton_Click;

                Grid.SetColumn(button, i % 3);
                Grid.SetRow(button, i / 3);
                buttonGrid.Children.Add(button);

                if (i / 3 == 0)
                {
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                }
                if (i % 3 == 0)
                {
                    buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                }
            }
        }

        private void SetSettingAppSettingsActiveButtons()
        {
            UpdateUX(() => SetActiveButton(HelperLocationGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationGrouping));
            UpdateUX(() => SetActiveButton(HelperLocationOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationOrder));
            UpdateUX(() => SetActiveButton(HelperTransitionGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperTransitionGrouping));
            UpdateUX(() => SetActiveButton(HelperTransitionOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperTransitionOrder));
            UpdateUX(() => SetActiveButton(TrackerItemGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerItemGrouping));
            UpdateUX(() => SetActiveButton(TrackerItemOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerItemOrder));
            UpdateUX(() => SetActiveButton(TrackerTransitionGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerTransitionGrouping));
            UpdateUX(() => SetActiveButton(TrackerTransitionOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedTrackerTransitionOrder));
            UpdateUX(() => SetActiveButton(SpoilerItemGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerItemGrouping));
            UpdateUX(() => SetActiveButton(SpoilerItemOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerItemOrder));
            UpdateUX(() => SetActiveButton(SpoilerTransitionGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerTransitionGrouping));
            UpdateUX(() => SetActiveButton(SpoilerTransitionOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedSpoilerTransitionOrder));
        }
    }
}
