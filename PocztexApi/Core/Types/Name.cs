namespace PocztexApi.Core.Types;

public record Name(string Value)
{
    public static implicit operator Name(string name) => new(name);
}