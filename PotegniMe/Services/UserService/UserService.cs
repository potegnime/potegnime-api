namespace PotegniMe.Services.UserService
{
    public class UserService : IUserService
    {
        // Fields
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        // Constructor
        public UserService(DataContext context, IConfiguration configuration)
        {
            _context = context;
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
        public async Task UpdateUsername(string oldUsername, string newUsername)
        {
            // formatting - nothing can end with a trailing space
            oldUsername = oldUsername.Trim().ToLower();
            newUsername = newUsername.Trim().ToLower();

            // Check if username is already taken
            if (await _context.User.AnyAsync(u => u.Username == newUsername))
            {
                throw new ConflictExceptionDto("Uporabnik s tem uporabniškim imenom že obstaja!");
            }

            var user = await GetUserByUsername(oldUsername);
            user.Username = newUsername;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmail(string username, string newEmail)
        {
            // Input formatting - nothing can end with a trailing space
            newEmail = newEmail.Trim().ToLower();

            // Check if email is already taken
            if (await _context.User.AnyAsync(u => u.Email == newEmail))
            {
                throw new ConflictExceptionDto("Uporabnik s tem e-poštnim naslovom že obstaja!");
            }

            // Update email
            var user = await GetUserByUsername(username);
            user.Email = newEmail;
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePfp(string username, IFormFile profilePicture)
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

            User user = await GetUserByUsername(username);
            string profilePicFilePath = $"{username}{fileExtension}";
            string fullProfilePicFilePath = $"{storageFilePath}/{profilePicFilePath}";

            // Delete current profile picture, if exists
            var existingFiles = Directory.GetFiles(storageFilePath, $"{username}.*");
            foreach (var file in existingFiles)
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(username, StringComparison.OrdinalIgnoreCase))
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

        public async Task RemovePfp(string username)
        {
            // Get needed data from appsettings.json
            string? storageFilePath = _configuration["FileSystem:ProfilePics"];

            if (storageFilePath == null)
            {
                throw new Exception("Cannot access internal file storage data!");
            }

            // Delete profile picture - name like userId.*
            var user = await GetUserByUsername(username);
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

        public async Task UpdatePassword(string username, string password)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Geslo ne sme biti prazno!");

            string salt = BCrypt.Net.BCrypt.GenerateSalt();
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password, salt);

            // Update password and password salt
            var user = await GetUserByUsername(username);
            user.PasswordHash = hashedPassword;
            user.PasswordSalt = salt;
            await _context.SaveChangesAsync();
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

        public async Task<Role> GetUserRole(string username)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username) ??
                throw new Exception("Uporabnik s tem uporabniškim imenom ne obstaja!");
            // Load role relation
            _context.Entry(user).Reference(x => x.Role).Load();

            return user.Role;
        }

        public async Task<bool> IsAdmin(string username)
        {
            Role role = await GetUserRole(username);

            if (role.Name.ToLower() == "admin")
            {
                return true;
            }
            return false;
        }

        public async Task<bool> IsUploader(string username)
        {
            Role role = await GetUserRole(username);

            if (role.Name.ToLower() == "uploader")
            {
                return true;
            }
            return false;
        }

        public async Task DeleteUser(string username)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.Username == username) ??
                throw new Exception("Uporabnik s tem Id ne obstaja!");
            _context.User.Remove(user);
            await _context.SaveChangesAsync();
        }

        public RoleRequestStatus? GetRoleRequestStatus(int userId)
        {
            // TODO - db lookup
            // No request found -> return null
            return (RoleRequestStatus)new Random().Next(0, 3);
        }
    }
}
