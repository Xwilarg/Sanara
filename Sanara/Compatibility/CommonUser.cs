namespace Sanara.Compatibility;

public class CommonUser
{
    public CommonUser(Discord.IUser user)
    {
        IsAdmin = user is Discord.IGuildUser gUser && gUser.GuildPermissions.ManageGuild;
        Id = user.Id.ToString();
        Mention = user.Mention;
        Username = user.Username;
    }

    public CommonUser(StoatSharp.User user)
    {
        IsAdmin = false;
        Id = user.Id;
        Mention = user.Mention;
        Username = user.Username;
    }

    public bool IsAdmin { private set; get; }
    public string Id { private set; get; }
    public string Mention { private set; get; }
    public string Username { private set; get; }
}
