using MyFirstApi.Enums;
namespace MyFirstApi.Entities;

public class Category
{
    public int Id { get; set; }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => _name = value.Trim();
    }
    private string _description = string.Empty;
    public string Description
    {
        get => string.IsNullOrEmpty(_description) ? "No description available" : _description;
        set => _description = value?.Trim() ?? string.Empty;
    }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<Tool> Tools { get; set; } = new List<Tool>();
    public CategoryType Type { get; set; } = CategoryType.All;

    
}
