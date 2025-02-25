namespace VisualCard.Model
{
    public class CardPinChangeRequest
    {
        public string cardReference { get; set; }
        public string accountNumber { get; set; }
        public string oldPin { get; set; }
        public string newPin { get; set; }
    }
}
