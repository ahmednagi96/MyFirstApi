namespace MyFirstApi.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string? PasswordHash { get; set; } = string.Empty;
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetExpiresOn { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public List<Address> PrimaryAddresses { get; set; } = new();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserLogin> UserLogins { get; set; } = new List<UserLogin>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}
