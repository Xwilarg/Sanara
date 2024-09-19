namespace Sanara.Service;

public class StatData
{
    public DateTime LastMessage { set; get; } = DateTime.UtcNow;
    public DateTime Started { set; get; } = DateTime.UtcNow;
}
