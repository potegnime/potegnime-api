﻿using API.DTOs.Auth;
using API.DTOs.User;

namespace API.Services.AuthService
{
    public interface IAuthService
    {
        // Generate JWT
        Task<string> GenerateJwtToken(int userId);

        // Register user
        Task<string> RegisterAsync(UserRegisterDto user);

        // Login user
        Task<string> LoginAsync(UserLoginDto user);

        // Verify login - used to verify login credentials
        Task<bool> VerifyLogin(string username, string password);

        // Forgot password
        Task ForgotPassword(ForgotPasswordDto forgotPasswordDto);

        // Reset password
        Task<string> ResetPassword(ResetPasswordDto request);
    }
}
