namespace VirtualCard.Request
{
    public class AccountDetails
    {
        public string Nuban { get; set; }
        public string OldAccountNumber { get; set; }
        public string AccountName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public decimal? CurrentBalance { get; set; }
        public decimal? AvailableBalance { get; set; }
        public string Bvn { get; set; }
        public string AccountType { get; set; }
        public string Currency { get; set; }
        public string CurrencyCode { get; set; }
        public string AccountStatus { get; set; }
        public string BranchCode { get; set; }
        public string LedgerCode { get; set; }
        public string SubAccNum { get; set; }
        public string CustomerNumber { get; set; }
        public string Rest_Ind { get; set; }
        public string Tier { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime? DOB { get; set; }
        public string Email { get; set; }
        public string TypeOfDep { get; set; }
        public string AccountOfficer { get; set; }
        public string AccountrOfficerPhone { get; set; }
        public string AccountOfficerEmail { get; set; }
    }
}
