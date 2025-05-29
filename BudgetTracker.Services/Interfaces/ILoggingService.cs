namespace BudgetTracker.Services.Interfaces
{
    public interface ILoggingService
    {
        void LogError(string message, Exception? exception = null);

        void LogWarning(string message);

        void LogInfo(string message);
    }
} 