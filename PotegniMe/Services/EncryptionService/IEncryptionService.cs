namespace PotegniMe.Services.EncryptionService;

public interface IEncryptionService
{
    string Encrypt(string clearText);
    string Decrypt(string cipherText);
}