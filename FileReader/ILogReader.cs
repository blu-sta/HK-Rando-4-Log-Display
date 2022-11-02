namespace HK_Rando_4_Log_Display.FileReader
{
    public interface ILogReader
    {
        public bool IsFileFound { get; }
        public bool IsFileLoaded { get; }
        public void LoadData(string[] multiWorldPlayerNames);
        public void OpenFile();
    }
}
