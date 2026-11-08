using MyFirstApi.Middlewares;
using Hangfire;
using MyFirstApi.Configuration;
using Microsoft.Extensions.Options;
using Hangfire.Dashboard;
namespace MyFirstApi.Extensions;

public static class AddApplicationServices
{
    public static IApplicationBuilder UseApplicationServices(this IApplicationBuilder app)
    {
        // middleware pipeline (onion idea)
        //app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseExceptionHandler();
        app.UseStatusCodePages();
        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseWhen(
        ctx => ctx.Request.Path.StartsWithSegments("/api/test"),
        branch => branch.UseMiddleware<PerformanceMiddleware>());
        app.UseRateLimiter();
        var localizationOptions = app.ApplicationServices.GetRequiredService<IOptions<LocalizationSettings>>().Value;
        var options = new RequestLocalizationOptions()
            .SetDefaultCulture(localizationOptions.DefaultCulture)
            .AddSupportedCultures(localizationOptions.SupportedCultures)
            .AddSupportedUICultures(localizationOptions.SupportedCultures);
        app.UseRequestLocalization(options);
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseSwagger(c => c.SerializeAsV2 = true);
        app.UseSwaggerUI(c =>
        {c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1"); });
        app.UseHangfireDashboard("/hangfire", new DashboardOptions
        { Authorization = new[] { new LocalRequestsOnlyAuthorizationFilter() } });
        app.UseSession();
        return app;
    }
}
