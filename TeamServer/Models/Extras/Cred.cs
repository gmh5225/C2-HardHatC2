﻿using System;

namespace TeamServer.Models.Extras
{
    public class Cred
    {
        public string Name { get; set; }
        public string Domain { get; set; }
        public string CredentialValue { get; set; }
        public CredType Type { get; set; }
        public string SubType { get; set; }
        public DateTime CaptureTime { get; set; } = DateTime.UtcNow;
        public enum CredType
        {
            hash, password, ticket
        }
    }
}
