using InsuranceApp.Api.ExceptionHandling;
using InsuranceApp.Application;
using InsuranceApp.Infrastructure;
using InsuranceApp.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

#region Service Registration
builder.Services.AddControllers();

builder.Services.AddApplication(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);
#endregion


#region Logging Configuration
var logFolder = builder.Configuration["LoggingSettings:LogFolder"] ?? throw new InvalidOperationException("LoggingSettings > LogFolder is not configured.");
var logFileNameInfo = builder.Configuration["LoggingSettings:LogFileNameInfo"] ?? throw new InvalidOperationException("LoggingSettings > LogFileNameInfo is not configured.");
var logFileNameError = builder.Configuration["LoggingSettings:LogFileNameError"] ?? throw new InvalidOperationException("LoggingSettings > LogFileNameError is not configured.");

var logPathInfo = Path.Combine(AppContext.BaseDirectory, logFolder, logFileNameInfo);
var logPathError = Path.Combine(AppContext.BaseDirectory, logFolder, logFileNameError);


builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Information && logEvent.Level < LogEventLevel.Error)
            .WriteTo.File(
                logPathInfo,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}"
            )
        )
        .WriteTo.Logger(lc => lc
            .Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Error)
            .WriteTo.File(
                logPathError,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 90,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message:lj}{NewLine}{Exception}"
            )
        );
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
#endregion


#region Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{typeof(Program).Assembly.GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    options.IncludeXmlComments(xmlPath);
});
#endregion


#region Authentication and Authorization Configuration

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtSettings>>(
        (options, jwtSettingsOptions) =>
        {
            var settings = jwtSettingsOptions.Value;

            options.MapInboundClaims = false;

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = settings.Issuer,

                ValidateAudience = true,
                ValidAudience = settings.Audience,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(settings.SecretKey)),

                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,

                ClockSkew = TimeSpan.Zero
            };
        });

builder.Services.AddAuthorization();

#endregion


var app = builder.Build();


#region Application Startup
app.Lifetime.ApplicationStarted.Register(() =>
{
    var httpsUrl = app.Urls.FirstOrDefault(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

    Console.WriteLine($"Application started: {httpsUrl}");
});
#endregion

app.UseExceptionHandler();


#region Development Configuration
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
#endregion


#region Middleware Pipeline

app.UseExceptionHandler();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

#endregion

await app.RunAsync();
