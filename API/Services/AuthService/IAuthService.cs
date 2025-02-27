﻿using API.DTOs.Auth;
using API.DTOs.User;

namespace API.Services.AuthService
{
    public interface IAuthService
    {
        // Generate JWT
        Task<string> GenerateJwtToken(int userId);

        // Register user
        // Validate user doesn't exist yet, validate email is not banned, generate salt, hash password, create user, return user
        Task<string> RegisterAsync(UserRegisterDto user);

        // Login user
        // Validate user exists, validate user is not banned, validate password, generate token, return token
        Task<string> LoginAsync(UserLoginDto user);

        // Verify login
        // Used to verify login credentials
        Task<bool> VerifyLogin(string username, string password);

        // Forgot password
        // Returns true if user exists (email sent) and false if user does not exist (email not sent)
        Task<bool> ForgotPassword(string userEmail);
    }
}
