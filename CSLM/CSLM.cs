using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CSLM
{
    public class CSLM
    {
        // Variables
        private string _logFileName;
        public string LogFileName 
        { 
            get => _logFileName;
            set => _logFileName = value; 
        }

        public string _logFilePath { get; set; }
        public string LogFilePath
        {
            get => _logFilePath;
            set
            {
                _logFilePath = Path.GetFullPath(value);
                Directory.CreateDirectory(_logFilePath);
            }
        }

        private string _logType;
        public string LogType
        {
            get => _logType;
            set
            {
                _logType = NormalizeLogType(value);
                RefreshAllowedTypes();
            }
        }

        private bool _printToConsole;
        public bool PrintToConsole
        {
            get => _printToConsole;
            set => _printToConsole = value;
        }

        private string _timestampFormat;
        public string TimestampFormat
        {
            get => _timestampFormat;
            set => _timestampFormat = value;
        }

        private string _logFileFullPath => Path.Combine(_logFilePath, _logFileName);
        public string LogFileFullPath => _logFileFullPath;

        private readonly Dictionary<string, ConsoleColor> _typeColors = new()
        {
            { "INFO", ConsoleColor.Cyan },
            { "ERROR", ConsoleColor.Red },
            { "WARNING", ConsoleColor.Yellow },
            { "CRITICAL", ConsoleColor.DarkRed },
            { "DEBUG", ConsoleColor.Green },
            { "DEFAULT", ConsoleColor.Gray }
        };

        private readonly Dictionary<string, List<string>> _logLevels = new()
        {
            { "DEFAULT", new() { "ERROR", "INFO", "WARNING", "CRITICAL" } },
            { "DEBUG", new() { "ERROR", "INFO", "WARNING", "CRITICAL", "DEBUG" } },
            { "PRODUCTIVE", new() { "ERROR", "INFO", "CRITICAL" } },
            { "ERROR", new() { "ERROR" } },
            { "CRITICAL", new() { "CRITICAL" } },
            { "NONE", new() }
        };

        private List<string> _allowedTypes;


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
            if (_logLevels.TryGetValue(_logType, out var allowedTypes))
            {
                _allowedTypes = allowedTypes;
            }
            else
            {
                Console.WriteLine("[CSLM] [RefreshAllowedTypes] Invalid logType, fallback to DEFAULT");
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

            // Check if the entry type is allowed or custom, then log it
            if (_allowedTypes.Contains(entryType))
            {
                WriteLog(entryType, message);
            }
            else if (_logLevels["DEBUG"].Contains(entryType)) // Check if entry type is not a CSLM default => is custom type => always log
            //INFO: Debug contains all CLSM default types, therefore it is used to check for custom types
            {
                //Custom types are always logged       
                WriteLog(entryType, message);
            }
            else {}
        }

        private void WriteLog(string type, string message)
        {
            string timestamp = DateTime.Now.ToString(_timestampFormat);
            string output = $"[{timestamp}] [{type}] {message}";

            File.AppendAllText(_logFileFullPath, output + Environment.NewLine);

            if (_printToConsole)
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

        public void Info(string msg) => Entry("INFO", msg);
        public void Warn(string msg) => Entry("WARNING", msg);
        public void Error(string msg) => Entry("ERROR", msg);
        public void Crit(string msg) => Entry("CRITICAL", msg);
        public void Debug(string msg) => Entry("DEBUG", msg);

        public void LogCleanup(int retentionDays)
        {
            if (retentionDays <= 0) return;

            var files = Directory.GetFiles(_logFilePath);
            var threshold = DateTime.Now.AddDays(-retentionDays);

            int deleted = 0;
            foreach (var file in files)
            {
                if (File.GetLastWriteTime(file) < threshold)
                {
                    try
                    {
                        File.Delete(file);
                        deleted++;
                    }
                    catch (Exception ex)
                    {
                        Error($"LogCleanup failed: {Path.GetFileName(file)} -> {ex.Message}");
                    }
                }
            }

            Info($"LogCleanup finished. Deleted {deleted} old log files.");
        }
    }
}
