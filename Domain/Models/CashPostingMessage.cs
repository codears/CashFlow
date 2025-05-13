using System.ComponentModel.DataAnnotations;

namespace CashierWorker.Models
{
    public class CashPostingMessage
    {
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string PostingType { get; set; }
        [Required]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
