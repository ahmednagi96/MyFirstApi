namespace MyFirstApi.Services;

public class LoggerService : ILoggerService
{
    public string LogInfo(string message)
    {
        return $"Info: {message}";
    }
}
