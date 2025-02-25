namespace VirtualCard.Model
{
    public class Response
    {
        public bool Successful { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
    }
    public class FetchCardsResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public List<VirtualCard> Cards { get; set; }
    }
    public class VirtualCard
    {
        public string CardId { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryDate { get; set; }
        public string CardStatus { get; set; }
    }

}
