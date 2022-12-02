﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables;
using fiskaltrust.Middleware.Contracts.Repositories;
using fiskaltrust.Middleware.Storage.AcceptanceTest;
using fiskaltrust.Middleware.Storage.Azure.Repositories.MasterData;
using fiskaltrust.storage.V0.MasterData;

namespace fiskaltrust.Middleware.Storage.Azure.AcceptanceTest
{
    public class AzureAccountMasterDataRepositoryTests : AbstractAccountMasterDataRepositoryTests
    {
        public override Task<IMasterDataRepository<AccountMasterData>> CreateReadOnlyRepository(IEnumerable<AccountMasterData> entries) => CreateRepository(entries);

        public override async Task<IMasterDataRepository<AccountMasterData>> CreateRepository(IEnumerable<AccountMasterData> entries)
        {
            var repository = new AzureAccountMasterDataRepository(new QueueConfiguration { QueueId = Guid.NewGuid() }, new TableServiceClient(Constants.AzureStorageConnectionString));
            foreach (var entry in entries)
            {
                await repository.InsertAsync(entry).ConfigureAwait(false);
            }

            return repository;
        }
    }
}
