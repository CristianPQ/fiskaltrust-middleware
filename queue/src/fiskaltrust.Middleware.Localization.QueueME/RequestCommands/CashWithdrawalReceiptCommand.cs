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
    public class CashWithdrawalReceiptCommand : RequestCommand
    {
        public CashWithdrawalReceiptCommand(ILogger<RequestCommand> logger, IConfigurationRepository configurationRepository,
            IMiddlewareJournalMERepository journalMeRepository, IMiddlewareQueueItemRepository queueItemRepository, IMiddlewareActionJournalRepository actionJournalRepository, QueueMEConfiguration queueMeConfiguration) :
            base(logger, configurationRepository, journalMeRepository, queueItemRepository, actionJournalRepository, queueMeConfiguration)
        { }

        public override async Task<RequestCommandResponse> ExecuteAsync(IMESSCD client, ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, ftQueueME queueMe)
        {
            try
            {
                var scu = await ConfigurationRepository.GetSignaturCreationUnitMEAsync(queueMe.ftSignaturCreationUnitMEId.Value).ConfigureAwait(false);
                var registerCashWithdrawalRequest = new RegisterCashWithdrawalRequest
                {
                    Amount = request.cbReceiptAmount ?? request.cbChargeItems.Sum(x => x.Amount),
                    Moment = request.cbReceiptMoment,
                    RequestId = queueItem.ftQueueItemId,
                    SubsequentDeliveryType = null,
                    TcrCode = scu.TcrCode
                };
                await client.RegisterCashWithdrawalAsync(registerCashWithdrawalRequest).ConfigureAwait(false);
                var receiptResponse = CreateReceiptResponse(queue, request, queueItem);
                var actionJournalEntry = CreateActionJournal(queue, request.ftReceiptCase, queueItem);
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
                Logger.LogDebug(ex, "Fiscalization service is not reachable.");
                return await ProcessFailedReceiptRequest(queue, queueItem, request, queueMe).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "An exception occurred while processing this request.");
                throw;
            }
        }

        public override async Task<bool> ReceiptNeedsReprocessing(ftQueueME queueMe, ftQueueItem queueItem, ReceiptRequest request)
        {
            return await ActionJournalExists(queueItem, request.ftReceiptCase).ConfigureAwait(false) == false;
        }
    }
}
