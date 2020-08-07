using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Attributes
{
    public class RequireRunningGameAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (!StaticObjects.Games.Any(x => x.IsMyGame(context.Channel.Id)))
                return Task.FromResult(PreconditionResult.FromError("There is no game running in this channel."));
            return Task.FromResult(PreconditionResult.FromSuccess());
        }
    }
}
