namespace PocztexApi.Core.Types;

public record UniqueId(Guid Guid)
{
    public static readonly UniqueId Empty = new(Guid.Empty);

    public static implicit operator UniqueId(Guid guid) => new(guid);

    public static UniqueId CreateNew() => new(Guid.NewGuid());

    public static UniqueId Parse(string str) => Guid.Parse(str);
}