namespace InsuranceApp.Application.Abstractions.Security;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? BrokerId { get; }
}
