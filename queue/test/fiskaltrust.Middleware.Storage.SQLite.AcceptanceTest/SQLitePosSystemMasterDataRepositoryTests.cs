﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Abstractions;
using fiskaltrust.Middleware.Contracts.Repositories;
using fiskaltrust.Middleware.Storage.AcceptanceTest;
using fiskaltrust.Middleware.Storage.SQLite.AcceptanceTest.Helpers;
using fiskaltrust.Middleware.Storage.SQLite.Connection;
using fiskaltrust.Middleware.Storage.SQLite.DatabaseInitialization;
using fiskaltrust.Middleware.Storage.SQLite.Repositories.DE.MasterData;
using fiskaltrust.storage.V0.MasterData;
using Microsoft.Extensions.Logging;
using Moq;

namespace fiskaltrust.Middleware.Storage.SQLite.AcceptanceTest
{
    public class SQLitePosSystemMasterDataRepositoryTests : AbstractPosSystemMasterDataRepositoryTests
    {
        private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Guid.NewGuid().ToString());
        private SQLitePosSystemMasterDataRepository _repo;
        private readonly SqliteConnectionFactory _sqliteConnectionFactory = new SqliteConnectionFactory();

        public override Task<IMasterDataRepository<PosSystemMasterData>> CreateReadOnlyRepository(IEnumerable<PosSystemMasterData> entries) => CreateRepository(entries);

        public override async Task<IMasterDataRepository<PosSystemMasterData>> CreateRepository(IEnumerable<PosSystemMasterData> entries)
        {
            var databasMigrator = new DatabaseMigrator(_sqliteConnectionFactory, _path, new Dictionary<string, object>(), Mock.Of<ILogger<IMiddlewareBootstrapper>>());
            await databasMigrator.MigrateAsync();

            _repo = new SQLitePosSystemMasterDataRepository(_sqliteConnectionFactory, _path);
            foreach (var entry in entries)
            { 
                await _repo.CreateAsync(entry);
            }
            return _repo;
        }

        public override void DisposeDatabase()
        {
            _sqliteConnectionFactory.Dispose();
            if (File.Exists(_path))
            {
                FileHelpers.WaitUntilFileIsAccessible(_path);
                File.Delete(_path);
            }
        }
    }
}
