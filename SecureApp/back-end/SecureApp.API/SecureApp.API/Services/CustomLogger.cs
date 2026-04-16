namespace SecureApp.API.Services
{
    public class CustomLogger : ICustomLogger
    {
        private readonly string _filePath = "logs.txt";

        public void Log(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARNING", message);
        }

        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        private void WriteLog(string level, string message)
        {
            var logMessage = $"{DateTime.Now} [{level}] {message}";

            File.AppendAllText(_filePath, logMessage + Environment.NewLine);
            Console.WriteLine(logMessage);
        }
    }
}
