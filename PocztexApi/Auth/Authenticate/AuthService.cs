using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PocztexApi.Auth.Authenticate;

public class AuthService(IAccountsRepository accountsRepository, IPasswordHasher passwordHasher, PocztexApi.Core.Time.ITimer timer, AuthTokenConfig authTokenConfig)
{
    static readonly JwtSecurityTokenHandler tokenHandler = new();

    public async Task<AuthToken> Authenticate(Login login, Password password)
    {
        var account = await accountsRepository.GetByLogin(login) ??
            throw new AppException(responseMessage: "Bad name or password");

        var a = account.PasswordHash.Bytes;
        var b = passwordHasher.Hash(password).Bytes;

        if (a.Equals(b))
            throw new AppException(responseMessage: "Bad name or password");

        return CreateToken(account);
    }

    AuthToken CreateToken(Account account)
    {
        return tokenHandler.WriteToken(new JwtSecurityToken(
            issuer: ClaimsIdentity.DefaultIssuer,
            expires: timer.Now.AddDays(7),
            claims: [
                new Claim("account-id", account.UniqueId.Guid.ToString())
            ],
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(authTokenConfig.Key.Bytes), SecurityAlgorithms.HmacSha256)
        ));
    }
}