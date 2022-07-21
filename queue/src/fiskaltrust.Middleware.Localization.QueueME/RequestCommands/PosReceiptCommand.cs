﻿using System;
using System.Threading.Tasks;
using System.Linq;
using fiskaltrust.storage.V0;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using fiskaltrust.Middleware.Localization.QueueME.Models;
using fiskaltrust.ifPOS.v1;
using fiskaltrust.ifPOS.v1.me;
using fiskaltrust.Middleware.Localization.QueueME.Exceptions;
using fiskaltrust.Middleware.Localization.QueueME.Extensions;
using System.Collections.Generic;
using fiskaltrust.Middleware.Contracts.Constants;
using fiskaltrust.Middleware.Contracts.Repositories;
using fiskaltrust.Middleware.Localization.QueueME.Factories;
using Grpc.Core;

namespace fiskaltrust.Middleware.Localization.QueueME.RequestCommands
{
    public class PosReceiptCommand : RequestCommand
    {
        private readonly SignatureItemFactory _signatureItemFactory;

        public PosReceiptCommand(ILogger<RequestCommand> logger, IConfigurationRepository configurationRepository,
            IMiddlewareJournalMERepository journalMeRepository, IMiddlewareQueueItemRepository queueItemRepository,
            IMiddlewareActionJournalRepository actionJournalRepository, QueueMEConfiguration queueMeConfiguration, SignatureItemFactory signatureItemFactory) :
            base(logger, configurationRepository, journalMeRepository, queueItemRepository, actionJournalRepository, queueMeConfiguration)
        {
            _signatureItemFactory = signatureItemFactory;
        }

        public override async Task<RequestCommandResponse> ExecuteAsync(IMESSCD client, ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, ftQueueME queueMe, bool subsequent = false)
        {
            var scu = await ConfigurationRepository.GetSignaturCreationUnitMEAsync(queueMe.ftSignaturCreationUnitMEId.Value).ConfigureAwait(false);

            await ThrowIfCashDepositOutstanding().ConfigureAwait(false);
            await ThrowIfInvoiceAlreadyReceived(request.cbReceiptReference);

            var invoice = string.IsNullOrEmpty(request.ftReceiptCaseData) ? null : JsonConvert.DeserializeObject<Invoice>(request.ftReceiptCaseData);
            var invoiceDetails = await CreateInvoiceDetail(request, invoice, queueItem).ConfigureAwait(false);
            invoiceDetails.InvoicingType = InvoicingType.Invoice;

            return await SendInvoiceDetailToCis(client, queue, request, queueItem, queueMe, scu, invoiceDetails, subsequent);
        }

        protected async Task<RequestCommandResponse> SendInvoiceDetailToCis(IMESSCD client, ftQueue queue, ReceiptRequest request, ftQueueItem queueItem,
            ftQueueME queueMe, ftSignaturCreationUnitME scu, InvoiceDetails invoiceDetails, bool subsequent)
        {
            var operatorCode = GetOperatorCodeOrThrow(request.cbUser);

            var computeIicRequest = CreateComputeIicRequest(request, scu, invoiceDetails);
            var computeIicResponse = await client.ComputeIICAsync(computeIicRequest).ConfigureAwait(false);

            RegisterInvoiceResponse registerInvoiceResponse;
            try
            {
                var registerInvoiceRequest = CreateInvoiceRequest(request, queueItem, scu, invoiceDetails, computeIicResponse, operatorCode, subsequent);
                registerInvoiceResponse = await client.RegisterInvoiceAsync(registerInvoiceRequest).ConfigureAwait(false);
            }
            catch (RpcException ex)
            {
                return await CheckForFiscalizationException(queue, request, queueItem, queueMe, subsequent, ex);
            }
            catch (Exception ex)
            {
                Logger.LogDebug(ex, "Fiscalization service is not reachable.");
                return await ProcessFailedReceiptRequest(queue, queueItem, request, queueMe).ConfigureAwait(false);
            }
            await InsertJournalMe(queue, request, queueItem, scu, registerInvoiceResponse, invoiceDetails, computeIicResponse).ConfigureAwait(false);
            var receiptResponse = CreateReceiptResponse(queue, request, queueItem, invoiceDetails.YearlyOrdinalNumber);
            var signature = _signatureItemFactory.CreatePosReceiptSignatures(request, computeIicResponse, invoiceDetails.YearlyOrdinalNumber, scu);
            receiptResponse.ftSignatures = receiptResponse.ftSignatures.Extend(signature);

            return new RequestCommandResponse
            {
                ReceiptResponse = receiptResponse,
            };
        }

