namespace Shopilent.Infrastructure.Identity.Configuration.Settings;

public class PasswordOptions
{
    public int SaltSize { get; set; } = 16; // 128 bits
    public int HashSize { get; set; } = 32; // 256 bits
    public int Iterations { get; set; } = 10000;
}