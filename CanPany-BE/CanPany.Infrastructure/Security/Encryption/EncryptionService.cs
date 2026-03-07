using System.Security.Cryptography;
using System.Text;
using CanPany.Application.Interfaces.Services;

namespace CanPany.Infrastructure.Security.Encryption;

/// <summary>
/// AES-256-CBC encryption service with random salt and IV for each encryption.
/// Format: [16-byte salt][16-byte IV][ciphertext]
/// Key derivation: PBKDF2-SHA256 with 100,000 iterations
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int SaltSize = 16;
    private const int IvSize = 16;
    private const int KeySize = 32; // 256 bits
    private const int Pbkdf2Iterations = 100_000;

    public string Encrypt(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var salt = GenerateRandomBytes(SaltSize);
        var iv = GenerateRandomBytes(IvSize);
        var keyBytes = DeriveKey(key, salt);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        
        // Write salt + IV + ciphertext
        msEncrypt.Write(salt, 0, salt.Length);
        msEncrypt.Write(iv, 0, iv.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }

        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public string Decrypt(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var fullCipher = Convert.FromBase64String(cipherText);

        // Extract salt, IV, and ciphertext
        var salt = new byte[SaltSize];
        var iv = new byte[IvSize];
        var cipher = new byte[fullCipher.Length - SaltSize - IvSize];

        Array.Copy(fullCipher, 0, salt, 0, SaltSize);
        Array.Copy(fullCipher, SaltSize, iv, 0, IvSize);
        Array.Copy(fullCipher, SaltSize + IvSize, cipher, 0, cipher.Length);

        var keyBytes = DeriveKey(key, salt);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        
        return srDecrypt.ReadToEnd();
    }

    public byte[] EncryptBytes(byte[] data, string key)
    {
        if (data == null || data.Length == 0)
            return data ?? Array.Empty<byte>();

        var salt = GenerateRandomBytes(SaltSize);
        var iv = GenerateRandomBytes(IvSize);
        var keyBytes = DeriveKey(key, salt);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        using var msEncrypt = new MemoryStream();
        
        // Write salt + IV + ciphertext
        msEncrypt.Write(salt, 0, salt.Length);
        msEncrypt.Write(iv, 0, iv.Length);

        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(data, 0, data.Length);
        }

        return msEncrypt.ToArray();
    }

    public byte[] DecryptBytes(byte[] encryptedData, string key)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return encryptedData ?? Array.Empty<byte>();

        // Extract salt, IV, and ciphertext
        var salt = new byte[SaltSize];
        var iv = new byte[IvSize];
        var cipher = new byte[encryptedData.Length - SaltSize - IvSize];

        Array.Copy(encryptedData, 0, salt, 0, SaltSize);
        Array.Copy(encryptedData, SaltSize, iv, 0, IvSize);
        Array.Copy(encryptedData, SaltSize + IvSize, cipher, 0, cipher.Length);

        var keyBytes = DeriveKey(key, salt);

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(cipher);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var msResult = new MemoryStream();
        
        csDecrypt.CopyTo(msResult);
        return msResult.ToArray();
    }

    private static byte[] DeriveKey(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            Pbkdf2Iterations,
            HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(KeySize);
    }

    private static byte[] GenerateRandomBytes(int size)
    {
        var bytes = new byte[size];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return bytes;
    }
}
