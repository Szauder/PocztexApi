using PocztexApi.Core.Types;

namespace PocztexApi.Auth.PasswordHashing.Core.Types;

public record PasswordHash(byte[] Bytes) : SecretBase(Bytes)
{
    public static implicit operator PasswordHash(byte[] bytes) => new(bytes);
}