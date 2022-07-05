﻿using System;
using System.Threading.Tasks;
using System.Linq;
using fiskaltrust.storage.V0;
using Microsoft.Extensions.Logging;
using fiskaltrust.Middleware.Localization.QueueME.Models;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.ifPOS.v1.me;
using fiskaltrust.Middleware.Localization.QueueME.Exceptions;
using System.Collections.Generic;
using fiskaltrust.Middleware.Contracts.Repositories;

namespace fiskaltrust.Middleware.Localization.QueueME.RequestCommands
{
    public class CashDepositReceiptCommand : RequestCommand
    {
        public CashDepositReceiptCommand(ILogger<RequestCommand> logger, IConfigurationRepository configurationRepository,IMiddlewareJournalMERepository journalMeRepository, 
            IMiddlewareQueueItemRepository queueItemRepository, IMiddlewareActionJournalRepository actionJournalRepository, QueueMEConfiguration queueMeConfiguration) :
            base(logger, configurationRepository, journalMeRepository, queueItemRepository, actionJournalRepository, queueMeConfiguration)
        { }

        public override async Task<RequestCommandResponse> ExecuteAsync(IMESSCD client, ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, ftQueueME queueMe)
        {
            try
            {
                if (queueMe == null || !queueMe.ftSignaturCreationUnitMEId.HasValue)
                {
                    throw new EnuNotRegisteredException();
                }
                var scu = await ConfigurationRepository.GetSignaturCreationUnitMEAsync(queueMe.ftSignaturCreationUnitMEId.Value).ConfigureAwait(false);
                if (scu == null || string.IsNullOrEmpty(scu.TcrCode))
                {
                    throw new EnuNotRegisteredException();
                }
                var registerCashDepositRequest = new RegisterCashDepositRequest
                {
                    Amount = request.cbReceiptAmount ?? request.cbChargeItems.Sum(x => x.Amount),
                    Moment = request.cbReceiptMoment,
                    RequestId = queueItem.ftQueueItemId,
                    SubsequentDeliveryType = null,
                    TcrCode = scu.TcrCode,
                };

                var registerCashDepositResponse = await client.RegisterCashDepositAsync(registerCashDepositRequest).ConfigureAwait(false);
                await InsertJournalMe(queue, request, queueItem, registerCashDepositResponse).ConfigureAwait(false);
                var receiptResponse = CreateReceiptResponse(request, queueItem);
                var actionJournalEntry =  CreateActionJournal(queue, request.ftReceiptCase, queueItem);
                return new RequestCommandResponse
                {
                    ReceiptResponse = receiptResponse,
                    ActionJournals = new List<ftActionJournal>
                    {
                        actionJournalEntry
                    }
                };
            }
            catch(EntryPointNotFoundException ex)
            {
                Logger.LogDebug(ex, "TSE is not reachable.");
                return await ProcessFailedReceiptRequest(queueItem, request, queueMe).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An exception occurred while processing this request.");
                throw;
            }
        }

        public override async Task<bool> ReceiptNeedsReprocessing(ftQueueME queueMe, ftQueueItem queueItem, ReceiptRequest request)
        {
            var journalMe = await JournalMeRepository.GetByQueueItemId(queueItem.ftQueueItemId).FirstOrDefaultAsync().ConfigureAwait(false);
            return journalMe == null || string.IsNullOrEmpty(journalMe.FCDC);
        }

        private async Task InsertJournalMe(ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, RegisterCashDepositResponse registerCashDepositResponse)
        {
            var journal = new ftJournalME
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
            await JournalMeRepository.InsertAsync(journal).ConfigureAwait(false);
        }
    }
}
