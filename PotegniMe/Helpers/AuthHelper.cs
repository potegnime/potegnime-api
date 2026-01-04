using System.Security.Cryptography;

namespace PotegniMe.Helpers;

public static class AuthHelper
{
    private const string AllowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int PasskeyLength = 40;

    public static string GeneratePasskey()
    {
        var bytes = new byte[PasskeyLength];
        RandomNumberGenerator.Fill(bytes);
        var chars = new char[PasskeyLength];
        for (int i = 0; i < PasskeyLength; i++)
        {
            chars[i] = AllowedChars[bytes[i] % AllowedChars.Length];
        }

        return new string(chars);
    }

    public static string GeneratePasswordResetToken()
    {
        // same thing just a random string
        return  GeneratePasskey();
    }

    public static RSA LoadPrivateKey(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Private key file not found", path);
        string pem = File.ReadAllText(path);
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return rsa;
    }

    public static RSA LoadPublicKey(string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Public key file not found", path);
        string pem = File.ReadAllText(path);
        RSA rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        return rsa;
    }
}