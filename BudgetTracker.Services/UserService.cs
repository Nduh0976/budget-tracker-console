using BudgetTracker.Data;
using BudgetTracker.Models;

namespace BudgetTracker.Services
{
    public class UserService
    {
        public User ActiveUser { get; set; }

        private readonly DataStore _dataStore;

        public UserService()
        {
            ActiveUser = new User() { Name = string.Empty, Username = string.Empty};
            _dataStore = new DataStore();
        }

        public IEnumerable<User> GetUsers()
        {
            return _dataStore.GetUsers();
        }

        public Response<User> CreateUser(string? username, string? name)
        {
            if (string.IsNullOrWhiteSpace(username?.Trim()))
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Username cannot be empty or whitespace.",
                };
            }

            if (string.IsNullOrWhiteSpace(name?.Trim()))
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Name cannot be empty or whitespace.",
                };
            }

            if (!IsUsernameValid(username))
            {
                return new Response<User>
                {
                    Success = false,
                    Message = $"A user with the username '{username}' already exists.",
                };
            }
            
            var appUsers = GetUsers();

            var newId = appUsers.Any()
                ? appUsers.Max(u => u.Id) + 1
                : 1;

            var newUser = new User
            {
                Id = newId,
                Username = username,
                Name = name
            };

            _dataStore.AddUser(newUser);
            _dataStore.UpdateData();

            return new Response<User>
            {
                Success = true,
                Message = $"User '{username}' has been successfully created.",
                Data = newUser
            };
        }

        public Response<User> UpdateUser(string name)
        {
            if (string.IsNullOrWhiteSpace(name.Trim()))
            {
                return new Response<User>
                {
                    Success = false,
                    Message = "Username cannot be empty or whitespace.",
                };
            }

            var user = _dataStore.GetUserById(ActiveUser.Id);

            if (user != null)
            {
                user.Name = name;
                _dataStore.UpdateData();

                return new Response<User>
                {
                    Success = true,
                    Message = $"User '{user.Username}' has been successfully updated.",
                    Data = user
                };
            }

            return new Response<User>
            {
                Success = false,
                Message = "Username not found.",
            };
        }

        public void SetActiveUserById(int selectedUserId)
        {
            ActiveUser = _dataStore.GetUserById(selectedUserId)!;
        }

        public IList<string> GetUsersAsTable()
        {
            var userDescriptions = new List<string>();

            foreach (var user in GetUsers())
            {
                userDescriptions.Add(user.ToString());
            }

            return userDescriptions;
        }

        private bool IsUsernameValid(string username)
        {
            return !_dataStore.UserExists(username);
        }

        public bool RemoveUser()
        {
            return _dataStore.RemoveUser(ActiveUser.Id);
        }
    }
}
