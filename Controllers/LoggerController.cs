using Microsoft.AspNetCore.Mvc;
using MyFirstApi.Services;
namespace MyFirstApi.Controllers;

[ApiController]
[Route("[controller]")]
public class LoggerController : ControllerBase
{
    private readonly ILoggerService _loggerService;// backing field
    private readonly TrackService _trackService;
    public LoggerController(ILoggerService loggerService, TrackService trackService)// constructor injection
    {
        _loggerService = loggerService;
        _trackService = trackService;
    } 
    [HttpGet(Name = "GetLogger")]
    public IActionResult Get()
    {
        string msg1 = _loggerService.LogInfo("test 1");
        string msg2 = _trackService.TrackInfo("test 2");
        return Ok(new { msg1, msg2});
    }
}