namespace VisualCard.Model
{
    public class CardStatusRequest
    {
        public string cardReference { get; set; }
    }

    public class EncryptRequest
    {
        public string Request { get; set; }
    }

    public class EncryptResponse
    {
        public string Response { get; set; }
    }
}
