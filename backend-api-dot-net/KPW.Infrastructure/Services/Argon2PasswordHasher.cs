using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Isopoh.Cryptography.Argon2;
using KPW.Application.Interfaces;

namespace KPW.Infrastructure.Services;

public class Argon2PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return Argon2.Hash(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return Argon2.Verify(passwordHash, password);
    }
}
