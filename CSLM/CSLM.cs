using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Threading;
using CSLM.Async;

namespace CSLM
{
    public class LogEntry
    {
        public string Type;
        public DateTime Timestamp;
        public ConsoleColor Color;
        public string Message;
    }
    


    public class CSLM
    {
        //// Variables
        
        // CSLM ConfigManager
        private Config.ConfigManager _configManager;
        
        // CSLM Config
        private Config.Config _config;
        
        // CSLM Debug Mode
        private bool _cslmDebugMode = false;
        public bool CSLMDebugMode
        {
            get => _cslmDebugMode;
            set => _cslmDebugMode = value;
        }

        // Log file name, can contain tokens like %dd%, %MM%, %yyyy%, %hh%, %m%, %ss%
        // Example: "log_%dd%-%MM%-%yyyy%_%hh%-%m%-%ss%.txt"
        public string LogFileName
        {
            get => _config.fileConfig.fileName;
            set => _config.fileConfig.fileName = ReplaceTokens(value);
        }
        // Property to get or set the log file name


        // Relative log file path
        public string LogFilePath
        {
            get => _config.fileConfig.filePath;
            set 
            {
                _config.fileConfig.filePath = Path.GetFullPath(value);
                Directory.CreateDirectory(LogFilePath);
            }
        }


        // Log type, can be one of the predefined types or a custom type

        public string LogType
        {
            get => _config.logType;
            set
            {
                _config.logType = NormalizeLogType(value);
                RefreshAllowedTypes();
            }
        }

        // Bool to enable or disable printing log entries to the console
        public bool PrintToConsole
        {
            get => _config.printToConsole;
            set => _config.printToConsole = value;
        }


        // Timestamp format for log entries
        public string TimestampFormat
        {
            get => _config.timestampFormat;
            set => _config.timestampFormat = value;
        }

        // Full path of the log file
        public string LogFileFullPath => Path.Combine(LogFilePath, LogFileName);

        // Dictionary to hold log types and their corresponding console colors
        private readonly Dictionary<string, ConsoleColor> _typeColors = new()
        {
            { "INFO", ConsoleColor.Cyan },
            { "ERROR", ConsoleColor.Red },
            { "WARNING", ConsoleColor.Yellow },
            { "CRITICAL", ConsoleColor.DarkRed },
            { "DEBUG", ConsoleColor.Green },
            { "DEFAULT", ConsoleColor.Gray }
        };

        // Dictionary to hold log levels and their allowed types
        private readonly Dictionary<string, List<string>> _logLevels = new()
        {
            { "DEFAULT", new() { "ERROR", "INFO", "WARNING", "CRITICAL" } },
            { "DEBUG", new() { "ERROR", "INFO", "WARNING", "CRITICAL", "DEBUG" } },
            { "PRODUCTIVE", new() { "ERROR", "INFO", "CRITICAL" } },
            { "ERROR", new() { "ERROR" } },
            { "CRITICAL", new() { "CRITICAL" } },
            { "NONE", new() }
        };

        // List of allowed types for the current log type
        private List<string> _allowedTypes;

        // Lock object for thread safety
        // This is used to ensure that file access is thread-safe
        private readonly Lock _fileLock = new();

        // Lock object for logging operations
        // This is used to ensure that console output and file writing are thread-safe
        private readonly Lock _logLock = new();

        // Lock object for allowed types
        // This is used to ensure that the allowed types list is thread-safe when being modified
        private readonly Lock _allowedTypesLock = new();
        
        // async console manager
        private AsyncConsoleManager _consoleManager;
        
        // async file manager
        private AsyncFileManager _fileManager;
        

        // Main constructor

