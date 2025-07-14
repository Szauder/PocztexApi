using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PocztexApi.Accounts.Seed;
using PocztexApi.Core.Models;
using PocztexApi.Shared.Repositories.InMemory;
using System.Security.Claims;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

var authTokenConfig = new AuthTokenConfig(Key: Secret.FromUtf8("12345678901234561234567890123456"));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme,
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Trace);
});

builder.Services.AddAuthorization()
    .AddAuthentication(config =>
    {
        config.DefaultScheme = BearerTokenDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(BearerTokenDefaults.AuthenticationScheme, config =>
    {
        config.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(authTokenConfig.Key.Bytes)
        };

        config.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                context.Token = context.Request.Cookies["Auth"];
                return Task.CompletedTask;
            }
        };

        config.Validate();
    });

builder.Services.AddScoped<ExceptionsHandlingMiddleware>();

builder.Services.AddSingleton<PocztexApi.Core.Time.ITimer, SystemTimer>();

builder.Services.AddSingleton<IPasswordHasher, ShaPasswordHasher>();

builder.Services.AddSingleton<IAccountsRepository, AccountsInMemoryRepository>();
builder.Services.AddSingleton<IRepository<Account>, AccountsInMemoryRepository>();
builder.Services.AddScoped<AccountsService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddSingleton(authTokenConfig);

builder.Services.AddHostedService<SeederBackgroundProces>();
builder.Services.AddSingleton<ISeeder, AccountsSeeder>();
builder.Services.AddSingleton<SeedReader>();
builder.Services.AddScoped<ICreateRule<Account>, AccountsCreateRules.AccountWithThisSameLoginShouldNotExist>();
builder.Services.AddScoped<ICreateRule<Employee>, EmployeeCreateRules.ShouldNotExistOtherEmployeeWithThisSameNameOrSurnameOrPeselOrDocumentNumber>();

builder.Services.AddSingleton<IRepository<Employee>, CrudInMemoryRepository<Employee>>();
builder.Services.AddSingleton<IRepository<Shipment>, CrudInMemoryRepository<Shipment>>();
builder.Services.AddSingleton<IRepository<ShipmentEvent>, CrudInMemoryRepository<ShipmentEvent>>();
builder.Services.AddSingleton<IRepository<Payment>, CrudInMemoryRepository<Payment>>();
builder.Services.AddSingleton<IRepository<PaymentEvent>, CrudInMemoryRepository<PaymentEvent>>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app
        .UseSwagger(options =>
        {
            options.RouteTemplate = "docs/{documentname}/swagger.json";
        })
        .UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/docs/v1/swagger.json", "PocztexApi");
            options.RoutePrefix = "docs";
        });
}

app.UseCors();
app.UseMiddleware<ExceptionsHandlingMiddleware>();

app.MapGet("/hello", () => Results.Text($"Hello, {DateTimeOffset.UtcNow}")).AllowAnonymous().RequireCors();
app.MapGet("/helloauth", (ClaimsPrincipal principal) => Results.Text($"Hello {principal.Claims.Single(c => c.Type == "account-id").Value}, {DateTimeOffset.UtcNow}")).RequireAuthorization().RequireCors();

app.MapPost("/auth/register", AccountsEndpoints.Register).AllowAnonymous().RequireCors();
app.MapPost("/auth/login", AccountsEndpoints.Login).AllowAnonymous().RequireCors();
app.MapPost("/auth/logout", AccountsEndpoints.Logout).AllowAnonymous().RequireCors();
app.MapGet("/auth/currentaccount", AccountsEndpoints.CurrentAccount).AllowAnonymous().RequireCors();

app.MapGet("/account/list", AccountsEndpoints.GetList).RequireAuthorization().RequireCors();
app.MapGet("/account/{id}", AccountsEndpoints.GetByUniqueId).RequireAuthorization().RequireCors();
app.MapPost("/account", AccountsEndpoints.Create).RequireAuthorization().RequireCors();
app.MapDelete("/account", AccountsEndpoints.Delete).RequireAuthorization().RequireCors();

