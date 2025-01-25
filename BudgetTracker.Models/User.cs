namespace BudgetTracker.Models
{
    public class User
    {
        public int Id { get; set; }

        public required string Username { get; set; }

        public required string Name { get; set; }

        public bool Exists()
        {
            return Id > 0;
        }

        public override string ToString()
        {
            return $"{Id,-5} | {Username,-15} | {Name,-20}";
        }
    }
}
