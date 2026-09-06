namespace MenuMate.Modules.Auth.Application.Abstractions;

internal interface IAccountActionTokenService
{
    string GenerateCode();

    string GenerateToken();

    string Hash(string secret);

    bool Verify(string secret, string expectedHash);
}
