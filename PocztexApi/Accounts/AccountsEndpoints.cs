using Microsoft.AspNetCore.Mvc;
using PocztexApi.Auth.Authenticate;

public static class AccountsEndpoints
{
    public record RegisterAccountRequestDto(string Name, string Password);

    public static async Task<IResult> Register([FromBody] RegisterAccountRequestDto request, AccountsService accountsService)
    {
        var account = await accountsService.RegisterAccount(request.Name, request.Password);

        return Results.Ok(account);
    }   

    public record LoginRequestDto(string Name, string Password);

    public static async Task<IResult> Login(HttpContext http, [FromBody] LoginRequestDto request, AuthService authService)
    {
        var token = await authService.Authenticate(request.Name, request.Password);

        http.Response.Cookies.Append("Auth", token.Token, new CookieOptions
        {
            Domain = "localhost",
            HttpOnly = true,
            MaxAge = TimeSpan.FromDays(1),
            Secure = true,
            IsEssential = true,
            SameSite = SameSiteMode.Strict,
        });

        return Results.Ok(new { AuthToken = token.Token });
    }
}
