﻿using System;
using Microsoft.Azure.Cosmos.Table;

namespace fiskaltrust.Middleware.Storage.Azure.TableEntities
{
    public class AzureAccountMasterData : TableEntity
    {
        public Guid AccountId { get; set; }
        public string AccountName { get; set; }
        public string Street { get; set; }
        public string Zip { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string TaxId { get; set; }
        public string VatId { get; set; }
    }
}