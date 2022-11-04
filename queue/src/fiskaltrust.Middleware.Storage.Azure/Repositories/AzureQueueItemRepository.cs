﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.Middleware.Contracts.Repositories;
using fiskaltrust.Middleware.Storage.Azure.Mapping;
using fiskaltrust.Middleware.Storage.Azure.TableEntities;
using fiskaltrust.Middleware.Storage.Base.Extensions;
using fiskaltrust.storage.V0;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace fiskaltrust.Middleware.Storage.Azure.Repositories
{
    public class AzureQueueItemRepository : BaseAzureTableRepository<Guid, AzureFtQueueItem, ftQueueItem>, IMiddlewareQueueItemRepository, IMiddlewareRepository<ftQueueItem>
    {
        public AzureQueueItemRepository(Guid queueId, string connectionString)
            : base(queueId, connectionString, nameof(ftQueueItem)) { }

        protected override void EntityUpdated(ftQueueItem entity) => entity.TimeStamp = DateTime.UtcNow.Ticks;

        protected override Guid GetIdForEntity(ftQueueItem entity) => entity.ftQueueItemId;

        protected override ftQueueItem MapToStorageEntity(AzureFtQueueItem entity) => Mapper.Map(entity);

        protected override AzureFtQueueItem MapToAzureEntity(ftQueueItem entity) => Mapper.Map(entity);

        public IAsyncEnumerable<ftQueueItem> GetEntriesOnOrAfterTimeStampAsync(long fromInclusive, int? take = null)
        {
            var result = GetEntriesOnOrAfterTimeStampAsync(fromInclusive).ToListAsync().Result.OrderBy(x => x.TimeStamp);
            if (take.HasValue)
            {
                return result.Take(take.Value).ToAsyncEnumerable();
            }
            return result.ToAsyncEnumerable();
        }

        public async IAsyncEnumerable<ftQueueItem> GetByReceiptReferenceAsync(string cbReceiptReference, string cbTerminalId)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ftQueueItem.cbReceiptReference), QueryComparisons.Equal, cbReceiptReference);
            if (!string.IsNullOrWhiteSpace(cbTerminalId))
            {
                filter = TableQuery.CombineFilters(filter, TableOperators.And, TableQuery.GenerateFilterCondition(nameof(ftQueueItem.cbTerminalID), QueryComparisons.Equal, cbTerminalId));
            }
            var result = await GetAllAsync(filter).ToListAsync().ConfigureAwait(false);
            foreach (var item in result)
            {
                yield return MapToStorageEntity(item);
            }
        }

        public async IAsyncEnumerable<ftQueueItem> GetPreviousReceiptReferencesAsync(ftQueueItem ftQueueItem)
        {
            var receiptRequest = JsonConvert.DeserializeObject<ReceiptRequest>(ftQueueItem.request);
            if (!receiptRequest.IncludeInReferences() || (string.IsNullOrWhiteSpace(receiptRequest.cbPreviousReceiptReference) && string.IsNullOrWhiteSpace(ftQueueItem.cbReceiptReference)))
            {
                yield break;
            }

            var refFilter = TableQuery.CombineFilters(TableQuery.GenerateFilterCondition(nameof(ftQueueItem.cbReceiptReference), QueryComparisons.Equal, receiptRequest.cbPreviousReceiptReference),
                TableOperators.Or,
                TableQuery.GenerateFilterCondition(nameof(ftQueueItem.cbReceiptReference), QueryComparisons.Equal, ftQueueItem.cbReceiptReference));

            var filter = TableQuery.GenerateFilterCondition(nameof(ftQueueItem.ftQueueRow), QueryComparisons.LessThan, ftQueueItem.ftQueueRow.ToString());
            filter = TableQuery.CombineFilters(filter, TableOperators.And, refFilter);
            var result = await GetAllAsync(filter).ToListAsync();
            foreach (var item in result)
            {
                yield return MapToStorageEntity(item);
            }
        }

        public async IAsyncEnumerable<ftQueueItem> GetQueueItemsAfterQueueItem(ftQueueItem ftQueueItem)
        {
            var filter = TableQuery.GenerateFilterCondition(nameof(ftQueueItem.ftQueueRow), QueryComparisons.GreaterThanOrEqual, ftQueueItem.ftQueueRow.ToString());
            var result = await GetAllAsync(filter).ToListAsync();
            foreach (var item in result)
            {
                yield return MapToStorageEntity(item);
            }
        }

        public async IAsyncEnumerable<string> GetGroupedReceiptReference(long? fromIncl, long? toIncl)
        {
            var groupByLastNamesQuery =
                    from queueItem in await GetAllAsync().ToListAsync()
                    where
                    fromIncl.HasValue ? queueItem.TimeStamp >= fromIncl.Value : true &&
                    toIncl.HasValue ? queueItem.TimeStamp <= toIncl.Value : true &&
                    JsonConvert.DeserializeObject<ReceiptRequest>(queueItem.request).IncludeInReferences()
                    group queueItem by queueItem.cbReceiptReference into newGroup
                    orderby newGroup.Key
                    select newGroup;
            await foreach (var entry in groupByLastNamesQuery.ToAsyncEnumerable())
            {
                yield return entry.Key;
            }
        }
        public async IAsyncEnumerable<ftQueueItem> GetQueueItemsForReceiptReference(string receiptReference)
        {
            var queueItemsForReceiptReference =
                from queueItem in await GetAllAsync().ToListAsync()
                where JsonConvert.DeserializeObject<ReceiptRequest>(queueItem.request).IncludeInReferences() && queueItem.cbReceiptReference == receiptReference
                orderby queueItem.TimeStamp
                select queueItem;
            await foreach (var entry in queueItemsForReceiptReference.ToAsyncEnumerable())
            {
                yield return MapToStorageEntity(entry);
            }
        }
        public async Task<ftQueueItem> GetFirstPreviousReceiptReferencesAsync(ftQueueItem ftQueueItem)
        {
            var receiptRequest = JsonConvert.DeserializeObject<ReceiptRequest>(ftQueueItem.request);
            var queueItemsForReceiptReference =
                            (from queueItem in await GetAllAsync().ToListAsync()
                             where receiptRequest.IncludeInReferences() && queueItem.cbReceiptReference == receiptRequest.cbPreviousReceiptReference
                             orderby queueItem.TimeStamp
                             select queueItem).ToAsyncEnumerable().Take(1);
            return (ftQueueItem) queueItemsForReceiptReference;
        }
    }
}
