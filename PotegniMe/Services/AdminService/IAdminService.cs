namespace PotegniMe.Services.AdminService;

public interface IAdminService
{
    // Update user role
    Task UpdateRole(string username, string roleName);
}
