namespace MyFirstApi.Entities;

public class RefreshToken
{
    public int Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresOn { get; set; }
    public DateTime? RevokedOn { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string? CreatedByIp { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsRevoked => RevokedOn is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
