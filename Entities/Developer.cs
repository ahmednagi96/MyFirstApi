namespace MyFirstApi.Entities;
public class Developer
{
    public int Id { get; set; }
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public Address MainAddress { get; set; } = default!;
}