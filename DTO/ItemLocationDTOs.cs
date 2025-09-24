using System;

namespace HK_Rando_4_Log_Display.DTO
{
    public class LocationPreview : ItemWithLocation
    {
        public string CostString { get; set; }
        public int PrimaryCost { get; set; }
        public int SecondaryCost { get; set; }
    }

    public class SpoilerItemWithLocation : ItemWithLocation
    {
        public SpoilerItemWithLocation() { }

        public string Cost { get; set; }
        public bool IsObtained { get; set; }

        public SpoilerItemWithLocation(SpoilerItemWithLocation other, bool isOutOfLogic)
        {
            Item = other.Item;
            Location = other.Location;
            Location.IsOutOfLogic = isOutOfLogic;
            Cost = other.Cost;
            IsObtained = other.IsObtained;
        }
    }
    
    public class ItemWithLocation
    {
        public Item Item { get; set; }
        public Location Location { get; set; }
        public DateTime? TimeAdded => Location.TimeAdded;
    }
}
