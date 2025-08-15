namespace CSLM.Config
{

    public class Config
    {
        internal string logType = "DEFAULT";
        internal bool printToConsole = true;
        internal string timestampFormat = "dd-MM-yyyy HH:mm:ss.fff";
        internal string logStoreType = "File"; // Options: SQL, SQLite, File
        internal FileConfig fileConfig = new FileConfig();
        internal SQLiteConfig sqliteConfig = new SQLiteConfig();
        internal SQLConfig sqlConfig = new SQLConfig();
    }

    internal class FileConfig
    {
        private string filePath = "\\logs";
        private string fileName = "log-%hh%-%m%-%ss%.txt";
    }

    internal class SQLConfig
    {

        private string sqlServer = "localhost";
        private int sqlPort = 1433;
        private string sqlUser = "cslm_user";
        private string sqlPassword = "password";
        private string databaseName = "CSLM";
        private string tablePrefix = "CSLM_";
    }

    internal class SQLiteConfig
    {
        private string sqliteFilePath = "cslm.db";
        private string tablePrefix = "CSLM_";
    }


}

