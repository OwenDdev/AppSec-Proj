namespace SecureApp.API.Services
{
    public interface ICustomLogger
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}
