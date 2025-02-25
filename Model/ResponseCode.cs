using Newtonsoft.Json;

namespace VisualCard.Model
{
    public class ResponseCode
    {
        [JsonProperty("access_token")] // Match JSON field name
    public string AccessToken { get; set; }

    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; } // In seconds
}
}
