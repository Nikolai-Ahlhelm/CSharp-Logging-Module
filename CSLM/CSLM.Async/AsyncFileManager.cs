using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CSLM.Async
{
    internal class AsyncFileManager : AsyncLogEntryManager
    {
        
        // constructor
        public AsyncFileManager(CSLM log) : base(log)
        {
            _log = log; 
            
            // launch process queue task
            Task.Run(ProcessQueue);
        }

        // Process Queue task
        private protected override void Process(object queueObject)
        {
            if (queueObject is LogEntry logEntry)
            {
                //Console.ForegroundColor = ConsoleColor.Magenta;
                //Console.WriteLine("[CLSM] [FileManager:Process] Appending log entry to file.");
                //Console.ResetColor();
                string output = $"[{logEntry.Timestamp.ToString(_log.TimestampFormat)}] [{logEntry.Type}] {logEntry.Message}";
                File.AppendAllText(_log.LogFileFullPath, $"{output}{Environment.NewLine}");
            }
            else
            {
                //Console.ForegroundColor = ConsoleColor.Magenta;
                //Console.WriteLine("[CLSM] [FileManager:Process] Invalid log entry type.");
                //Console.ResetColor();
            }
        }
    }    
}    