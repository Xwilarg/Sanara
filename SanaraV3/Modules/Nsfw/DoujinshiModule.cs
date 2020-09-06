using Discord;
using Discord.Commands;
using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using SanaraV3.Exceptions;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadDoujinshiHelp()
        {
            _help.Add(new Help("Doujinshi", new[] { new Argument(ArgumentType.OPTIONAL, "tags/id") }, "Get a random doujinshi. You can either provide some tags or directly give its id.", true));
            _help.Add(new Help("Download doujinshi", new[] { new Argument(ArgumentType.MANDATORY, "id") }, "Download a doujinshi given its id.", true));
        }
    }
}

namespace SanaraV3.Modules.Nsfw
{
    public sealed class DoujinshiModule : ModuleBase
    {
        [Command("Download doujinshi", RunMode = RunMode.Async), RequireNsfw, Alias("Download doujin")]
        public async Task GetDownloadDoujinshiAsync(int id)
        {
            string path = id + "_" + DateTime.Now.ToString("HHmmssff") + StaticObjects.Random.Next(0, int.MaxValue);
            Directory.CreateDirectory("Saves/Download/" + path); // Folder that contains the ZIP
            Directory.CreateDirectory("Saves/Download/" + path + "/" + id); // Folder that will inside the ZIP
            GalleryElement elem;
            try
            {
                elem = await SearchClient.SearchByIdAsync(id);
            }
            catch (InvalidArgumentException)
            {
                throw new CommandFailed("There is no doujinshi with this id.");
            }
            int i = 1;
            foreach (var page in elem.pages)
            {
                string extension = "." + page.format.ToString().ToLower();
                // Write each page in the folder
                File.WriteAllBytes("Saves/Download/" + path + "/" + id + "/" + Get3DigitNumber(i.ToString()) + extension,
                    await StaticObjects.HttpClient.GetByteArrayAsync("https://i.nhentai.net/galleries/" + elem.mediaId + "/" + i + extension));
                i++;
            }
            string finalPath = "Saves/Download/" + path + "/" + id + ".zip";
            ZipFile.CreateFromDirectory("Saves/Download/" + path + "/" + id, "Saves/Download/" + path + "/" + id + ".zip");

            // Delete all files
            for (i = Directory.GetFiles("Saves/Download/" + path + "/" + id).Length - 1; i >= 0; i--)
                File.Delete(Directory.GetFiles("Saves/Download/" + path + "/" + id)[i]);

            // Delete folder
            Directory.Delete("Saves/Download/" + path + "/" + id);

            FileInfo fi = new FileInfo(finalPath);
            if (fi.Length < 8000000) // 8MB
            {
                await Context.Channel.SendFileAsync(finalPath);
            }
            else
            {
                Directory.CreateDirectory(StaticObjects.UploadWebsiteLocation + path);
                File.Copy(finalPath, StaticObjects.UploadWebsiteLocation + path + "/" + id + ".zip");
                await ReplyAsync(StaticObjects.UploadWebsiteUrl + path + "/" + id + ".zip" + Environment.NewLine + "You file will be deleted after 10 minutes.");
                _ = Task.Run(async () =>
                {
                    await Task.Delay(600000); // 10 minutes
                    File.Delete(StaticObjects.UploadWebsiteLocation + path + "/" + id + ".zip");
                    Directory.Delete(StaticObjects.UploadWebsiteLocation + path);
                });
            }
            File.Delete(finalPath);
            Directory.CreateDirectory("Saves/Download/" + path + "/" + id);
        }

        /// <summary>
        /// Return a number on a 3 character string (pad with 0)
        /// </summary>
        private static string Get3DigitNumber(string nb)
        {
            if (nb.Length == 3)
                return nb;
            if (nb.Length == 2)
                return "0" + nb;
            return "00" + nb;
        }

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
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Do the 'Download doujinshi' command with the id '{result.id}' to download the doujinshi."
                }
            }.Build();
        }
    }
}
