using PotegniMe.Services.UserService;

namespace PotegniMe.Services.AdminService
{
    public class AdminService(DataContext context, IUserService userService) : IAdminService
    {
        // Methods
        public async Task UpdateRole(string username, string roleName)
        {
            Role role = context.Roles.FirstOrDefault(r => r.Name == roleName) ??
                throw new ArgumentException("Role not found");
            User user = await userService.GetUserByUsername(username);
            user.Role = role;
            user.RoleId = role.RoleId;
            await context.SaveChangesAsync();
        }
    }
}
