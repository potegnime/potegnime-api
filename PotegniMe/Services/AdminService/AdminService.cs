using PotegniMe.Services.EmailService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Services.AdminService
{
    public class AdminService : IAdminService
    {
        // Fields
        public readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;

        // Constructor
        public AdminService(DataContext context, IUserService userService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _userService = userService;
            _emailService = emailService;
            _configuration = configuration;
        }

        // Methods
        public async Task UpdateRole(string username, string roleName)
        {
            Role role = _context.Role.FirstOrDefault(r => r.Name == roleName) ??
                throw new ArgumentException("Role not found");
            User user = await _userService.GetUserByUsername(username);
            user.Role = role;
            user.RoleId = role.RoleId;
            await _context.SaveChangesAsync();
        }

        // Helper methods
    }
}
