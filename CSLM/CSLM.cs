using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CSLM
{
    public class CSLM
    {
        //// Variables

        // Log file name, can contain tokens like %dd%, %MM%, %yyyy%, %hh%, %m%, %ss%
        // Example: "log_%dd%-%MM%-%yyyy%_%hh%-%m%-%ss%.txt"
        private string _logFileName;
        // Property to get or set the log file name
        public string LogFileName
        {
            get => _logFileName;
            set => _logFileName = value;
        }

        // Relative log file path
        private string _logFilePath { get; set; }
        // Property to get or set the log file path
        public string LogFilePath
        {
            get => _logFilePath;
            set
            {
                _logFilePath = Path.GetFullPath(value);
                Directory.CreateDirectory(_logFilePath);
            }
        }

        // Log type, can be one of the predefined types or a custom type

        private string _logType;
        // Property to get or set the log type
        public string LogType
        {
            get => _logType;
            set
            {
                _logType = NormalizeLogType(value);
                RefreshAllowedTypes();
            }
        }

        // Bool to enable or disable printing log entries to the console
        private bool _printToConsole;

        // Property to get or set whether to print log entries to the console
        public bool PrintToConsole
        {
            get => _printToConsole;
            set => _printToConsole = value;
        }

        // Timestamp format for log entries
        private string _timestampFormat;

        // Property to get or set the timestamp format
        public string TimestampFormat
        {
            get => _timestampFormat;
            set => _timestampFormat = value;
        }

        // Full path of the log file
        private string _logFileFullPath => Path.Combine(_logFilePath, _logFileName);

        // Property to get the full path of the log file
        public string LogFileFullPath => _logFileFullPath;

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
        private readonly object _fileLock = new();

        // Lock object for logging operations
        // This is used to ensure that console output and file writing are thread-safe
        private readonly object _logLock = new();

        // Lock object for allowed types
        // This is used to ensure that the allowed types list is thread-safe when being modified
        private readonly object _allowedTypesLock = new();

        // Constructor

        public CSLM(string logFileName, string logFilePath, string logType = "DEFAULT", bool printToConsole = true, string timestampFormat = "dd-MM-yyyy HH:mm:ss.fff")
        {
            // Replace file name token
            _logFileName = ReplaceTokens(logFileName);

            // Ensure log file path is absolute and create directory if it doesn't exist
            _logFilePath = Path.GetFullPath(logFilePath);
            Directory.CreateDirectory(_logFilePath);

            // Normalize log type
            _logType = NormalizeLogType(logType);

            // Fill _allowedTypes with the initial log type
            RefreshAllowedTypes();

            _printToConsole = printToConsole;
            _timestampFormat = timestampFormat;

        }

        private void RefreshAllowedTypes()
        {
            lock (_allowedTypesLock)
            { 
                if (_logLevels.TryGetValue(_logType, out var allowedTypes))
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
            DateTime now = DateTime.Now;
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

        private string NormalizeEntryType(string type)
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

        public void Entry(string type, string message)
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
                else if (!_logLevels["DEBUG"].Contains(entryType)) // Check if entry type is not a CSLM default => is custom type => always log
                //INFO: Debug contains all CLSM default types, therefore it is used to check for custom types
                {
                    //Custom types are always logged       
                    WriteLog(entryType, message);
                }
                else { }
            }
        }

        private void WriteLog(string type, string message)
        {
            string timestamp = DateTime.Now.ToString(_timestampFormat);
            string output = $"[{timestamp}] [{type}] {message}";

            lock (_fileLock)
            {
                File.AppendAllText(_logFileFullPath, output + Environment.NewLine);
            }
            

            if (_printToConsole)
            {
                lock (_logLock)
                {
                    var color = _typeColors.ContainsKey(type) ? _typeColors[type] : ConsoleColor.Magenta;
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"[{timestamp}] ");
                    Console.ForegroundColor = color;
                    Console.Write($"[{type}] ");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
            }
        }

        public void Info(string msg) => Entry("INFO", msg);
        public void Warn(string msg) => Entry("WARNING", msg);
        public void Error(string msg) => Entry("ERROR", msg);
        public void Crit(string msg) => Entry("CRITICAL", msg);
        public void Debug(string msg) => Entry("DEBUG", msg);


    }
}
