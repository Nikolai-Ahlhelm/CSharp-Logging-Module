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
    public class AsyncConsoleManager
    {
        // Log queue
        private readonly ConcurrentQueue<LogEntry> _logQueue = new();   
        
        // log event -> trigger dequeue
        private readonly AutoResetEvent _logEvent = new(false);
        
        // de-/activate check for event
        private bool _running = true;
        
        // log main
        private CSLM _log;
        
        // constructor
        public AsyncConsoleManager(CSLM log)
        {
            _log = log; 
            
            // launch process queue task
            Task.Run(ProcessQueue);
        }
        
        // async log entry method
        public void AsyncLogEntry(LogEntry logEntry)
        {
            _logQueue.Enqueue(logEntry); 
            _logEvent.Set();
        }
        
        // Process Queue task
        private async Task ProcessQueue()
        {
            while (_running)
            {
                _logEvent.WaitOne();

                while (_logQueue.TryDequeue(out var logEntry))
                {
                    SetConsoleColor(ConsoleColor.Magenta);
                    Console.WriteLine($"[CLSM] [ConsoleManager:ProcessQueue] Queue count: {_logQueue.Count}");  
                    
                    SetConsoleColor(ConsoleColor.Gray);
                    Console.Write($"[{logEntry.Timestamp.ToString(_log.TimestampFormat)}] ");
                    
                    SetConsoleColor(logEntry.Color);
                    Console.Write($"[{logEntry.Type}] ");
                    
                    SetConsoleColor(ConsoleColor.Gray);
                    Console.WriteLine(logEntry.Message);
                    
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


