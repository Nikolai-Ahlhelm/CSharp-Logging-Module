using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace CSLM.Cleanup
{
    public class LogFileCleanupManager
    {
        // CLSM instance for logging
        private CSLM _log;
        // Property to get or set the log module
        public CSLM Log
        {
            get => _log;
            set
            { 
                if (value == null) throw new ArgumentNullException(nameof(value), "Log cannot be null.");
                _log = value;
            }
        }

        // Directory where log files are stored
        private string _logDirectory;

        // Property to get or set the log directory
        public string LogDirectory
        {
            get => _logDirectory;
            set
            {
                _logDirectory = Path.GetFullPath(value);
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public LogFileCleanupManager(CSLM log, string logDirectory)
        {
            LogDirectory = logDirectory;
            Log = log;
            
        }

        /*   
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
                        Error($"[CSLM:LogCleanup] LogCleanup failed: {Path.GetFileName(file)} -> {ex.Message}");
                    }
                }
            }

            Info($"[CSLM:LogCleanup] LogCleanup finished. Deleted {deleted} old log files.");
        }*/

    }
}