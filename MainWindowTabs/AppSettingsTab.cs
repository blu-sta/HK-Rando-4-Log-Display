using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using static HK_Rando_4_Log_Display.Constants.Constants;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private void AddButtonsToAppSettingsTab()
        {
            AddButtonsToGrid(HelperLocationGroupingOptions, HelperLocationGroupOptions);
            AddButtonsToGrid(HelperLocationOrderingOptions, HelperLocationOrderOptions);
            AddButtonsToGrid(HelperLocationOutOfLogicOrderingOptions, HelperLocationOutOfLogicOrderOptions);
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

                if (options.Length == 4)
                {
                    Grid.SetColumn(button, i % 2);
                    Grid.SetRow(button, i / 2);
                    buttonGrid.Children.Add(button);

                    if (i / 2 == 0)
                    {
                        buttonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    }
                    if (i % 2 == 0)
                    {
                        buttonGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                    }
                }
                else
                {
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
        }

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
                case nameof(HelperLocationOutOfLogicOrderOptions):
                    _appSettings.SelectedHelperLocationOutOfLogicOrder = Array.FindIndex(HelperLocationOutOfLogicOrderingOptions, x => x == buttonText);
                    UpdateUX(() => SetActiveButton(HelperLocationOutOfLogicOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationOutOfLogicOrder));
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

        private void SetSettingAppSettingsActiveButtons()
        {
            UpdateUX(() => SetActiveButton(HelperLocationGroupOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationGrouping));
            UpdateUX(() => SetActiveButton(HelperLocationOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationOrder));
            UpdateUX(() => SetActiveButton(HelperLocationOutOfLogicOrderOptions.Children.OfType<Button>().ToArray(), _appSettings.SelectedHelperLocationOutOfLogicOrder));
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

        private static void SetActiveButton(Button[] buttons, int activeIndex)
        {
            for (var i = 0; i < buttons.Length; i++)
            {
                buttons[i].Background = i == activeIndex ? Brushes.LightBlue : Brushes.LightGray;
            }
        }
    }
}
