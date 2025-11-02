using System.Security.Claims;

namespace PotegniMe.Services.AdminService
{
    public interface IAdminService
    {
        // Update user role
        Task UpdateRole(Claim claim, string roleName);
    }
}
