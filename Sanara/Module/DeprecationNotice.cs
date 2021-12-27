using Discord.Commands;

namespace Sanara.Module
{
    public class DeprecationNotice : ModuleBase
    {
        [Command("Botinfo"), Alias("Help", "Ping", "Gdpr",
            "Anime", "Manga", "LightNovel", "Drama",
            "Safebooru", "Booru", "E629", "E926", "Gelbooru", "Rule34", "Konachan",
            "Complete", "VNQuote", "Inspire")]
        public async Task Deprecated(params string[] _)
        {
            await ReplyAsync(
                "Discord is removing for bots the ability to read message content starting April 2022\n" +
                "Therefore Sanara is now adapting to these changes and all commands are moved to slash commands\n\n" +
                "**How does that works?**\n" +
                "Just type / and you'll see the list of commands for all bots, choose your command from there and press enter. More information here: https://support.discord.com/hc/en-us/articles/1500000368501-Slash-Commands-FAQ\n\n" +
                "**Slash commands doesn't work for me**\n" +
                "Don't forget to reinvite the bot so it has application permissions: https://discord.com/api/oauth2/authorize?client_id=" + StaticObjects.ClientId + "&scope=bot%20applications.commands"
            );
        }
    }
}
