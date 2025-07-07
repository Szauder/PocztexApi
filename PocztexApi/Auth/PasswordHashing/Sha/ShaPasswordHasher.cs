using System.Security.Cryptography;

namespace PocztexApi.Auth.PasswordHashing.Sha;

public class ShaPasswordHasher : IPasswordHasher
{
    public PasswordHash Hash(Password password) => SHA3_512.HashData(password.Utf8Bytes);
}