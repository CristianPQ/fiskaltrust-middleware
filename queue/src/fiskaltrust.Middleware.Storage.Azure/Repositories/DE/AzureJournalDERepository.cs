﻿using System;
using System.Collections.Generic;
using System.Linq;
using fiskaltrust.Middleware.Contracts.Repositories;
using fiskaltrust.Middleware.Storage.Azure.Mapping;
using fiskaltrust.Middleware.Storage.Azure.TableEntities;
using fiskaltrust.storage.V0;

namespace fiskaltrust.Middleware.Storage.Azure.Repositories.DE
{
    public class AzureJournalDERepository : BaseAzureTableRepository<Guid, AzureFtJournalDE, ftJournalDE>, IJournalDERepository, IMiddlewareRepository<ftJournalDE>
    {
        public AzureJournalDERepository(Guid queueId, string connectionString)
            : base(queueId, connectionString, nameof(ftJournalDE)) { }

        protected override void EntityUpdated(ftJournalDE entity) => entity.TimeStamp = DateTime.UtcNow.Ticks;

        protected override Guid GetIdForEntity(ftJournalDE entity) => entity.ftJournalDEId;

        protected override AzureFtJournalDE MapToAzureEntity(ftJournalDE entity) => Mapper.Map(entity);

        protected override ftJournalDE MapToStorageEntity(AzureFtJournalDE entity) => Mapper.Map(entity);

        public IAsyncEnumerable<ftJournalDE> GetEntriesOnOrAfterTimeStampAsync(long fromInclusive, int? take = null)
        {
            var result = GetEntriesOnOrAfterTimeStampAsync(fromInclusive).ToListAsync().Result.OrderBy(x => x.TimeStamp);
            if (take.HasValue)
            {
                return result.Take(take.Value).ToAsyncEnumerable();
            }
            return result.ToAsyncEnumerable();
        }
    }
}
