using SupportAPI.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddKeyPerFile(directoryPath: "/run/secrets", optional: true);
builder.Services.AddSupportApi(builder.Configuration, builder.Environment);

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
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<UserProvisioningMiddleware>();
app.MapControllers();

app.Run();
