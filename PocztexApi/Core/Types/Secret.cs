namespace PocztexApi.Core.Types;

public record Secret(byte[] Bytes) : SecretBase(Bytes)
{
    public static implicit operator Secret(byte[] bytes) => new(bytes);
}
