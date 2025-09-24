using System;

namespace HK_Rando_4_Log_Display.DTO
{
    public class TransitionWithDestination
    {
        public TransitionWithDestination() { }

        public Transition Source { get; set; }
        public Transition Destination { get; set; }
        public bool IsTraversed { get; set; }

        public TransitionWithDestination(TransitionWithDestination other)
        {
            Source = other.Source;
            Destination = other.Destination;
            IsTraversed = other.IsTraversed;
        }
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
        public string SceneDescription { get; set; }
    }
}
