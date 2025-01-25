namespace BudgetTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public int BudgetId { get; set; }

        public int CategoryId { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public required string Description { get; set; }
    }
}
