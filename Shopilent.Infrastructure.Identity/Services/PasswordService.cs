using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Shopilent.Application.Abstractions.Identity;
using Shopilent.Infrastructure.Identity.Abstractions;
using Shopilent.Infrastructure.Identity.Configuration.Settings;

namespace Shopilent.Infrastructure.Identity.Services;

internal class PasswordService : IPasswordService
{
    private readonly PasswordOptions _options;

    public PasswordService(IOptions<PasswordOptions> options)
    {
        _options = options.Value;
    }

    public string HashPassword(string plainPassword)
    {
        // Generate a random salt
        byte[] salt = new byte[_options.SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Hash the password with the salt
        byte[] hash = HashPasswordWithSalt(plainPassword, salt);

        // Combine the salt and hash
        byte[] hashBytes = new byte[_options.SaltSize + _options.HashSize];
        Array.Copy(salt, 0, hashBytes, 0, _options.SaltSize);
        Array.Copy(hash, 0, hashBytes, _options.SaltSize, _options.HashSize);

        // Convert to Base64 string
        return Convert.ToBase64String(hashBytes);
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        // Convert the hashed password from Base64 string
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);

        // Extract the salt
        byte[] salt = new byte[_options.SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, _options.SaltSize);

        // Extract the original hash
        byte[] originalHash = new byte[_options.HashSize];
        Array.Copy(hashBytes, _options.SaltSize, originalHash, 0, _options.HashSize);

        // Hash the input password with the same salt
        byte[] compareHash = HashPasswordWithSalt(plainPassword, salt);

        // Compare the hashes
        return SlowEquals(originalHash, compareHash);
    }

    private byte[] HashPasswordWithSalt(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            password,
            salt,
            _options.Iterations,
            HashAlgorithmName.SHA256);

        return pbkdf2.GetBytes(_options.HashSize);
    }

    private static bool SlowEquals(byte[] a, byte[] b)
    {
        uint diff = (uint)a.Length ^ (uint)b.Length;
        for (int i = 0; i < a.Length && i < b.Length; i++)
        {
            diff |= (uint)(a[i] ^ b[i]);
        }

        return diff == 0;
    }
}