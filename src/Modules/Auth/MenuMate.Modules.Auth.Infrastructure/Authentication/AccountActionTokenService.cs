using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using MenuMate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal sealed class AccountActionTokenService(IOptions<AccountActionOptions> options)
    : IAccountActionTokenService
{
    private readonly byte[] _secret = Encoding.UTF8.GetBytes(options.Value.HashSecret);

    public string GenerateCode() => RandomNumberGenerator.GetInt32(0, 1_000_000)
        .ToString("D6", CultureInfo.InvariantCulture);

    public string GenerateToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(48))
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

    public string Hash(string secret)
    {
        byte[] hash = HMACSHA256.HashData(_secret, Encoding.UTF8.GetBytes(secret));
        return Convert.ToHexString(hash);
    }

    public bool Verify(string secret, string expectedHash)
    {
        byte[] actual = Convert.FromHexString(Hash(secret));
        byte[] expected;
        try
        {
            expected = Convert.FromHexString(expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
