using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MyFirstApi.DTOs.Auth;
using MyFirstApi.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MyFirstApi.Controllers;

[EnableRateLimiting("auth")]
[Produces("application/json")]
public class AuthController(IAuthService authService, ICurrentUser currentUser) : ApiControllerBase
{
    private const string RefreshTokenCookieName = "refreshToken";

    [AllowAnonymous]
    [HttpPost("signup")]
    [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)][EnableRateLimiting("auth")]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponseDto>> Register(
        [FromBody] RegisterDto model,
        CancellationToken ct)
    {
        var result = await authService.RegisterAsync(model, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponseDto>> Login(
        [FromBody] LoginDto model,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(model, ClientIp(), ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> RefreshToken(
        [FromBody] RefreshTokenRequestDto? model,
        CancellationToken ct)
    {
        var rawToken = ReadRefreshToken(model);
        var result = await authService.RefreshTokenAsync(rawToken, ClientIp(), ct);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
        return Ok(result);
    }

    [AllowAnonymous]
    //لي هنا بقي معمول تسجيل الخروج الوانينموس ازاي طيب ؟ لان الاكسس توكين لما يخلص بعد ربع ساعه مثلا وضغطت عالزرار الخاص بتسجيل الخروج كدا انا المفروض برا ف الطلب هيتنفذ طيب اشمعن باقي الزراير مش كدا لان الفرونت اند مع باقي الزرارير بيلاقي الاكسس توكن خلص ويرجعلوا 401 ف يروح يعمل ريفريش توكن انما تسجيل الخروج مش هينفع لانو عايز يلغي التوكن ازاي هيعملوا ريفريش حلقه لوب هي كدا فاهمني 
    [HttpPost("logout")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponseDto>> Logout(
        [FromBody] RefreshTokenRequestDto? model,
        CancellationToken ct)
    {
        var rawToken = Request.Cookies[RefreshTokenCookieName] ?? model?.RefreshToken;
        if (!string.IsNullOrWhiteSpace(rawToken))
            await authService.RevokeTokenAsync(rawToken, ct);

        DeleteRefreshCookie();
        return Ok(new MessageResponseDto("تم تسجيل الخروج بنجاح."));
    }

    [Authorize]
    [HttpPost("logout-all")]
    //اشمعن دا اوثورايز بقي عشان بيعتمد علي الاي دي الخاص بالمستخدم فاهمني عشان يجيب كل الرقريش توكنز من الداتابيز ويلغيهم بس كدا كمان الغاء الجلسه من الاجهزه الاخري عمليه حساسه تتطلب صلاحيه بردو 
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponseDto>> LogoutAll(CancellationToken ct)
    {
        await authService.RevokeAllSessionsAsync(currentUser.GetRequiredUserId(), ct);
        DeleteRefreshCookie();
        return Ok(new MessageResponseDto("تم إنهاء كل الجلسات."));
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MessageResponseDto>> ForgotPassword(
        [FromBody] ForgotPasswordDto model,
        CancellationToken ct)
    {
        await authService.ForgotPasswordAsync(model.Email, ct);
        return Ok(new MessageResponseDto(
            "إذا كان البريد موجوداً، فقد تم إرسال تعليمات إعادة تعيين كلمة المرور."));
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponseDto>> ResetPassword(
        [FromBody] ResetPasswordDto model,
        CancellationToken ct)
    {
        await authService.ResetPasswordAsync(model, ct);
        DeleteRefreshCookie();
        return Ok(new MessageResponseDto("تم تغيير كلمة المرور بنجاح. سجّل الدخول من جديد."));
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MessageResponseDto>> ChangePassword(
        [FromBody] ChangePasswordDto model,
        CancellationToken ct)
    {
        await authService.ChangePasswordAsync(currentUser.GetRequiredUserId(), model, ct);
        DeleteRefreshCookie();
        return Ok(new MessageResponseDto("تم تغيير كلمة المرور. سجّل الدخول من جديد."));
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthUserDto>> Me(CancellationToken ct)
        => Ok(await authService.GetMeAsync(currentUser.GetRequiredUserId(), ct));

    private string ReadRefreshToken(RefreshTokenRequestDto? model)
    {
        var token = model?.RefreshToken ?? Request.Cookies[RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(token))
            throw new UnauthorizedAccessException("Refresh token مفقود.");

        return token;
    }

    private string? ClientIp()
        => HttpContext.Connection.RemoteIpAddress?.ToString();

    private void SetRefreshCookie(string refreshToken, DateTime expiresOn)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Expires = expiresOn,
            Path = "/api/Auth"
        });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true, // لمنع التحكم من الجافاسكربت 
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax,
            Path = "/api/Auth"
        });
    }

    [AllowAnonymous]
    [HttpGet("google-login")]
    [ProducesResponseType(StatusCodes.Status302Found)]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback))
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [AllowAnonymous]
    [HttpGet("google-callback")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthResponseDto>> GoogleCallback(CancellationToken ct)
    {
        var auth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        if (!auth.Succeeded || auth.Principal is null)
            throw new UnauthorizedAccessException("فشل التحقق من حساب Google.");

        var result = await authService.LoginWithExternalAsync(
            ReadGoogleLogin(auth.Principal),
            ClientIp(),
            ct);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresOn);
        return Ok(result);
    }

    private static ExternalLoginDto ReadGoogleLogin(ClaimsPrincipal principal)
    {
        var email = principal.FindFirstValue(ClaimTypes.Email);
        var providerKey = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var name = principal.FindFirstValue(ClaimTypes.Name);

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(providerKey))
            throw new ArgumentException("لم نتمكن من قراءة البريد أو معرف الحساب من Google.");

        return new ExternalLoginDto("Google", providerKey, email, name);
    }
}
