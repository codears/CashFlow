namespace CashierWorker.Models
{
    public class CashPostingMessage
    {
        public required decimal Amount { get; set; }
        public required string PostingType { get; set; }
        public required DateTimeOffset CreatedAt { get; set; }
    }
}
