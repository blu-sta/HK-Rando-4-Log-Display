using System;

namespace HK_Rando_4_Log_Display.DTO
{
    public class TransitionWithDestination
    {
        public Transition Source { get; set; }
        public Transition Destination { get; set; }
    }

    public class Transition : ReferenceTransition
    {
        public bool IsOutOfLogic { get; set; }
        public DateTime? TimeAdded { get; set; }
    }

    public class ReferenceTransition : TransitionImport
    {
        public string MapArea { get; set; }
        public string TitledArea { get; set; }
    }
}
