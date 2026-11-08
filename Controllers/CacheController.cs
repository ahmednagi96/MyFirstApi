using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using MyFirstApi.Data;
using MyFirstApi.Entities;
using System.Text.Json;

namespace MyFirstApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CacheController(IMemoryCache cache,IDistributedCache distributedCache,AppDbContext context) : ControllerBase
{
    [HttpGet]
    public IActionResult CacheTest()
    {
        const string cacheKey = "test";

        if (cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return Ok(new
            {
                source = "Get",
                value = cachedValue
            });
        }

        const string value = "Cache Test";
        cache.Set(cacheKey, value, TimeSpan.FromMinutes(5));

        return Ok(new
        {
            source = "set",
            value
        });
    }
    [HttpGet("test")]
    public async Task<IActionResult> GetFavouritePosts()
    {
        var value=await cache.GetOrCreateAsync("favourite-posts", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await context.Posts.Select(p => new { p.Id, p.Title, p.Content }).ToListAsync();
        });
        return Ok(new
        {
            source = "cache",
            value
        });
    }

    [HttpGet("distributed")]
    public async Task<IActionResult> GetCachedCategories()
    {
        const string cacheKey = "expensive_categories_list";
        var cachedData = await distributedCache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
        {
            // لو البيانات موجودة في الكاش، رجعها فوراً (سرعة خارقة ومن غير ما نكلم الداتا بيز!)
            var productsFromCache = JsonSerializer.Deserialize<List<Category>>(cachedData);
            return Ok(new { Source = "From Redis Cache 🚀", Data = productsFromCache });
        }
        var categories = await context.Categories.ToListAsync();
        await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(categories), new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });
        return Ok(new { Source = "From Database", Data = categories });
    }
    [HttpGet("session/set")]
    public IActionResult SetSessionData([FromQuery] string userName)
    {
        // حفظ قيمة جوه السشن
        HttpContext.Session.SetString("UserFullName", userName);
        HttpContext.Session.SetInt32("LoginTime", (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        return Ok($"Session saved successfully for: {userName}");
    }
    [HttpGet("session/get")]
    public IActionResult GetSessionData()
    {
        // استرجاع القيمة من السشن
        var userName = HttpContext.Session.GetString("UserFullName");
        if (string.IsNullOrEmpty(userName))
            return NotFound("No active session found!");

        return Ok(new { Message = $"Hello {userName}, your session is active via Redis!" });
    }
}
