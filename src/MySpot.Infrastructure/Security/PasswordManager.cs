using Microsoft.AspNetCore.Identity;
using MySpot.Application.Security;
using MySpot.Core.Entities;

namespace MySpot.Infrastructure.Security;

internal sealed class PasswordManager : IPasswordManager
{
    private readonly IPasswordHasher<User> _passwordHasher;

    public PasswordManager(IPasswordHasher<User> _passwordHasher)
    {
        this._passwordHasher = _passwordHasher;
    }
    
    public string Secure(string password)
        => _passwordHasher.HashPassword(null!, password);

    public bool Validate(string password, string securedPassword)
        => _passwordHasher.VerifyHashedPassword(null!, securedPassword, password)
            is PasswordVerificationResult.Success;
}
