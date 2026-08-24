using ECommerce.Application.Mapping;
using ECommerce.Shared.Constants;
using ECommerce.Infrastructure.Email;
using ECommerce.Infrastructure.Files;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Realtime;
using ECommerce.Infrastructure.Security;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mapster;
using MapsterMapper;
namespace ECommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
  
        MapsterConfig.RegisterMappings();
        services.AddSingleton(Mapster.TypeAdapterConfig.GlobalSettings);
        services.AddScoped<MapsterMapper.IMapper, MapsterMapper.ServiceMapper>();
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                }));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<JwtValidator>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        var emailChannel = System.Threading.Channels.Channel.CreateBounded<EmailWorkItem>(new System.Threading.Channels.BoundedChannelOptions(10_000)
        {
            FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false
        });
        services.AddSingleton(emailChannel);

        services.AddSingleton<SmtpEmailService>();
        services.AddSingleton<IEmailService, QueuedEmailService>();
        services.AddHostedService<EmailBackgroundWorker>();

        services.AddMemoryCache();

        services.AddScoped<ECommerce.Application.Features.Discounts.Common.DiscountResolver>();
        services.AddScoped<IDiscountResolver>(sp =>
            new ECommerce.Application.Features.Discounts.Common.CachedDiscountResolver(
                sp.GetRequiredService<ECommerce.Application.Features.Discounts.Common.DiscountResolver>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ECommerce.Application.Features.Discounts.Common.CachedDiscountResolver>>()));

        services.AddSignalR();
        services.AddSingleton<IRealtimeNotifier, RealtimeNotifier>();

        services.AddValidatorsFromAssembly(typeof(ECommerce.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ECommerce.Application.Common.Behaviors.ValidationBehavior<,>).Assembly);
            cfg.AddOpenBehavior(typeof(ECommerce.Application.Common.Behaviors.ValidationBehavior<,>));
        });

        services.AddTransient<ECommerce.Application.Features.Categories.Queries.GetAllCategories.GetAllCategoriesQueryHandler>();
        services.AddTransient<IRequestHandler<ECommerce.Application.Features.Categories.Queries.GetAllCategories.GetAllCategoriesQuery, List<ECommerce.Application.DTOs.Catalog.CategoryDto>>>(sp =>
            new ECommerce.Application.Features.Categories.Queries.GetAllCategories.CachedGetAllCategoriesQueryHandler(
                sp.GetRequiredService<ECommerce.Application.Features.Categories.Queries.GetAllCategories.GetAllCategoriesQueryHandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ECommerce.Application.Features.Categories.Queries.GetAllCategories.CachedGetAllCategoriesQueryHandler>>()));

        services.AddTransient<ECommerce.Application.Features.Brands.Queries.GetAllBrands.GetAllBrandsQueryHandler>();
        services.AddTransient<IRequestHandler<ECommerce.Application.Features.Brands.Queries.GetAllBrands.GetAllBrandsQuery, List<ECommerce.Application.DTOs.Catalog.BrandDto>>>(sp =>
            new ECommerce.Application.Features.Brands.Queries.GetAllBrands.CachedGetAllBrandsQueryHandler(
                sp.GetRequiredService<ECommerce.Application.Features.Brands.Queries.GetAllBrands.GetAllBrandsQueryHandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>()));

        services.AddTransient<ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard.GetAdminDashboardQueryHandler>();
        services.AddTransient<IRequestHandler<ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard.GetAdminDashboardQuery, ECommerce.Application.DTOs.Dashboard.AdminDashboardDto>>(sp =>
            new ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard.CachedGetAdminDashboardQueryHandler(
                sp.GetRequiredService<ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard.GetAdminDashboardQueryHandler>(),
                sp.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<ECommerce.Application.Features.Dashboard.Queries.GetAdminDashboard.CachedGetAdminDashboardQueryHandler>>()));


        services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Smart";
                options.DefaultChallengeScheme = "Smart";
            })
            .AddPolicyScheme("Smart", "Smart", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.ToString();
                    if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return JwtAuthenticationOptions.SchemeName;
                    }

                    if (context.Request.Path.StartsWithSegments("/hubs") && context.Request.Query.ContainsKey("access_token"))
                    {
                        return JwtAuthenticationOptions.SchemeName;
                    }

                    return CookieAuthDefaults.Scheme;
                };
            })
            .AddScheme<JwtAuthenticationOptions, JwtAuthenticationHandler>(JwtAuthenticationOptions.SchemeName, _ => { })
            .AddCookie(CookieAuthDefaults.Scheme, options =>
            {
                options.Cookie.Name = "ECommerce.Auth";
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.SlidingExpiration = true;
            })
        
            .AddCookie(IdentityConstants.ExternalScheme, options =>
            {
                options.Cookie.Name = "ECommerce.External";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
            });

        var googleClientId = configuration["Authentication:Google:ClientId"];
        var googleClientSecret = configuration["Authentication:Google:ClientSecret"];
        if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
        {
            services.AddAuthentication().AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
            {
                options.ClientId = googleClientId;
                options.ClientSecret = googleClientSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.CallbackPath = "/signin-google";
            });
        }

        var facebookAppId = configuration["Authentication:Facebook:AppId"];
        var facebookAppSecret = configuration["Authentication:Facebook:AppSecret"];
        if (!string.IsNullOrWhiteSpace(facebookAppId) && !string.IsNullOrWhiteSpace(facebookAppSecret))
        {
            services.AddAuthentication().AddFacebook(FacebookDefaults.AuthenticationScheme, options =>
            {
                options.AppId = facebookAppId;
                options.AppSecret = facebookAppSecret;
                options.SignInScheme = IdentityConstants.ExternalScheme;
                options.CallbackPath = "/signin-facebook";
            });
        }

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RazorPagesAuth", policy => policy
                .AddAuthenticationSchemes(CookieAuthDefaults.Scheme)
                .RequireAuthenticatedUser());

            options.AddPolicy("RazorPagesAdmin", policy => policy
                .AddAuthenticationSchemes(CookieAuthDefaults.Scheme)
                .RequireRole(AppConstants.Roles.Admin));
        });

        return services;
    }
}
