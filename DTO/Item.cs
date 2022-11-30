namespace HK_Rando_4_Log_Display.DTO
{
    public class Item : ReferenceItem
    {
        public string MWPlayerName { get; set; }
    }

    public class ReferenceItem : ItemImport
    {
        public string PreviewName { get; set; }
    }
}
