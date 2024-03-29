﻿using HK_Rando_4_Log_Display.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

using static HK_Rando_4_Log_Display.Utils.Utils;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private static TextBlock GenerateButtonTextBlock(string text) => new()
        {
            Text = text,
            TextWrapping = TextWrapping.WrapWithOverflow,
            TextAlignment = TextAlignment.Center
        };

        private static StackPanel GenerateStackPanel() => new() { Margin = GenerateStandardThickness() };

        private static Thickness GenerateStandardThickness() => new(20, 0, 0, 0);

        private static Expander GenerateExpanderWithContent(string headerName, object contentObject, HashSet<string> expandedHashset, string headerNameExtension = null)
        {
            var expander = new Expander
            {
                Name = headerName.AsObjectName(),
                Header = ConcatStrings(headerName, headerNameExtension),
                Content = contentObject,
                IsExpanded = expandedHashset.Contains(headerName.AsObjectName())
            };
            expander.Expanded += (object _, RoutedEventArgs e) =>
            {
                expandedHashset.Add((e.Source as Expander).Name);
            };
            expander.Collapsed += (object _, RoutedEventArgs e) =>
            {
                expandedHashset.Remove((e.Source as Expander).Name);
            };
            return expander;
        }

        private static Expander GenerateExpanderWithContent(string headerName, object contentObject, BoolWrapper expandedBoolWrapper, string headerNameExtension = null)
        {
            var expander = new Expander
            {
                Name = headerName.AsObjectName(),
                Header = ConcatStrings(headerName, headerNameExtension),
                Content = contentObject,
                IsExpanded = expandedBoolWrapper.Value
            };
            expander.Expanded += (object _, RoutedEventArgs e) =>
            {
                if (expander.Name == (e.Source as Expander).Name)
                    expandedBoolWrapper.Value = true;
            };
            expander.Collapsed += (object _, RoutedEventArgs e) =>
            {
                if (expander.Name == (e.Source as Expander).Name)
                    expandedBoolWrapper.Value = false;
            };
            return expander;
        }

        private class BoolWrapper
        {
            public bool Value { get; set; }
            public BoolWrapper(bool value) { Value = value; }
            public BoolWrapper() { }
        }

        private static Grid GenerateAutoStarGrid(List<KeyValuePair<string, string>> itemsForGrid)
        {
            var objectGrid = new Grid
            {
                Margin = GenerateStandardThickness()
            };
            var colDef1 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Auto)
            };
            var colDef2 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            };
            objectGrid.ColumnDefinitions.Add(colDef1);
            objectGrid.ColumnDefinitions.Add(colDef2);

            var textBlocks = itemsForGrid.SelectMany((x, i) =>
            {
                var rowDef = new RowDefinition();
                objectGrid.RowDefinitions.Add(rowDef);

                var leftBlock = x.Key.StartsWith("<b>")
                    ? GetBoldTextBlock(x.Key[3..])
                    : x.Key.StartsWith("<s>")
                    ? GetStrikethroughTextBlock(x.Key[3..])
                    : GetStandardTextBlock(x.Key);

                Grid.SetColumn(leftBlock, 0);
                Grid.SetRow(leftBlock, i);

                var rightBlock = x.Value.StartsWith("<b>")
                    ? GetBoldTextBlock(x.Value[3..])
                    : x.Value.StartsWith("<s>")
                    ? GetStrikethroughTextBlock(x.Value[3..])
                    : GetStandardTextBlock(x.Value);
                rightBlock.Margin = GenerateStandardThickness();
                Grid.SetColumn(rightBlock, 1);
                Grid.SetRow(rightBlock, i);

                return new[] { leftBlock, rightBlock };
            });
            textBlocks.ToList().ForEach(x => objectGrid.Children.Add(x));
            return objectGrid;
        }

        private static TextBlock GetBoldTextBlock(string message)
        {
            var textBlock = new TextBlock { Text = "" };
            textBlock.Inlines.Add(new Run(message) { FontWeight = FontWeights.Bold });
            return textBlock;
        }

        private static TextBlock GetStrikethroughTextBlock(string message) => new() { Text = message, TextDecorations = TextDecorations.Strikethrough };

        private static TextBlock GetStandardTextBlock(string message) => new() { Text = message };

        #region Events

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

        private readonly DispatcherTimer _dispatcherTimer = new() { Interval = TimeSpan.FromSeconds(5) };

        private void ShowInPopup(string message)
        {
            MainPopup_TextBox.Text = message;
            MainPopup.IsOpen = true;

            _dispatcherTimer.Stop();
            _dispatcherTimer.Tick -= ClosePopup;
            _dispatcherTimer.Start();
            _dispatcherTimer.Tick += ClosePopup;
        }

        private void ClosePopup(object __, EventArgs _)
        {
            MainPopup.IsOpen = false;
            _dispatcherTimer.Stop();
        }

        #endregion
    }
}
