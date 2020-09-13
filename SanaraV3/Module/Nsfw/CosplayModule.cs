using Discord;
using Discord.Commands;
using SanaraV3.Exception;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadCosplayHelp()
        {
            _help.Add(new Help("Cosplay", new[] { new Argument(ArgumentType.OPTIONAL, "tags") }, "Get a random cosplay.", true));
        }
    }
}

namespace SanaraV3.Module.Nsfw
{
    public sealed class CosplayModule : ModuleBase
    {
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
                    Text = $"Do the 'Download doujinshi' command with the id '{sM.Groups[2].Value + "/" + sM.Groups[3].Value}' to download the doujinshi."
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
