﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.storage.V0;
using fiskaltrust.Middleware.Contracts.RequestCommands;
using fiskaltrust.ifPOS.v1.it;
using System.Linq;
using fiskaltrust.Middleware.Localization.QueueIT.Extensions;
using fiskaltrust.Middleware.Localization.QueueIT.Factories;
using fiskaltrust.Middleware.Localization.QueueIT.Services;
using fiskaltrust.Middleware.Contracts.Extensions;
using fiskaltrust.Middleware.Localization.QueueIT.Exceptions;
using Newtonsoft.Json;
using fiskaltrust.Middleware.Contracts.Repositories;

namespace fiskaltrust.Middleware.Localization.QueueIT.RequestCommands
{
    public struct RefundDetails
    {
        public string Serialnumber { get; set; }
        public long ZRepNumber { get; set; }
        public long ReceiptNumber { get; set; }
        public DateTime ReceiptDateTime { get; set; }
    }

    public class PosReceiptCommand : RequestCommand
    {
        public override long CountryBaseState => Constants.Cases.BASE_STATE;
        protected override IQueueRepository IQueueRepository => _iQueueRepository;
        private readonly IQueueRepository _iQueueRepository;
        private readonly IConfigurationRepository _configurationRepository;
        private readonly SignatureItemFactoryIT _signatureItemFactoryIT;
        private readonly IMiddlewareJournalITRepository _journalITRepository;
        private readonly IITSSCD _client;

        public PosReceiptCommand(IITSSCDProvider itIsscdProvider, SignatureItemFactoryIT signatureItemFactoryIT, IMiddlewareJournalITRepository journalITRepository, IConfigurationRepository configurationRepository,IQueueRepository iQeueRepository)
        {
            _client = itIsscdProvider.Instance;
            _signatureItemFactoryIT = signatureItemFactoryIT;
            _journalITRepository = journalITRepository;
            _iQueueRepository = iQeueRepository;
            _configurationRepository = configurationRepository;
        }

        public override async Task<RequestCommandResponse> ExecuteAsync(ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, bool isRebooking = false)
        {
            var journals = await _journalITRepository.GetAsync().ConfigureAwait(false);
            if (journals.Where(x => x.cbReceiptReference.Equals(request.cbReceiptReference)).Any())
            {
                throw new CbReferenceExistsException(request.cbReceiptReference);
            }

            var queueIt = await _iQueueRepository.GetQueueAsync(queue.ftQueueId).ConfigureAwait(false);

            var receiptResponse = CreateReceiptResponse(queue, request, queueItem, queueIt.CashBoxIdentification, CountryBaseState);

            if (request.IsFailedReceipt() && request.cbReceiptMoment.Date >= DateTime.Now.Date.AddDays(-12)) // TODO We'll have to check if this calculation is correct. Or maybe we should check this on SCU side?
            {
                receiptResponse.ftState = Constants.States.ToOldForLateSigning;

                return new RequestCommandResponse
                {
                    ReceiptResponse = receiptResponse,
                    ActionJournals = new List<ftActionJournal>()
                };
            }

            FiscalReceiptResponse response;
            if (request.IsVoid())
            {
                var fiscalReceiptRefund = await CreateRefundAsync(request).ConfigureAwait(false);
                response = await _client.FiscalReceiptRefundAsync(fiscalReceiptRefund).ConfigureAwait(false);
            }
            else
            {
                var fiscalReceiptinvoice = CreateInvoice(request);
                response = await _client.FiscalReceiptInvoiceAsync(fiscalReceiptinvoice).ConfigureAwait(false);
            }
            if (!response.Success)
            {
                if (response.ErrorInfo.StartsWith("[ERR-Connection]") && !isRebooking)
                {
                    await ProcessFailedReceiptRequest(queue, queueItem, request).ConfigureAwait(false);
                }
                else
                {
                    throw new Exception(response.ErrorInfo);
                }
            }
            else
            {
                receiptResponse.ftReceiptIdentification += $"{response.ReceiptNumber}Z{response.ZRepNumber}";
                receiptResponse.ftSignatures = _signatureItemFactoryIT.CreatePosReceiptSignatures(response);
                var journalIT = CreateJournalIT(queue, queueIt, request, queueItem, response);
                await _journalITRepository.InsertAsync(journalIT).ConfigureAwait(false);
            }

            return new RequestCommandResponse
            {
                ReceiptResponse = receiptResponse,
                Signatures = receiptResponse.ftSignatures.ToList(),
                ActionJournals = new List<ftActionJournal>()
            };
        }

