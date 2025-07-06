namespace PocztexApi.Shared.Middlewares;

public sealed class ExceptionsHandlingMiddleware(IWebHostEnvironment environment, ILogger<ExceptionsHandlingMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (AppException e)
        {
            await e.GetResult().ExecuteAsync(context);
        }
        catch (Exception e)
        {
            logger.LogCritical(e, e.Message, []);

            if (environment.IsDevelopment())
                throw;

            await Results.InternalServerError().ExecuteAsync(context);
        }
    }
}