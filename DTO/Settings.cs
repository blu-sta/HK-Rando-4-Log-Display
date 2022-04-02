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
