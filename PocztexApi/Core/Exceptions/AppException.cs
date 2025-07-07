namespace PocztexApi.Core.Exceptions;

public class AppException(string? responseMessage = null) : Exception
{
    public string? ResponseMessage { get; init; } = responseMessage;

    public virtual IResult GetResult()
    {
        if (ResponseMessage is not null)
            return Results.BadRequest(new { ErrorMessage = ResponseMessage });

        return Results.BadRequest();
    }
}