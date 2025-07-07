using System.Text;

namespace PocztexApi.Auth.PasswordHashing.Core.Types;

public record Password(string Value)
{
    public byte[] Utf8Bytes => Encoding.UTF8.GetBytes(Value);

    public static implicit operator Password(string password) => new(password);
}