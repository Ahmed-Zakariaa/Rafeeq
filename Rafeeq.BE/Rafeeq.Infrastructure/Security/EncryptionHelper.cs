using System.Security.Cryptography;
using System.Text;

namespace Rafeeq.Infrastructure.Security;

/// <summary>
/// Encrypts integer IDs for use in URLs (so raw DB ids are never exposed). AES-256 + random IV.
/// NOTE: the key is hardcoded for the v1 scaffold — move it to configuration/secret before any real deploy.
/// </summary>
public static class EncryptionHelper
{
    // Dev default (exactly 32 bytes). Override from config in real environments via Configure().
    private static byte[] _key = Encoding.UTF8.GetBytes("Rafeeq_v1_Static_Key_32bytes!!__");

    /// <summary>Set the AES key from a configured secret (any length → hashed to 32 bytes). Call once at startup.</summary>
    public static void Configure(string secret)
    {
        if (!string.IsNullOrWhiteSpace(secret))
            _key = SHA256.HashData(Encoding.UTF8.GetBytes(secret));
    }

    public static string EncryptToUrl(int id) => Encrypt(id.ToString());
    public static int DecryptFromUrl(string cipher) => int.Parse(Decrypt(cipher));

    public static string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        using var enc = aes.CreateEncryptor();
        var plain = Encoding.UTF8.GetBytes(plainText);
        var cipher = enc.TransformFinalBlock(plain, 0, plain.Length);
        var result = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, result, aes.IV.Length, cipher.Length);
        return Base64UrlEncode(result);
    }

    public static string Decrypt(string cipherText)
    {
        var bytes = Base64UrlDecode(cipherText);
        using var aes = Aes.Create();
        aes.Key = _key;
        var iv = new byte[16];
        Buffer.BlockCopy(bytes, 0, iv, 0, 16);
        aes.IV = iv;
        using var dec = aes.CreateDecryptor();
        var plain = dec.TransformFinalBlock(bytes, 16, bytes.Length - 16);
        return Encoding.UTF8.GetString(plain);
    }

    private static string Base64UrlEncode(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] Base64UrlDecode(string s)
    {
        s = s.Replace('-', '+').Replace('_', '/');
        switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; }
        return Convert.FromBase64String(s);
    }
}
