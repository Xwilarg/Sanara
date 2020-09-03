using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace SanaraV3.Attributes
{
    public sealed class RequireAdminAttribute : PreconditionAttribute
    {
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var gUser = context.User as IGuildUser;
            
            if (gUser == null) // Private message
                return Task.FromResult(PreconditionResult.FromSuccess());

            if (gUser.Guild.OwnerId == gUser.Id || gUser.GuildPermissions.ManageGuild)
                return Task.FromResult(PreconditionResult.FromSuccess());
            return Task.FromResult(PreconditionResult.FromError("Only admin have access to this command."));
        }
    }
}
