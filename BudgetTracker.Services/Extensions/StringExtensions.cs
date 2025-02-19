using System.Text.RegularExpressions;

namespace BudgetTracker.Services.Extensions
{
    public static class StringExtensions
    {
        public static int? GetFirstNumber(this string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            var parts = input.Split(' ', 2);
            var firstPart = parts[0];

            var match = Regex.Match(firstPart, @"^\d+");

            if (match.Success)
            {
                return int.Parse(match.Value);
            }

            return null;
        }
    }
}
