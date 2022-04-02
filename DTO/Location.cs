using System;

namespace HK_Rando_4_Log_Display.DTO
{
    public class LocationWithTime : Location
    {
        public DateTime TimeAdded { get; set; }
        public bool IsOutOfLogic { get; set; }

        public LocationWithTime() { }

        public LocationWithTime(Location location, bool isOutOfLogic) : this(location, DateTime.Now, isOutOfLogic) { }
        
        public LocationWithTime(Location location, DateTime time, bool isOutOfLogic)
        {
            Name = location?.Name;
            SceneName = location?.SceneName;
            MapArea = location?.MapArea;
            TitledArea = location?.TitledArea;
            TimeAdded = time;
            IsOutOfLogic = isOutOfLogic;
        }
    }


    public class Location
    {
        public string Name { get; set; }
        public string SceneName { get; set; }
        public string MapArea { get; set; }
        public string TitledArea { get; set; }
    }

    public class LocationImport
    {
        public string Name { get; set; }
        public string SceneName { get; set; }
    }

    public class RoomImport
    {
        public string SceneName { get; set; }
        public string MapArea { get; set; }
        public string TitledArea { get; set; }
    }
}
