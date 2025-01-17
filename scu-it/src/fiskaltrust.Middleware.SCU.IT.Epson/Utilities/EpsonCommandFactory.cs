﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Channels;
using System.Xml.Serialization;
using fiskaltrust.ifPOS.v1.it;
using fiskaltrust.Middleware.SCU.IT.Epson.Exceptions;
using fiskaltrust.Middleware.SCU.IT.Epson.Models;
using fiskaltrust.Middleware.SCU.IT.Epson.Extensions;


namespace fiskaltrust.Middleware.SCU.IT.Epson.Utilities
{
    public class EpsonCommandFactory
    {
        private readonly EpsonScuConfiguration _epsonScuConfiguration;

        public EpsonCommandFactory(EpsonScuConfiguration epsonScuConfiguration)
        {

            _epsonScuConfiguration = epsonScuConfiguration;
        }

        public string CreateInvoiceRequestContent(FiscalReceiptInvoice request)
        {
            var fiscalReceipt = CreateFiscalReceipt(request);
            if (!string.IsNullOrEmpty(request.DisplayText))
            {
                fiscalReceipt.DisplayText.Add(new DisplayText() { Data = request.DisplayText });
            }
            fiscalReceipt.ItemAndMessages = request.GetItemAndMessages();
            fiscalReceipt.AdjustmentAndMessages = request.GetAdjustmentAndMessages();
            return SoapSerializer.Serialize(fiscalReceipt);
        }


        public string CreateRefundRequestContent(FiscalReceiptRefund request)
        {
            var fiscalReceipt = CreateFiscalReceipt(request);
            fiscalReceipt.PrintRecMessage = new PrintRecMessage()
            {
                Operator = request.Operator,
                Message = request.DisplayText,
                MessageType = (int) Messagetype.AdditionalInfo
            };
            fiscalReceipt.PrintRecRefund = request.Refunds.Select(GetPrintRecRefund).ToList();
            return SoapSerializer.Serialize(fiscalReceipt);
        }

        public string CreateQueryPrinterStatusRequestContent()
        {
            var queryPrinterStatus = new QueryPrinterStatusCommand { QueryPrinterStatus = new QueryPrinterStatus { StatusType = 1 } };
            return SoapSerializer.Serialize(queryPrinterStatus);
        }

        public string CreatePrintZReportRequestContent(DailyClosingRequest request)
        {
            var fiscalReport = new FiscalReport
            {
                ZReport = new ZReport { Operator = request.Operator },
                DisplayText = string.IsNullOrEmpty(request.DisplayText) ? null : new DisplayText() { Data = request.DisplayText }
            };
            return SoapSerializer.Serialize(fiscalReport);
        }

        public string CreateNonFiscalReceipt(NonFiscalRequest request)
        {
            var printerNonFiscal = new PrinterNonFiscal
            {
                PrintNormals = request.NonFiscalPrints.Select(x => new PrintNormal() { Data = x.Data, Font = x.Font }).ToList()
            };
            return SoapSerializer.Serialize(printerNonFiscal);
        }

        public static T? Deserialize<T>(Stream stream) where T : class
        {
            var reader = new XmlSerializer(typeof(T));
            return reader.Deserialize(stream) as T;
        }

        private PrintRecRefund GetPrintRecRefund(Refund recRefund)
        {
            if (recRefund.UnitPrice != 0 && recRefund.Quantity != 0)
            {
                return new PrintRecRefund
                {
                    Description = recRefund.Description,
                    Quantity = recRefund.Quantity,
                    UnitPrice = recRefund.UnitPrice,
                    Department = recRefund.VatGroup
                };
            }
            else
            {
                throw new Exception("Refund properties not set properly!");
            }
        }


        private FiscalReceipt CreateFiscalReceipt(FiscalReceiptInvoice request)
        {
            var fiscalReceipt = new FiscalReceipt
            {
                LotteryID = !string.IsNullOrEmpty(request.LotteryID) ? new LotteryID() { Code = request.LotteryID} : null,
                PrintBarCode = !string.IsNullOrEmpty(request.Barcode) ? new PrintBarCode()
                {
                    Code = request.Barcode,
                    CodeType = _epsonScuConfiguration.CodeType,
                    Height = _epsonScuConfiguration.BarCodeHeight,
                    HRIFont = _epsonScuConfiguration.BarCodeHRIFont,
                    HRIPosition = _epsonScuConfiguration.BarCodeHRIPosition,
                    Position = _epsonScuConfiguration.BarCodePosition,
                    Width = _epsonScuConfiguration.BarCodeWidth
                } : null
            };
            fiscalReceipt.BeginFiscalReceipt.Operator = GetOperator(request.Operator);
            fiscalReceipt.EndFiscalReceipt.Operator = GetOperator(request.Operator);
            fiscalReceipt.RecTotalAndMessages = request.Payments.GetTotalAndMessages();
            return fiscalReceipt;
        }

        private string GetOperator(string cbUser)
        {
            if (string.IsNullOrEmpty(cbUser))
            {
                return cbUser;
            } 
            var isvalid = int.TryParse(cbUser, out var operatory);
            var inRange = operatory > 0 && operatory < 13;
            isvalid = isvalid && inRange;
            if (!isvalid)
            {
                throw new OperatorException(cbUser);
            }
            return cbUser;
        }

        private FiscalReceipt CreateFiscalReceipt(FiscalReceiptRefund request)
        {
            var fiscalReceipt = new FiscalReceipt
            {
                LotteryID = !string.IsNullOrEmpty(request.LotteryID) ? new LotteryID() { Code = request.LotteryID } : null,
                PrintBarCode = !string.IsNullOrEmpty(request.Barcode) ? new PrintBarCode()
                {
                    Code = request.Barcode,
                    CodeType = _epsonScuConfiguration.CodeType,
                    Height = _epsonScuConfiguration.BarCodeHeight,
                    HRIFont = _epsonScuConfiguration.BarCodeHRIFont,
                    HRIPosition = _epsonScuConfiguration.BarCodeHRIPosition,
                    Position = _epsonScuConfiguration.BarCodePosition,
                    Width = _epsonScuConfiguration.BarCodeWidth
                } : null
            };
            fiscalReceipt.BeginFiscalReceipt.Operator = GetOperator(request.Operator);
            fiscalReceipt.EndFiscalReceipt.Operator = GetOperator(request.Operator);
            fiscalReceipt.RecTotalAndMessages = request.Payments.GetTotalAndMessages();
            return fiscalReceipt;
        }

    }
}