        public CSLM(
            string logFileName = "log-%hh%-%m%-%ss%.txt", 
            string logFilePath = "logs", 
            string logType = "DEFAULT", 
            bool printToConsole = true, 
            string timestampFormat = "dd-MM-yyyy HH:mm:ss.fff"
            )
        {
            // Initialize async managers
            _consoleManager = new AsyncConsoleManager(this);
            _fileManager = new AsyncFileManager(this);

            // Initialize config & config manager
            _configManager = new Config.ConfigManager(this);
            _config = _configManager._config;

            // Apply parameters to config
            LogFileName = logFileName;
            LogFilePath = logFilePath;
            LogType = logType;
            PrintToConsole = printToConsole;
            TimestampFormat = timestampFormat;
        }
        
        public static CSLM ConfigFileMode(string configFilePath)
        {
            var cslm = new CSLM(); // normal init
            // config manager & config
            Console.WriteLine($"[CSLM:Core] [ConfigFileMode] Loading config from {Path.GetFullPath(configFilePath)}");
            cslm._configManager._configPath = Path.GetFullPath(configFilePath);
            cslm._configManager.LoadConfigFile(Path.GetFullPath(configFilePath));
            Console.WriteLine($"[CSLM:Core] [ConfigFileMode] Config applied");

            return cslm;
        }


        private void RefreshAllowedTypes()
        {
            lock (_allowedTypesLock)
            { 
                if (_logLevels.TryGetValue(LogType, out var allowedTypes))
                {
                    _allowedTypes = allowedTypes;
                }
                else
                {
                    //Console.WriteLine("[CSLM:Core] [RefreshAllowedTypes] Invalid logType, fallback to DEFAULT");
                    Error("[CSLM:Core] [RefreshAllowedTypes] Invalid logType, fallback to DEFAULT");
                    _allowedTypes = _logLevels["DEFAULT"];
                }
            }
        }


        private string ReplaceTokens(string fileName)
        {
            var now = DateTime.Now;
            return fileName
                .Replace("%dd%", now.ToString("dd"))
                .Replace("%MM%", now.ToString("MM"))
                .Replace("%yyyy%", now.ToString("yyyy"))
                .Replace("%hh%", now.ToString("HH"))
                .Replace("%m%", now.ToString("mm"))
                .Replace("%ss%", now.ToString("ss"));
        }

        private string NormalizeLogType(string type)
        {
            return type.ToUpper() switch
            {
                "DEF" => "DEFAULT",
                "DBG" => "DEBUG",
                "PROD" => "PRODUCTIVE",
                "ERR" => "ERROR",
                "CRIT" => "CRITICAL",
                _ => type.ToUpper()
            };
        }

        private static string NormalizeEntryType(string type)
        {
            return type.ToUpper() switch
            {
                "ERR" or "E" => "ERROR",
                "INF" or "I" => "INFO",
                "WARN" or "W" => "WARNING",
                "CRIT" or "C" => "CRITICAL",
                "DBG" or "D" => "DEBUG",
                _ => type.ToUpper()
            };
        }

        public void Entry(string type, string message, bool isCSLMDebugMessage = false)
        {
            // Normalize entry type
            string entryType = NormalizeEntryType(type);

            lock (_allowedTypesLock)
            {
                // Check if the entry type is allowed or custom, then log it
                if (_allowedTypes.Contains(entryType))
                {
                    WriteLog(entryType, message);
                }
                else if (!_logLevels["DEBUG"].Contains(entryType)) // Check if the entry type is not a CSLM default => is custom type => always log
                //INFO: Debug contains all CLSM default types, therefore, it is used to check for custom types
                {
                    //Custom types are always logged       
                    WriteLog(entryType, message);
                }
            }
        }

        internal void DebugWriteLog(string type, string message)
        {
            if (_cslmDebugMode)
            {
                string normalizedType = NormalizeEntryType(type);
                string clsmType = $"CLSM:{type}";
            
                // create logEntry object
                LogEntry debugLogEntry = new LogEntry();
                debugLogEntry.Type = clsmType;
                debugLogEntry.Timestamp = DateTime.Now;
                debugLogEntry.Message = message;
                debugLogEntry.Color = _typeColors.GetValueOrDefault(normalizedType, ConsoleColor.Magenta);  
                lock (_fileLock) { _fileManager.AddToQueue(debugLogEntry); }
                if (PrintToConsole) { lock (_logLock) { _consoleManager.AddToQueue(debugLogEntry); } }
            }
        }

