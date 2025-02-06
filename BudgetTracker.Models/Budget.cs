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

        public bool Exists()
        {
            return Id > 0;
        }

        public override string ToString()
        {
            return $"{Id,-5} | {Name,-30} | {StartDate,-12:yyyy-MM-dd} | {EndDate,-12:yyyy-MM-dd} | {Amount,-10:C}";
        }
    }
}
