using System.Security.Claims;

namespace API.Services.UserService
{
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
        Task UpdateUsername(Claim claim, string username);

        // Update user email
        Task UpdateEmail(Claim claim, string email);

        // Get user pfp stream with mime
        Task<(Stream, string)> GetPfpStreamWithMime(int userId);

        // Get user pfp in base 64
        Task<string> GetPfpBase64(int userId);

        // Update user pfp
        Task UpdatePfp(Claim claim, IFormFile profilePicture);

        // Remove user pfp
        Task RemovePfp(Claim claim);

        // Update user password
        Task UpdatePassword(Claim claim, string newPassword);

        // Get user by id
        Task<User> GetUserById(int userId);

        // Get user by username
        Task<User> GetUserByUsername(string username);

        // Get user by email
        Task<User> GetUserByEmail(string email);

        // Get user role
        Task<Role> GetUserRole(int userId);

        // Check if user is admin
        Task<bool> IsAdmin(int userId);

        // Check if user is uploader
        Task<bool> IsUploader (int userId);

        // Delete user
        Task DeleteUser(int userId);

        // Get uploader request status
        RoleRequestStatus? GetRoleRequestStatus(int userId);
    }
}
