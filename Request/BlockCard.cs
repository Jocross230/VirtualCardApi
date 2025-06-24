namespace VirtualCard.Request
{
    public class BlockCard
    {
        public string accountNumber { get; set; }
        public string cardReference { get; set; }
    }
    public class root 
    {
        public BlockCard cards { get; set; }
    }
    
}
