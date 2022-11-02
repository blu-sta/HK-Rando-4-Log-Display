using HK_Rando_4_Log_Display.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HK_Rando_4_Log_Display
{
    public partial class MultiWorldWindow : Window
    {
        public static string[] PlayerNames = Array.Empty<string>();

        public MultiWorldWindow(string[] multiWorldPlayerNames)
        {
            InitializeComponent();
            
            PlayerNames = multiWorldPlayerNames;
            if (PlayerNames.Any())
            {
                InitialTextBox.Text = PlayerNames[0];
                for (int i = 1; i < PlayerNames.Length; i++)
                {
                    var playerName = PlayerNames[i];
                    var textBox = AddTextBox();
                    textBox.Text = playerName;
                }
                AddTextBox();
            }
            var lastTextBox = MultiWorldStackPanel.Children.OfType<TextBox>().LastOrDefault();
            if (lastTextBox != null)
            {
                lastTextBox.Focus();
                lastTextBox.CaretIndex = lastTextBox.Text.Length;
            }

#if DEBUG
            Predict_Button.Visibility = Visibility.Visible;
#endif
        }

        private void Add_Click(object sender, RoutedEventArgs e) => 
            AddTextBox().Focus();

        private TextBox AddTextBox()
        {
            var textBox = new TextBox
            {
                Style = FindResource("MultiWorldPlayerTextBox") as Style
            };
            MultiWorldStackPanel.Children.Add(textBox);
            return textBox;
        }


        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if (MultiWorldStackPanel.Children.Count > 0)
            {
                MultiWorldStackPanel.Children.RemoveAt(MultiWorldStackPanel.Children.Count - 1);
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            MultiWorldStackPanel.Children.Clear();
        }

        private void Predict_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Spoiler log does not have this information!!
            // Try again with something else?
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            PlayerNames = MultiWorldStackPanel.Children.OfType<TextBox>()
                .Select(x => x.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToArray();
            Close();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = sender as ScrollViewer;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta / 8.0);
            e.Handled = true;
        }
    }
}
