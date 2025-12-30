namespace PotegniMe.Services.UserService
{
    public class UserService(DataContext context, IConfiguration configuration) : IUserService
    {

        // Methods
        public async Task<List<User>> GetAllUsers()
        {
            try
            {
                return await context.User.ToListAsync();
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
                var user = await context.User.FirstOrDefaultAsync(u => u.Username == username || u.Email == email);
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
                var user = await context.User.FirstOrDefaultAsync(u => u.Username == username);
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
                var user = await context.User.FirstOrDefaultAsync(u => u.UserId == userId);
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
            if (await context.User.AnyAsync(u => u.Username == newUsername))
            {
                throw new ConflictExceptionDto("Uporabnik s tem uporabniškim imenom že obstaja!");
            }

            var user = await GetUserByUsername(oldUsername);
            user.Username = newUsername;
            await context.SaveChangesAsync();
        }

        public async Task UpdateEmail(string username, string newEmail)
        {
            // Input formatting - nothing can end with a trailing space
            newEmail = newEmail.Trim().ToLower();

            // Check if email is already taken
            if (await context.User.AnyAsync(u => u.Email == newEmail))
            {
                throw new ConflictExceptionDto("Uporabnik s tem e-poštnim naslovom že obstaja!");
            }

            // Update email
            var user = await GetUserByUsername(username);
            user.Email = newEmail;
            await context.SaveChangesAsync();
        }

        public async Task UpdatePfp(string username, IFormFile profilePicture)
        {
            // Get needed data from appsettings.json
            var supportedFormats = configuration.GetSection("FileSystem:SupportedImageFormats").Get<string[]>();
            string? storageFilePath = configuration["FileSystem:ProfilePics"];
            int? maxProfilePicSize = Convert.ToInt32(configuration["FileSystem:ProfilePicsSizeLimit"]);

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
            await context.SaveChangesAsync();
        }

        public async Task RenamePfp(string oldUsername, string newUsername)
        {
            string? storageFilePath = configuration["FileSystem:ProfilePics"];
            if (string.IsNullOrEmpty(storageFilePath)) throw new Exception("Cannot access internal file storage data!");

            var user = await GetUserByUsername(newUsername);

            var existingFiles = Directory.GetFiles(storageFilePath, $"{oldUsername}.*");
            if (existingFiles.Length == 0) return;

            // Rename the file
            foreach (var oldFilePath in existingFiles)
            {
                string extension = Path.GetExtension(oldFilePath);
                string newFilePath = Path.Combine(storageFilePath, $"{newUsername}{extension}");
                File.Move(oldFilePath, newFilePath);
                user.ProfilePicFilePath = $"{newUsername}{extension}";
            }

            await context.SaveChangesAsync();
        }

        public async Task RemovePfp(string username)
        {
            // Get needed data from appsettings.json
            string? storageFilePath = configuration["FileSystem:ProfilePics"];

            if (storageFilePath == null)
            {
                throw new Exception("Cannot access internal file storage data!");
            }

            // Delete profile picture - name like username.*
            // Delete current profile picture, if exists
            var existingFiles = Directory.GetFiles(storageFilePath, $"{username}.*");
            foreach (var file in existingFiles)
            {
                if (Path.GetFileNameWithoutExtension(file).Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(file);
                }
            }

            // Update database image
            var user = await GetUserByUsername(username);
            user.ProfilePicFilePath = null;
            await context.SaveChangesAsync();
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
            await context.SaveChangesAsync();
        }

        public async Task<User> GetUserByUsername(string username)
        {
            User user = await context.User
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username) ?? throw new NotFoundException();
            return user;
        }

        public async Task<User> GetUserByEmail(string email)
        {
            User user = await context.User.FirstOrDefaultAsync(u => u.Email == email) ??
                throw new NotFoundException();
            return user;
        }

        public async Task<Role> GetUserRole(string username)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.Username == username) ??
                throw new Exception("Uporabnik s tem uporabniškim imenom ne obstaja!");
            // Load role relation
            context.Entry(user).Reference(x => x.Role).Load();

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
            return role.Name.ToLower() == "uploader";
        }

        public async Task DeleteUser(string username)
        {
            var user = await context.User.FirstOrDefaultAsync(u => u.Username == username) ??
                throw new Exception("Uporabnik s tem uporabniškim imenom ne obstaja!");
            context.User.Remove(user);
            await context.SaveChangesAsync();
        }

        public RoleRequestStatus? GetRoleRequestStatus(int userId)
        {
            // TODO - db lookup
            // No request found -> return null
            return (RoleRequestStatus) new Random().Next(0, 3);
        }
    }
}
