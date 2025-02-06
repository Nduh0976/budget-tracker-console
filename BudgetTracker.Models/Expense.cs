using Newtonsoft.Json;

namespace BudgetTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public int BudgetId { get; set; }

        public int CategoryId { get; set; }

        [JsonIgnore]
        public virtual Category? Category { get; set; }

        public decimal Amount { get; set; }

        public DateTime Date { get; set; }

        public required string Description { get; set; }

        public override string ToString()
        {
            return $"{Id,-5} | {Description,-30} | {Category?.Name,-20} | {Date,-12:yyyy-MM-dd} | {Amount,-10:C}";
        }
    }
}
