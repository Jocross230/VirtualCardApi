namespace VirtualCard.Dtos
{
    public class CreatedCard
    {
        public int Id { get; set; }
        public string? alias { get; set; }
        public string? clientReference { get; set; }
        public string? cardReference { get; set; }
        public string? accountNumber { get; set; }
        public string? pan { get; set; }
        public string? seqNr { get; set; }
        public string? expiryDate { get; set; }
        public string? pinOffset { get; set; }
        public string? cvv { get; set; }
        public string? cvv2 { get; set; }
        public string? pinInfo { get; set; }
        public string? track2 { get; set; }
        public string? customerId { get; set; }
        public string? defaultAccountType { get; set; }
        public bool? blocked { get; set; }
        public int? failedPinAttempts { get; set; }
        public string? creationChannel { get; set; }
    }
}
