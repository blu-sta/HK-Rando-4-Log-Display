namespace HK_Rando_4_Log_Display.DTO
{
    public class ItemWithLocation : Item
    {
        public string Location { get; set; }

        public ItemWithLocation(Item item) : this (item, string.Empty) { }

        public ItemWithLocation(Item item, string location)
        {
            Name = item?.Name;
            Pool = item?.Pool;
            Location = location;
        }
    }
    
    public class Item
    {
        public string Name { get; set; }
        public string Pool { get; set; }
    }
}
