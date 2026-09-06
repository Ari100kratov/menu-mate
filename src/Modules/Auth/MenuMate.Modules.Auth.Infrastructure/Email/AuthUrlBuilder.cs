using MenuMate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace MenuMate.Modules.Auth.Infrastructure.Email;

internal sealed class AuthUrlBuilder(IOptions<PublicWebOptions> options) : IAuthUrlBuilder
{
    private readonly string _baseUrl = options.Value.BaseUrl.TrimEnd('/');

    public Uri BuildPasswordResetUrl(string token) => new(
        $"{_baseUrl}/reset-password#token={Uri.EscapeDataString(token)}",
        UriKind.Absolute);
}
