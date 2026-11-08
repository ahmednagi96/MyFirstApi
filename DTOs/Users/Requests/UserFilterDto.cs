using System.ComponentModel.DataAnnotations;

namespace MyFirstApi.DTOs.Users.Requests;

public class UserFilterDto
{
    public string? Search { get; set; }
    public string? Role { get; set; }
    public string? Status { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;
}
