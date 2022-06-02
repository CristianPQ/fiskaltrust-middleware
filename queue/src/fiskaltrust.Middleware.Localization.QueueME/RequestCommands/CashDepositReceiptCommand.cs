﻿using System;
using System.Threading.Tasks;
using System.Linq;
using fiskaltrust.storage.V0;
using Microsoft.Extensions.Logging;
using fiskaltrust.Middleware.Localization.QueueME.Models;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.ifPOS.v1.me;
using fiskaltrust.Middleware.Localization.QueueME.Exceptions;
using fiskaltrust.Middleware.Contracts.Constants;
using System.Collections.Generic;
using fiskaltrust.Middleware.Contracts.Repositories;

namespace fiskaltrust.Middleware.Localization.QueueME.RequestCommands
{
    public class CashDepositReceiptCommand : RequestCommand
    {
        public CashDepositReceiptCommand(ILogger<RequestCommand> logger, IConfigurationRepository configurationRepository,IMiddlewareJournalMERepository journalMERepository, 
            IMiddlewareQueueItemRepository queueItemRepository, IMiddlewareActionJournalRepository actionJournalRepository) :
            base(logger, configurationRepository, journalMERepository, queueItemRepository, actionJournalRepository)
        { }

        public override async Task<RequestCommandResponse> ExecuteAsync(IMESSCD client, ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, ftQueueME queueME)
        {
            try
            {
                if (queueME == null || !queueME.ftSignaturCreationUnitMEId.HasValue)
                {
                    throw new ENUNotRegisteredException();
                }
                var scu = await _configurationRepository.GetSignaturCreationUnitMEAsync(queueME.ftSignaturCreationUnitMEId.Value).ConfigureAwait(false);
                if (scu == null || string.IsNullOrEmpty(scu.TcrCode))
                {
                    throw new ENUNotRegisteredException();
                }
                var registerCashDepositRequest = new RegisterCashDepositRequest()
                {
                    Amount = request.cbReceiptAmount ?? request.cbChargeItems.Sum(x => x.Amount),
                    Moment = request.cbReceiptMoment,
                    RequestId = queueItem.ftQueueItemId,
                    SubsequentDeliveryType = null,
                    TcrCode = scu.TcrCode,
                };

                var registerCashDepositResponse = await client.RegisterCashDepositAsync(registerCashDepositRequest).ConfigureAwait(false);
                await InsertJournalME(queue, request, queueItem, registerCashDepositResponse).ConfigureAwait(false);
                var receiptResponse = CreateReceiptResponse(request, queueItem);
                var actionJournalEntry =  CreateActionJournal(queue, request.ftReceiptCase, queueItem);
                return new RequestCommandResponse()
                {
                    ReceiptResponse = receiptResponse,
                    ActionJournals = new List<ftActionJournal>()
                    {
                        actionJournalEntry
                    }
                };
            }
            catch (Exception ex)
            {
                var t = ex.GetType().Name;
                if (ex.GetType().Name == ENDPOINTNOTFOUND)
                {
                    _logger.LogDebug(ex, "TSE not reachable.");
                    return await ProcessFailedReceiptRequest(queueItem, request, queueME).ConfigureAwait(false);
                }
                _logger.LogCritical(ex, "An exception occured while processing this request.");
                throw;
            }
        }

        public override async Task<bool> ReceiptNeedsReprocessing(ftQueueME queueME, ftQueueItem queueItem, ReceiptRequest request)
        {
            var journalME = await _journalMERepository.GetByQueueItemId(queueItem.ftQueueItemId).FirstOrDefaultAsync().ConfigureAwait(false);
            if (journalME == null || string.IsNullOrEmpty(journalME.FCDC))
            {
                return true;
            }
            return false;
       }

        private async Task InsertJournalME(ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, RegisterCashDepositResponse registerCashDepositResponse)
        {
            var journal = new ftJournalME()
            {
                ftJournalMEId = Guid.NewGuid(),
                ftQueueId = queue.ftQueueId,
                ftQueueItemId = queueItem.ftQueueItemId,
                cbReference = request.cbReceiptReference,
                Number = queue.ftReceiptNumerator,
                FCDC = registerCashDepositResponse.FCDC,
                JournalType = request.ftReceiptCase,
                TimeStamp = DateTime.UtcNow.Ticks
            };
            await _journalMERepository.InsertAsync(journal).ConfigureAwait(false);
        }
    }
}
