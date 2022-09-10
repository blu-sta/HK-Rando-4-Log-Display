using HK_Rando_4_Log_Display.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HK_Rando_4_Log_Display.FileReader
{
    public interface IItemSpoilerReader : ILogReader
    {
        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool();
        public List<ItemWithLocation> GetItems();
    }

    public class ItemSpoilerReader : IItemSpoilerReader
    {
        private readonly IResourceLoader _resourceLoader;
        private readonly List<ItemWithLocation> _spoilerItems = new();

        public bool IsFileFound { get; private set; }

        public ItemSpoilerReader(IResourceLoader resourceLoader)
        {
            _resourceLoader = resourceLoader;
            LoadData();
        }

        public void LoadData()
        {
            var filepath = Constants.ItemSpoilerLogPath;
            if (!File.Exists(filepath))
            {
                IsFileFound = false;
                return;
            }

            IsFileFound = true;
            var itemSpoilerData = File.ReadAllLines(filepath).ToList();

            LoadItemSpoiler(itemSpoilerData);
        }

        private void LoadItemSpoiler(List<string> itemSpoilerData)
        {
            _spoilerItems.Clear();
            var start = itemSpoilerData.IndexOf("[");
            if (start < 0)
            {
                return;
            }
            var end = itemSpoilerData.IndexOf("]", start);
            if (end < 0)
            {
                return;
            }

            var itemSpoilerString = string.Join("", itemSpoilerData.Where((_, i) => i >= start && i <= end));

            var spoilerItems = JsonConvert.DeserializeObject<List<SpoilerItem>>(itemSpoilerString);
            spoilerItems.ForEach(x =>
            {
                var item = x.Item;
                var location = x.Location;
                var cost = x.Costs != null ? string.Join(",", x.Costs) : null;
                var referenceItem = _resourceLoader.Items.FirstOrDefault(y => y.Name == item) 
                    ?? new Item { Name = item, Pool = location == "Start" ? "Start" : "undefined" };

                var itemWithLocation = new ItemWithLocation(referenceItem, x.Costs == null ? location : $"{location} [{cost}]");
                _spoilerItems.Add(itemWithLocation);
            });
        }

        private class SpoilerItem
        {
            public string Item { get; set; }
            public string Location { get; set; }
            public string[] Costs { get; set; }
        }

        public Dictionary<string, List<ItemWithLocation>> GetItemsByPool() =>
            _spoilerItems.GroupBy(x => x.Pool).OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.ToList());

        public List<ItemWithLocation> GetItems() => _spoilerItems;
    }
}
