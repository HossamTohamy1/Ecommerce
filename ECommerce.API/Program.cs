using System.Globalization;
using System.Threading.RateLimiting;
using ECommerce.API.Middleware;
using ECommerce.Infrastructure;
using ECommerce.Infrastructure.Persistence;
using ECommerce.Infrastructure.Realtime;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ECommerce.API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithMachineName()
        .WriteTo.Console());

    builder.Services.AddLocalization();
    builder.Services.AddSingleton<Microsoft.AspNetCore.Mvc.DataAnnotations.IValidationAttributeAdapterProvider, ECommerce.API.Infrastructure.Localization.CustomValidationAttributeAdapterProvider>();

    builder.Services.AddOptions<MvcOptions>()
        .Configure<IStringLocalizer<SharedResource>>((options, localizer) =>
        {
            options.ModelMetadataDetailsProviders.Add(new ECommerce.API.Infrastructure.Localization.LocalizedDisplayMetadataProvider(localizer));
            ECommerce.API.Infrastructure.Localization.ModelBindingMessageProviderExtensions.ConfigureLocalizedModelBindingMessages(options.ModelBindingMessageProvider, localizer);
        });

    builder.Services.AddControllers()
        .AddViewLocalization()
        .AddDataAnnotationsLocalization(options =>
        {
            options.DataAnnotationLocalizerProvider = (type, factory) =>
                factory.Create(typeof(SharedResource));
        });

    builder.Services.AddRazorPages(options =>
    {
        options.Conventions.AuthorizeFolder("/Cart", "RazorPagesAuth");
        options.Conventions.AuthorizeFolder("/Wishlist", "RazorPagesAuth");
        options.Conventions.AuthorizeFolder("/Addresses", "RazorPagesAuth");
        options.Conventions.AuthorizeFolder("/Orders", "RazorPagesAuth");
        options.Conventions.AuthorizeFolder("/Orders/Admin", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/Products/Admin", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/Categories", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/Brands", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/Discounts", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/Reviews", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/Notifications", "RazorPagesAuth");
        options.Conventions.AuthorizeFolder("/Chat", "RazorPagesAuth");
        options.Conventions.AuthorizeFolder("/Chat/Admin", "RazorPagesAdmin");
        options.Conventions.AuthorizeFolder("/AuditLogs", "RazorPagesAdmin");
    })
    .AddViewLocalization()
    .AddDataAnnotationsLocalization(options =>
    {
        options.DataAnnotationLocalizerProvider = (type, factory) =>
            factory.Create(typeof(SharedResource));
    });

    builder.Services.AddInfrastructure(builder.Configuration);

    var supportedCultures = new[] { new CultureInfo("en"), new CultureInfo("ar") };
    builder.Services.Configure<RequestLocalizationOptions>(options =>
    {
        options.DefaultRequestCulture = new RequestCulture("en");
        options.SupportedCultures = supportedCultures;
        options.SupportedUICultures = supportedCultures;

        options.RequestCultureProviders = new IRequestCultureProvider[]
        {
            new CookieRequestCultureProvider { CookieName = CookieRequestCultureProvider.DefaultCookieName },
            new QueryStringRequestCultureProvider { QueryStringKey = "culture", UIQueryStringKey = "culture" },
            new AcceptLanguageHeaderRequestCultureProvider(),
        };
    });

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddPolicy("auth", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
    });

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Default", policy =>
        {
            policy.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials();
        });
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var provider = scope.ServiceProvider;
        var dbContext = provider.GetRequiredService<ApplicationDbContext>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = provider.GetRequiredService<UserManager<ECommerce.Domain.Entities.ApplicationUser>>();
        var seedLogger = provider.GetRequiredService<ILogger<Program>>();

        try
        {
            seedLogger.LogInformation("Applying EF Core database migrations...");
            await dbContext.Database.MigrateAsync();
            seedLogger.LogInformation("Database migrations applied successfully.");

            seedLogger.LogInformation("Starting database seeding via DbInitializer...");
            await DbInitializer.SeedAsync(dbContext, roleManager, userManager, seedLogger);
            seedLogger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            seedLogger.LogError(ex, "FATAL: Database migration or seeding failed during startup: {Message}", ex.Message);
            throw;
        }
    }

    app.UseRequestLocalization();

    app.UseRequestLoggingBehaviour();

    app.UseMiddleware<ExceptionHandlingMiddleware>();

    //if (!app.Environment.IsDevelopment())
    //{
    app.UseHttpsRedirection();
    app.UseExceptionHandler("/Error");
    //}

    app.UseStaticFiles();

    app.UseCors("Default");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapRazorPages();
    app.MapHub<NotificationHub>("/hubs/notifications");
    app.MapHub<ChatHub>("/hubs/chat");

    app.Run();
}
catch (Exception ex) when (ex.GetType().Name is not "HostAbortedException" && ex.GetType().Name is not "StopTheHostException")
{
    Log.Fatal(ex, "ECommerce.API terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
