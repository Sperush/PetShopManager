using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using PetShop.DTO;

namespace PetShop.Services
{
    public class PaymentService
    {
        private readonly HttpClient _http;
        private readonly CommissionService _settings;

        public PaymentService(HttpClient http, CommissionService settings)
        {
            _http = http;
            _settings = settings;
        }

        public async Task<string?> CreatePaymentLinkAsync(long orderCode, int amount, string description, string baseUrl)
        {
            var clientId = await _settings.GetSettingAsync("payos_client_id");
            var apiKey = await _settings.GetSettingAsync("payos_api_key");
            var checksumKey = await _settings.GetSettingAsync("payos_checksum_key");

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(checksumKey))
                return null;

            var returnUrl = $"{baseUrl}/payment-success?orderCode={orderCode}";
            var cancelUrl = $"{baseUrl}/payment-cancel?orderCode={orderCode}";


            var signatureData = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";
            var signature = CalculateHmac(signatureData, checksumKey);

            var requestBody = new
            {
                orderCode = orderCode,
                amount = amount,
                description = description,
                cancelUrl = cancelUrl,
                returnUrl = returnUrl,
                signature = signature
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api-merchant.payos.vn/v2/payment-requests");
            request.Headers.Add("x-client-id", clientId);
            request.Headers.Add("x-api-key", apiKey);
            request.Content = JsonContent.Create(requestBody);

            var response = await _http.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PayOSResponse>();
                return result?.data?.checkoutUrl;
            }

            return null;
        }

        private string CalculateHmac(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }

    public class PayOSResponse
    {
        public string code { get; set; }
        public string desc { get; set; }
        public PayOSData data { get; set; }
    }

    public class PayOSData
    {
        public string checkoutUrl { get; set; }
    }
}