        private void WriteLog(string type, string message)
        {
            var totalTime = System.Diagnostics.Stopwatch.StartNew();
            
            // create logEntry object
            LogEntry logEntry = new LogEntry();
            logEntry.Type = type;
            logEntry.Timestamp = DateTime.Now;
            logEntry.Message = message;
            logEntry.Color = _typeColors.GetValueOrDefault(type, ConsoleColor.Magenta);
            
            
            //string output = $"[{logEntry.Timestamp.ToString(TimestampFormat)}] [{type}] {message}";

            var fileWriteTime = System.Diagnostics.Stopwatch.StartNew();
            lock (_fileLock)
            {
                _fileManager.AddToQueue(logEntry);
                //File.AppendAllText(LogFileFullPath, $"{output}{Environment.NewLine}");
                //
            }
            fileWriteTime.Stop();
            
            var consoleWriteTime = System.Diagnostics.Stopwatch.StartNew();

            lock (_logLock)
            {
                if (PrintToConsole)
                {
                _consoleManager.AddToQueue(logEntry);
                }
                
                consoleWriteTime.Stop();
                totalTime.Stop();
                
                DebugWriteLog("Debug", $"totalTime: {totalTime.Elapsed.TotalMilliseconds} ms");
                DebugWriteLog("Debug", $"fileWriteTime: {fileWriteTime.Elapsed.TotalMilliseconds} ms");
                DebugWriteLog("Debug", $"consoleWriteTime: {consoleWriteTime.Elapsed.TotalMilliseconds} ms");
            }
        }
        
        

        /// OLD WRITE LOG FUNCTION WITHOUT ASYNC
        /*
        private void WriteLog(string type, string message)
        {
            var totalTime = System.Diagnostics.Stopwatch.StartNew();
            var fileWriteTime = System.Diagnostics.Stopwatch.StartNew();
            string timestamp = DateTime.Now.ToString(TimestampFormat);
            string output = $"[{timestamp}] [{type}] {message}";

            
            lock (_fileLock)
            {
                
                File.AppendAllText(LogFileFullPath, $"{output}{Environment.NewLine}");
                fileWriteTime.Stop();
            }
            
            var consoleWriteTime = System.Diagnostics.Stopwatch.StartNew();
            if (PrintToConsole)
            {
                lock (_logLock)
                {
                    var color = _typeColors.GetValueOrDefault(type, ConsoleColor.Magenta);
                    SetConsoleColor(ConsoleColor.Gray);
                    Console.Write($"[{timestamp}] ");
                    
                    SetConsoleColor(color);
                    Console.Write($"[{type}] ");
                    
                    SetConsoleColor(ConsoleColor.Gray);
                    Console.WriteLine(message);
                    
                    Console.ResetColor();
                }
            }
            consoleWriteTime.Stop();
            totalTime.Stop();
            
            Console.WriteLine($"totalTime: {totalTime.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"fileWriteTime: {fileWriteTime.Elapsed.TotalMilliseconds} ms");
            Console.WriteLine($"consoleWriteTime: {consoleWriteTime.Elapsed.TotalMilliseconds} ms");
        }*/

        private static void SetConsoleColor(ConsoleColor color)
        {
            if (Console.ForegroundColor != color)
            {
                Console.ForegroundColor = color;
            }
        }

        public void Info(string msg) => Entry("INFO", msg);
        public void Warn(string msg) => Entry("WARNING", msg);
        public void Error(string msg) => Entry("ERROR", msg);
        public void Crit(string msg) => Entry("CRITICAL", msg);
        public void Debug(string msg) => Entry("DEBUG", msg);
        
    }
}
