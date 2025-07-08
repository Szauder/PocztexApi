namespace PocztexApi.Accounts.Core.Types;

public record Login(string Value)
{
    public static implicit operator Login(string login) => new(login);
}