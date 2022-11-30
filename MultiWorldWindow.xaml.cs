using HK_Rando_4_Log_Display.FileReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace HK_Rando_4_Log_Display
{
    public partial class MultiWorldWindow : Window
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "Used by parent window")]
        public static string[] PlayerNames = Array.Empty<string>();
        private readonly IItemSpoilerReader _itemSpoilerReader;

        public MultiWorldWindow(string[] multiWorldPlayerNames, IItemSpoilerReader itemSpoilerReader)
        {
            InitializeComponent();
            
            _itemSpoilerReader = itemSpoilerReader;

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
            if (MultiWorldStackPanel.Children.Count == 0)
            {
                AddTextBox().Focus();
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            MultiWorldStackPanel.Children.Clear();
            AddTextBox().Focus();
        }

        private void Predict_Click(object sender, RoutedEventArgs e)
        {
            var allItems = _itemSpoilerReader.GetItems();
            var existingNames = PlayerNames.Where(x => allItems.Any(y => y.Item.MWPlayerName == x || y.Location.MWPlayerName == x));
            var unrecognisedLocations = allItems.Where(x => x.Location.Pool.StartsWith(">")).Select(x => x.Location.Name);
            var unrecognisedItems = allItems.Where(x => x.Item.Pool.StartsWith(">")).Select(x => x.Item.Name);
            var unrecognisedValues = unrecognisedLocations.Concat(unrecognisedItems);
            var possibleNames = existingNames
                .Concat(unrecognisedValues.Select(x => Regex.Match(x, "^(.*)'s ").Groups[1].Value))
                .Distinct()
                .Where(x => !string.IsNullOrEmpty(x))
                .ToList();
            if (possibleNames.Any())
            {
                MultiWorldStackPanel.Children.Clear();
                possibleNames.ForEach(x =>
                {
                    var textBox = AddTextBox();
                    textBox.Text = x;
                });
                AddTextBox().Focus();
            }
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
