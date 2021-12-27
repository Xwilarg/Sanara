/*
namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadCosplayHelp()
        {
            _submoduleHelp.Add("Cosplay", "Get cosplay of characters");
            _help.Add(("Nsfw", new Help("Cosplay", "Cosplay", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random cosplay.", new string[0], Restriction.Nsfw, "Cosplay Ikazuchi")));
        }
    }
}

namespace SanaraV3.Module.Nsfw
{
    public sealed class CosplayModule : ModuleBase
    {
        // TODO: We can probably centralize downloads somewhere
        [Command("Download cosplay", RunMode = RunMode.Async), Alias("dl cosplay")]
        public async Task DownloadCosplayAsync(string id)
        {
            var splitId = id.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (splitId.Length != 2)
                throw new CommandFailed("The format of the given id is incorrect.");

            string idFirst = splitId[0];
            string idSecond = splitId[1];
            if (!idFirst.All(x => char.IsLetterOrDigit(x)) || !idSecond.All(x => char.IsLetterOrDigit(x)))
                throw new CommandFailed("The format of the given id is incorrect.");

            string html;
            int nbPages;
            int limitPages;
            try
            {
                html = await StaticObjects.HttpClient.GetStringAsync("https://e-hentai.org/g/" + idFirst + "/" + idSecond);
                var m = Regex.Match(html, "Showing [0-9]+ - ([0-9]+) of ([0-9]+) images");
                if (!m.Success)
                    throw new CommandFailed("There is no cosplay with this id.");
                limitPages = int.Parse(m.Groups[1].Value);
                nbPages = int.Parse(m.Groups[2].Value);

                await ReplyAsync("Preparing download... This might take some time.");
            }
            catch (HttpException)
            {
                throw new CommandFailed("There is no cosplay with this id.");
            }

            string path = id + "_" + DateTime.Now.ToString("HHmmssff") + StaticObjects.Random.Next(0, int.MaxValue);

            string idStr = string.Join("-", splitId);

            Directory.CreateDirectory("Saves/Download/" + path);
            Directory.CreateDirectory("Saves/Download/" + path + "/" + idStr);
            int nextPage = 1;
            for (int i = 1; i <= nbPages; i++)
            {
                var imageMatch = Regex.Match(html, "<a href=\"https:\\/\\/e-hentai.org\\/s\\/([a-z0-9]+)\\/" + idFirst + "-" + i + "\">");
                string html2 = await StaticObjects.HttpClient.GetStringAsync("https://e-hentai.org/s/" + imageMatch.Groups[1].Value + "/" + idFirst + "-" + i);
                Match m = Regex.Match(html2, "<img id=\"img\" src=\"([^\"]+)\"");
                string url = m.Groups[1].Value;
                string extension = "." + url.Split('.').Last();
                File.WriteAllBytes("Saves/Download/" + path + "/" + idStr + "/" + DoujinModule.Get3DigitNumber(i.ToString()) + extension,
                await StaticObjects.HttpClient.GetByteArrayAsync(url));
                if (i == limitPages)
                {
                    html = await StaticObjects.HttpClient.GetStringAsync("https://e-hentai.org/g/" + idFirst + "/" + idSecond + "/?p=" + nextPage);
                    m = Regex.Match(html, "Showing [0-9]+ - ([0-9]+) of [0-9]+ images");
                    limitPages = int.Parse(m.Groups[1].Value);
                    nextPage++;
                }
            }

            string finalPath = "Saves/Download/" + path + "/" + idStr + ".zip";
            ZipFile.CreateFromDirectory("Saves/Download/" + path + "/" + idStr, finalPath);
            for (int i = Directory.GetFiles("Saves/Download/" + path + "/" + idStr).Length - 1; i >= 0; i--)
                File.Delete(Directory.GetFiles("Saves/Download/" + path + "/" + idStr)[i]);
            Directory.Delete("Saves/Download/" + path + "/" + idStr);

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

        [Command("Cosplay", RunMode = RunMode.Async)]
        public async Task CosplayAsync()
            => await CosplayAsync("");

        [Command("Cosplay", RunMode = RunMode.Async)]
        public async Task CosplayAsync([Remainder]string tags)
        {
            // 959 means we only take cosplays
            string url = "https://e-hentai.org/?f_cats=959&f_search=" + Uri.EscapeDataString(tags);
            string html = await StaticObjects.HttpClient.GetStringAsync(url);
            Match m = Regex.Match(html, "Showing ([0-9,]+) result"); // Get number of results

            if (!m.Success)
                throw new CommandFailed("There is no cosplay with these tags");

            int rand = StaticObjects.Random.Next(0, int.Parse(m.Groups[1].Value.Replace(",", ""))); // Number is displayed like 10,000 so we remove the comma to parse it
            html = await StaticObjects.HttpClient.GetStringAsync(url + "&page=" + (rand / 25)); // There are 25 results by page
            var sM = Regex.Matches(html, "<a href=\"(https:\\/\\/e-hentai\\.org\\/g\\/([a-z0-9]+)\\/([a-z0-9]+)\\/)\"")[rand % 25];
            string finalUrl = sM.Groups[1].Value;
            html = await StaticObjects.HttpClient.GetStringAsync(finalUrl);

            // Getting tags
            List<string> allTags = new List<string>();
            string htmlTags = html.Split(new[] { "taglist" }, StringSplitOptions.None)[1].Split(new[] { "Showing" }, StringSplitOptions.None)[0];
            foreach (Match match in Regex.Matches(htmlTags, ">([^<]+)<\\/a><\\/div>"))
                allTags.Add(match.Groups[1].Value);

            // To get the cover image, we first must go the first image of the gallery then we get it
            string htmlCover = await StaticObjects.HttpClient.GetStringAsync(Regex.Match(html, "<a href=\"([^\"]+)\"><img alt=\"0*1\"").Groups[1].Value);
            string imageUrl = Regex.Match(htmlCover, "<img id=\"img\" src=\"([^\"]+)\"").Groups[1].Value;

            // Getting rating
            string rating = Regex.Match(html, "average_rating = ([0-9.]+)").Groups[1].Value;

            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", allTags),
                Title = HttpUtility.HtmlDecode(Regex.Match(html, "<title>(.+) - E-Hentai Galleries<\\/title>").Groups[1].Value),
                Url = finalUrl,
                ImageUrl = imageUrl,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Do the 'Download cosplay' command with the id '{sM.Groups[2].Value + "/" + sM.Groups[3].Value}' to download the doujinshi."
                },
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Rating",
                        Value = rating,
                        IsInline = true
                    }
                }
            }.Build());
        }
    }
}
*/