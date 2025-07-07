namespace PocztexApi.Auth.Authenticate.Core.Types;

public record AuthToken(string Token)
{
    public static implicit operator AuthToken(string token) => new(token);
}