using BudgetTracker.Models;

namespace BudgetTracker.Services.Interfaces
{
    public interface IUserService
    {
        bool ActiveUserExists();

        Response<User> CreateUser(string? username, string? name);

        int GetActiveUserId();

        string GetActiveUserName();

        IEnumerable<User> GetUsers();

        IList<string> GetUsersAsTable();

        bool RemoveUser();

        void SetActiveUserById(int selectedUserId);

        void SetActiveUser(User user);

        Response<User> UpdateUser(string name);
    }
}