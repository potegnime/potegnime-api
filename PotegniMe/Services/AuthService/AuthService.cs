using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using PotegniMe.Services.EmailService;
using PotegniMe.Services.UserService;

namespace PotegniMe.Services.AuthService
{
    public class AuthService : IAuthService
    {
        // Fields
        public readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IUserService _userService;
        private readonly IEmailService _emailService;
        private readonly String _appKey;

        // Constructor
        public AuthService(DataContext context, IUserService userService, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _userService = userService;
            _emailService = emailService;
            _configuration = configuration;
            _appKey = Environment.GetEnvironmentVariable("POTEGNIME_APP_KEY") ??
                      throw new Exception("Cannot find TMDB API KEY");
        }

        // Methods
        public async Task<string> GenerateJwtToken(string username)
        {
            if (!await _userService.UserExists(username))
            {
                throw new ArgumentException("Uporabnik s tem uporabniškim imenom ne obstaja!");
            }

            User user = await _context.User.FirstOrDefaultAsync(u => u.Username == username) ??
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
            if (await _userService.UserExists(request.Username, request.Email))
            {
                throw new ConflictExceptionDto("Uporabnik s tem uporabniškim imenom ali e-poštnim naslovom že obstaja!");
            }
            string salt = GenerateSalt();
            string hashedPassword = HashPassword(request.Password, salt);

            Role role = _context.Role.FirstOrDefault(r => r.Name == "user") ??
                throw new ArgumentException("Role not found");
            User newUser = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = hashedPassword,
                PasswordSalt = salt,
                ProfilePicFilePath = null,
                JoinedDate = DateTime.UtcNow,
                RoleId = role.RoleId,
                Role = role
            };
            // Add new user instance to the database
            _context.User.Add(newUser);
            await _context.SaveChangesAsync();
            // Save changes to the database
            await _context.SaveChangesAsync();
            return GenerateJwtTokenString(newUser);
        }

        public async Task<string> LoginAsync(UserLoginDto request)
        {
            // Input validation
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Uporabniško ime in geslo sta obvezna!");
            }

            // Input formatting - nothing cannot end with a trailing space
            request.Username = request.Username.Trim().ToLower();
            request.Password = request.Password.Trim();

            // Check if user exists
            if (!await _userService.UserExists(request.Username))
            {
                throw new ArgumentException("Napačno uporabniško ime ali geslo!");
            }

            // Get user by username
            User user = await _userService.GetUserByUsername(request.Username);

            // Check if password is correct
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                throw new ArgumentException("Napačno uporabniško ime ali geslo!");
            }

            // Generate JWT token
            return GenerateJwtTokenString(user);
        }

        public async Task<bool> VerifyLogin(string username, string password)
        {
            // Get user by username
            User user = await _userService.GetUserByUsername(username);

            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return false;
            }
            return true;
        }

        public async Task ForgotPassword(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                string userEmail = forgotPasswordDto.Email.Trim().ToLower();
                User user = await _userService.GetUserByEmail(userEmail);
                // Generate reset token
                string token = GeneratePasswordResetToken();

                // Save token to the database
                user.PasswordResetToken = token;
                user.PasswordResetTokenExpiration = DateTime.UtcNow.AddHours(1);
                await _context.SaveChangesAsync();

                // Send email
                string baseUrl = _configuration["AppSettings:Audience"] ??
                    throw new ArgumentException("Base URL not configured!");
                if (!baseUrl.EndsWith('/')) baseUrl += '/';

                Dictionary<string, string> templateData = new()
            {
                { "username", user.Username.ToString() },
                { "reset_link", $"{baseUrl}ponastavi-geslo?token={token}" }
            };
                await _emailService.SendEmailAsync(userEmail, templateData);
            }
            catch (NotFoundException)
            {
                // User does not exist, cannot send email
                // Do not throw an exception, as this would allow for checking if email exists in the database
                return;
            }
            catch (SendGridLimitException)
            {
                throw new SendGridLimitException();
            }
        }

        public async Task<string> ResetPassword(ResetPasswordDto resetPasswordDto)
        {
            string providedToken = resetPasswordDto.Token;
            string newPassword = resetPasswordDto.Password;

            // Check token validity
            User user = await _context.User.FirstOrDefaultAsync(u => u.PasswordResetToken == providedToken) ??
                throw new InvalidTokenException("Neveljaven token za posodovitev gesla. Prosimo poskusite ponovno");

            // Check if token is expired
            if (user.PasswordResetTokenExpiration < DateTime.UtcNow)
            {
                throw new ExpiredTokenException("Povezava za ponastavitev gesla je potekla. Prosimo poskusite ponovno");
            }

            // Reset password and delete token + expiration
            user.PasswordHash = HashPassword(newPassword, user.PasswordSalt);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiration = null;
            await _context.SaveChangesAsync();
            return GenerateJwtTokenString(user);
        }

        // Helper methods
        private string GenerateJwtTokenString(User user)
        {
            _context.Entry(user).Reference(u => u.Role).Load();

            List<Claim> claims = new List<Claim>
            {
                new Claim("uid", user.UserId.ToString()),
                new Claim("username", user.Username.ToString()),
                new Claim("email", user.Email.ToString()),
                new Claim("role", user.Role.Name.ToString().ToLower()),
                new Claim("joined", user.JoinedDate.ToString())
            };

            RoleRequestStatus? uploaderRequestStatus = _userService.GetRoleRequestStatus(user.UserId);
            if (uploaderRequestStatus is RoleRequestStatus status)
            {
                claims.Add(new Claim("uploaderRequestStatus", status.ToString().ToLower()));
            }

            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appKey));
            SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            JwtSecurityToken token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds,
                issuer: _configuration.GetSection("AppSettings:Issuer").Value,
                audience: _configuration.GetSection("AppSettings:Audience").Value
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
