using Newtonsoft.Json;
using System.Text;
using VirtualCard.Model;
using VirtualCard.Dtos;


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

        public async Task<ResponseToken> GetToken2()
        {
            
            if (_cachedToken != null && DateTime.Now < _tokenExpiryTime)
            {
                _logger.LogInformation("Using cached OAuth token, valid until {Expiry}", _tokenExpiryTime);
                return _cachedToken;
            }

           
            var tokenUrl = _config["VirtualCardApi:TokenUrl"];
            var base64Auth = _config["OAuth:Authorization"]; 
            var cookieValue = _config["OAuth:Cookie"];       

            if (string.IsNullOrEmpty(tokenUrl) || string.IsNullOrEmpty(base64Auth))
            {
                _logger.LogError("Token URL or Authorization header missing in configuration.");
                throw new Exception("Missing Token URL or Authorization key in configuration.");
            }

            using var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl);

            
            request.Headers.Add("Authorization", base64Auth);
            request.Headers.Add("Accept", "application/json");

            if (!string.IsNullOrEmpty(cookieValue))
                request.Headers.Add("Cookie", cookieValue);

            
            request.Content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("grant_type", "client_credentials"),
        new KeyValuePair<string, string>("scope", "profile")
    });

            try
            {
                _logger.LogInformation("Requesting new OAuth token from {Url}", tokenUrl);
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Token request failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    throw new Exception($"Failed to get token: {response.StatusCode} - {errorContent}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<ResponseToken>(responseString);

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    _logger.LogError("Invalid or empty token response received.");
                    throw new Exception("Invalid token response received.");
                }

                
                _cachedToken = tokenResponse;
                _tokenExpiryTime = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn - 60);

                _logger.LogInformation("New OAuth token acquired, expires at {Expiry}", _tokenExpiryTime);
                return tokenResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving OAuth token.");
                throw;
            }
        }

    }
}
