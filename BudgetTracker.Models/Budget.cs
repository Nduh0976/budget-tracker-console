namespace BudgetTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public required string Name { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Amount { get; set; }
    }
}
