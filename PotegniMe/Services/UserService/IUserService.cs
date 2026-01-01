namespace PotegniMe.Services.UserService;

public interface IUserService
{
// Get all users
    Task<List<User>> GetAllUsers();

    // Verify if user exists
    // Returns true if user exists, false if not
    Task<bool> UserExists(string username, string email);
    Task<bool> UserExists(string username);
    Task<bool> UserExists(int userId);

    // User based methods
    // Update user username
    Task UpdateUsername(string oldUsername, string newUsername);

    // Update user email
    Task UpdateEmail(string username, string newEmail);

    // Update user pfp
    Task UpdatePfp(string username, IFormFile profilePicture);

    // Rename user pfp
    Task RenamePfp(string oldUsername, string newUsername);

    // Remove user pfp
    Task RemovePfp(string username);

    // Update user password
    Task UpdatePassword(string username, string newPassword);

    // Get user by username
    Task<User> GetUserByUsername(string username);

    // Get user by email
    Task<User> GetUserByEmail(string email);

    // Get user role
    Task<Role> GetUserRole(string username);

    // Check if user is admin
    Task<bool> IsAdmin(string username);

    // Check if user is uploader
    Task<bool> IsUploader (string username);

    // Delete user
    Task DeleteUser(string username);

    // Get uploader request status
    RoleRequestStatus? GetRoleRequestStatus(int userId);
}

