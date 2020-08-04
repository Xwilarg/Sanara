using Discord.Commands;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Nsfw
{
    public sealed class DoujinModule : ModuleBase, IModule
    {
        public string GetModuleName()
            => "NSFW";

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw]
        public async Task GetDoujinshi([Remainder]string tags = "")
        {

        }
    }
}
