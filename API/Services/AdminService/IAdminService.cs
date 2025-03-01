using System.Security.Claims;

namespace API.Services.AdminService
{
    public interface IAdminService
    {
        // Update user role
        Task UpdateRole(Claim claim, string roleName);
    }
}
