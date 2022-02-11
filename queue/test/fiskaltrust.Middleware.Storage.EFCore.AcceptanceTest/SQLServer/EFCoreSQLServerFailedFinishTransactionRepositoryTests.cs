﻿// using System;
// using System.Collections.Generic;
// using fiskaltrust.Middleware.Abstractions;
// using fiskaltrust.Middleware.Contracts.Data;
// using fiskaltrust.Middleware.Contracts.Models.Transactions;
// using fiskaltrust.Middleware.Storage.AcceptanceTest;
// using fiskaltrust.Middleware.Storage.EFCore.AcceptanceTest.SQLServer.Fixtures;
// using fiskaltrust.Middleware.Storage.EFCore.Repositories.DE;
// using fiskaltrust.Middleware.Storage.EFCore.SQLServer;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Xunit;

// namespace fiskaltrust.Middleware.Storage.EFCore.AcceptanceTest.SQLServer
// {
//     [Collection(EFCoreSqlServerStorageCollectionFixture.CollectionName)]
//     public class EFCoreSQLServerFailedFinishTransactionRepositoryTests : AbstractFailedFinishTransactionRepositoryTests
//     {
//         public override IPersistentTransactionRepository<FailedFinishTransaction> CreateReadOnlyRepository(IEnumerable<FailedFinishTransaction> entries) => CreateRepository(entries);

//         public override IPersistentTransactionRepository<FailedFinishTransaction> CreateRepository(IEnumerable<FailedFinishTransaction> entries)
//         {
//             var optionsBuilder = new DbContextOptionsBuilder<SQLServerMiddlewareDbContext>();
//             optionsBuilder.UseSqlServer(EFCoreSqlServerConnectionStringFixture.DatabaseConnectionString);

//             var queueId = Guid.NewGuid();
//             var repository = new EFCoreFailedFinishTransactionRepository(new SQLServerMiddlewareDbContext(optionsBuilder.Options, queueId));
//             EFCoreSQLServerStorageBootstrapper.Update(optionsBuilder.Options, queueId, Mock.Of<ILogger<IMiddlewareBootstrapper>>());

//             foreach (var item in entries)
//             {
//                 repository.InsertAsync(item).Wait();
//             }

//             return repository;
//         }
//     }
// }
