namespace PocztexApi.Core.Time;

public interface ITimer
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}