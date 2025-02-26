using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Ocsp;
using RestSharp;
using System.Net.Http.Headers;
using System.Text;
using VirtualCard.Model;
using VirtualCard.TokenResponses;
using VisualCard.Helper;
using VisualCard.Interface;
using VisualCard.Model;
using RestClientOptions = RestSharp.RestClientOptions;

namespace VisualCard.Services
{
    public class VirtualCardServices : IVirtualCard
    {
        private readonly string _encryptionKey;


        public readonly HttpClient _httpClient;
        public readonly ILogger<VirtualCardServices> _logger;

        private readonly IConfiguration _configuration;
        private readonly ICryptoUtils _cryptoUtils;
        private readonly GenerateTokens _generateToken;
        private readonly RestClient _client;

        public VirtualCardServices(ILogger<VirtualCardServices> logger, GenerateTokens generateToken, IConfiguration configuration, HttpClient httpClient, ICryptoUtils cryptoUtils)
        {
            _logger = logger;
            _configuration = configuration;
            _httpClient = httpClient;
            _cryptoUtils = cryptoUtils;
            _generateToken = generateToken;
            _encryptionKey = _configuration["AppSettings:enc_key"];
            var options = new RestClientOptions(_configuration.GetValue<string>("BaseUrl"))
            {
                MaxTimeout = -1
            };
            _client = new RestClient(options);
        }

        public async Task<String> BlockCardAsync(BlockCardRequest req)
        {
           

            // Serialize the request
            var json = JsonConvert.SerializeObject(req);
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // 3. Validate Base64 encoding
            

            // 4. Retrieve a fresh access to
            var tokenResponse = await _generateToken.GetToken2();

            // 5. Create HTTP client and request
            var client = new RestClient("https://virtualcard-middleware-isw.k8.isw.la");
            var request = new RestRequest("/virtualcard/api/v1/cardBlock", Method.Post) // Corrected endpoint
                .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                .AddHeader("Content-Type", "application/json")
                .AddHeader("Accept", "application/json")
                .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                .AddBody(encryptedData);

            // 6. Execute the request
            RestResponse response = await client.ExecuteAsync(request);

            // 7. Handle errors
            if (!response.IsSuccessful)
            {
                string errorMessage = response.Content ?? "Unknown error occurred.";
                throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
            }

            // var jsonresponse = await response.Content.ReadAsStringAsync();
            // string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            //return decryptdata;

            string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

            return decryptedData;


        }

        public async Task<string> ChangeCardPinAsync(CardPinChangeRequest req)
        {

            var json = JsonConvert.SerializeObject(req);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/pinChange";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responses = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(responses, _configuration["AppSettings:enc_key"]);
            return decryptdata;

        }

        public async Task<string> ResetCardPinAsync(CardPinResetRequest req)
        {
            var json = JsonConvert.SerializeObject(req);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/pinReset";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonresponse = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            return decryptdata;

            //return await response.Content.ReadAsStringAsync();

        }

        public async Task<string> GetCardStatusAsync(CardStatusRequest request)
        {
            var json = JsonConvert.SerializeObject(request);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/cardStatus";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonresponse = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            return decryptdata;
        }
        
