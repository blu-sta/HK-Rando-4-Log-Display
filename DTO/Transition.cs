using System;

namespace HK_Rando_4_Log_Display.DTO
{
    public class TransitionWithTime : Transition
    {
        public DateTime TimeAdded { get; set; }
        public bool IsOutOfLogic { get; set; }

        public TransitionWithTime() { }

        public TransitionWithTime(Transition transition, bool isOutOfLogic) : this(transition, DateTime.Now, isOutOfLogic) { }

        public TransitionWithTime(Transition transition, DateTime time, bool isOutOfLogic)
        {
            SceneName = transition?.SceneName;
            DoorName = transition?.DoorName;
            MapArea = transition?.MapArea;
            TitledArea = transition?.TitledArea;
            TimeAdded = time;
            IsOutOfLogic = isOutOfLogic;
        }
    }
    
    public class TransitionWithDestination : Transition
    {
        public string DestinationName => $"{DestinationSceneName}[{DestinationDoorName}]";
        public string DestinationSceneName { get; set; }
        public string DestinationDoorName { get; set; }

        public TransitionWithDestination(Transition transition, string destinationSceneName, string destinationDoorName)
        {
            SceneName = transition.SceneName;
            DoorName = transition.DoorName;
            MapArea = transition.MapArea;
            TitledArea = transition.TitledArea;
            DestinationSceneName = destinationSceneName;
            DestinationDoorName = destinationDoorName;
        }
    }

    public class Transition
    {
        public string Name => $"{SceneName}[{DoorName}]";
        public string SceneName { get; set; }
        public string DoorName { get; set; }
        public string MapArea { get; set; }
        public string TitledArea { get; set; }
    }

    public class TransitionImport
    {
        public string SceneName { get; set; }
        public string DoorName { get; set; }
    }
}
