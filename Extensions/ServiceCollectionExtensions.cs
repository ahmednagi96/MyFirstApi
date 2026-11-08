using System.Text;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.MemoryStorage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyFirstApi.Configuration;
using MyFirstApi.Controllers;
using MyFirstApi.Data;
using MyFirstApi.Entities;
using MyFirstApi.Middlewares;
using MyFirstApi.Profiles;
using MyFirstApi.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using PermissionConstants = MyFirstApi.Domain.Constants.Permissions;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using MyFirstApi.Repositories;
using MyFirstApi.Domain.MediatR;
using MediatR;
using MyFirstApi.Domain.MediatR.Behaviors;
using MyFirstApi.Data.Interceptors;

namespace MyFirstApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var storageType = configuration["FileStorage:Type"];
        services.AddHttpContextAccessor();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUser, CurrentUser>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<TrackService>();
        services.AddScoped<ILoggerService, LoggerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IPostRepository, PostRepository>();
        if (storageType == "S3")
        {
            services.AddScoped<IFileStorageService, S3FileStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }
        return services;
    }

    public static IServiceCollection AddPersistence(
        this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o =>
            o
            .UseSqlServer(config.GetConnectionString("DefaultConnection"))
            .AddInterceptors(new SoftDeleteInterceptor(), new AuditInterceptor())
            );
        return services;
    }

    public static IServiceCollection AddAppOptions(this IServiceCollection services, IConfiguration configuration)// options pattern
    {
        services.Configure<Sale>(configuration.GetSection("Sale"));
        services.Configure<Tool>(configuration.GetSection("Tool"));
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<GmailSettings>(configuration.GetSection("GmailSettings"));
        services.Configure<LocalizationSettings>(configuration.GetSection("LocalizationSettings"));
        return services;
    }

    public static IServiceCollection AddExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
            {
                ctx.ProblemDetails.Instance ??= ctx.HttpContext.Request.Path;
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
            };
        });
        return services;
    }

    public static IServiceCollection AddAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section is missing.");
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = jwt.Audience,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                ClockSkew = TimeSpan.Zero,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role,
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier
            };
            options.Events = new JwtBearerEvents
            {
                OnChallenge = async context =>
                {
                    context.HandleResponse();

                    var detail = context.AuthenticateFailure switch
                    {
                        SecurityTokenExpiredException =>
                            "انتهت صلاحية التوكن. يرجى تسجيل الدخول مرة أخرى.",
                        not null =>
                            "التوكن غير صالح.",
                        _ => string.IsNullOrWhiteSpace(context.Request.Headers.Authorization)
                            ? "يجب تسجيل الدخول للوصول لهذا المورد."
                            : "التوكن غير صالح."
                    };

                    await context.HttpContext.WriteProblemAsync(
                        StatusCodes.Status401Unauthorized,
                        "Unauthorized",
                        detail);
                },
                OnForbidden = async context =>
                {
                    await context.HttpContext.WriteProblemAsync(
                        StatusCodes.Status403Forbidden,
                        "Forbidden",
                        "ليس لديك صلاحية لتنفيذ هذا الإجراء.");
                }
            };
        })
        // 3. إضافة الـ Cookie المؤقتة (تُستخدم فقط في رحلة الـ OAuth)
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        // 4. إضافة مزود Google
        .AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
        {
            options.ClientId = configuration["Authentication:Google:ClientId"]!;
            options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            // ربط نتائج Google بالـ Cookie المؤقتة
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        }); 
    return services;
    }
    public static IServiceCollection AddPermissionAuthorization(this IServiceCollection services)
    {
    services.AddAuthorization(options =>
        {
           var permissions = typeof(PermissionConstants)
                                                        .GetNestedTypes() // هيدخل جوه الكلاسات الفرعية (Products, Categories, Tools)
                                                        .SelectMany(c => c.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy))
                                                        .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                                                        .Select(fi => (string) fi.GetValue(null)!);

            foreach (var permission in permissions)
            {
                if (permission == PermissionConstants.Categories.Delete){
                    options.AddPolicy(permission, policy =>
                    policy.RequireClaim(TokenService.PermissionClaimType, permission)
                    .RequireRole("Admin")
                    );
                }
                else{
                    options.AddPolicy(permission, policy => 
                    policy.RequireClaim(TokenService.PermissionClaimType, permission));
                }
            }
        }
    );
    return services;
    }

    public static IServiceCollection AddAuthRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, _) =>
            {
                await context.HttpContext.WriteProblemAsync(
                    StatusCodes.Status429TooManyRequests,
                    "Too Many Requests",
                    "تم تجاوز عدد المحاولات المسموح بها. حاول مرة أخرى لاحقاً.");
            };
            options.AddFixedWindowLimiter("auth", limiter =>
            {
                limiter.PermitLimit = 10;
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.QueueLimit = 0;
            });
        });

        return services;
    }

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Enter: Bearer {your JWT}. Example: Bearer eyJhbGciOi...",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
    public static IServiceCollection AddHangfireServices(this IServiceCollection services,IConfiguration configiration)
    {
        // 1. تسجيل Hangfire في الـ Memory
      services.AddHangfire(config => config
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UseSqlServerStorage(configiration.GetConnectionString("DefaultConnection"))); // ربط الداتابيس

        // 2. تشغيل الـ Hangfire Server اللي هيسحب وينفذ الـ Jobs في الخلفية
      services.AddHangfireServer();
        return services;
    }
    public static IServiceCollection AddLocalization(this IServiceCollection services, IConfiguration configuration)
    {
        // ربط الإعدادات بالكلاس
        services.AddLocalization(options => options.ResourcesPath = "Resources");
        return services;
    }
    public static IServiceCollection AddCaching(this IServiceCollection services)
    {
        services.AddMemoryCache();
        return services;
    }

    public static IServiceCollection AddMediatRService(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
            cfg.NotificationPublisherType = typeof(SafeNotificationPublisher);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        });
        return services;
    }

    public static IServiceCollection AddAppFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(Program).Assembly);
        return services;
    }
    public static IServiceCollection AddRedisStackExchange(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "MyFirstApi_";
        });
        return services;
    }

    public static IServiceCollection AddSessionService(this IServiceCollection services)
    {
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        return services;
    }
}
