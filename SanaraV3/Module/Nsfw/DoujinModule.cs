using Discord;
using Discord.Commands;
using NHentaiSharp.Core;
using NHentaiSharp.Exception;
using NHentaiSharp.Search;
using SanaraV3.Attribute;
using SanaraV3.Exception;
using SanaraV3.Subscription.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadDoujinHelp()
        {
            _submoduleHelp.Add("Doujin", "Get self published work");
            _help.Add(("Nsfw", new Help("Doujin", "Doujinshi", new[] { new Argument(ArgumentType.OPTIONAL, "tags/id") }, "Get a random doujinshi. You can either provide some tags or directly give its id.", new[] { "Doujin", "Nhentai" }, Restriction.Nsfw, "Doujinshi kancolle yuri")));
            _help.Add(("Nsfw", new Help("Doujin", "Dlsite", new[] { new Argument(ArgumentType.MANDATORY, "query") }, "Get the most popular work from dlsite given a query.", Array.Empty<string>(), Restriction.Nsfw, "Dlsite 艦隊")));
            _help.Add(("Nsfw", new Help("Doujin", "Dlrand", Array.Empty<Argument>(), "Get a random work from dlsite.", Array.Empty<string>(), Restriction.Nsfw, null)));
            _help.Add(("Nsfw", new Help("Doujin", "Download doujinshi", new[] { new Argument(ArgumentType.MANDATORY, "id") }, "Download a doujinshi given its id.", new[] { "Download doujin" }, Restriction.Nsfw, "Download doujin 321633")));
            _help.Add(("Nsfw", new Help("Doujin", "Subscribe doujinshi", new[] { new Argument(ArgumentType.MANDATORY, "text channel"), new Argument(ArgumentType.OPTIONAL, "tags") }, "Get information on all new doujinshi in a channel.", new[] { "Subscribe doujin", "Subscribe nhentai" }, Restriction.Nsfw | Restriction.AdminOnly, "Subscribe doujinshi +\"ke-ta\"")));
            _help.Add(("Nsfw", new Help("Doujin", "Unsubscribe doujinshi", new Argument[0], "Remove a doujinshi subscription.", new[] { "Unsubscribe doujin", "Unsubscribe nhentai" }, Restriction.Nsfw | Restriction.AdminOnly, null)));
            _help.Add(("Nsfw", new Help("Doujin", "Doujinshi popularity", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get the most popular doujinshi given some tags. If none are provided, give the trending ones.", new[] { "Doujinshi popularity", "Doujinshi p", "Doujin p", "Doujin popularity", "Nhentai p", "Nhentai popularity" }, Restriction.Nsfw | Restriction.AdminOnly, null)));
        }
    }
}

namespace SanaraV3.Module.Nsfw
{
    public sealed class DoujinModule : ModuleBase
    {
        [Command("Subscribe doujinshi"), Alias("Subscribe doujin", "Subscribe nhentai"), RequireNsfw, RequireAdmin]
        public async Task SubscribeDoujinshiAsync(ITextChannel chan, params string[] tags)
        {
            if (!chan.IsNsfw)
                throw new CommandFailed("Destination channel must be NSFW.");
            await StaticObjects.Db.SetSubscriptionAsync(Context.Guild.Id, "nhentai", chan, new NHentaiTags(tags, true));
            await ReplyAsync($"You subscribed for doujinshi to {chan.Mention}.");
        }

        [Command("Unsubscribe doujinshi"), Alias("Unsubscribe doujin", "Unsubscribe nhentai"), RequireNsfw, RequireAdmin]
        public async Task UnsubscribeDoujinshiAsync()
        {
            if (!await StaticObjects.Db.HasSubscriptionExistAsync(Context.Guild.Id, "nhentai"))
                await ReplyAsync("There is no active doujinshi subscription.");
            else
                await StaticObjects.Db.RemoveSubscriptionAsync(Context.Guild.Id, "nhentai");
        }

        [Command("Download doujinshi", RunMode = RunMode.Async), RequireNsfw, Alias("Download doujin", "Download nhentai", "dl doujinshi", "dl doujin", "dl nhentai", "dldj")]
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

            await ReplyAsync("Preparing download... This might take some time.");

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
        public static string Get3DigitNumber(string nb) // TODO: Move this function elsewhere since it's also used by CosplayModule
        {
            if (nb.Length == 3)
                return nb;
            if (nb.Length == 2)
                return "0" + nb;
            return "00" + nb;
        }

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw, Alias("Doujin", "NHentai")]
        public async Task GetDoujinshiAsync() // Doujin search with no tags
        {
            var result = await SearchClient.SearchAsync();
            int page = StaticObjects.Random.Next(0, result.numPages) + 1;
            result = await SearchClient.SearchAsync(page);
            await ReplyAsync(embed: FormatDoujinshi(result.elements[StaticObjects.Random.Next(0, result.elements.Length)]));
        }

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw, Alias("Doujin", "NHentai")]
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

        [Command("Doujinshi", RunMode = RunMode.Async), RequireNsfw, Priority(1), Alias("Doujin", "NHentai")]
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

        [Command("Doujinshi popularity", RunMode = RunMode.Async), Priority(2), RequireNsfw, Alias("Doujinshi p", "Doujin popularity", "Doujin p", "Nhentai popularity", "Nhentai p")]
        public async Task PopularityAsync()
        {
            string html = await StaticObjects.HttpClient.GetStringAsync("https://nhentai.net/");
            html = html.Split(new[] { "<div class=\"container index-container index-popular\">" }, StringSplitOptions.None)[1]
                .Split(new[] { "<div class=\"container index-container\">" }, StringSplitOptions.None)[0];
            var elems = new List<Diaporama.Impl.Doujinshi>();
            foreach (var match in Regex.Matches(html, "<a href=\"\\/g\\/([0-9]+)\\/\"").Cast<Match>())
            {
                var doujinshi = await SearchClient.SearchByIdAsync(int.Parse(match.Groups[1].Value));
                elems.Add(new Diaporama.Impl.Doujinshi(doujinshi.url.AbsoluteUri, doujinshi.pages[0].imageUrl.AbsoluteUri, doujinshi.prettyTitle, doujinshi.tags.Select(x => x.name).ToArray(), doujinshi.id));
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
        }

        [Command("Doujinshi popularity", RunMode = RunMode.Async), Priority(2), RequireNsfw, Alias("Doujinshi p", "Doujin popularity", "Doujin p", "Nhentai popularity", "Nhentai p")]
        public async Task PopularityAsync([Remainder]string tags)
        {
            NHentaiSharp.Search.SearchResult result;
            try
            {
                result = await SearchClient.SearchWithTagsAsync(tags.Split(' '), 1, PopularitySort.AllTime);
            }
            catch (InvalidArgumentException)
            {
                throw new CommandFailed("There is no doujin with these tags");
            }
            var elems = new List<Diaporama.Impl.Doujinshi>();
            foreach (var doujinshi in result.elements.Take(5))
            {
                elems.Add(new Diaporama.Impl.Doujinshi(doujinshi.url.AbsoluteUri, doujinshi.pages[0].imageUrl.AbsoluteUri, doujinshi.prettyTitle, doujinshi.tags.Select(x => x.name).ToArray(), doujinshi.id));
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
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

        [Command("Dlrand", RunMode = RunMode.Async), RequireNsfw]
        public async Task Dlrand()
        {
            var last = DateTime.Now.Subtract(new TimeSpan(7, 0, 0, 0));
            var url = "https://www.dlsite.com/maniax/fsr/=/language/jp/regist_date_start/" + last.ToString("yyyy-MM-dd") + "/ana_flg/off/work_category%5B0%5D/doujin/order%5B0%5D/trend/per_page/30/release_term/week/show_type/1/from/fs.detail";

            var html = await StaticObjects.HttpClient.GetStringAsync(url);
            int maxId = int.Parse(Regex.Match(html, "RJ([0-9]+)\\.html").Groups[1].Value) + 1;
            int id;
            string doujinUrl;
            HttpResponseMessage msg;

            do
            {
                id = StaticObjects.Random.Next(1, maxId);
                doujinUrl = "https://www.dlsite.com/maniax/work/=/product_id/RJ" + id + ".html";
                msg = await StaticObjects.HttpClient.GetAsync(doujinUrl);
            } while (msg.StatusCode == HttpStatusCode.NotFound);
            html = await msg.Content.ReadAsStringAsync();

            var title = Regex.Match(html, "<meta property=\"og:title\" content=\"([^\"]+)").Groups[1].Value;
            title = HttpUtility.HtmlDecode(title[0..^9]);
            var imageUrl = Regex.Match(html, "<meta property=\"og:image\" content=\"([^\"]+)").Groups[1].Value;
            var description = HttpUtility.HtmlDecode(Regex.Match(html, "<meta name=\"description\" content=\"([^\"]+)").Groups[1].Value);
            var price = Regex.Match(html, "class=\"price[^\"]*\">([0-9,]+)").Groups[1].Value.Replace(',', ' ');
            var type = Regex.Match(html, "work_type[^\"]+\"><[^>]+>([^<]+)").Groups[1].Value;
            html = html.Contains("main_genre") ?
                html.Split(new[] { "main_genre" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]
                : "";
            var tags = Regex.Matches(html, "<a href=\"[^\"]+\">([^<]+)").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();

            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = title,
                Url = doujinUrl,
                ImageUrl = imageUrl,
                Description = description,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Type",
                        Value = type,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Price",
                        Value = price + " ¥",
                        IsInline = true
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Tags: {string.Join(", ", tags)}"
                }
            }.Build());
        }

        [Command("Dlsite", RunMode = RunMode.Async), RequireNsfw]
        public async Task Dlsite([Remainder]string query = "")
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://www.dlsite.com/maniax/fsr/=/language/jp/sex_category%5B0%5D/male/ana_flg/all/order%5B0%5D/trend/genre_and_or/or/options_and_or/or/per_page/100/show_type/1/from/fsr.again/keyword/" + query);

            // Parse HTML to only keep search results
            html = html.Split(new[] { "id=\"search_result_list\"" }, StringSplitOptions.None)[1];
            html = html.Split(new[] { "class=\"result_contents\"" }, StringSplitOptions.None)[0];

            var elems = new List<Diaporama.Impl.Dlsite>();
            foreach (var elem in html.Split("work_1col_thumb", StringSplitOptions.None).Skip(1))
            {
                var url = Regex.Match(elem, "href=\"([^\"]+)\"").Groups[1].Value;
                var preview = "http:" + Regex.Match(elem, "src=\"([^\"]+)\"").Groups[1].Value;
                var name = Regex.Match(elem, "alt=\"([^\"]+)\"").Groups[1].Value;
                var id = long.Parse(Regex.Match(elem, "[A-Z]{2}([^\\.]+)\\.html").Groups[1].Value);
                var rating = Regex.Match(elem, "star_rating star_([0-9]{2})");
                var nbDownload = Regex.Match(elem, "<span class=\"_dl_count_[A-Z]{2}[0-9]+\">([0-9,]+)").Groups[1].Value.Replace(',', ' ');
                var price = Regex.Match(elem, "<span class=\"work_price[^\"]*\">([0-9,]+)").Groups[1].Value.Replace(',', ' ');
                var description = HttpUtility.HtmlDecode(Regex.Match(elem, "<dd class=\"work_text\">([^<]+)+").Groups[1].Value);
                var type = Regex.Match(elem, "work_type[^\"]+\">([^<]+)").Groups[1].Value;
                var subElem = elem.Contains("search_tag") ?
                    elem.Split(new[] { "search_tag" }, StringSplitOptions.None)[1].Split(new[] { "</dd>" }, StringSplitOptions.None)[0]
                    : "";
                var tags = Regex.Matches(subElem, "<a href=\"[^\"]+\">([^<]+)").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
                elems.Add(new(url, preview, name, id, rating.Length > 1 ? int.Parse(rating.Groups[1].Value) / 10f : null, nbDownload, price, description, tags, type));
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
        }
    }
}
