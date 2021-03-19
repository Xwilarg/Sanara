using Discord;
using Discord.WebSocket;
using SanaraV3.Diaporama.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Diaporama
{
    public static class ReactionManager
    {
        public static async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel chan, SocketReaction react)
        {
            string emote = react.Emote.ToString();
            // If emote is not from the bot and is an arrow emote
            if (react.User.IsSpecified && react.User.Value.Id != StaticObjects.ClientId && Constants.DIAPORAMA_EMOTES.Contains(emote) && StaticObjects.Diaporamas.ContainsKey(msg.Id))
            {
                var dMsg = await msg.GetOrDownloadAsync();
                var elem = StaticObjects.Diaporamas[msg.Id];
                int nextPage = GetNextPage(elem.CurrentPage, elem.Elements.Length - 1, emote);
                if (nextPage != elem.CurrentPage) // No need to modify anything if we didn't change the page
                {
                    var next = elem.Elements[nextPage];
                    if (next is Reddit reddit)
                        await dMsg.ModifyAsync(x => x.Embed = Post(reddit, nextPage + 1, elem.Elements.Length));
                    else if (next is Doujinshi doujinshi)
                        await dMsg.ModifyAsync(x => x.Embed = Post(doujinshi, nextPage + 1, elem.Elements.Length));
                    else if (next is Dlsite dlsite)
                        await dMsg.ModifyAsync(x => x.Embed = Post(dlsite, nextPage + 1, elem.Elements.Length));
                    else
                        throw new ArgumentException("Unknown type for next");
                    StaticObjects.Diaporamas[msg.Id].CurrentPage = nextPage;
                }
                if (!(dMsg.Author is IGuildUser author) || author.GuildPermissions.ManageMessages) // If we have the perms to delete the emote we do so
                    await dMsg.RemoveReactionAsync(react.Emote, react.User.Value); // TODO: Check for channel perms
            }
        }

        private static int GetNextPage(int current, int max, string emote)
        {
            if (emote == "◀️")
            {
                if (current != 0)
                    return current - 1;
            }
            else if (emote == "▶️")
            {
                if (current != max)
                    return current + 1;
            }
            else if (emote == "⏪")
            {
                return 0;
            }
            else if (emote == "⏩")
            {
                return max;
            }
            else
                throw new ArgumentException("Invalid value for emote: " + emote);
            return current;
        }

        public static Embed Post(Reddit reddit, int currPage, int maxPage)
        {
            string content = reddit.Content;
            if (content.Length > 2048)
                content = content.Substring(0, 2042) + "[...]";
            return new EmbedBuilder
            {
                Color = reddit.IsNsfw ? Color.Red : Color.Green,
                Title = reddit.Title,
                ImageUrl = reddit.Image?.AbsoluteUri,
                Url = reddit.Link.AbsoluteUri,
                Description = content,
                Footer = new EmbedFooterBuilder
                {
                    Text = reddit.Flairs + $"\nScore: {reddit.Ups}" + (maxPage == 1 ? "" : $"\nPage {currPage} out of {maxPage}")
                }
            }.Build();
        }

        public static Embed Post(Doujinshi doujin, int currPage, int maxPage)
        {
            return new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", doujin.Tags),
                Title = doujin.Title,
                Url = doujin.Url,
                ImageUrl = doujin.ImageUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Do the 'Download doujinshi' command with the id '{doujin.Id}' to download the doujinshi.\nPage {currPage} out of {maxPage}"
                }
            }.Build();
        }

        public static Embed Post(Dlsite elem, int currPage, int maxPage)
        {
            return new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = elem.Title,
                Url = elem.Url,
                ImageUrl = elem.ImageUrl,
                Description = elem.Description,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Type",
                        Value = elem.Type,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Price",
                        Value = elem.Price + " ¥",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Rating",
                        Value = elem.Rating.HasValue ? elem.Rating.Value + " / 5" : "Not rated",
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Number of Download",
                        Value = elem.NbDownload == "" ? "Not release yet" : elem.NbDownload,
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Tags: {string.Join(", ", elem.Tags)}\nPage {currPage} out of {maxPage}"
                }
            }.Build();
        }
    }
}
