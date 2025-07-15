namespace Sanara.Compatibility;

public class CommonUser
{
    public CommonUser(Discord.IUser user)
    {
        IsAdmin = user is Discord.IGuildUser gUser && gUser.GuildPermissions.ManageGuild;
        Id = user.Id.ToString();
    }

    public CommonUser(RevoltSharp.User user)
    {
        IsAdmin = false;
        Id = user.Id;
    }

    public bool IsAdmin { private set; get; }
    public string Id { private set; get; }
}
