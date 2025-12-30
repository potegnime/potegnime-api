using System.Globalization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using PotegniMe.Core.Exceptions;
using PotegniMe.Helpers;
using PotegniMe.Services.EmailService;
using PotegniMe.Services.EncryptionService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Services.AuthService
{
    public class AuthService(DataContext context, IUserService userService, IEmailService emailService, IEncryptionService encryptionService, IConfiguration configuration) : IAuthService
    {
        private readonly string _appKey = Environment.GetEnvironmentVariable("POTEGNIME_APP_KEY") ?? throw new Exception($"{Constants.Constants.DotEnvErrorCode} POTEGNIME_APP_KEY");

        public async Task<string> GenerateJwtToken(string username)
        {
            if (!await userService.UserExists(username)) throw new ArgumentException("Uporabnik s tem uporabniškim imenom ne obstaja!");

            User user = await context.User.FirstOrDefaultAsync(u => u.Username == username) ??
                throw new ArgumentException("Uporabnik s tem uporabniškim imenom ne obstaja!");

            return GenerateJwtTokenString(user);
        }

        public async Task<string> RegisterAsync(UserRegisterDto request)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password)
                || string.IsNullOrWhiteSpace(request.Email))
            {
                throw new ArgumentException("Uporabniške ime, e-poštni naslov in geslo so obvezni!");
            }

            // Input formatting - nothing can end with a trailing space
            request.Email = request.Email.Trim().ToLower();
            request.Username = request.Username.Trim().ToLower();
            request.Password = request.Password.Trim();

            // Check if user exists
            if (await userService.UserExists(request.Username, request.Email))
            {
                throw new ConflictException("Uporabnik s tem uporabniškim imenom ali e-poštnim naslovom že obstaja!");
            }
            string salt = GenerateSalt();
            string hashedPassword = HashPassword(request.Password, salt);
            string passkey = AuthHelper.GeneratePasskey();
            string passkeyCipher = encryptionService.Encrypt(passkey);

            Role role = context.Role.FirstOrDefault(r => r.Name == "user") ?? throw new ArgumentException("Role not found");
            User newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                ProfilePicFilePath = null,
                JoinedDate = DateTime.UtcNow,
                RoleId = role.RoleId,
                Role = role,
                PasskeyCipher = passkeyCipher
            };
            // Add new user instance to the database
            context.User.Add(newUser);
            await context.SaveChangesAsync();
            return GenerateJwtTokenString(newUser);
        }

        public async Task<string> LoginAsync(UserLoginDto request)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password)) throw new ArgumentException("Uporabniško ime in geslo sta obvezna!");

            // Input formatting - nothing cannot end with a trailing space
            request.Username = request.Username.Trim().ToLower();
            request.Password = request.Password.Trim();

            // Check if user exists
            if (!await userService.UserExists(request.Username)) throw new UnauthorizedAccessException("Napačno uporabniško ime ali geslo!");

            User user = await userService.GetUserByUsername(request.Username);
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) throw new UnauthorizedAccessException("Napačno uporabniško ime ali geslo!");
            
            return GenerateJwtTokenString(user);
        }

        public async Task<bool> VerifyLogin(string username, string password)
        {
            User user = await userService.GetUserByUsername(username);
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }

        public async Task ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                string userEmail = forgotPasswordDto.Email.Trim().ToLower();
                User user = await userService.GetUserByEmail(userEmail);
                // Generate reset token
                string token = GeneratePasswordResetToken();

                // Save token to the database
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);
                await context.SaveChangesAsync();

                // Send email
                string baseUrl = configuration["AppSettings:Audience"] ?? throw new ArgumentException("Base URL not configured!");
                if (!baseUrl.EndsWith('/')) baseUrl += '/';

                Dictionary<string, string> templateData = new()
            {
                { "username", user.Username },
                { "reset_link", $"{baseUrl}reset-password?token={token}" }
            };
                await emailService.SendEmailAsync(userEmail, templateData);
            }
            catch (NotFoundException)
            {
                // User does not exist, cannot send email
                // Do not throw an exception, as this would allow for checking if email exists in the database
            }
        }

        public async Task<string> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            string providedToken = resetPasswordDto.Token;
            string newPassword = resetPasswordDto.Password;

            // Check token validity
            User user = await context.User.FirstOrDefaultAsync(u => u.PasswordResetToken == providedToken) ??
                throw new ArgumentException("Neveljaven token za posodovitev gesla. Prosimo poskusite ponovno");

            // Check if token is expired
            if (user.PasswordResetTokenExpiration < DateTime.UtcNow)
            {
                throw new ArgumentException("Povezava za ponastavitev gesla je potekla. Prosimo poskusite ponovno");
            }

            // Reset password and delete token + expiration
            user.PasswordHash = HashPassword(newPassword, user.PasswordSalt);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            await context.SaveChangesAsync();
            return GenerateJwtTokenString(user);
        }

        // Helper methods
        private string GenerateJwtTokenString(User user)
        {
            context.Entry(user).Reference(u => u.Role).Load();
            bool hasPfp = user.ProfilePicFilePath != null;

            List<Claim> claims = new List<Claim>
            {
                new Claim("uid", user.UserId.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email),
                new Claim("role", user.Role.Name.ToLower()),
                new Claim("joined", user.JoinedDate.ToString("O", CultureInfo.InvariantCulture)),
                new Claim("hasPfp", hasPfp ? "true" : "false")
            };

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds,
                issuer: configuration.GetSection("AppSettings:Issuer").Value,
                audience: configuration.GetSection("AppSettings:Audience").Value
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        private string HashPassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        private string GeneratePasswordResetToken()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] tokenData = new byte[32];
                rng.GetBytes(tokenData);
                return Convert.ToBase64String(tokenData);
            }
        }
    }
}
