using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MemoryStorage;
using Microsoft.Extensions.Options;
using MyFirstApi.Configuration;
using MyFirstApi.Controllers;
using MyFirstApi.Domain.Constants;
using MyFirstApi.Extensions;
using MyFirstApi.Middlewares;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices(builder.Configuration)
                .AddAppOptions(builder.Configuration)
                .AddPersistence(builder.Configuration)
                .AddExceptionHandler()
                .AddAutoMapper()
                .AddAuthentication(builder.Configuration)
                .AddPermissionAuthorization()
                .AddAuthRateLimiter()
                .AddSwaggerWithJwt()
                .AddHangfireServices(builder.Configuration)
                .AddRedisStackExchange(builder.Configuration)
                .AddCaching()
                .AddSessionService()
                .AddLocalization(builder.Configuration)
                .AddMediatRService()
                .AddAppFluentValidation();

builder.Services.AddControllers()
.AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
builder.Services.AddEndpointsApiExplorer();

// build instances of services
var app = builder.Build();
app.UseApplicationServices();
app.MapControllers();
app.Run();


