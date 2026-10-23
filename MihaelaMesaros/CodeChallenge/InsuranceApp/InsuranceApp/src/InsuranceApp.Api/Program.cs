using InsuranceApp.Api.ExceptionHandling;
using InsuranceApp.Application;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.User;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure;
using Serilog;
using Serilog.Events;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

#region Service Registration
builder.Services.AddControllers();

builder.Services.AddApplication();

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

var jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured.");
var jwtIssuer = builder.Configuration["JwtSettings:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured.");
var jwtAudience = builder.Configuration["JwtSettings:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured.");

var secretKeyBytes = Convert.FromBase64String(jwtSecretKey);
if (secretKeyBytes.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must contain at least 32 bytes.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKeyBytes),

            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
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
