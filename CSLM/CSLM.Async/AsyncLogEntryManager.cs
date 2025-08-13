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
    internal class AsyncLogEntryManager : AsyncManager
    {
        // constructor
        protected AsyncLogEntryManager(CSLM log) : base(log)
        {
            _log = log; 
            
            // launch process queue task
            Task.Run(ProcessQueue);
        }
    }
    
}    