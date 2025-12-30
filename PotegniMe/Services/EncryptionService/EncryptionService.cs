using System.Security.Cryptography;
using System.Text;

namespace PotegniMe.Services.EncryptionService;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService()
    {
        var keyBase64 = Environment.GetEnvironmentVariable("POTEGNIME_AES_KEY") ?? throw new Exception($"{Constants.Constants.DotEnvErrorCode} POTEGNIME_AES_KEY");
        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length is not 32) throw new Exception($"{Constants.Constants.InternalErrorCode} AES key must be 32 bytes");
    }
    
    public string Encrypt(string clearText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();

        var plainBytes = Encoding.UTF8.GetBytes(clearText);
        var cipherBytes = encryptor.TransformFinalBlock(
            plainBytes, 0, plainBytes.Length);

        // prepend IV to ciphertext
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }
    
    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var iv = new byte[16];
        var cipherBytes = new byte[fullCipher.Length - 16];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, 16);
        Buffer.BlockCopy(fullCipher, 16, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(
            cipherBytes, 0, cipherBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }
}