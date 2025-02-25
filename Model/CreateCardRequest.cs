namespace VisualCard.Model
{
    public class CreateCardRequest
    {
        public string? lastName { get; set; }
        public string? city { get; set; }
        public string? accountType { get; set; }
        public string? postalCode { get; set; }
        public string? streetAddressLine2 { get; set; }
        public string? userId { get; set; }
        public string? mobileNr { get; set; }
        public string? cardProgram { get; set; }
        public string? firstName { get; set; }
        public string? accountId { get; set; }
        public string? emailAddress { get; set; }
        public string? nameOnCard { get; set; }
        public string? streetAddress { get; set; }
        public string? countryCode { get; set; }
        public string? issuerNr { get; set; }
        public string? state { get; set; }
        public string? currencyCode { get; set; }
        public string? alias { get; set; }
        public string? clientReference { get; set; }
    }
    public class CreateCardResponse 
    {
        public string alias { get; set; }
        public string clientReference { get; set; }
        public string cardReference { get; set; }
        public string accountNumber { get; set; }
        public string pan { get; set; }
        public string seqNr { get; set; }
        public string expiryDate { get; set; }
        public string pinOffset { get; set; }
        public string cvv { get; set; }
        public string cvv2 { get; set; }
        public string pinInfo { get; set; }
        public string track2 { get; set; }
        public string customerId { get; set; }
        public string defaultAccountType { get; set; }
        public string blocked { get; set; }
        public string failedPinAttempts { get; set; }
        public string creationChannel { get; set; }

    }

}
