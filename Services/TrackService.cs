namespace MyFirstApi.Services;

public class TrackService
{
    private readonly ILoggerService _loggerService;

    public TrackService(ILoggerService loggerService)
    {
        _loggerService = loggerService;
    }

    public string TrackInfo(string message)
    {
        _loggerService.LogInfo(message);
        return message;
    }
}
