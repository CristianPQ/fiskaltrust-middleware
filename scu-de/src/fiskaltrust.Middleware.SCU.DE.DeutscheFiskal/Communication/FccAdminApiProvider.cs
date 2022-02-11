﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using fiskaltrust.Middleware.SCU.DE.DeutscheFiskal.Communication.Helpers;
using fiskaltrust.Middleware.SCU.DE.DeutscheFiskal.Constants;
using fiskaltrust.Middleware.SCU.DE.DeutscheFiskal.Exceptions;
using fiskaltrust.Middleware.SCU.DE.DeutscheFiskal.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace fiskaltrust.Middleware.SCU.DE.DeutscheFiskal.Communication
{
    public class FccAdminApiProvider
    {
        private readonly DeutscheFiskalSCUConfiguration _configuration;
        private readonly Uri _baseAddress;

        public FccAdminApiProvider(DeutscheFiskalSCUConfiguration configuration)
        {
            _configuration = configuration;
            _baseAddress = FccUriHelper.GetFccUri(configuration);
        }

        public async Task<List<ClientResponseDto>> GetClientsAsync()
        {
            using var client = GetBasicAuthAdminClient();
            var response = await client.GetAsync("clients");
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<ClientResponseDto>>(responseContent);
            }

            throw new FiskalCloudException($"Communication error ({response.StatusCode}) while getting registered clients (GET /clients). Response: {responseContent}", 
                (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), "GET /clients");
        }

        public async Task CreateClientAsync(string clientId)
        {
            var request = new CreateClientRequestDto
            {
                ClientId = clientId,
                ErsIdentifier = clientId,
                RegistrationToken = _configuration.ActivationToken,
                BriefDescription = clientId,
                TypeOfSystem = "Default"
            };

            using var client = GetBasicAuthAdminClient();
            var requestContent = JsonConvert.SerializeObject(request);
            var response = await client.PostAsync("registration", new StringContent(requestContent, Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                throw new FiskalCloudException($"Communication error ({response.StatusCode}) while registering client (POST /registration). Request: '{requestContent}', Response: '{responseContent}'", 
                    (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), "POST /registration");
            }
        }

        public async Task<FccInfoResponseDto> GetFccInfoAsync()
        {
            using var client = new HttpClient
            {
                BaseAddress = _baseAddress
            };

            var response = await client.GetAsync("info");
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FccInfoResponseDto>(responseContent);
            }

            throw new FiskalCloudException($"Communication error ({response.StatusCode}) while requesting FCC info (GET /info). Response: '{responseContent}'", 
                (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), "GET /info");
        }

        public async Task<SelfCheckResponseDto> GetSelfCheckResultAsync()
        {
            using var client = GetOAuthAdminClient();
            var response = await client.GetAsync("selfcheck");
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<SelfCheckResponseDto>(responseContent);
            }

            throw new FiskalCloudException($"Communication error ({response.StatusCode}) while getting self check result (GET /selfcheck). Response: '{responseContent}'", 
                (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), "GET /selfcheck");
        }

        public async Task<TssDetailsResponseDto> GetTssDetailsAsync()
        {
            using var client = GetOAuthAdminClient();
            var response = await client.GetAsync("tssdetails");
            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<TssDetailsResponseDto>(responseContent);
            }

            throw new FiskalCloudException($"Communication error ({response.StatusCode}) while getting TSS details (GET /tssdetails). Response: '{responseContent}'", 
                (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), "GET /tssdetails");
        }

        public async Task<byte[]> ExportSingleTransactionAsync(ulong transactionNumber)
        {
            using var client = GetOAuthAdminClient();
            var response = await client.GetAsync($"export/transactions/{transactionNumber}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Convert.FromBase64String(responseContent);
            }

            throw new FiskalCloudException($"Communication error ({response.StatusCode}) while exporting single transaction (GET /export/transactions/{transactionNumber}). Response: '{responseContent}'", 
                (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), $"GET /export/transactions/{transactionNumber}");
        }

        public async Task RequestExportAsync(string targetFile, DateTime startDate, DateTime endDate, string clientId = null)
        {
            var url = $"export/transactions/time?startDate={startDate:yyyy-MM-dd'T'HH:mm:ss'Z'}&endDate={endDate:yyyy-MM-dd'T'HH:mm:ss'Z'}";
            if (clientId != null)
            {
                url += $"&clientId={clientId}";
            }

            var response = await GetOAuthAdminClient().GetAsync(url);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                File.WriteAllBytes(targetFile, Array.Empty<byte>());
                return;
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                File.WriteAllBytes(targetFile, Convert.FromBase64String(responseContent));
            }
            else
            {
                throw new FiskalCloudException($"Communication error ({response.StatusCode}) while requesting export (GET {url}). Response: '{responseContent}", 
                    (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), $"GET /{url}");
            }
        }

        public async Task AcknowledgeAllTransactionsAsync(DateTime startDate, DateTime endDate, string clientId = null)
        {
            var url = $"export/transactions/time?startDate={startDate:yyyy-MM-dd'T'HH:mm:ss'Z'}&endDate={endDate:yyyy-MM-dd'T'HH:mm:ss'Z'}";
            if (clientId != null)
            {
                url += $"&clientId={clientId}";
            }

            var response = await GetOAuthAdminClient().PostAsync(url, new StringContent("ACK", Encoding.UTF8, "text/plain"));
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
            {
                throw new FiskalCloudException($"Communication error ({response.StatusCode}): {responseContent}", 
                    (int) response.StatusCode, ErrorHelper.GetErrorType(responseContent), $"POST /{url}");
            }
        }

        private HttpClient GetBasicAuthAdminClient()
        {
            var client = new HttpClient { BaseAddress = _baseAddress };
            var credentials = Encoding.ASCII.GetBytes($"admin:{_configuration.ErsCode}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));

            return client;
        }

        private HttpClient GetOAuthAdminClient()
        {
            var clientConfig = new ClientConfiguration
            {
                BaseAddress = _baseAddress,
                UserName = "admin-auth-client-id",
                Password = "admin-auth-client-secret",
                GrantType = "password",
                AdditionalProperties = new Dictionary<string, string>
                {
                    { "username", "admin" },
                    { "password", _configuration.ErsCode },
                }
            };
            return new HttpClient(new AuthenticatedHttpClientHandler(clientConfig))
            {
                BaseAddress = _baseAddress
            };
        }
    }
}
