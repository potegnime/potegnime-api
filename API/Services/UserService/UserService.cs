using API.Services.FileService;
using System.Security.Claims;

namespace API.Services.UserService
{
    public class UserService : IUserService
    {
        // Fields
        private readonly DataContext _context;
        private readonly IFileService _fileService;
        private readonly IConfiguration _configuration;

        // Constructor
        public UserService(DataContext context, IFileService fileService, IConfiguration configuration)
        {
            _context = context;
            _fileService = fileService;
            _configuration = configuration;
        }

        // Methods
        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await _context.User.ToListAsync();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UserExists(string username, string email)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
                return user != null;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UserExists(string username)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username);
                return user != null;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public async Task<bool> UserExists(int userId)
        {
            try
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId);
                return user != null;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        // User based methods
        public async Task UpdateUsername(Claim claim, string username)
        {
            // Input formatting - nothing can end with a trailing space
            username = username.Trim().ToLower();

            // Check if username is already taken
            if (await _context.User.AnyAsync(u => u.Username == username))
            {
                throw new ConflictExceptionDto("Uporabnik s tem uporabniškim imenom že obstaja!");
            }

            var user = await GetUserById(int.Parse(claim.Value));
            user.Username = username;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmail(Claim claim, string email)
        {
            // Input formatting - nothing can end with a trailing space
            email = email.Trim().ToLower();

            // Check if email is already taken
            if (await _context.User.AnyAsync(u => u.Email == email))
            {
                throw new ConflictExceptionDto("Uporabnik s tem e-poštnim naslovom že obstaja!");
            }

            // Update email
            var user = await GetUserById(int.Parse(claim.Value));
            user.Email = email;
            await _context.SaveChangesAsync();
        }

        public async Task<(Stream, string)> GetPfpStreamWithMime(int userId)
        {
            // Get needed data from appsettings.json
            string? storageFilePath = _configuration["FileSystem:ProfilePics"];

            if (storageFilePath == null)
            {
                throw new Exception("Cannot access internal file storage data!");
            }

            // Find the profile picture file name (same as user id)
            var user = await GetUserById(userId);
            string pfpFilePath = user.ProfilePicFilePath;
            if (pfpFilePath == null)
            {
                // User doesn't have a profile picture, return null
                return (null, null);
            }
            string fullPfpFilePath = $"{storageFilePath}/{pfpFilePath}";

            var fileStream = new FileStream(fullPfpFilePath, FileMode.Open, FileAccess.Read);
            string mimeType = _fileService.GetMimeType(fullPfpFilePath);
            return (fileStream, mimeType);
        }

        public async Task<string> GetPfpBase64(int userId)
        {
            // Get needed data from appsettings.json
            string? storageFilePath = _configuration["FileSystem:ProfilePics"];

            if (storageFilePath == null)
            {
                throw new Exception("Cannot access internal file storage data!");
            }

            // Find the profile picture file name (same as user id)
            var user = await GetUserById(userId);
            string pfpFilePath = user.ProfilePicFilePath;
            if (pfpFilePath == null)
            {
                // User doesn't have a profile picture, return null
                return null;
            }
            string fullPfpFilePath = $"{storageFilePath}/{pfpFilePath}";

            return _fileService.ConvertFileToBase64(fullPfpFilePath, FileSystemFileType.ProfileImage);
        }

        public async Task UpdatePfp(Claim claim, IFormFile profilePicture)
        {
            // Get needed data from appsettings.json
            var supportedFormats = _configuration.GetSection("FileSystem:SupportedImageFormats").Get<string[]>();
            string? storageFilePath = _configuration["FileSystem:ProfilePics"];
            int? maxProfilePicSize = Convert.ToInt32(_configuration["FileSystem:ProfilePicsSizeLimit"]);

            if (supportedFormats == null || storageFilePath == null || maxProfilePicSize == null)
            {
                throw new Exception("Cannot access internal file storage data!");
            }

            // Make sure folder exists
            Directory.CreateDirectory(storageFilePath);

            // Check if profile picture file type is supported
            string fileExtension = Path.GetExtension((profilePicture.FileName).ToLowerInvariant());
            if (!supportedFormats.Any(f => f.Equals(fileExtension, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException($"Tip naložene datoteke ({fileExtension}) ni podprt!");
            }

            // Check profile picture size limit
            if (profilePicture.Length > maxProfilePicSize)
            {
                throw new ArgumentException("Naložena datoteka je prevelika. Največa velikost datoteke je 5MB!");
            }

            // Rename image to match user id
            var user = await GetUserById(int.Parse(claim.Value));
            int userId = user.UserId;
            string profilePicFilePath = $"{userId}{fileExtension}";
            string fullProfilePicFilePath = $"{storageFilePath}/{profilePicFilePath}";

            // Delete current profile picture, if exists
            var existingFiles = Directory.GetFiles(storageFilePath, $"{userId}.*");
            foreach (var file in existingFiles)
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(Convert.ToString(userId), StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }

            // Save image to path from appsettings.json
            using (var stream = new FileStream(fullProfilePicFilePath, FileMode.Create))
            {
                await profilePicture.CopyToAsync(stream);
            }

            // Update database image
            user.ProfilePicFilePath = profilePicFilePath;
            await _context.SaveChangesAsync();
        }

        public async Task RemovePfp(Claim claim)
        {
            // Get needed data from appsettings.json
            string? storageFilePath = _configuration["FileSystem:ProfilePics"];

            if (storageFilePath == null)
            {
                throw new Exception("Cannot access internal file storage data!");
            }

            // Delete profile picture - name like userId.*
            var user = await GetUserById(int.Parse(claim.Value));
            int userId = user.UserId;
            // Delete current profile picture, if exists
            var existingFiles = Directory.GetFiles(storageFilePath, $"{userId}.*");
            foreach (var file in existingFiles)
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(Convert.ToString(userId), StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }

            // Updata database image
            user.ProfilePicFilePath = null;
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePassword(Claim claim, string password)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Geslo ne sme biti prazno!");

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            // Update password and password salt
            var user = await GetUserById(int.Parse(claim.Value));
            user.PasswordHash = hashedPassword;
            user.PasswordSalt = salt;
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserById(int userId)
        {
            User user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId) ?? throw new NotFoundException();
            return user;
        }

        public async Task<User> GetUserByUsername(string username)
        {
            User user = await _context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username) ?? throw new NotFoundException();
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            User user = await _context.User.FirstOrDefaultAsync(u => u.Email == email) ??
                throw new NotFoundException();
            return user;
        }

        public async Task<Role> GetUserRole(int userId)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId) ??
                throw new Exception("Uporabnik s tem Id ne obstaja!");
            // Load role relation
            _context.Entry(user).Reference(x => x.Role).Load();

            return user.Role;
        }

        public async Task<bool> IsAdmin(int userId)
        {
            Role role = await GetUserRole(userId);

            if (role.Name.ToLower() == "admin")
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsUploader(int userId)
        {
            Role role = await GetUserRole(userId);

            if (role.Name.ToLower() == "uploader")
            {
                return true;
            }
            return false;
        }

        public async Task DeleteUser(int userId)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId) ??
                throw new Exception("Uporabnik s tem Id ne obstaja!");
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
        }

        public RoleRequestStatus? GetRoleRequestStatus(int userId)
        {
            // TODO - db lookup
            // No request found -> return null
            return (RoleRequestStatus)new Random().Next(0, 3); ;
        }
    }
}
