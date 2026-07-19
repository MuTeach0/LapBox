namespace LapBox.Application.Common.Interfaces.Identity;

public interface IUser
{
    Guid? Id { get; }
    List<string> Roles { get; }
    bool IsInRole(string role);
    bool IsCustomer { get; }
}