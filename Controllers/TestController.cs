using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyFirstApi.Configuration;
using MyFirstApi.Data;
using MyFirstApi.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using PermissionConsts = MyFirstApi.Domain.Constants.Permissions;
using PermissionEntity = MyFirstApi.Entities.Permission;

namespace MyFirstApi.Controllers;
 

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{

    private readonly AppDbContext _context;
    public TestController(AppDbContext context)
    {
        _context = context;
    }
    [HttpGet(Name = "GetPersons")]
    public IActionResult Get()
    {
        var persons = Enumerable.Range(1, 10).Select(index =>
        new Person(index, $"Person {index}", index * 10)).ToList();
        return Ok(persons);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var person = await _context.Persons.FindAsync(id);
        if (person == null)
        {
            return NotFound($"Person with id {id} not found");
        }
        return Ok(person);
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Person person)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = person.Id }, person);
    }


    [HttpGet("get-configure-data")]
    public IActionResult GetConfigureData(IConfiguration configuration)
    {
        return Ok(configuration.GetValue<string>("Tool:Version"));
        //return Ok(configuration.GetSection("Tool").Get<Tool>().Version);
    }

    [HttpGet("get-sale-data/{price}")]
    public IActionResult GetSaleData(IOptionsSnapshot<Sale> sale, int price )
    {
        var discount = sale.Value.DiscountPercentage;
        var discountedPrice = price - (price * discount / 100);
        return Ok($"Discounted Price: {discountedPrice} Discount: {discount}");
    }

    [HttpGet("store-Permissions")]
    public async Task<IActionResult> StorePermissions(CancellationToken ct)
    {
        var permissionNames = typeof(PermissionConsts)
            .GetNestedTypes()
            .SelectMany(type => type.GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
            .Where(field => field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
            .Select(field => (string)field.GetValue(null)!)
            .ToList();

        var existing = await _context.Permissions
            .Where(p => permissionNames.Contains(p.Name))
            .Select(p => p.Name)
            .ToListAsync(ct);

        foreach (var name in permissionNames.Except(existing))
        {
            _context.Permissions.Add(new PermissionEntity
            {
                Name = name,
                Module = GetModule(name)
            });
        }

        await _context.SaveChangesAsync(ct);
        return Ok("Permissions stored successfully");
    }

    [NonAction]
    private static string GetModule(string permission)
    {
        var parts = permission.Split('.');
        return parts.Length > 1 ? parts[1] : parts[0];
    }

    [HttpGet("store-Roles")]
    public async Task<IActionResult> StoreRoles()
    {
        var roles = new List<Role>
        {
            new Role{
                Name = "Admin",
                Description = "Admin role"
            },
            new Role{
                Name = "User",
                Description = "User role"
            }
        };
        _context.Roles.AddRange(roles);
        await _context.SaveChangesAsync();
        return Ok("Roles stored successfully");
    }

    [Authorize]
[HttpGet("get-authorized-data")]
public async Task GetAuthorizedData()
{
    // الطريقة الاحترافية والمعيارية لإرجاع 409 Conflict
       await HttpContext.Response.WriteAsync("{\"error\": \"Too many requests. Slow down.\"}");
return;
    }

}

public record Person(int Id,string Name, int Age);
