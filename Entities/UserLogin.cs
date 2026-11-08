namespace MyFirstApi.Entities;

public class UserLogin
{
    public int Id { get; set; }

    // اسم المزود الخارجي مثل: "Google", "Facebook", "GitHub"
    public string LoginProvider { get; set; } = string.Empty;

    // المعرف الفريد الخاص بالمستخدم لدى المزود (Subject ID / User ID من جوجل مثلاً)
    public string ProviderKey { get; set; } = string.Empty;

    // اسم معروض اختيارياً للمزود (مثلاً: "Google Account")
    public string? ProviderDisplayName { get; set; }

    // FK للمستخدم صاحب هذا الحساب
    public int UserId { get; set; }

    // Navigation Property
    public User User { get; set; } = null!;
}