        protected string GetOperatorCodeOrThrow(string cbUser)
        {
            try
            {
                if (string.IsNullOrEmpty(cbUser))
                {
                    throw new ArgumentNullException(nameof(cbUser));
                }
                var user = JsonConvert.DeserializeObject<User>(cbUser);
                return cbUser == null
                    ? throw new Exception("The parsed object is null.")
                    : user.OperatorCode;
            }
            catch (Exception ex)
            {
                throw new UserNotParsableException($"Could not parse cbUser '{cbUser}' from request, which needs to be set for this type of receipt. Please see docs.fiskaltrust.cloud for more details about the format.", ex);
            }
        }

        protected async Task ThrowIfInvoiceAlreadyReceived(string receiptReference)
        {
            if (await JournalMeRepository.GetByReceiptReference(receiptReference).AnyAsync())
            {
                throw new InvoiceAlreadyReceivedException("The field cbReceiptReference must be unique, but has already been used. Please make sure to use an unique cbReceiptReference for each receipt.");
            }
        }

        protected async Task ThrowIfCashDepositOutstanding()
        {
            var journalMes = await JournalMeRepository.GetAsync().ConfigureAwait(false);
            var journalMe = journalMes.Where(x => x.JournalType == 0x4D45_0000_0000_0007).OrderByDescending(x => x.TimeStamp).FirstOrDefault();
            if (journalMe == null || new DateTime(journalMe.TimeStamp).Date != DateTime.UtcNow.Date)
            {
                throw new CashDepositOutstandingException("This receipt could not be processed, as the first receipt each day must be a cash-deposit according to Montenegrin law.");
            }
        }

        protected async Task<InvoiceDetails> CreateInvoiceDetail(ReceiptRequest request, Invoice invoice, ftQueueItem queueItem, bool isVoid = false)
        {
            var invoiceDetails = new InvoiceDetails
            {
                InvoiceType = request.GetInvoiceType(),
                SelfIssuedInvoiceType = invoice?.TypeOfSelfiss == null ? null : (SelfIssuedInvoiceType) Enum.Parse(typeof(SelfIssuedInvoiceType), invoice.TypeOfSelfiss),
                TaxFreeAmount = request.cbChargeItems.Where(x => x.GetVatRate().Equals(0)).Sum(x => x.Amount),
                PaymentDetails = request.GetPaymentMethodTypes(isVoid),
                Currency = new CurrencyDetails { CurrencyCode = "EUR", ExchangeRateToEuro = 1 },
                Buyer = GetBuyer(request),
                ItemDetails = GetInvoiceItems(request, isVoid),
                Fees = invoice != null ? GetFees(invoice) : new List<InvoiceFee>(),
                ExportedGoodsAmount = request.cbChargeItems.Where(x => x.IsExportGood()).Sum(x => x.Amount),
                TaxPeriod = new TaxPeriod
                {
                    Month = (uint) request.cbReceiptMoment.Month,
                    Year = (uint) request.cbReceiptMoment.Year
                },
                YearlyOrdinalNumber = await GetNextOrdinalNumber(queueItem).ConfigureAwait(false)
            };

            invoiceDetails.NetAmount = invoiceDetails.ItemDetails.Sum(x => x.NetAmount);
            invoiceDetails.TotalVatAmount = invoiceDetails.ItemDetails.Sum(x => x.NetAmount * x.VatRate / 100);
            invoiceDetails.GrossAmount = invoiceDetails.ItemDetails.Sum(x => x.GrossAmount);
            if (!isVoid)
            {
                return invoiceDetails;
            }
            invoiceDetails.TaxFreeAmount *= -1;
            invoiceDetails.ExportedGoodsAmount *= -1;
            return invoiceDetails;
        }
        private async Task InsertJournalMe(ftQueue queue, ReceiptRequest request, ftQueueItem queueItem, ftSignaturCreationUnitME scu, RegisterInvoiceResponse registerInvoiceResponse,
                            InvoiceDetails invoice, ComputeIICResponse computeIicResponse)
        {
            var lastEntry = await JournalMeRepository.GetLastEntryAsync().ConfigureAwait(false);
            var lastNumber = lastEntry?.Number ?? 0;
            var journal = new ftJournalME
            {
                ftJournalMEId = Guid.NewGuid(),
                ftQueueId = queue.ftQueueId,
                ftQueueItemId = queueItem.ftQueueItemId,
                cbReference = request.cbReceiptReference,
                Number = lastNumber + 1,
                FIC = registerInvoiceResponse.FIC,
                IIC = computeIicResponse.IIC,
                JournalType = (long) JournalTypes.JournalME,
                ftOrdinalNumber = (int) invoice.YearlyOrdinalNumber
            };
            journal.ftInvoiceNumber = string.Concat(scu.BusinessUnitCode, '/', journal.ftOrdinalNumber, '/', request.cbReceiptMoment.Year, '/', scu.TcrCode);
            await JournalMeRepository.InsertAsync(journal).ConfigureAwait(false);
        }
        private static ComputeIICRequest CreateComputeIicRequest(ReceiptRequest request, ftSignaturCreationUnitME scu, InvoiceDetails invoiceDetails)
        {
            return new ComputeIICRequest
            {
                SoftwareCode = scu.SoftwareCode,
                TcrCode = scu.TcrCode,
                BusinessUnitCode = scu.BusinessUnitCode,
                Moment = request.cbReceiptMoment,
                YearlyOrdinalNumber = invoiceDetails.YearlyOrdinalNumber,
                GrossAmount = invoiceDetails.GrossAmount,
            };
        }

