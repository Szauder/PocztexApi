using PocztexApi.Auth.PasswordHashing.Core.Types;

namespace PocztexApi.Auth.PasswordHashing.Core;

public interface IPasswordHasher
{
    PasswordHash Hash(Password password);
}