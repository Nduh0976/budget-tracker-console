using BudgetTracker.Services.Interfaces;

namespace BudgetTracker.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly string _logDirectory;
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();

        public LoggingService()
        {
            var filePath = AppDomain.CurrentDomain.BaseDirectory.Replace(@"\BudgetTracker.Console\bin\Debug\net8.0\", @"\BudgetTracker.Data\");
            _logDirectory = Path.Combine(filePath, "Logs");
            _logFilePath = Path.Combine(_logDirectory, $"BudgetTracker_{DateTime.Now:yyyy-MM-dd}.log");

            // Ensure log directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }
        }

        public void LogError(string message, Exception? exception = null)
        {
            var logMessage = FormatLogMessage("ERROR", message);

            if (exception != null)
            {
                logMessage += $"\nException: {exception.Message}";
                logMessage += $"\nStack Trace: {exception.StackTrace}";
            }

            WriteToFile(logMessage);
        }

        public void LogWarning(string message)
        {
            WriteToFile(FormatLogMessage("WARNING", message));
        }

        public void LogInfo(string message)
        {
            WriteToFile(FormatLogMessage("INFO", message));
        }

        private string FormatLogMessage(string level, string message)
        {
            return $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
        }

        private void WriteToFile(string message)
        {
            try
            {
                lock (_lockObject)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
                Console.WriteLine($"Original message: {message}");
            }
        }
    }
} 