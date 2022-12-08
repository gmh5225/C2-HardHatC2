﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace TeamServer.Models
{

    [Serializable]
    public class EngineerTaskResult
    { 
        public string Id { get; set; } //Task ID
      
        public string Result { get; set; }
        
        public bool IsHidden { get; set; }

        public string EngineerId { get; set; }
      
        public EngTaskStatus Status { get; set; }
    }
    public enum EngTaskStatus
    {
        Running = 2,
        Complete = 3,
        CompleteWithWarnings = 4,
        CompleteWithErrors = 5,
        Failed = 6
    }
}
