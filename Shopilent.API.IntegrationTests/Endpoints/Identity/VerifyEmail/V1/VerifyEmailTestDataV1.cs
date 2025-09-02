using Bogus;

namespace Shopilent.API.IntegrationTests.Endpoints.Identity.VerifyEmail.V1;

public static class VerifyEmailTestDataV1
{
    private static readonly Faker _faker = new();

    public static string CreateValidToken()
    {
        return Guid.NewGuid().ToString("N");
    }

    public static string CreateEmptyToken()
    {
        return string.Empty;
    }

    public static string CreateNullToken()
    {
        return null!;
    }

    public static string CreateWhitespaceToken()
    {
        return "   ";
    }

    public static string CreateInvalidFormatToken()
    {
        return "invalid-token-format";
    }

    public static string CreateExpiredToken()
    {
        return "expired-token-12345678901234567890123456789012";
    }

    public static string CreateAlreadyUsedToken()
    {
        return "used-token-12345678901234567890123456789012";
    }

    public static string CreateNonExistentToken()
    {
        return "nonexistent-12345678901234567890123456789012";
    }

    public static class EdgeCases
    {
        public static string CreateExtremelyLongToken()
        {
            return new string('a', 1000);
        }

        public static string CreateTokenWithSpecialCharacters()
        {
            return "token-with-special!@#$%^&*()characters";
        }

        public static string CreateTokenWithUnicodeCharacters()
        {
            return "tökén-wïth-üñïçödé-çhàrâçtérs";
        }

        public static string CreateTokenWithSpaces()
        {
            return "token with spaces in middle";
        }

        public static string CreateTokenWithNewlines()
        {
            return "token\nwith\nnewlines";
        }

        public static string CreateTokenWithTabs()
        {
            return "token\twith\ttabs";
        }
    }
}