app.MapGet("/employee/list", EmployesEndpoints.GetList);
app.MapGet("/employee/{id}", EmployesEndpoints.GetByUniqueId);
app.MapPost("/employee", EmployesEndpoints.Create);
app.MapDelete("/employee", EmployesEndpoints.Delete);

app.MapGet("/shipment/list", ShipmentEndpoints.GetList);
app.MapGet("/shipment/{id}", ShipmentEndpoints.GetByUniqueId);
app.MapPost("/shipment", ShipmentEndpoints.Create);
app.MapDelete("/shipment", ShipmentEndpoints.Delete);

app.MapGet("/shipmentevent/list", ShipmentEventEndpoints.GetList);
app.MapGet("/shipmentevent/{id}", ShipmentEventEndpoints.GetByUniqueId);
app.MapPost("/shipmentevent", ShipmentEventEndpoints.Create);
app.MapDelete("/shipmentevent", ShipmentEventEndpoints.Delete);

app.MapGet("/payment/list", PaymentEndpoints.GetList);
app.MapGet("/payment/{id}", PaymentEndpoints.GetByUniqueId);
app.MapPost("/payment", PaymentEndpoints.Create);
app.MapDelete("/payment", PaymentEndpoints.Delete);

app.MapGet("/paymentevent/list", PaymentEventEndpoints.GetList);
app.MapGet("/paymentevent/{id}", PaymentEventEndpoints.GetByUniqueId);
app.MapPost("/paymentevent", PaymentEventEndpoints.Create);
app.MapDelete("/paymentevent", PaymentEventEndpoints.Delete);

app.Run();

public record ClientDataDto(
    string Name,
    string Surname,

    string Street,
    string City,
    string PostalCode
)
{
    public ClientData ToDomain() => new(Name, Surname, Street, City, PostalCode);

    public static ClientDataDto CreateFromDomain(ClientData domain) => new(domain.Name, domain.Surname, domain.Street, domain.City, domain.PostalCode);
}

public record CreateShipmentRequestDto(double Weight, int Width, int Height, int Deep, string Description, ClientDataDto ClientData) : ICreateRequestDto<Shipment>
{
    public Shipment CreateDomain()
    {
        return new Shipment(
            PocztexApi.Core.Types.UniqueId.CreateNew(),
            Weight,
            Width,
            Height,
            Deep,
            Description,
            ClientData.ToDomain()
        );
    }
}

public record ShipmentDto(
    Guid Id,
    double Weight, 
    int Width, 
    int Height, 
    int Deep, 
    string Description, 
    ClientDataDto Addressee
) : IDto<Shipment>
{
    public static IDto<Shipment> CreateFromDomain(Shipment domain)
    {
        return new ShipmentDto(
            domain.UniqueId.Guid, 
            domain.Weight, 
            domain.Width, 
            domain.Height, 
            domain.Deep, 
            domain.Description, 
            ClientDataDto.CreateFromDomain(domain.Addressee)
        );
    }
}

public class ShipmentEndpoints : CrudEndpoints<Shipment>
{
    public static Task<IResult> Create(IRepository<Shipment> repository, IEnumerable<ICreateRule<Shipment>> rules, [FromBody] CreateShipmentRequestDto requestDto) =>
        GenericCreateFromDto<CreateShipmentRequestDto, ShipmentDto>(repository, rules, requestDto);

    public static Task<IResult> Delete(IRepository<Shipment> repository, [FromBody] DeleteRequestDto requestDto) =>
        GenericDeleteByUniqueId(repository, requestDto);

    public static Task<IResult> GetByUniqueId(IRepository<Shipment> repository, [FromRoute] Guid id) =>
        GenericGetByUniqueId<ShipmentDto>(repository, id);

    public static Task<IResult> GetList(IRepository<Shipment> repository) =>
        GenericGetAll<ShipmentDto>(repository);
}

