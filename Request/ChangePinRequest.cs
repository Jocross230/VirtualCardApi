namespace VirtualCard.Request
{
    public class ChangePinRequest
    {
        public string cardReference { get; set; }
        public string accountNumber { get; set; }
        public string oldPin { get; set; }
        public string newPin { get; set; }
    }
}
