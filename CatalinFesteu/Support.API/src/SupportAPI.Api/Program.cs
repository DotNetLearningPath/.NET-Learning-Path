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

var keycloakSettings = builder.Configuration.GetSection("Keycloak")
    ?? throw new InvalidOperationException("Identity Provider settings are missing");

var connectionString = builder.Configuration.GetConnectionString("SupportDb");



// Add services to the container.
builder.Services.AddControllers();

var frontendOrigins = builder.Configuration
    .GetSection("Frontend:Origins").Get<string[]>()
    ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy => policy
        .WithOrigins(frontendOrigins)
        .WithMethods("GET", "POST")
        .WithHeaders("Authorization", "Content-Type", "Accept")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var realmUrl = keycloakSettings["RealmUrl"];

    options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(
                    $"{realmUrl}/protocol/openid-connect/auth"),
                TokenUrl = new Uri(
                    $"{realmUrl}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    ["openid"] = "Authenticate the user"
                }
            }
        }
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Keycloak", document)] = ["openid"]
    });
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddScoped<IValidator<CreateUserRequestCommand>, CreateRequestCommandValidator>();
builder.Services.AddScoped<IValidator<GetRequestByIdQuery>, GetRequestByIdQueryValidator>();

builder.Services.AddScoped<ICommandHandler<CreateUserRequestCommand, UserRequestResult>, CreateRequestCommandHandler>();
builder.Services.AddScoped<IQueryHandler<GetRequestByIdQuery, UserRequestResult>, GetRequestByIdQueryHandler>();



builder.Services.AddInfrastructure(connectionString!);
builder.Services.AddAuthorization();

builder.Services.AddJwtAuthentication(keycloakSettings);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId("support-realm");
        options.OAuthUsePkce();
        options.OAuthScopes("openid");
    });
}

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();


app.UseMiddleware<UserProvisioningMiddleware>();

app.MapControllers();

app.Run();
