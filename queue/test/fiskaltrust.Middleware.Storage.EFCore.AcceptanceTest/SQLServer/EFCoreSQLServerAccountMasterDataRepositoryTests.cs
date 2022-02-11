﻿// using System;
// using System.Collections.Generic;
// using fiskaltrust.Middleware.Abstractions;
// using fiskaltrust.Middleware.Contracts.Repositories;
// using fiskaltrust.Middleware.Storage.AcceptanceTest;
// using fiskaltrust.Middleware.Storage.EFCore.AcceptanceTest.SQLServer.Fixtures;
// using fiskaltrust.Middleware.Storage.EFCore.Repositories.DE.MasterData;
// using fiskaltrust.Middleware.Storage.EFCore.SQLServer;
// using fiskaltrust.storage.V0.MasterData;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Xunit;

// namespace fiskaltrust.Middleware.Storage.EFCore.AcceptanceTest.SQLServer
// {
//     [Collection(EFCoreSqlServerStorageCollectionFixture.CollectionName)]
//     public class EFCoreSQLServerAccountMasterDataRepositoryTests : AbstractAccountMasterDataRepositoryTests
//     {
//         public override IMasterDataRepository<AccountMasterData> CreateReadOnlyRepository(IEnumerable<AccountMasterData> entries) => CreateRepository(entries);

//         public override IMasterDataRepository<AccountMasterData> CreateRepository(IEnumerable<AccountMasterData> entries)
//         {
//             var optionsBuilder = new DbContextOptionsBuilder<SQLServerMiddlewareDbContext>();
//             optionsBuilder.UseSqlServer(EFCoreSqlServerConnectionStringFixture.DatabaseConnectionString);

//             var queueId = Guid.NewGuid();
//             var repository = new EFCoreAccountMasterDataRepository(new SQLServerMiddlewareDbContext(optionsBuilder.Options, queueId));
//             EFCoreSQLServerStorageBootstrapper.Update(optionsBuilder.Options, queueId, Mock.Of<ILogger<IMiddlewareBootstrapper>>());

//             foreach (var item in entries)
//             {
//                 repository.InsertAsync(item).Wait();
//             }

//             return repository;
//         }
//     }
// }