        private ftJournalIT CreateJournalIT(ftQueue queue, IQueue queueIt, ReceiptRequest request, ftQueueItem queueItem, FiscalReceiptResponse response) =>
            new ftJournalIT
            {
                ftJournalITId = Guid.NewGuid(),
                ftQueueId = queue.ftQueueId,
                ftQueueItemId = queueItem.ftQueueItemId,
                cbReceiptReference = request.cbReceiptReference,
                ftSignaturCreationUnitITId = queueIt.ftSignaturCreationUnitId.Value,
                JournalType = request.ftReceiptCase & 0xFFFF,
                ReceiptDateTime = response.ReceiptDateTime,
                ReceiptNumber = response.ReceiptNumber,
                ZRepNumber = response.ZRepNumber,
                DataJson = response.ReceiptDataJson,
                TimeStamp = DateTime.UtcNow.Ticks
            };

        private static FiscalReceiptInvoice CreateInvoice(ReceiptRequest request)
        {
            var fiscalReceiptRequest = new FiscalReceiptInvoice()
            {
                //Barcode = ChargeItem.ProductBarcode,
                //TODO DisplayText = "Message on customer display",
                Items = request.cbChargeItems.Where(x => !x.IsPaymentAdjustment()).Select(p => new Item
                {
                    Description = p.Description,
                    Quantity = p.Quantity,
                    UnitPrice = p.UnitPrice ?? p.Amount / p.Quantity,
                    Amount = p.Amount,
                    VatGroup = p.GetVatGroup()
                }).ToList(),
                PaymentAdjustments = request.GetPaymentAdjustments(),
                Payments = request.cbPayItems?.Select(p => new Payment
                {
                    Amount = p.Amount,
                    Description = p.Description,
                    PaymentType = p.GetPaymentType()
                }).ToList()
            };
            return fiscalReceiptRequest;
        }

        private async Task<FiscalReceiptRefund> CreateRefundAsync(ReceiptRequest request)
        {
            var refundDetails = await GetRefundDetailsAsync(request).ConfigureAwait(false);
            var fiscalReceiptRequest = new FiscalReceiptRefund()
            {
                //TODO Barcode = "0123456789" 
                Operator = "0",
                DisplayText = $"REFUND {refundDetails.ZRepNumber:D4} {refundDetails.ReceiptNumber:D4} {refundDetails.ReceiptDateTime:ddMMyyyy} {refundDetails.Serialnumber}",
                Refunds = request.cbChargeItems?.Select(p => new Refund
                {
                    Description = p.Description,
                    Quantity = Math.Abs(p.Quantity),
                    UnitPrice = p.UnitPrice ?? 0,
                    Amount = Math.Abs(p.Amount),
                    VatGroup = p.GetVatGroup(),
                    OperationType = p.GetRefundOperationType()
                }).ToList(),
                PaymentAdjustments = request.GetPaymentAdjustments(),
                Payments = request.cbPayItems?.Select(p => new Payment
                {
                    Amount = p.Amount,
                    Description = p.Description,
                    PaymentType = p.GetPaymentType(),
                    Index = 1
                }).ToList()
            };

            return fiscalReceiptRequest;
        }

        private async Task<RefundDetails> GetRefundDetailsAsync(ReceiptRequest request)
        {
            var journalIt = await _journalITRepository.GetAsync().ConfigureAwait(false);
            var receipt = journalIt.Where(x => x.cbReceiptReference.Equals(request.cbPreviousReceiptReference)).FirstOrDefault() ?? throw new RefundException($"Receipt {request.cbPreviousReceiptReference} was not found!");
            var scu = await _configurationRepository.GetSignaturCreationUnitITAsync(receipt.ftSignaturCreationUnitITId).ConfigureAwait(false);
            var deviceInfo = JsonConvert.DeserializeObject<DeviceInfo>(scu.InfoJson);
            return new RefundDetails()
            {
                ReceiptNumber = receipt.ReceiptNumber,
                ZRepNumber = receipt.ZRepNumber,
                ReceiptDateTime = receipt.ReceiptDateTime,
                Serialnumber = deviceInfo.SerialNumber
            };
        }

        public override async Task<bool> ReceiptNeedsReprocessing(ftQueue queue, ReceiptRequest request, ftQueueItem queueItem)
        {
            var journalIt = await _journalITRepository.GetByQueueItemId(queueItem.ftQueueItemId).ConfigureAwait(false);
            return journalIt == null;
        }
    }
}
