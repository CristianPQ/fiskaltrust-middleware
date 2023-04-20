﻿using System.Xml.Serialization;
using System.Collections.Generic;
using fiskaltrust.Middleware.SCU.IT.Epson.Utilities;

namespace fiskaltrust.Middleware.SCU.IT.Epson.Models
{
    [XmlType("printRecLotteryID")]
    public class LotteryID
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "code")]
        public string? Code { get; set; }

        public static LotteryID FromString(string code) => new() { Code = code };
    }


    [XmlType("displayText")]
    public class DisplayText
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "data")]
        public string? Data { get; set; }

        public static DisplayText FromString(string data) => new() { Data = data };
    }

    [XmlType("printRecMessage")]
    public class PrintRecMessage
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "messageType")]
        public string? MessageType { get; set; }
        [XmlAttribute(AttributeName = "index")]
        public string? Index { get; set; }
        [XmlAttribute(AttributeName = "font")]
        public string? Font { get; set; }
        [XmlAttribute(AttributeName = "message")]
        public string? Message { get; set; }
        [XmlAttribute(AttributeName = "comment")]
        public string? Comment { get; set; }
    }

    [XmlRoot(ElementName = "beginFiscalReceipt")]
    public class BeginFiscalReceipt
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
    }

    [XmlRoot(ElementName = "printRecItem")]
    public class PrintRecItem
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlIgnore]
        public decimal Quantity { get; set; }
        [XmlAttribute(AttributeName = "quantity")]
        public string QuantityStr
        {
            get => Quantity.ToString(EpsonFormatters.QuantityFormatter);

            set
            {
                if (decimal.TryParse(value, out var quantity))
                {
                    Quantity = quantity;
                }
            }
        }
        [XmlIgnore]
        public decimal UnitPrice { get; set; }
        [XmlAttribute(AttributeName = "unitPrice")]
        public string UnitPriceStr
        {
            get => UnitPrice.ToString(EpsonFormatters.CurrencyFormatter);
            set
            {
                if (decimal.TryParse(value, out var unitPrice))
                {
                    UnitPrice = unitPrice;
                }
            }
        }
        [XmlAttribute(AttributeName = "department")]
        public int Department { get; set; }
        [XmlAttribute(AttributeName = "justification")]
        public int Justification { get; set; } = 1;

    }


    [XmlRoot(ElementName = "printRecRefund")]
    public class PrintRecRefund
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlIgnore]
        public decimal Quantity { get; set; }
        [XmlAttribute(AttributeName = "quantity")]
        public string QuantityStr
        {
            get => Quantity.ToString(EpsonFormatters.QuantityFormatter);

            set
            {
                if (decimal.TryParse(value, out var quantity))
                {
                    Quantity = quantity;
                }
            }
        }
        [XmlIgnore]
        public decimal UnitPrice { get; set; }
        [XmlAttribute(AttributeName = "unitPrice")]
        public string UnitPriceStr
        {
            get => UnitPrice.ToString(EpsonFormatters.CurrencyFormatter);
            set
            {
                if (decimal.TryParse(value, out var unitPrice))
                {
                    UnitPrice = unitPrice;
                }
            }
        }
        [XmlIgnore]
        public decimal? Amount { get; set; }
        [XmlAttribute(AttributeName = "amount")]
        public string? AmountStr
        {
            get => Amount.HasValue ? Amount.Value.ToString(EpsonFormatters.CurrencyFormatter) : null;

            set
            {
                if (decimal.TryParse(value, out var amount))
                {
                    Amount = amount;
                }
            }
        }
        [XmlIgnore]
        public int? OperationType { get; set; }

        [XmlAttribute(AttributeName = "operationType")]
        public string? OperationTypeStr
        {
            get => OperationType.HasValue ? OperationType.ToString() : null;

            set
            {
                if (int.TryParse(value, out var operationType))
                {
                    OperationType = operationType;
                }
            }
        }


        [XmlAttribute(AttributeName = "department")]
        public int Department { get; set; }
        [XmlAttribute(AttributeName = "justification")]
        public int Justification { get; set; } = 1;
    }

    [XmlRoot(ElementName = "printRecItemAdjustment")]
    public class PrintRecItemAdjustment
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }
        [XmlAttribute(AttributeName = "adjustmentType")]
        public int AdjustmentType { get; set; }
        [XmlIgnore]
        public decimal? Amount { get; set; }
        [XmlAttribute(AttributeName = "amount")]
        public string? AmountStr
        {
            get => Amount.HasValue ? Amount.Value.ToString(EpsonFormatters.CurrencyFormatter) : null;

            set
            {
                if (decimal.TryParse(value, out var amount))
                {
                    Amount = amount;
                }
            }
        }
        [XmlAttribute(AttributeName = "department")]
        public int Department { get; set; }
        [XmlAttribute(AttributeName = "justification")]
        public int Justification { get; set; } = 1;
    }

    [XmlRoot(ElementName = "printRecSubtotalAdjustment")]
    public class PrintRecSubtotalAdjustment
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }

        /// <summary>  
        /// +value is surcharge -value is discount 
        ///determines discount/surcharge operation to perform:
        ///1 = Discount on subtotal with subtotal printed out
        ///2 = Discount on subtotal without subtotal printed out
        /// 6 = Surcharge on subtotal with subtotal printed out
        /// 7 = Surcharge on subtotal without subtotal printed out
        /// </summary>
        [XmlAttribute(AttributeName = "adjustmentType")]
        public int AdjustmentType { get; set; }
        [XmlIgnore]
        public decimal Amount { get; set; }
        [XmlAttribute(AttributeName = "amount")]
        public string AmountStr
        {
            get => Amount.ToString(EpsonFormatters.CurrencyFormatter);
            set
            {
                if (decimal.TryParse(value, out var amount))
                {
                    Amount = amount;
                }
            }
        }
        [XmlAttribute(AttributeName = "justification")]
        public int Justification { get; set; }
    }

    [XmlRoot(ElementName = "printRecSubtotal")]
    public class PrintRecSubtotal
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "option")]
        public int Option { get; set; } = 0;
    }

    [XmlRoot(ElementName = "printBarCode")]
    public class PrintBarCode
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }
        [XmlAttribute(AttributeName = "position")]
        public int Position { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public int Width { get; set; }
        [XmlAttribute(AttributeName = "height")]
        public int Height { get; set; }
        [XmlAttribute(AttributeName = "hRIPosition")]
        public int HRIPosition { get; set; }
        [XmlAttribute(AttributeName = "hRIFont")]
        public char HRIFont { get; set; }
        [XmlAttribute(AttributeName = "codeType")]
        public string? CodeType { get; set; }
        [XmlAttribute(AttributeName = "code")]
        public string? Code { get; set; }
    }

    [XmlRoot(ElementName = "printRecTotal")]
    public class PrintRecTotal
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; }

        [XmlAttribute(AttributeName = "description")]
        public string? Description { get; set; }

        [XmlIgnore]
        public decimal Payment { get; set; }

        [XmlAttribute(AttributeName = "payment")]
        public string PaymentStr
        {
            get => Payment.ToString(EpsonFormatters.CurrencyFormatter);
            set
            {
                if (decimal.TryParse(value, out var payment))
                {
                    Payment = payment;
                }
            }
        }
        [XmlAttribute(AttributeName = "paymentType")]
        public int PaymentType { get; set; }

        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "justification")]
        public int Justification { get; set; } = 1;
    }

    [XmlRoot(ElementName = "endFiscalReceipt")]
    public class EndFiscalReceipt
    {
        [XmlAttribute(AttributeName = "operator")]
        public string? Operator { get; set; } 
    }

    [XmlRoot(ElementName = "printerFiscalReceipt")]
    public class FiscalReceipt
    {

        [XmlElement(ElementName = "displayText")]
        public DisplayText? DisplayText { get; set; }

        [XmlElement(ElementName = "printRecMessage")]
        public List<PrintRecMessage>? PrintRecMessage { get; set; }

        [XmlElement(ElementName = "beginFiscalReceipt")]
        public BeginFiscalReceipt BeginFiscalReceipt { get; set; } = new BeginFiscalReceipt();

        [XmlElement(ElementName = "printRecItem")]
        public List<PrintRecItem>? PrintRecItem { get; set; }

        [XmlElement(ElementName = "printRecRefund")]
        public List<PrintRecRefund>? PrintRecRefund { get; set; }

        [XmlElement(ElementName = "printRecItemVoid")]
        public List<PrintRecItemAdjustment>? PrintRecItemAdjustment { get; set; }

        [XmlElement(ElementName = "printRecSubtotalAdjustment")]
        public List<PrintRecSubtotalAdjustment>? PrintRecSubtotalAdjustment { get; set; }

        [XmlElement(ElementName = "printRecSubtotal")]
        public PrintRecSubtotal? PrintRecSubtotal { get; set; }

        [XmlElement(ElementName = "printBarCode")]
        public PrintBarCode? PrintBarCode { get; set; }

        [XmlElement(ElementName = "printRecLotteryID")]
        public LotteryID? LotteryID { get; set; }

        [XmlElement(ElementName = "printRecTotal")]
        public List<PrintRecTotal>? PrintRecTotal { get; set; }

        [XmlElement(ElementName = "endFiscalReceipt")]
        public EndFiscalReceipt EndFiscalReceipt { get; set; }= new EndFiscalReceipt();
    }

}
