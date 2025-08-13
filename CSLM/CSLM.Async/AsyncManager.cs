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
    internal class AsyncManager
    {
        // Log queue
        private protected readonly ConcurrentQueue<object> _queue = new();   
        
        // log event -> trigger dequeue
        private protected readonly AutoResetEvent _eventStartDequeue = new(false);
        
        // de-/activate check for event
        private protected bool _running = true;
        
        // log main
        private protected CSLM _log;
        
        // constructor
        protected AsyncManager(CSLM log)
        {
            _log = log; 
            
            // launch process queue task
            //Task.Run(ProcessQueue);
        }
        
        // async log entry method
        public virtual void AddToQueue(object queueObject)
        {
            _queue.Enqueue(queueObject); 
            _eventStartDequeue.Set();
        }
        
        // Process Queue task
        private protected async Task ProcessQueue()
        {
            while (_running)
            {
                _eventStartDequeue.WaitOne();

                while (_queue.TryDequeue(out object queueObject))
                {
                    // Execute 
                    Process(queueObject);
                }
            }
        }

        private protected virtual void Process(object queueObject)
        {
            // Code of the specific processing logic goes here.
        }
        

    }
    
}    