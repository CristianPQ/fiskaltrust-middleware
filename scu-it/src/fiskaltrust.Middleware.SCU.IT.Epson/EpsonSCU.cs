﻿using System.Threading.Tasks;
using fiskaltrust.ifPOS.v1.it;
using fiskaltrust.Middleware.SCU.IT.Configuration;
using Microsoft.Extensions.Logging;

namespace fiskaltrust.Middleware.SCU.IT.Epson;

#nullable enable
public sealed class EpsonSCU : IITSSCD 
{
    private readonly EpsonScuConfiguration _configuration;
    private readonly ILogger<EpsonSCU> _logger;

    public EpsonSCU(ILogger<EpsonSCU> logger, EpsonScuConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public Task<ScuItEchoResponse> EchoAsync(ScuItEchoRequest request) => throw new System.NotImplementedException();
    public Task<EndExportSessionResponse> EndExportSessionAsync(EndExportSessionRequest request) => throw new System.NotImplementedException();
   public Task<FiscalReceiptResponse> FiscalReceiptInvoiceAsync(FiscalReceiptInvoice request) => throw new System.NotImplementedException();
    public Task<FiscalReceiptResponse> FiscalReceiptRefundAsync(FiscalReceiptRefund request) => throw new System.NotImplementedException();
    public Task<PrinterStatus> GetPrinterInfoAsync() => throw new System.NotImplementedException();
    public Task<PrinterStatus> GetPrinterStatusAsync() => throw new System.NotImplementedException();
    public Task<StartExportSessionResponse> StartExportSessionAsync(StartExportSessionRequest request) => throw new System.NotImplementedException();
}
