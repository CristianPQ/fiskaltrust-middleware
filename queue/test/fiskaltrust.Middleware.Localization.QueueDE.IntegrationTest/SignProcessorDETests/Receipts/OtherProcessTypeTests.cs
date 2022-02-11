﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using fiskaltrust.Middleware.Localization.QueueDE.IntegrationTest.SignProcessorDETests.Fixtures;
using Xunit;
using FluentAssertions;

namespace fiskaltrust.Middleware.Localization.QueueDE.IntegrationTest.SignProcessorDETests.Receipts
{
    public class OtherProcessTypeTests : IClassFixture<SignProcessorDependenciesFixture>
    {
        private readonly ReceiptTests _receiptTests;
        private readonly SignProcessorDependenciesFixture _fixture;
        public OtherProcessTypeTests(SignProcessorDependenciesFixture fixture)
        {
            _receiptTests = new ReceiptTests(fixture);
            _fixture = fixture;
        }
        [Fact]
        public async Task OtherProcessType_IsNoImplicitFlowAndOpenTrans_ExpectArgumentException() => await _receiptTests.ExpectArgumentExceptionReceiptReference(_receiptTests.GetReceipt("OrderProcessType", "OrderTypeNoImplNoOpen", 0x4445000000000014), "No transactionnumber found for cbReceiptReference '{0}'.").ConfigureAwait(false);

        [Fact]
        public async Task OtherProcessType_ValidRequestTraining_ExpectValidResponse()
        {
            var receiptRequest = _receiptTests.GetReceipt("OrderProcessType", "OtherProcessType_ValidRequestTrain", 0x4445000100020014);
            var signProcessor = _fixture.CreateSignProcessorForSignProcessorDE(false, DateTime.Now.AddHours(-1));
            var receiptResponse = await signProcessor.ProcessAsync(receiptRequest);
            await ReceiptTestResults.IsResponseValidAsync(_fixture, receiptResponse, receiptRequest, string.Empty).ConfigureAwait(false);
            receiptResponse.ftSignatures.Should().NotBeNull();
            var trainingsignature = receiptResponse.ftSignatures.Where(x => x.ftSignatureType.Equals(4096)).FirstOrDefault();
            trainingsignature.Should().NotBeNull();
            trainingsignature.Caption.Should().Be("Trainingsbuchung");
        }
    }
}