        public async Task<string> CreateCardAsync(CreateCardRequest req)
        {
            try
            {
                // Generate a unique client reference
                var clientReference = Guid.NewGuid().ToString();
                req.GetType().GetProperty("clientReference")?.SetValue(req, clientReference);

                // Serialize the request
                var json = JsonConvert.SerializeObject(req);

                // Encrypt the request data
                string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

                // Validate Base64 format
                if (!IsBase64String(encryptedData))
                    throw new Exception("Encryption error: Encrypted data is not a valid Base64 string.");

                // Get a fresh access token
                var tokenResponse = await _generateToken.GetToken2();

                // Create the HTTP client and request
                var client = new RestClient("https://virtualcard-middleware-isw.k8.isw.la");
                var request = new RestRequest("/virtualcard/api/v1/createCard", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);  // Ensure proper formatting

                // Send the request
                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                // Decrypt the response data
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                return decryptedData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Virtual Card Creation Failed: {ex.Message}");
            }
            



        }
        private bool IsBase64String(string base64)
        {
            try
            {
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }



        public async Task<string> FetchCardsByCreationChannelAsync(FetchCardsByCreationChannelRequest request)
        {
            var json = JsonConvert.SerializeObject(request);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/fetchCardsByChannel";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonresponse = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            return decryptdata;
        }

        public async Task<String> FetchCardExcludedAsync(FetchCardRequest req)
        {
            try
            {
                // Generate a unique client reference
                //var clientReference = Guid.NewGuid().ToString();
                //req.GetType().GetProperty("clientReference")?.SetValue(req, clientReference);

                // Serialize the request
                var json = JsonConvert.SerializeObject(req);

                // Encrypt the request data
                string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

                // Validate Base64 format
                if (!IsBase64String(encryptedData))
                    throw new Exception("Encryption error: Encrypted data is not a valid Base64 string.");

                // Get a fresh access token
                var tokenResponse = await _generateToken.GetToken2();

                // Create the HTTP client and request
                var client = new RestClient("https://virtualcard-middleware-isw.k8.isw.la");
                var request = new RestRequest("/virtualcard/api/v1/fetchCards", Method.Post)
                    .AddHeader("IssuerID", _configuration["AppSettings:IssuerID"])
                    .AddHeader("Content-Type", "application/json")
                    .AddHeader("Accept", "application/json")
                    .AddHeader("Authorization", $"Bearer {tokenResponse.AccessToken.Trim()}")
                    .AddHeader("ChannelID", _configuration["AppSettings:ChannelID"])
                    .AddBody(encryptedData);  // Ensure proper formatting

                // Send the request
                RestResponse response = await client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    string errorMessage = response.Content ?? "Unknown error occurred.";
                    throw new Exception($"Error: {response.StatusCode}, Details: {errorMessage}");
                }

                // Decrypt the response data
                string decryptedData = _cryptoUtils.DecryptData(response.Content, _configuration["AppSettings:enc_key"]);

                return decryptedData;
            }
            catch (Exception ex)
            {
                throw new Exception($"Virtual Card Creation Failed: {ex.Message}");
            }
        }


        public async Task<string> FetchCardIncludedAsync(FetchCardRequest1 req)
        {
            var json = JsonConvert.SerializeObject(req);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/fetchCards";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonresponse = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            return decryptdata;
        }

        public async Task<string> GetStatementAsync(GetStatementRequest request)
        {
            var json = JsonConvert.SerializeObject(request);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/statement";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonresponse = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            return decryptdata;
        }

        public async Task<string> UnblockCardAsync(UnBlockCardRequest request)
        {
            var json = JsonConvert.SerializeObject(request);

            // Encrypt the request data and ensure proper encoding
            string encryptedData = _cryptoUtils.EncryptData(json, _configuration["AppSettings:enc_key"]);

            // Validate Base64 format


            // Get a fresh access token
            var tokenResponse = await _generateToken.GetToken2();
            var url = $"{_configuration.GetValue<string>("BaseUrl")}/virtualcard/api/v1/cardUnblock";

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, url);

            // Set required headers
            httpRequest.Headers.Add("IssuerID", _configuration["AppSettings:IssuerID"]);
            httpRequest.Headers.Add("Accept", "application/json");
            httpRequest.Headers.Add("Authorization", $"Bearer {tokenResponse.AccessToken}");
            httpRequest.Headers.Add("ChannelID", _configuration["AppSettings:ChannelID"]);

            // Set request content FIRST before modifying headers
            httpRequest.Content = new StringContent(encryptedData, Encoding.UTF8, "text/plain");

            // Now modify Content-Type correctly
            httpRequest.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain"); // ✅ Correct placement

            // Send the request
            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var jsonresponse = await response.Content.ReadAsStringAsync();
            string decryptdata = _cryptoUtils.DecryptData(jsonresponse, _configuration["AppSettings:enc_key"]);
            return decryptdata;
        }
    }
}

