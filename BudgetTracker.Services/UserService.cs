using BudgetTracker.Data.Interfaces;
using BudgetTracker.Models;
using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class UserService : IUserService
    {
        private User activeUser { get; set; }

        private readonly IDataStore _dataStore;

        public UserService(IDataStore dataStore)
        {
            activeUser = new User() { Name = string.Empty, Username = string.Empty};
            _dataStore = dataStore;
        }
        
        public bool ActiveUserExists()
        {
            return activeUser != null && activeUser.Exists();
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

            var newUser = new User
            {
                Id = _dataStore.GenerateNextUserId(),
                Username = username,
                Name = name
            };

            _dataStore.AddUser(newUser);

            return new Response<User>
            {
                Success = true,
                Message = $"User '{username}' has been successfully created.",
                Data = newUser
            };
        }

        public int GetActiveUserId()
        {
            return activeUser.Id;
        }

        public string GetActiveUserName()
        {
            return activeUser.Name;
        }
        
        public IEnumerable<User> GetUsers()
        {
            return _dataStore.GetUsers();
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
        
        public bool RemoveUser()
        {
            return _dataStore.RemoveUser(activeUser.Id);
        }

        public void SetActiveUserById(int selectedUserId)
        {
            activeUser = _dataStore.GetUserById(selectedUserId)!;
        }

        public void SetActiveUser(User user)
        {
            activeUser = user;
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

            var user = _dataStore.GetUserById(activeUser.Id);

            if (user != null)
            {
                _dataStore.UpdateNameOfUser(user.Id, name);

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

        private bool IsUsernameValid(string username)
        {
            return !_dataStore.UserExists(username);
        }
    }
}