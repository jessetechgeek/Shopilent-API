namespace Shopilent.Infrastructure.Identity.Abstractions;

internal interface IPasswordService
{
    string HashPassword(string plainPassword);
    bool VerifyPassword(string plainPassword, string hashedPassword);
}