using Newtonsoft.Json;

namespace BudgetTracker.Data
{
    public class DataStore
    {
        private readonly string _filePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\BudgetTracker.Console\bin\Debug\net8.0\", @"\BudgetTracker.Data\ApplicationData.json");

        public ApplicationData ApplicationData { get; set; }

        public DataStore()
        {
            ApplicationData = new ApplicationData();
        }

        public void LoadData()
        {
            if (File.Exists(_filePath))
            {
                var jsonData = File.ReadAllText(_filePath);
                var appData = JsonConvert.DeserializeObject<ApplicationData>(jsonData);

                if (appData != null)
                {
                    ApplicationData = appData;
                }
            }
            else
            {
                Console.WriteLine("Data file not found.");
            }
        }
    }
}
