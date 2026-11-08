namespace MyFirstApi.Entities;

public record Tool
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = string.Empty;

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;
}