namespace MenuMate.Modules.Auth.Application.Abstractions;

internal interface IAuthUrlBuilder
{
    Uri BuildPasswordResetUrl(string token);
}
