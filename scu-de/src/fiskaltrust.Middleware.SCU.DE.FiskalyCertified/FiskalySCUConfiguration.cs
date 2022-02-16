﻿using System;

namespace fiskaltrust.Middleware.SCU.DE.FiskalyCertified
{
    public class FiskalySCUConfiguration
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public Guid TssId { get; set; }
        public string AdminPin { get; set; }
        public bool EnableTarFileExport { get; set; } = true;
        public virtual string CertificationId { get; set; } = "BSI-K-TR-0403-2021";
        public bool DisplayCertificationIdAddition { get; set; } = false;
        public string CertificationIdAddition { get; set; }
        public string ApiEndpoint { get; set; } = "https://kassensichv-middleware.fiskaly.com/api/v2";
        public int FiskalyClientTimeout { get; set; } = 30000;
        public string ProxyServer { get; set; }
        public int? ProxyPort { get; set; }
        public string ProxyUsername { get; set; }
        public string ProxyPassword { get; set; }
        public int? MaxClientCount { get; set; }
    }
}