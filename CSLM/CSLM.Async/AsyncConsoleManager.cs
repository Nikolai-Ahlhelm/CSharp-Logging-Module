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
    internal class AsyncConsoleManager : AsyncFileManager
    {
        
        private static readonly object _consoleLock = new();
        
        // constructor
        public AsyncConsoleManager(CSLM log) : base(log)
        {
            _log = log; 
            
            // launch process queue task
            Task.Run(ProcessQueue);
        }
        
        private protected override void Process(object queueObject)
        {
            lock (_consoleLock)
            {
                //SetConsoleColor(ConsoleColor.Magenta);
                //Console.WriteLine("[CLSM] [ConsoleManager:Process] Process executed.");
                //Console.ResetColor();

                if (queueObject is LogEntry logEntry)
                {
                    //SetConsoleColor(ConsoleColor.Magenta);
                    //Console.WriteLine($"[CLSM] [ConsoleManager:Process] Processing log entry: {logEntry.Message}");

                    SetConsoleColor(ConsoleColor.White);
                    Console.Write($"[{logEntry.Timestamp.ToString(_log.TimestampFormat)}] ");

                    SetConsoleColor(logEntry.Color);
                    Console.Write($"[{logEntry.Type}] ");

                    SetConsoleColor(ConsoleColor.White);
                    Console.WriteLine(logEntry.Message);

                    Console.ResetColor();
                }
                else
                {
                    SetConsoleColor(ConsoleColor.Magenta);
                    Console.WriteLine("[CLSM] [ConsoleManager:Process] Invalid object type.");
                    Console.ResetColor();
                }
            }
        }
            
        
        private static void SetConsoleColor(ConsoleColor color)
        {
            if (Console.ForegroundColor != color)
            {
                Console.ForegroundColor = color;
            }
        }
    }    
}


