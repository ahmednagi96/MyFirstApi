using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MyFirstApi.Services;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    public int? UserId
    {
        get
        {
            var value = FindClaim(ClaimTypes.NameIdentifier)
                ?? FindClaim(JwtRegisteredClaimNames.Sub);

            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email =>
        FindClaim(ClaimTypes.Email) ?? FindClaim(JwtRegisteredClaimNames.Email);

    public string? Role => FindClaim(ClaimTypes.Role);

    public int GetRequiredUserId()
        => UserId ?? throw new UnauthorizedAccessException("جلسة العمل غير صالحة.");

    private string? FindClaim(string type)
        => httpContextAccessor.HttpContext?.User.FindFirstValue(type);
}
