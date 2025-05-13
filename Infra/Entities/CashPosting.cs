namespace CashFlow.Infra.Entities
{
    public class CashPosting
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public decimal Amount { get; set; }

        public required string PostingType { get; set; }

        public string? Description { get; set; }
    }
}
