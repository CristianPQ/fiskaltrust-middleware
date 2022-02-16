﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using fiskaltrust.storage.V0;

namespace fiskaltrust.Middleware.Storage.SQLite.Repositories.DE
{
    public class SQLiteJournalDERepository : AbstractSQLiteRepository<Guid, ftJournalDE>, IJournalDERepository
    {
        public SQLiteJournalDERepository(ISqliteConnectionFactory connectionFactory, string path) : base(connectionFactory, path) { }

        public override void EntityUpdated(ftJournalDE entity) => entity.TimeStamp = DateTime.UtcNow.Ticks;

        public override async Task<ftJournalDE> GetAsync(Guid id) => await DbConnection.QueryFirstOrDefaultAsync<ftJournalDE>("Select * from ftJournalDE where ftJournalDEId = @JournalDEId", new { JournalDEId = id }).ConfigureAwait(false);

        public override async Task<IEnumerable<ftJournalDE>> GetAsync() => await DbConnection.QueryAsync<ftJournalDE>("select * from ftJournalDE").ConfigureAwait(false);

        public async Task InsertAsync(ftJournalDE journal)
        {
            if (await GetAsync(GetIdForEntity(journal)).ConfigureAwait(false) != null)
            {
                throw new Exception("Already exists");
            }

            EntityUpdated(journal);
            var sql = "INSERT INTO ftJournalDE " +
                      "(ftJournalDEId, Number, FileName, FileExtension, FileContentBase64, ftQueueItemId, ftQueueId, TimeStamp) " +
                      "Values (@ftJournalDEId, @Number, @FileName, @FileExtension, @FileContentBase64, @ftQueueItemId, @ftQueueId, @TimeStamp);";
            await DbConnection.ExecuteAsync(sql, journal).ConfigureAwait(false);
        }

        protected override Guid GetIdForEntity(ftJournalDE entity) => entity.ftJournalDEId;
    }
}