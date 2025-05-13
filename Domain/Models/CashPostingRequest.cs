namespace CashFlow.Domain.Models
{
    public class CashPostingRequest
    {
        public decimal Amount { get; set; }
        public required string PostingType { get; set; }
        public string? Description { get; set; }
    }
}