        private static RegisterInvoiceRequest CreateInvoiceRequest(ReceiptRequest request, ftQueueItem queueItem, ftSignaturCreationUnitME scu, InvoiceDetails invoiceDetails, ComputeIICResponse computeIicResponse, string operatorCode, bool subsequent)
        {
            return new RegisterInvoiceRequest
            {
                InvoiceDetails = invoiceDetails,
                SoftwareCode = scu.SoftwareCode,
                TcrCode = scu.TcrCode,
                IsIssuerInVATSystem = true,
                BusinessUnitCode = scu.BusinessUnitCode,
                Moment = request.cbReceiptMoment,
                OperatorCode = operatorCode,
                RequestId = queueItem.ftQueueItemId,
                SubsequentDeliveryType = subsequent ? SubsequentDeliveryType.NoInternet : null,
                IIC = computeIicResponse.IIC,
                IICSignature = computeIicResponse.IICSignature
            };
        }

        private static List<InvoiceFee> GetFees(Invoice invoice)
        {
            return invoice.Fees?.Select(fee => new InvoiceFee { Amount = fee.Amount, FeeType = (FeeType) Enum.Parse(typeof(FeeType), fee.FeeType.ToString()) }).ToList();
        }

        private static List<InvoiceItem> GetInvoiceItems(ReceiptRequest request, bool isVoid)
        {
            return request.cbChargeItems.Length > 1000
                ? throw new MaxInvoiceItemsExceededException()
                : request.cbChargeItems.Select(chargeItem => chargeItem.GetInvoiceItem(isVoid)).ToList();
        }

        private static BuyerDetails GetBuyer(ReceiptRequest request)
        {
            if (string.IsNullOrEmpty(request.cbCustomer))
            {
                return null;
            }
            try
            {
                var buyer = JsonConvert.DeserializeObject<Buyer>(request.cbCustomer);
                return buyer == null
                    ? throw new Exception("Value in Field cbCustomer could not be parsed")
                    : new BuyerDetails
                    {
                        IdentificationNumber = buyer.IdentificationNumber,
                        IdentificationType = (BuyerIdentificationType) Enum.Parse(typeof(BuyerIdentificationType), buyer.BuyerIdentificationType),
                        Name = buyer.Name,
                        Address = buyer.Address,
                        Town = buyer.Town,
                        Country = buyer.Country,
                    };
            }
            catch (Exception ex)
            {
                throw new BuyerNotParsableException("The field cbCustomer could not be parsed. If you need to add customer data, please use the correct format according to docs.fiskaltrust.cloud.", ex);
            }
        }

        public override async Task<bool> ReceiptNeedsReprocessing(ftQueueME queueMe, ftQueueItem queueItem, ReceiptRequest request)
        {
            var journalMe = await JournalMeRepository.GetByQueueItemId(queueItem.ftQueueItemId).FirstOrDefaultAsync().ConfigureAwait(false);
            return journalMe == null || string.IsNullOrEmpty(journalMe.IIC) || string.IsNullOrEmpty(journalMe.FIC);
        }

        protected async Task<ulong> GetNextOrdinalNumber(ftQueueItem queueItem)
        {
            var lastJournal = await JournalMeRepository.GetLastEntryAsync().ConfigureAwait(false);
            if (lastJournal == null)
            {
                return 1;
            }
            var lastQueueItem = await QueueItemRepository.GetAsync(lastJournal.ftQueueItemId);
            return queueItem.cbReceiptMoment.Year == lastQueueItem.cbReceiptMoment.Year
                ? (ulong) (lastJournal.ftOrdinalNumber + 1)
                : 1;
        }
    }
}
