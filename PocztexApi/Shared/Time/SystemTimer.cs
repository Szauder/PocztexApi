namespace PocztexApi.Shared.Time;

public class SystemTimer : Core.Time.ITimer
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}