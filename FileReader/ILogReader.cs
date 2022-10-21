namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ILogReader
    {
        public bool IsFileFound { get; }
        public void LoadData();
        public void OpenFile();
    }
}
