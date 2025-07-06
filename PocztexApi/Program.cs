var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer().AddSwaggerGen();

builder.Services.AddLogging(config =>
{
    config.SetMinimumLevel(LogLevel.Trace);
});

builder.Services.AddScoped<ExceptionsHandlingMiddleware>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionsHandlingMiddleware>();

app.MapGet("/hello", () => Results.Text($"Hello, {DateTimeOffset.UtcNow}"));

app.Run();
