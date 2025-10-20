namespace VirtualCard.Request
{
    public class ChangePinResponse
    {
        public string id { get; set; }
        public string cardReference { get; set; }
        public string accountNumber { get; set; }
        public string oldPin { get; set; }
        public string newPin { get; set; }
    }
}
