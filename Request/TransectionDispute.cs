using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VirtualCard.Request
{
    public class TransectionDispute
    {
        [Key] // Primary key for database
        [JsonIgnore] // Hides from Swagger and API responses
        public int Id { get; set; }
        public long AccountNumber { get; set; }
        public string TransactionId { get; set; }
        public string Subject { get; set; }
        public string From { get; set; } = "notifications@suntrustng.com";
        public string To { get; set; } 
        public string Message { get; set; }
        public string? Bcc { get; set; }
        public string? Cc { get; set; }
       
    }
}


