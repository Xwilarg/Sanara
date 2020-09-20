using Discord.Commands;
using SanaraV3.Exception;
using System.Threading.Tasks;

namespace SanaraV3.Module.Community
{
    public class CommunityModule : ModuleBase
    {
        [Command("Profile"), Alias("P")]
        public Task Community(params string[] _)
        {
            throw new NotYetAvailable();
        }
    }
}
