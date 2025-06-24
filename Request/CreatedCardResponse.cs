namespace VirtualCard.Request
{
    
    public class Roots
    {
        public bool successful { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public string defaultPin { get; set; }
        public CreatedCard card { get; set; }
        public BlockCard cards { get; set; }
    }
}
