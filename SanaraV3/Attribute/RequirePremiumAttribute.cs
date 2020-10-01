using Discord.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Attribute
{
    public sealed class RequirePremiumAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (StaticObjects.AllowedPremium.Contains(context.User.Id.ToString()))
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("You must be premium to use this command. Use the premium command for more information."));
        }
    }
}
