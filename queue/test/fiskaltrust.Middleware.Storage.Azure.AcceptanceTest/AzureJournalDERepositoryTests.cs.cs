﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Storage.AcceptanceTest;
using fiskaltrust.Middleware.Storage.Azure.Repositories.DE;
using fiskaltrust.storage.V0;

namespace fiskaltrust.Middleware.Storage.Azure.AcceptanceTest
{
    public class AzureJournalDERepositoryTests : AbstractJournalDERepositoryTests
    {
        public override async Task<IReadOnlyJournalDERepository> CreateReadOnlyRepository(IEnumerable<ftJournalDE> entries) => await CreateRepository(entries);

        public override async Task<IJournalDERepository> CreateRepository(IEnumerable<ftJournalDE> entries)
        {
            var azureJournalDERepository = new AzureJournalDERepository(Guid.NewGuid(), Constants.AzureStorageConnectionString);
            foreach (var entry in entries)
            {
                await azureJournalDERepository.InsertAsync(entry);
            }

            return azureJournalDERepository;
        }
    }
}
