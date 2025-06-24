


namespace VirtualCard.Request
{
    public class CreatedCard
    {
        public int Id { get; set; }
        public string alias { get; set; }
        public string clientReference { get; set; }
        public string cardReference { get; set; }
        public string accountNumber { get; set; }
        public string pan { get; set; }
        public string seqNr { get; set; }
        public string issuerNr { get; set; }
        public string userId { get; set; }
        public string pinOffset { get; set; }
        public string customerId { get; set; }
        public string defaultAccountType { get; set; }
        public bool blocked { get; set; }
        public int failedPinAttempts { get; set; }
        public string creationChannel { get; set; }
    }
    public class Root
    {
        public bool successful { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string defaultPin { get; set; }
        public CreatedCard card { get; set; }
       // public BlockCard cards { get; set; }
    }
}
