namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// AES-256 encryption service interface for encrypting sensitive data at rest
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText, string key);
    string Decrypt(string cipherText, string key);
    byte[] EncryptBytes(byte[] data, string key);
    byte[] DecryptBytes(byte[] encryptedData, string key);
}
