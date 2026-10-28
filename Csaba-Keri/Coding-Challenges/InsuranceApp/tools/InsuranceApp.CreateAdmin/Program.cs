using InsuranceApp.Application.Common.Security;
using InsuranceApp.Infrastructure;
using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace InsuranceApp.CreateAdmin;

internal static class Program
{
    private static async Task<int> Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);

        var connectionString = builder.Configuration.GetConnectionString("InsuranceDatabase");
        
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            await Console.Error.WriteLineAsync("Set ConnectionStrings:InsuranceDatabase in this project's User Secrets.");
            return 1;
        }

        builder.Services.AddInfrastructure(connectionString);

        using var host = builder.Build();
        await using var scope = host.Services.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        try
        {
            if (!await roleManager.RoleExistsAsync(ApplicationRoles.Admin))
            {
                await Console.Error.WriteLineAsync($"\nThe {ApplicationRoles.Admin} role does not exist. Apply the database migrations first.");
                return 1;
            }

            Console.Write("\nAdmin email: ");
            var email = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(email))
            {
                await Console.Error.WriteLineAsync("Email is required.");
                return 1;
            }

            email = email.Trim();

            if (await userManager.FindByEmailAsync(email) is not null)
            {
                await Console.Error.WriteLineAsync("\nAn account with this email already exists. No changes were made.");
                return 1;
            }

            Console.Write("\nAdmin password (hidden): ");
            var password = ReadPassword();

            if (string.IsNullOrWhiteSpace(password))
            {
                await Console.Error.WriteLineAsync("Password is required.");
                return 1;
            }

            var user = new ApplicationUser(email);

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            EnsureSucceeded(await userManager.CreateAsync(user, password));
            EnsureSucceeded(await userManager.AddToRoleAsync(user, ApplicationRoles.Admin));

            await transaction.CommitAsync();

            Console.WriteLine("\nAdmin account created successfully.");
            return 0;
        }
        catch (InvalidOperationException exception)
        {
            await Console.Error.WriteLineAsync(exception.Message);
            return 1;
        }
    }

    private static string ReadPassword()
    {
        var password = new StringBuilder();
        
        while (true)
        {
            var key = Console.ReadKey(intercept: true);
            
            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                return password.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Length--;
                }
            }
            else if (!char.IsControl(key.KeyChar))
            {
                password.Append(key.KeyChar);
            }
        }
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                "\nIdentity operation failed: " + string.Join(", ", result.Errors.Select(error => error.Code))
            );
        }
    }
}
