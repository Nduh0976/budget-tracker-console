using BudgetTracker.Models;
using Newtonsoft.Json;

namespace BudgetTracker.Data
{
    public class DataStore
    {
        private readonly string _filePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\BudgetTracker.Console\bin\Debug\net8.0\", @"\BudgetTracker.Data\ApplicationData.json");

        private ApplicationData _applicationData { get; set; }

        public DataStore()
        {
            _applicationData = new ApplicationData();
            LoadData();
        }

        public void AddUser(User newUser)
        {
            _applicationData.Users.Add(newUser);
        }

        public IEnumerable<User> GetUsers()
        {
            return _applicationData.Users;
        }
        
        public User? GetUserById(int userId)
        {
            return _applicationData.Users.FirstOrDefault(u => u.Id == userId);
        }

        public bool UserExists(string username)
        {
            return _applicationData.Users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateData()
        {
            var jsonData = JsonConvert.SerializeObject(_applicationData, Formatting.Indented);
            File.WriteAllText(_filePath, jsonData);
        }

        private void LoadData()
        {
            if (File.Exists(_filePath))
            {
                var jsonData = File.ReadAllText(_filePath);
                var appData = JsonConvert.DeserializeObject<ApplicationData>(jsonData);

                if (appData != null)
                {
                    _applicationData = appData;
                }
            }
            else
            {
                Console.WriteLine("Data file not found.");
            }
        }
    }
}
