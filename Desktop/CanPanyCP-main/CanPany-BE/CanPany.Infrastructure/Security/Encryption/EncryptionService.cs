using System.Security.Cryptography;
using System.Text;

namespace CanPany.Infrastructure.Security.Encryption;

/// <summary>
/// AES-256 encryption service
/// </summary>
public interface IEncryptionService
{
    string Encrypt(string plainText, string key);
    string Decrypt(string cipherText, string key);
    byte[] EncryptBytes(byte[] data, string key);
    byte[] DecryptBytes(byte[] encryptedData, string key);
}

public class EncryptionService : IEncryptionService
{
    public string Encrypt(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;

        var keyBytes = DeriveKey(key, 32); // 32 bytes = 256 bits for AES-256
        var iv = new byte[16]; // AES block size is 128 bits = 16 bytes
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            using (var msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(iv, 0, iv.Length); // Prepend IV
                
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }

                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    public string Decrypt(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;

        var keyBytes = DeriveKey(key, 32);
        var fullCipher = Convert.FromBase64String(cipherText);
        var iv = new byte[16];
        var cipher = new byte[fullCipher.Length - 16];

        Array.Copy(fullCipher, 0, iv, 0, 16);
        Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            using (var msDecrypt = new MemoryStream(cipher))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
        }
    }

    public byte[] EncryptBytes(byte[] data, string key)
    {
        if (data == null || data.Length == 0)
            return data ?? Array.Empty<byte>();

        var keyBytes = DeriveKey(key, 32);
        var iv = new byte[16];
        
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(iv);
        }

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var encryptor = aes.CreateEncryptor())
            using (var msEncrypt = new MemoryStream())
            {
                msEncrypt.Write(iv, 0, iv.Length);
                
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(data, 0, data.Length);
                }

                return msEncrypt.ToArray();
            }
        }
    }

    public byte[] DecryptBytes(byte[] encryptedData, string key)
    {
        if (encryptedData == null || encryptedData.Length == 0)
            return encryptedData ?? Array.Empty<byte>();

        var keyBytes = DeriveKey(key, 32);
        var iv = new byte[16];
        var cipher = new byte[encryptedData.Length - 16];

        Array.Copy(encryptedData, 0, iv, 0, 16);
        Array.Copy(encryptedData, 16, cipher, 0, cipher.Length);

        using (var aes = Aes.Create())
        {
            aes.Key = keyBytes;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using (var decryptor = aes.CreateDecryptor())
            using (var msDecrypt = new MemoryStream(cipher))
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (var msResult = new MemoryStream())
            {
                csDecrypt.CopyTo(msResult);
                return msResult.ToArray();
            }
        }
    }

    private byte[] DeriveKey(string password, int keyLength)
    {
        // Use PBKDF2 to derive key from password
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("CanPanySalt2024"), 10000, HashAlgorithmName.SHA256))
        {
            return pbkdf2.GetBytes(keyLength);
        }
    }
}

