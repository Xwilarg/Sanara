using Discord;
using Discord.WebSocket;

namespace Sanara.Module.Button
{
    public class Settings
    {
        public static async Task DatabaseDump(SocketMessageComponent ctx)
        {
            await ctx.RespondAsync("```json\n" +
                await StaticObjects.Db.DumpAsync(((ITextChannel)ctx.Channel).Guild.Id) +
                "\n```", ephemeral: true);
        }
    }
}
