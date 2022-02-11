﻿// using System;
// using System.Collections.Generic;
// using fiskaltrust.Middleware.Abstractions;
// using fiskaltrust.Middleware.Contracts.Repositories;
// using fiskaltrust.Middleware.Storage.AcceptanceTest;
// using fiskaltrust.Middleware.Storage.EFCore.AcceptanceTest.SQLServer.Fixtures;
// using fiskaltrust.Middleware.Storage.EFCore.Repositories;
// using fiskaltrust.Middleware.Storage.EFCore.SQLServer;
// using fiskaltrust.storage.V0;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Logging;
// using Moq;
// using Xunit;

// namespace fiskaltrust.Middleware.Storage.EFCore.AcceptanceTest.SQLServer
// {
//     [Collection(EFCoreSqlServerStorageCollectionFixture.CollectionName)]
//     public class EFCoreSQLServerQueueItemRespositoryTests : AbstractQueueItemRepositoryTests
//     {
//         public override IReadOnlyQueueItemRepository CreateReadOnlyRepository(IEnumerable<ftQueueItem> entries) => CreateRepository(entries);

//         public override IMiddlewareQueueItemRepository CreateRepository(IEnumerable<ftQueueItem> entries)
//         {
//             var optionsBuilder = new DbContextOptionsBuilder<SQLServerMiddlewareDbContext>();
//             optionsBuilder.UseSqlServer(EFCoreSqlServerConnectionStringFixture.DatabaseConnectionString);

//             var queueId = Guid.NewGuid();
//             var repository = new EFCoreQueueItemRepository(new SQLServerMiddlewareDbContext(optionsBuilder.Options, queueId));
//             EFCoreSQLServerStorageBootstrapper.Update(optionsBuilder.Options, queueId, Mock.Of<ILogger<IMiddlewareBootstrapper>>());

//             foreach (var item in entries)
//             {
//                 repository.InsertAsync(item).Wait();
//             }

//             return repository;
//         }
//     }
// }
