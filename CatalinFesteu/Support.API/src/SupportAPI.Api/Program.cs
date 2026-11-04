using SupportAPI.Api;
using SupportAPI.Application;
using SupportAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("SupportDb")
    ?? throw new InvalidOperationException("Connection string 'SupportDb' is not configured"); ;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddIdentityAndAccessManagement(builder.Configuration);

builder.Services.AddSwaggerSetup(builder.Configuration);

builder.Services.AddControllers();

var app = builder.Build();

app.UseMiddleware<GlobalErrorHandlingMiddleware>();

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
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserProvisioningMiddleware>();
app.MapControllers();

app.Run();
