using Microsoft.AspNetCore.Http.Features;
using SupportAPI.Api;
using SupportAPI.Api.Middleware;
using SupportAPI.Application;
using SupportAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
var builderConfiguration = builder.Configuration;

var connectionString = builderConfiguration.GetConnectionString("SupportDb")
    ?? throw new InvalidOperationException("Connection string 'SupportDb' is not configured"); ;

builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddIdentityAndAccessManagement(builderConfiguration);

builder.Services.AddSwaggerSetup(builderConfiguration);
builder.Services.AddErrorHandling();
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        var activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
    };
});

builder.Services.AddControllers();

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

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<UserProvisioningMiddleware>();

app.MapControllers();

app.Run();
