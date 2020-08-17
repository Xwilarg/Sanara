using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace SanaraV3.Diaporama
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReactionManager
    {
        public static async Task ReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chan, SocketReaction react)
        {
            string emote = react.Emote.ToString();
            // If emote is not from the bot and is an arrow emote
            if (react.User.Value.Id != StaticObjects.ClientId && (emote == "◀️" || emote == "▶️" || emote == "⏪" || emote == "⏩") && StaticObjects.Diaporamas.ContainsKey(msg.Id))
            {
                var dMsg = await msg.GetOrDownloadAsync();
                var elem = StaticObjects.Diaporamas[msg.Id];
                int nextPage = elem.CurrentPage;
                if (emote == "◀️")
                {
                    if (nextPage != 0)
                        nextPage--;
                }
                else if (emote == "▶️")
                {
                    if (nextPage != elem.Elements.Length - 1)
                        nextPage++;
                }
                else if (emote == "⏪")
                {
                    nextPage = 0;
                }
                else if (emote == "⏩")
                {
                    nextPage = elem.Elements.Length - 1;
                }
                if (nextPage != elem.CurrentPage) // No need to modify anything if we didn't change the page
                {
                    var next = elem.Elements[nextPage];
                    if (next is Reddit)
                        await dMsg.ModifyAsync(x => x.Embed = Post((Reddit)next));
                    else
                        throw new ArgumentException("Unknown type for next");
                    StaticObjects.Diaporamas[msg.Id].CurrentPage = nextPage;
                }
                var author = dMsg.Author as IGuildUser;
                if (author == null || author.GuildPermissions.ManageMessages) // If we have the perms to delete the emote we do so
                    await dMsg.RemoveReactionAsync(react.Emote, react.User.Value); // TODO: Check for channel perms
            }
        }

        public static Embed Post(Reddit reddit)
        {
            return new EmbedBuilder
            {
                Color = reddit.IsNsfw ? Color.Red : Color.Green,
                Title = reddit.Title,
                ImageUrl = reddit.Image?.AbsoluteUri,
                Url = reddit.Link.AbsoluteUri,
                Description = reddit.Content,
                Footer = new EmbedFooterBuilder
                {
                    Text = reddit.Flairs + "\nScore: " + reddit.Ups
                }
            }.Build();
        }
    }
}
