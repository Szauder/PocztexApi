using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PocztexApi.Accounts.Seed;
using PocztexApi.Core.Seeding;
using PocztexApi.Shared.Seeding;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var authTokenConfig = new AuthTokenConfig(Key: Secret.FromUtf8("12345678901234561234567890123456"));

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
builder.Services.AddScoped<AccountsService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddSingleton(authTokenConfig);

builder.Services.AddHostedService<SeederBackgroundProces>();
builder.Services.AddSingleton<ISeeder, AccountsSeeder>();
builder.Services.AddSingleton<SeedReader>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionsHandlingMiddleware>();

app.MapGet("/hello", () => Results.Text($"Hello, {DateTimeOffset.UtcNow}"));
app.MapGet("/helloauth", (ClaimsPrincipal principal) => Results.Text($"Hello {principal.Claims.Single(c => c.Type == "account-id").Value}, {DateTimeOffset.UtcNow}")).RequireAuthorization();

app.MapPost("/account/register", AccountsEndpoints.Register);
app.MapPost("/account/login", AccountsEndpoints.Login);

app.Run();
