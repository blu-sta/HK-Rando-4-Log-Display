using System;

namespace HK_Rando_4_Log_Display.DTO
{
    public class Location : ReferenceLocation
    {
        public bool IsOutOfLogic { get; set; }
        public DateTime? TimeAdded { get; set; }
    }

    public class ReferenceLocation : LocationImport
    {
        public string Pool { get; set; }
        public string MapArea { get; set; }
        public string TitledArea { get; set; }
        public string SceneDescription { get; set; }
    }
}
