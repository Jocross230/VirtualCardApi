namespace VirtualCard.Request
{
    public class ResetPinRequest
    {
        public string accountNumber { get; set; }
        public string cardReference { get; set; }
    }
}
