namespace BudgetTracker.Models
{
    public class Category
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public override string ToString()
        {
            return $"{Id,-5} | {Name,-30}";
        }
    }
}