public record CreateShipmentEventRequestDto(
    Guid ShipmentId,
    string ShipmentType
) : ICreateRequestDto<ShipmentEvent>
{
    public ShipmentEvent CreateDomain()
    {
        return new ShipmentEvent(
            PocztexApi.Core.Types.UniqueId.CreateNew(), 
            ShipmentId, 
            ShipmentEventTypeHelper.ToType(ShipmentType), 
            DateTime.Now
        );
    }
}

public record ShipmentEventDto(
    Guid Id,
    Guid ShipmentId,
    string ShipmentType,
    DateTime Time
) : IDto<ShipmentEvent>
{
    public static IDto<ShipmentEvent> CreateFromDomain(ShipmentEvent domain)
    {
        return new ShipmentEventDto(
            domain.UniqueId.Guid,
            domain.ShipmentId.Guid,
            ShipmentEventTypeHelper.ToString(domain.Type),
            domain.Time
        );
    }
}

public class ShipmentEventEndpoints : CrudEndpoints<ShipmentEvent>
{
    public static Task<IResult> Create(IRepository<ShipmentEvent> repository, IEnumerable<ICreateRule<ShipmentEvent>> rules, [FromBody] CreateShipmentEventRequestDto requestDto) =>
        GenericCreateFromDto<CreateShipmentEventRequestDto, ShipmentEventDto>(repository, rules, requestDto);

    public static Task<IResult> Delete(IRepository<ShipmentEvent> repository, [FromBody] DeleteRequestDto requestDto) =>
        GenericDeleteByUniqueId(repository, requestDto);

    public static Task<IResult> GetByUniqueId(IRepository<ShipmentEvent> repository, [FromRoute] Guid id) =>
        GenericGetByUniqueId<ShipmentEventDto>(repository, id);

    public static Task<IResult> GetList(IRepository<ShipmentEvent> repository) =>
        GenericGetAll<ShipmentEventDto>(repository);
}

public record CreatePaymentRequestDto(
        
) : ICreateRequestDto<Payment>
{
    public Payment CreateDomain()
    {
        throw new NotImplementedException();
    }
}

public record PaymentDto() : IDto<Payment>
{
    public static IDto<Payment> CreateFromDomain(Payment domain)
    {
        throw new NotImplementedException();
    }
}

public class PaymentEndpoints : CrudEndpoints<Payment>
{
    public static Task<IResult> Create(IRepository<Payment> repository, IEnumerable<ICreateRule<Payment>> rules, [FromBody] CreatePaymentRequestDto requestDto) =>
        GenericCreateFromDto<CreatePaymentRequestDto, PaymentDto>(repository, rules, requestDto);

    public static Task<IResult> Delete(IRepository<Payment> repository, [FromBody] DeleteRequestDto requestDto) =>
        GenericDeleteByUniqueId(repository, requestDto);

    public static Task<IResult> GetByUniqueId(IRepository<Payment> repository, [FromRoute] Guid id) =>
        GenericGetByUniqueId<PaymentDto>(repository, id);

    public static Task<IResult> GetList(IRepository<Payment> repository) =>
        GenericGetAll<PaymentDto>(repository);
}

public record CreatePaymentEventRequestDto() : ICreateRequestDto<PaymentEvent>
{
    public PaymentEvent CreateDomain()
    {
        throw new NotImplementedException();
    }
}

public record PaymentEventDto() : IDto<PaymentEvent>
{
    public static IDto<PaymentEvent> CreateFromDomain(PaymentEvent domain)
    {
        throw new NotImplementedException();
    }
}

public class PaymentEventEndpoints : CrudEndpoints<PaymentEvent>
{
    public static Task<IResult> Create(IRepository<PaymentEvent> repository, IEnumerable<ICreateRule<PaymentEvent>> rules, [FromBody] CreatePaymentEventRequestDto requestDto) =>
        GenericCreateFromDto<CreatePaymentEventRequestDto, PaymentEventDto>(repository, rules, requestDto);

    public static Task<IResult> Delete(IRepository<PaymentEvent> repository, [FromBody] DeleteRequestDto requestDto) =>
        GenericDeleteByUniqueId(repository, requestDto);

