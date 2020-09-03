using Discord.Commands;
using SanaraV3.Attributes;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
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
