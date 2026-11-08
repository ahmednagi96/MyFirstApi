using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using MyFirstApi.Data;
using MyFirstApi.Entities;
using MyFirstApi;
using MyFirstApi.Filters;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MiddlewaresController(IStringLocalizer<SharedResources> localizer,AppDbContext _Context) : ControllerBase
{

    [HttpGet]
    [KeyHeaderFilter]
    public IActionResult Get([FromHeader(Name = "X-Request-Id")] string? requestKey)
    {
        return Ok("nagi-get" + requestKey);
    }

    [HttpPost]
    [KeyHeaderFilter]
    public IActionResult Post([FromBody] string body)
    {
        return Ok("nagi-post" + body);
    }

    [HttpGet("localization")]
    public IActionResult GetLocalization()
    {
        System.Threading.Thread.Sleep(1500);
        return Ok(localizer["Name"] + " " + localizer["Job"]);
    }


    [TypeFilter(typeof(LoggingFilter))]
    [HttpGet("logging")]
    public IActionResult GetLogging([FromQuery] string name ="ahmed test", [FromQuery]  int? age = 20)
    {
        return Ok("nagi-logging" + name + " " + age);
    }


    [HttpGet("cache")]
    [TypeFilter(typeof(CacheFilter))]
    public async Task<IActionResult> GetCache(CancellationToken ct)
    {
        var Categories=await _Context.Categories
        .ToListAsync(ct);
        return Ok(Categories);
    }

}
