namespace Sanara.Compatibility;

public class CommonTextChannel
{
    public CommonTextChannel(Discord.ITextChannel tChan)
    {
        OwnerId = tChan.Guild.OwnerId.ToString();
        IsNsfw = tChan.IsNsfw;
    }

    public CommonTextChannel(RevoltSharp.TextChannel tChan)
    {
        OwnerId = tChan.Server.OwnerId;
        IsNsfw = tChan.IsNsfw;
    }

    public string OwnerId { private set; get; }
    public bool IsNsfw { private set; get; }
}
