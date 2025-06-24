using Newtonsoft.Json;
using System.Text;
using VisualCard.Model;


namespace VirtualCard.TokenResponses
{
    public class GenerateTokens
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<GenerateTokens> _logger;
        private readonly object _lock = new object();
        private static ResponseToken _cachedToken;
        private static DateTime _tokenExpiryTime;
        public GenerateTokens(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<GenerateTokens> logger)
        {
            _httpClient = httpClientFactory.CreateClient("TokenService");
            _config = config;
            _logger = logger;
        }

        public async Task<dynamic> GetToken()
        {
            if (_cachedToken != null && DateTime.Now < _tokenExpiryTime)
            {
                return _cachedToken;
            }

            var tokenUrl = _config["VirtualCardApi:BaseUrl"]; // Store token URL in appsettings
            var clientId = _config["OAuth:ClientId"];
            var clientSecret = _config["OAuth:ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new Exception("Client ID or Secret is missing in configuration.");
            }

            // Encode credentials as Base64
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            request.Headers.Add("Authorization", $"Basic {credentials}");
            request.Headers.Add("Accept", "application/json");

            var formContent = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("grant_type", "client_credentials"),
            new KeyValuePair<string, string>("scope", "profile")
        });

            request.Content = formContent;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to get token: {response.StatusCode} - {errorContent}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonConvert.DeserializeObject<ResponseToken>(responseString);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                throw new Exception("Invalid token response received.");
            }

            // Cache token with expiry time
            _cachedToken = tokenResponse;
            _tokenExpiryTime = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);

            return tokenResponse;
        }

        public async Task<ResponseToken> GetToken2()

        {
            if (_cachedToken != null && DateTime.Now < _tokenExpiryTime)
            {
                return _cachedToken;
            }

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://passport.interswitchng.com/passport/oauth/token");
            /*var clientId = "IKIA4790B5F64C1D2D46CD82F6883503CB8B582968DF";
            var clientSecret = "pbsDADiFkx3pXD1oG1g9o7M/y7xKj7vWpr4w0s7DFzA=";
            var byteArray = System.Text.Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
            var base64 = Convert.ToBase64String(byteArray);
            request.Headers.Add("Authorization", $"Basic {base64}");*/
            request.Headers.Add("Authorization", "Basic SUtJQTQ3OTBCNUY2NEMxRDJENDZDRDgyRjY4ODM1MDNDQjhCNTgyOTY4REY6cGJzREFEaUZreDNwWEQxb0cxZzlvN00veTd4S2o3dldwcjR3MHM3REZ6QT0=");
            request.Headers.Add("Cookie", "SESSION=d7ba8f94-833a-4254-9158-8d71ca8ee334");

            var collection = new List<KeyValuePair<string, string>>
    {
        new("grant_type", "client_credentials"),
        new("scope", "profile")
    };

            request.Content = new FormUrlEncodedContent(collection);

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Token request failed: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
            }

            var res = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<ResponseToken>(res);

            if (responseObject == null || string.IsNullOrEmpty(responseObject.AccessToken))
            {
                throw new Exception("Failed to deserialize access token response.");
            }

            // Cache token to avoid redundant requests
            _cachedToken = responseObject;
            _tokenExpiryTime = DateTime.Now.AddSeconds(responseObject.ExpiresIn - 60); // Buffer of 60 seconds

            return responseObject;


        }
       
    }
}
