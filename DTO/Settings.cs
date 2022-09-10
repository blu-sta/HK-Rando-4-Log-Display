namespace HK_Rando_4_Log_Display.DTO
{
    public class Settings
    {
        public int? SelectedHelperLocationGrouping { get; set; }
        public int? SelectedHelperLocationOrder { get; set; }
        public int? SelectedHelperTransitionGrouping { get; set; }
        public int? SelectedHelperTransitionOrder { get; set; }
        public int? SelectedTrackerItemGrouping { get; set; }
        public int? SelectedTrackerItemOrder { get; set; }
        public int? SelectedTrackerTransitionGrouping { get; set; }
        public int? SelectedTrackerTransitionOrder { get; set; }
        public int? SelectedSpoilerItemGrouping { get; set; }
        public int? SelectedSpoilerItemOrder { get; set; }
        public int? SelectedSpoilerTransitionGrouping { get; set; }
        public int? SelectedSpoilerTransitionOrder { get; set; }

        public void ResetOutOfRangeValues(
            int helperGroupingsLength,
            int helperOrdersLength,
            int trackerItemGroupingsLength,
            int trackerItemOrdersLength,
            int trackerTransitionOrdersLength,
            int spoilerItemGroupingsLength,
            int spoilerItemOrdersLength,
            int spoilerTransitionOrders)
        {
            if(SelectedHelperLocationGrouping >= helperGroupingsLength)
                SelectedHelperLocationGrouping = null;
            if (SelectedHelperLocationOrder >= helperOrdersLength)
                SelectedHelperLocationOrder = null;
            if (SelectedHelperTransitionGrouping >= helperGroupingsLength)
                SelectedHelperTransitionGrouping = null;
            if (SelectedHelperTransitionOrder >= helperOrdersLength)
                SelectedHelperTransitionOrder = null;
            if (SelectedTrackerItemGrouping >= trackerItemGroupingsLength)
                SelectedTrackerItemGrouping = null;
            if (SelectedTrackerItemOrder >= trackerItemOrdersLength)
                SelectedTrackerItemOrder = null;
            if (SelectedTrackerTransitionGrouping >= helperGroupingsLength)
                SelectedTrackerTransitionGrouping = null;
            if (SelectedTrackerTransitionOrder >= trackerTransitionOrdersLength)
                SelectedTrackerTransitionOrder = null;
            if (SelectedSpoilerItemGrouping >= spoilerItemGroupingsLength)
                SelectedSpoilerItemGrouping = null;
            if (SelectedSpoilerItemOrder >= spoilerItemOrdersLength)
                SelectedSpoilerItemOrder = null;
            if (SelectedSpoilerTransitionGrouping >= helperGroupingsLength)
                SelectedSpoilerTransitionGrouping = null;
            if (SelectedSpoilerTransitionOrder >= spoilerTransitionOrders) 
                SelectedSpoilerTransitionOrder = null;
        }

        public void SetDefaultValues()
        {
            SelectedHelperLocationGrouping ??= 0;
            SelectedHelperLocationOrder ??= 1;
            SelectedHelperTransitionGrouping ??= 0;
            SelectedHelperTransitionOrder ??= 1;
            SelectedTrackerItemGrouping ??= 0;
            SelectedTrackerItemOrder ??= 1;
            SelectedTrackerTransitionGrouping ??= 0;
            SelectedTrackerTransitionOrder ??= 0;
            SelectedSpoilerItemGrouping ??= 0;
            SelectedSpoilerItemOrder ??= 1;
            SelectedSpoilerTransitionGrouping ??= 0;
            SelectedSpoilerTransitionOrder ??= 0;
        }
    }
}
