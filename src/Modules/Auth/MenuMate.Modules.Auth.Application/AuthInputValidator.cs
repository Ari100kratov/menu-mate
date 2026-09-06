using System.ComponentModel.DataAnnotations;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application;

internal static class AuthInputValidator
{
    private static readonly EmailAddressAttribute EmailValidator = new();

    public static Result ValidatePassword(string password) =>
        PasswordPolicy.IsValid(password)
            ? Result.Success()
            : Result.Failure(AuthErrors.InvalidPasswordLength);

    public static Result ValidateEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && email.Length <= 320 && EmailValidator.IsValid(email)
            ? Result.Success()
            : Result.Failure(AuthErrors.InvalidEmail);

    public static Result ValidateDisplayName(string displayName) =>
        !string.IsNullOrWhiteSpace(displayName) && displayName.Trim().Length <= 120
            ? Result.Success()
            : Result.Failure(AuthErrors.InvalidDisplayName);
}
