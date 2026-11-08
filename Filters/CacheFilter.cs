using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
//using System.Collections.Concurrent;
using System.Text.Json;
namespace MyFirstApi.Filters;

public class CacheFilter : IAsyncResourceFilter
{
   // public static readonly ConcurrentDictionary<string, string> cache = new();
    private readonly IMemoryCache _cache;
    public CacheFilter(IMemoryCache cache)
    {
        _cache = cache;
    }
    public async Task OnResourceExecutionAsync(ResourceExecutingContext context, ResourceExecutionDelegate next)
    {

        var cacheKey = context.HttpContext.Request.Path.ToString();
        if(_cache.TryGetValue(cacheKey, out string? cachedResult))
        {
            context.Result = new ContentResult
            {
                Content = cachedResult,
                ContentType = "application/json",
                StatusCode = 200,
            };
            return;
        }
        var result = await next();
        if(result.Result is ObjectResult objectResult && objectResult.Value is not null)
        {
            var jsonResult = JsonSerializer.Serialize(objectResult.Value);
            _cache.Set(cacheKey, jsonResult,TimeSpan.FromMinutes(5));
        }
    }
}
