namespace Sanara.Compatibility;

public class CommonTextChannel : CommonMessageChannel
{
    public CommonTextChannel(Discord.ITextChannel tChan) : base(tChan)
    {
        OwnerId = tChan.Guild.OwnerId.ToString();
        IsNsfw = tChan.IsNsfw;
    }

    public CommonTextChannel(RevoltSharp.TextChannel tChan) : base(tChan)
    {
        OwnerId = tChan.Server.OwnerId;
        IsNsfw = tChan.IsNsfw;
    }

    public string OwnerId { private set; get; }
    public bool IsNsfw { private set; get; }
}
