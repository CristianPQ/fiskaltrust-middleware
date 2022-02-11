﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Storage.Azure.TableEntities.Configuration;
using fiskaltrust.Middleware.Storage.Azure.Repositories.Configuration;
using fiskaltrust.storage.V0;

namespace fiskaltrust.Middleware.Storage.Azure.Repositories
{
    public class AzureConfigurationRepository : IConfigurationRepository
    {
        private readonly BaseAzureTableRepository<Guid, AzureFtCashBox, ftCashBox> _cashBoxRepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtQueue, ftQueue> _queueRepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtQueueAT, ftQueueAT> _queueATRepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtQueueDE, ftQueueDE> _queueDERepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtQueueFR, ftQueueFR> _queueFRRepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtSignaturCreationUnitAT, ftSignaturCreationUnitAT> _signaturCreationUnitATRepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtSignaturCreationUnitDE, ftSignaturCreationUnitDE> _signaturCreationUnitDERepository;
        private readonly BaseAzureTableRepository<Guid, AzureFtSignaturCreationUnitFR, ftSignaturCreationUnitFR> _signaturCreationUnitFRRepository;

        public AzureConfigurationRepository() { }

        public AzureConfigurationRepository(Guid queueId, string connectionString)
        {
            _cashBoxRepository = new AzureCashBoxRepository(queueId, connectionString);
            _queueRepository = new AzureQueueRepository(queueId, connectionString);
            _queueATRepository = new AzureQueueATRepository(queueId, connectionString);
            _queueDERepository = new AzureQueueDERepository(queueId, connectionString);
            _queueFRRepository = new AzureQueueFRRepository(queueId, connectionString);
            _signaturCreationUnitATRepository = new AzureSignaturCreationUnitATRepository(queueId, connectionString);
            _signaturCreationUnitDERepository = new AzureSignaturCreationUnitDERepository(queueId, connectionString);
            _signaturCreationUnitFRRepository = new AzureSignaturCreationUnitFRRepository(queueId, connectionString);
        }

        public async Task<ftCashBox> GetCashBoxAsync(Guid cashBoxId) => await _cashBoxRepository.GetAsync(cashBoxId).ConfigureAwait(false);
        public async Task<IEnumerable<ftCashBox>> GetCashBoxListAsync() => await _cashBoxRepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateCashBoxAsync(ftCashBox cashBox) => await _cashBoxRepository.InsertOrUpdateAsync(cashBox).ConfigureAwait(false);

        public async Task<ftQueue> GetQueueAsync(Guid id) => await _queueRepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftQueue>> GetQueueListAsync() => await _queueRepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateQueueAsync(ftQueue queue) => await _queueRepository.InsertOrUpdateAsync(queue).ConfigureAwait(false);

        public async Task<ftQueueAT> GetQueueATAsync(Guid id) => await _queueATRepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftQueueAT>> GetQueueATListAsync() => await _queueATRepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateQueueATAsync(ftQueueAT queueAT) => await _queueATRepository.InsertOrUpdateAsync(queueAT).ConfigureAwait(false);

        public async Task<ftQueueDE> GetQueueDEAsync(Guid id) => await _queueDERepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftQueueDE>> GetQueueDEListAsync() => await _queueDERepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateQueueDEAsync(ftQueueDE queueDE) => await _queueDERepository.InsertOrUpdateAsync(queueDE).ConfigureAwait(false);

        public async Task<ftQueueFR> GetQueueFRAsync(Guid id) => await _queueFRRepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftQueueFR>> GetQueueFRListAsync() => await _queueFRRepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateQueueFRAsync(ftQueueFR queueFR) => await _queueFRRepository.InsertOrUpdateAsync(queueFR).ConfigureAwait(false);

        public async Task<ftSignaturCreationUnitAT> GetSignaturCreationUnitATAsync(Guid id) => await _signaturCreationUnitATRepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftSignaturCreationUnitAT>> GetSignaturCreationUnitATListAsync() => await _signaturCreationUnitATRepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateSignaturCreationUnitATAsync(ftSignaturCreationUnitAT scu) => await _signaturCreationUnitATRepository.InsertOrUpdateAsync(scu).ConfigureAwait(false);

        public async Task<ftSignaturCreationUnitDE> GetSignaturCreationUnitDEAsync(Guid id) => await _signaturCreationUnitDERepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftSignaturCreationUnitDE>> GetSignaturCreationUnitDEListAsync() => await _signaturCreationUnitDERepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateSignaturCreationUnitDEAsync(ftSignaturCreationUnitDE scu) => await _signaturCreationUnitDERepository.InsertOrUpdateAsync(scu).ConfigureAwait(false);

        public async Task<ftSignaturCreationUnitFR> GetSignaturCreationUnitFRAsync(Guid id) => await _signaturCreationUnitFRRepository.GetAsync(id).ConfigureAwait(false);
        public async Task<IEnumerable<ftSignaturCreationUnitFR>> GetSignaturCreationUnitFRListAsync() => await _signaturCreationUnitFRRepository.GetAsync().ConfigureAwait(false);
        public async Task InsertOrUpdateSignaturCreationUnitFRAsync(ftSignaturCreationUnitFR scu) => await _signaturCreationUnitFRRepository.InsertOrUpdateAsync(scu).ConfigureAwait(false);
    }
}
