﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace fiskaltrust.Middleware.SCU.DE.FiskalyCertified.Models
{
    public class TransactionRequestDto
    {
        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("client_id")]
        public Guid ClientId { get; set; }

        [JsonProperty("schema")]
        public TransactionDataDto Data { get; set; }

        [JsonProperty("metadata")]
        public Dictionary<string, object> metadata { get; set; }
    }
}
