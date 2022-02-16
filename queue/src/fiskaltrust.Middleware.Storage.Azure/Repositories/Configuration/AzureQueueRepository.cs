﻿using System;
using fiskaltrust.Middleware.Storage.Azure.Mapping;
using fiskaltrust.Middleware.Storage.Azure.TableEntities.Configuration;
using fiskaltrust.storage.V0;

namespace fiskaltrust.Middleware.Storage.Azure.Repositories.Configuration
{
    public class AzureQueueRepository : BaseAzureTableRepository<Guid, AzureFtQueue, ftQueue>
    {
        public AzureQueueRepository(Guid queueId, string connectionString)
            : base(queueId, connectionString, nameof(ftQueue)) { }

        protected override void EntityUpdated(ftQueue entity) => entity.TimeStamp = DateTime.UtcNow.Ticks;

        protected override Guid GetIdForEntity(ftQueue entity) => entity.ftQueueId;

        protected override AzureFtQueue MapToAzureEntity(ftQueue entity) => Mapper.Map(entity);

        protected override ftQueue MapToStorageEntity(AzureFtQueue entity) => Mapper.Map(entity);
    }
}