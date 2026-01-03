namespace PotegniMe.Services.UserService;

public interface IUserService
{
    // Get all users
    Task<List<User>> GetAllUsers();

    // Verify if user exists
    Task<bool> UserExists(string username, string email);
    Task<bool> UserExists(string username);

    // User based methods
    Task UpdateUsername(string oldUsername, string newUsername);
    Task UpdateEmail(string username, string newEmail);
    Task UpdatePfp(string username, IFormFile profilePicture);
    Task RenamePfp(string oldUsername, string newUsername);
    Task RemovePfp(string username);
    Task UpdatePassword(string username, string newPassword);
    Task DeleteUser(string username);

    // Get user methods
    Task<User> GetUserById(int userId);
    Task<User> GetUserByUsername(string username);
    Task<User> GetUserByEmail(string email);
    Task<User> GetUserByRefreshToken(string refreshToken);

    // Roles
    Task<Role> GetUserRole(string username);
    Task<bool> IsAdmin(string username);
    Task<bool> IsUploader (string username);
    
    // Get uploader request status
    RoleRequestStatus? GetRoleRequestStatus(int userId);
}

