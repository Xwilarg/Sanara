using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SanaraV3.Attributes
{
    public sealed class RequirePremiumAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            return Task.FromResult(PreconditionResult.FromError("You must be premium to use this command. Use the premium command for more information."));
        }
    }
}
