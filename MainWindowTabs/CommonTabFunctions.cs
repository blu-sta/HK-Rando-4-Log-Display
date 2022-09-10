using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace HK_Rando_4_Log_Display
{
    public partial class MainWindow
    {
        private Grid GenerateAutoStarGrid(List<KeyValuePair<string, string>> itemsForGrid)
        {
            var objectGrid = new Grid
            {
                Margin = new Thickness(20, 0, 0, 0)
            };
            var colDef1 = new ColumnDefinition();
            var colDef2 = new ColumnDefinition();
            colDef1.Width = new GridLength(1, GridUnitType.Auto);
            colDef2.Width = new GridLength(1, GridUnitType.Star);
            objectGrid.ColumnDefinitions.Add(colDef1);
            objectGrid.ColumnDefinitions.Add(colDef2);

            var textBlocks = itemsForGrid.SelectMany((x, i) =>
            {
                var rowDef = new RowDefinition();
                objectGrid.RowDefinitions.Add(rowDef);

                var leftBlock = x.Key.StartsWith("<b>")
                    ? GetBoldTextBlock(x.Key.Substring(3))
                    : x.Key.StartsWith("<s>")
                    ? GetStrikethroughTextBlock(x.Key.Substring(3))
                    : GetStandardTextBlock(x.Key);

                Grid.SetColumn(leftBlock, 0);
                Grid.SetRow(leftBlock, i);

                var rightBlock = x.Value.StartsWith("<b>")
                    ? GetBoldTextBlock(x.Value.Substring(3))
                    : x.Value.StartsWith("<s>")
                    ? GetStrikethroughTextBlock(x.Value.Substring(3))
                    : GetStandardTextBlock(x.Value);
                rightBlock.Margin = new Thickness(20, 0, 0, 0);
                Grid.SetColumn(rightBlock, 1);
                Grid.SetRow(rightBlock, i);

                return new[] { leftBlock, rightBlock };
            });
            textBlocks.ToList().ForEach(x => objectGrid.Children.Add(x));
            return objectGrid;
        }

        private TextBlock GetBoldTextBlock(string message)
        {
            var textBlock = new TextBlock { Text = "" };
            textBlock.Inlines.Add(new Run(message) { FontWeight = FontWeights.Bold });
            return textBlock;
        }

        private TextBlock GetStrikethroughTextBlock(string message) => new TextBlock { Text = message, TextDecorations = TextDecorations.Strikethrough };

        private TextBlock GetStandardTextBlock(string message) => new TextBlock { Text = message };
    }
}
