using Discord.Commands;
using SanaraV2.Modules.Base;
using System.Threading.Tasks;

namespace SanaraV2.Community
{
    public class CommunityModule : ModuleBase
    {
        [Command("Profile")]
        public async Task Leave(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Community);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Community);
            await Context.Channel.SendFileAsync("Saves/Assets/Background.png");
        }
    }
}
