namespace VisualCard.Model
{
    public class GetStatementRequest
    {
        public string cardReference { get; set; }
        public string fromDate { get; set; }
        public string toDate { get; set; }
        public int tranCount { get; set; }
        public int reference { get; set; }
        public int forward { get; set; }
        public int ordering { get; set; }
    }
}
