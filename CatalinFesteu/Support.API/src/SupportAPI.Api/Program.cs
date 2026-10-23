using FluentValidation;
using Microsoft.OpenApi;
using SupportAPI.Api;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.CommandHandlers;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Queries;
using SupportAPI.Application.QueryHandlers;
using SupportAPI.Application.Results;
using SupportAPI.Application.Validations;
using SupportAPI.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyPerFile(
    directoryPath: "/run/secrets",
    optional: true);

var jwtSettingsSection = builder.Configuration.GetSection(JwtSettings.SectionName)
    ?? throw new InvalidOperationException("JWT settings are missing");
var connectionString = builder.Configuration.GetConnectionString("SupportDb");



// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter the access token"
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });
});



builder.Services.AddOptions<JwtSettings>()
    .Bind(jwtSettingsSection)
    .Validate(settings =>
        !string.IsNullOrWhiteSpace(settings.Issuer) &&
        !string.IsNullOrWhiteSpace(settings.Audience) &&
        !string.IsNullOrWhiteSpace(settings.SigningKey) &&
        settings.AccessTokenMinutes > 0, "Jwt settings are invalid"
    )
    .ValidateOnStart();



builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();

builder.Services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
builder.Services.AddScoped<IValidator<CreateRequestCommand>, CreateRequestCommandValidator>();
builder.Services.AddScoped<IValidator<GetRequestByIdQuery>, GetRequestByIdQueryValidator>();

builder.Services.AddScoped<ICommandHandler<LoginCommand, LoginResult>, LoginCommandHandler>();
builder.Services.AddScoped<ICommandHandler<RegisterUserCommand, RegisterUserResult>, RegisterUserCommandHandler>();
builder.Services.AddScoped<ICommandHandler<CreateRequestCommand, SingleRequestResult>, CreateRequestCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetRequestByIdQuery, SingleRequestResult>, GetRequestByIdQueryHandler>();



builder.Services.AddInfrastructure(connectionString!);
builder.Services.AddUserRegistration();
builder.Services.AddAuthorization();

var jwtSettings = jwtSettingsSection.Get<JwtSettings>();

builder.Services.AddJwtAuthentication(jwtSettings);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
