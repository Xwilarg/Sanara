using Discord;
using Discord.Commands;
using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using SanaraV3.Exceptions;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Nsfw
{
    public sealed class DoujinshiModule : ModuleBase, IModule
    {
        public string GetModuleName()
            => "NSFW";

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw, Alias("Doujin")]
        public async Task GetDoujinshiAsync() // Doujin search with no tags
        {
            var result = await SearchClient.SearchAsync();
            int page = StaticObjects.Random.Next(0, result.numPages) + 1;
            result = await SearchClient.SearchAsync(page);
            await ReplyAsync(embed: FormatDoujinshi(result.elements[StaticObjects.Random.Next(0, result.elements.Length)]));
        }

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw, Alias("Doujin")]
        public async Task GetDoujinshiAsync([Remainder]string tags) // Doujin search with tags given
        {
            NHentaiSharp.Search.SearchResult result;
            try
            {
                result = await SearchClient.SearchWithTagsAsync(tags);
            }
            catch (InvalidArgumentException)
            {
                throw new CommandFailed("There is no doujin with these tags");
            }
            int page = StaticObjects.Random.Next(0, result.numPages) + 1;
            result = await SearchClient.SearchWithTagsAsync(new[] { tags }, page);
            await ReplyAsync(embed: FormatDoujinshi(result.elements[StaticObjects.Random.Next(0, result.elements.Length)]));
        }

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw, Priority(1), Alias("Doujin")]
        public async Task GetDoujinshiAsync(int id)
        {
            try
            {
                var result = await SearchClient.SearchByIdAsync(id);
                await ReplyAsync(embed: FormatDoujinshi(result));
            }
            catch (InvalidArgumentException)
            {
                throw new CommandFailed("These is no doujin with this id");
            }
        }

        private Embed FormatDoujinshi(GalleryElement result)
        {
            return new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", result.tags.Select(x => x.name)),
                Title = result.prettyTitle,
                Url = result.url.AbsoluteUri,
                ImageUrl = result.pages[0].imageUrl.AbsoluteUri,
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Do the 'Download doujinshi' command with the id '{result.id}' to download the doujinshi."
                }
            }.Build();
        }
    }
}
