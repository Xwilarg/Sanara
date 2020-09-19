using Discord.Commands;
using SanaraV3.Attribute;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadSettingHelp()
        {
            _submoduleHelp.Add("Setting", "Modify the bot behavior for your server");
            _help.Add(("Administration", new Help("Setting", "Prefix", new[] { new Argument(ArgumentType.OPTIONAL, "prefix") }, "Change the bot prefix. Is no information is provided, display the current one.", new string[0], Restriction.AdminOnly, "Prefix s.")));
        }
    }
}

namespace SanaraV3.Module.Administration
{
    public class SettingModule : ModuleBase
    {

        [Command("Prefix"), RequireAdminAttribute]
        public async Task Prefix()
        {
            await ReplyAsync("Your current prefix is " + StaticObjects.Db.GetGuild(Context.Guild.Id).Prefix);
        }

        [Command("Prefix"), RequireAdminAttribute]
        public async Task Prefix(string prefix)
        {
            await StaticObjects.Db.UpdatePrefixAsync(Context.Guild.Id, prefix);
            await ReplyAsync("Your prefix was updated to " + prefix);
        }
    }
}