    public static Task<IResult> GetByUniqueId(IRepository<PaymentEvent> repository, [FromRoute] Guid id) =>
        GenericGetByUniqueId<PaymentEventDto>(repository, id);

    public static Task<IResult> GetList(IRepository<PaymentEvent> repository) =>
        GenericGetAll<PaymentEventDto>(repository);
}

public record CreateAccountDto(
    string Login,
    string Password
);

public record Employee(
    PocztexApi.Core.Types.UniqueId UniqueId, 
    string Name, 
    string Surname, 
    DateTime BirthDate, 
    Pesel? Pesel, 
    string DocumentNumber, 
    bool IsPolandCitizen
) : IModel;

public record EmployeeDto(Guid Id, string Name, string Surname, DateTime BirthDate, string Pesel, string DocumentNumber, bool IsPolandCitizen) : IDto<Employee>
{
    public static IDto<Employee> CreateFromDomain(Employee domain)
    {
        return new EmployeeDto(
            domain.UniqueId.Guid,
            domain.Name,
            domain.Surname,
            domain.BirthDate,
            domain.Pesel.Value,
            domain.DocumentNumber,
            domain.IsPolandCitizen
        );
    }
}

public record CreateEmployeeRequestDto(string Name, string Surname, DateTime BirthDate, string Pesel, string DocumentNumber, bool IsPolandCitizen) : ICreateRequestDto<Employee>
{
    public Employee CreateDomain()
    {
        return new(
            PocztexApi.Core.Types.UniqueId.CreateNew(),
            Name,
            Surname,
            BirthDate,
            Pesel,
            DocumentNumber,
            IsPolandCitizen
        );
    }
}

public class EmployesEndpoints : CrudEndpoints<Employee>
{
    public static Task<IResult> Create(IRepository<Employee> repository, IEnumerable<ICreateRule<Employee>> rules, [FromBody] CreateEmployeeRequestDto requestDto) =>
        GenericCreateFromDto<CreateEmployeeRequestDto, EmployeeDto>(repository, rules, requestDto);

    public static Task<IResult> Delete(IRepository<Employee> repository, [FromBody] DeleteRequestDto requestDto) =>
        GenericDeleteByUniqueId(repository, requestDto);

    public static Task<IResult> GetByUniqueId(IRepository<Employee> repository, [FromRoute] Guid id) =>
        GenericGetByUniqueId<EmployeeDto>(repository, id);

    public static Task<IResult> GetList(IRepository<Employee> repository) => 
        GenericGetAll<EmployeeDto>(repository);
}

public interface ICreateRule<T>
{
    Task ValidateRule(T model);
}

public class AccountsCreateRules
{
    public class AccountWithThisSameLoginShouldNotExist(IAccountsRepository accountsRepository) : ICreateRule<Account>
    {
        public async Task ValidateRule(Account model)
        {
            if (await accountsRepository.GetByLogin(model.Login) is not null)
                throw new AppException("Account with this same login already exist");
        }
    }
}

public class EmployeeCreateRules
{
    public class ShouldNotExistOtherEmployeeWithThisSameNameOrSurnameOrPeselOrDocumentNumber(IRepository<Employee> repository) : ICreateRule<Employee>
    {
        public async Task ValidateRule(Employee model)
        {
            var employes = await repository.GetAll();

            if (employes.Any(e => e.Name == model.Name && e.Surname == model.Surname))
                throw new AppException("Exist employee with this same name and surname");

            if (employes.Any(e => e.DocumentNumber == model.DocumentNumber))
                throw new AppException("Exist employee with this same document number");

            if (model.Pesel is not null && employes.Any(e => e.Pesel == model.Pesel))
                throw new AppException("Exist employee with this same PESEL");
        }
    }
}

public record Pesel(string Value)
{
    public static implicit operator Pesel(string pesel)
    {
        if (string.IsNullOrEmpty(pesel) || Regex.IsMatch(pesel, @"[\d]{11}") == false)
            throw new AppException("Pesel is in incorrect format");

        return new(pesel);
    }
}

