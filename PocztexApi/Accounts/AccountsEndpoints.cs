using Microsoft.AspNetCore.Mvc;
using PocztexApi.Core.Models;
using System.Security.Claims;

public class AccountsEndpoints : CrudEndpoints<Account>
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

    public static void Logout(HttpContext http)
    {
        http.Response.Cookies.Delete("Auth");
    }

    public static async Task<IResult> CurrentAccount(ClaimsPrincipal principal, IAccountsRepository accountsRepository)
    {
        var claim = principal.Claims.SingleOrDefault(c => c.Type == "account-id");

        if (claim is not null)
        {
            var account = await accountsRepository.GetByUniqueId(UniqueId.Parse(claim.Value));

            if (account is not null)
                return Results.Ok(AccountDto.CreateFromDomain(account));
        }

        return Results.Unauthorized();
    }

    public static async Task<IResult> GetAccounts(IAccountsRepository accountsRepository, [FromQuery(Name = "locked")] bool? filterLocked = null)
    {
        var accounts = await accountsRepository.GetByFilters(filterLocked);
        return Results.Ok(accounts.Select(a => (object)AccountDto.CreateFromDomain(a)));
    }

    public static Task Create(CreateAccountDto requestDto, AccountsService accountsService)
    {
        return accountsService.RegisterAccount(requestDto.Login, requestDto.Password);
    }

    public static Task<IResult> Delete(IRepository<Account> repository, [FromBody] DeleteRequestDto requestDto) =>
        GenericDeleteByUniqueId(repository, requestDto);

    public static Task<IResult> GetByUniqueId(IRepository<Account> repository, [FromRoute] Guid id) =>
        GenericGetByUniqueId<AccountDto>(repository, id);

    public static Task<IResult> GetList(IRepository<Account> repository) =>
        GenericGetAll<AccountDto>(repository);
}

public class CrudEndpoints<TDomain> where TDomain : IModel
{
    protected static async Task<IResult> GenericGetAll<TDto>(IRepository<TDomain> repository) where TDto : IDto<TDomain>
    {
        var models = await repository.GetAll();
        return Results.Ok(models.Select(m => (object)TDto.CreateFromDomain(m)));
    }

    protected static async Task<IResult> GenericGetByUniqueId<TDto>(IRepository<TDomain> repository, [FromRoute] Guid id) where TDto : IDto<TDomain>
    {
        var model = await repository.GetByUniqueId(id);
        return model is null ? Results.NotFound() : Results.Ok(TDto.CreateFromDomain(model));
    }

    protected static async Task<IResult> GenericCreateFromDto<TCreateRequestDto, TDto>(IRepository<TDomain> repository, IEnumerable<ICreateRule<TDomain>> rules, [FromBody] TCreateRequestDto requestDto)
            where TCreateRequestDto : ICreateRequestDto<TDomain> where TDto : IDto<TDomain>
    {
        var domain = requestDto.CreateDomain();

        foreach (var rule in rules)
            await rule.ValidateRule(domain);

        return Results.Ok(TDto.CreateFromDomain(await repository.CreateAndReturn(domain)));
    }

    protected static async Task<IResult> GenericDeleteByUniqueId(IRepository<TDomain> repository, [FromBody] DeleteRequestDto requestDto)
    {
        await repository.DeleteByUniqueId(requestDto.Id);
        return Results.Ok();
    }
}

public interface IDto<TDomain>
{
    abstract static IDto<TDomain> CreateFromDomain(TDomain domain);
}

public interface ICreateRequestDto<TDomain>
{
    TDomain CreateDomain();
}

public record DeleteRequestDto(
    Guid Id
);

public record AccountDto(
    Guid Id,
    bool Locked,
    string Login
) : IDto<Account>
{
    public static IDto<Account> CreateFromDomain(Account account) => new AccountDto(
        account.UniqueId.Guid,
        account.Locked,
        account.Login.Value
    );
}

public record Shipment(
    UniqueId UniqueId,
    double Weight, // in kg
    int Width,  // in mm
    int Height, // in mm
    int Deep,   // in mm
    string Description,

    ClientData Addressee
) : IModel;

public enum ShipmentEventType
{
    Acceptance,
    SentToDelivery,
    Delivered,
}

public static class ShipmentEventTypeHelper
{
    public static string ToString(ShipmentEventType t)
    {
        return t switch
        {
            ShipmentEventType.Acceptance => "acceptance",
            ShipmentEventType.SentToDelivery => "sent",
            ShipmentEventType.Delivered => "delivered",

            _ => throw new AppException()
        };
    }

    public static ShipmentEventType ToType(string t)
    {
        return t switch
        {
            "acceptance" => ShipmentEventType.Acceptance,
            "sent" => ShipmentEventType.SentToDelivery,
            "delivered" => ShipmentEventType.Delivered,

            _ => throw new AppException()
        };
    }
}

public record ShipmentEvent(
    UniqueId UniqueId,
    UniqueId ShipmentId,
    ShipmentEventType Type,
    DateTime Time
) : IModel;

public record Payment(
    UniqueId UniqueId,
    decimal Amount
) : IModel;

public record PaymentEvent(
    UniqueId UniqueId,
    UniqueId PaymentUniqueId, 
    PaymentStatus Status, 
    DateTime Time
) : IModel;

public enum PaymentStatus
{
    WaitForPayment,
    Payd
}

public record ClientData(
    string Name,
    string Surname,

    string Street,
    string City,
    string PostalCode
);
