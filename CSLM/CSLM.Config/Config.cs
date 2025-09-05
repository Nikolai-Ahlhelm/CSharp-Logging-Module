using Newtonsoft.Json;

namespace CSLM.Config
{

    [JsonObject(MemberSerialization.Fields)]
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

    [JsonObject(MemberSerialization.Fields)]
    internal class FileConfig
    {
        internal string filePath = "\\logs";
        internal string fileName = "log-%hh%-%m%-%ss%.txt";
    }

    [JsonObject(MemberSerialization.Fields)]
    internal class SQLConfig
    {

        internal string sqlServer = "localhost";
        internal int sqlPort = 1433;
        internal string sqlUser = "cslm_user";
        internal string sqlPassword = "password";
        internal string databaseName = "CSLM";
        internal string tablePrefix = "CSLM_";
    }

    [JsonObject(MemberSerialization.Fields)]
    internal class SQLiteConfig
    {
        internal string sqliteFilePath = "cslm.db";
        internal string tablePrefix = "CSLM_";
    }


}

