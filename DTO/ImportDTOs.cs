namespace HK_Rando_4_Log_Display.DTO
{
    public class ItemImport
    {
        public string Name { get; set; }
        public string Pool { get; set; }
    }

    public class LocationImport
    {
        public string Name { get; set; }
        public string SceneName { get; set; }
    }

    public class TransitionImport
    {
        public string Name => $"{SceneName}[{DoorName}]";
        public string SceneName { get; set; }
        public string DoorName { get; set; }
    }

    public class RoomImport
    {
        public string SceneName { get; set; }
        public string MapArea { get; set; }
        public string TitledArea { get; set; }
    }
}
