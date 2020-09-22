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
            _help.Add(("Administration", new Help("Setting", "Anonymize", new[] { new Argument(ArgumentType.OPTIONAL, "value") }, "Set if your guild name can be displayed on Sanara stats page. Is no information is provided, display the current value.", new string[0], Restriction.AdminOnly, "Anonymize true")));
        }
    }
}

namespace SanaraV3.Module.Administration
{
    public class SettingModule : ModuleBase
    {

        [Command("Prefix"), RequireAdmin]
        public async Task Prefix()
        {
            await ReplyAsync("Your current prefix is " + StaticObjects.Db.GetGuild(Context.Guild.Id).Prefix);
        }

        [Command("Prefix"), RequireAdmin]
        public async Task Prefix(string prefix)
        {
            await StaticObjects.Db.UpdatePrefixAsync(Context.Guild.Id, prefix);
            await ReplyAsync("Your prefix was updated to " + prefix);
        }


        [Command("Anonymize"), RequireAdmin]
        public async Task Anonymize()
        {
            await ReplyAsync("Your current anonymize setting is set to " + StaticObjects.Db.GetGuild(Context.Guild.Id).Anonymize.ToString().ToLower());
        }

        [Command("Anonymize"), RequireAdmin]
        public async Task Anonymize(bool value)
        {
            await StaticObjects.Db.UpdateAnonymizeAsync(Context.Guild.Id, value);
            await ReplyAsync("Your anonymize setting was updated to " + value.ToString().ToLower());
        }
    }
}
