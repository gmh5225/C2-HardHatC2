﻿using System;

namespace TeamServer.Models.Extras
{
    public class HistoryEvent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Event { get; set; }
        public string Time { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        public string Status { get; set; } // used to set the color and symbol in the timeline valid options are "success", "info", "warning", "error"
    }
}
