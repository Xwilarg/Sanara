using Discord;
using Sanara.Exception;
using Sanara.Module.Command;
using System.Text.RegularExpressions;
using System.Web;

namespace Sanara.Module.Utility
{
    public static class EHentai
    {
        private static string GetUrl(int category, int rating, string tags)
        {
            string url = $"https://e-hentai.org/?f_cats={category}&f_search=" + Uri.EscapeDataString(tags);
            if (rating != 0)
            {
                url += $"&advsearch=1&f_sname=on&f_stags=on&f_sr=on&f_srdd={rating}";
            }
            return url;
        }

        public static async Task<int> GetEHentaiContentCountAsync(string name, int category, int rating, string tags)
        {
            var url = GetUrl(category, rating, tags);
            string html = await StaticObjects.HttpClient.GetStringAsync(url);
            Match m = Regex.Match(html, "Found about ([0-9,]+) result"); // Get number of results

            if (!m.Success)
            {
                throw new CommandFailed($"There is no {name} with these tags{(rating != 0 ? ", this might be due to the rating given in parameter being too high" : string.Empty)}");
            }

            return int.Parse(m.Groups[1].Value.Replace(",", "")); // Number is displayed like 10,000 so we remove the comma to parse it
        }

        public static async Task<MatchCollection> GetAllMatchesAsync(int category, int rating, string tags, int page)
        {
            string html = await StaticObjects.HttpClient.GetStringAsync(GetUrl(category, rating, tags) + "&page=" + page);
            return Regex.Matches(html, "<a href=\"(https:\\/\\/e-hentai\\.org\\/g\\/([a-z0-9]+)\\/([a-z0-9]+)\\/)\"");
        }


        public static async Task SendEmbedAsync(ICommandContext ctx, string name, Match sM)
        {
            var data = await GetEHentaiContentAsync(name, sM);

            var token = $"ehentai-{Guid.NewGuid()}/{sM.Groups[2].Value}/{sM.Groups[3].Value}/{name}";
            StaticObjects.EHentai.Add(token);
            var button = new ComponentBuilder()
                .WithButton("Download", token);
            await ctx.ReplyAsync(await StaticObjects.HttpClient.GetStreamAsync(data.ImageUrl), $"image{Path.GetExtension(data.ImageUrl)}", embed: data.Embed, components: button.Build());
        }

        private static async Task<(Embed Embed, string ImageUrl)> GetEHentaiContentAsync(string name, Match sM)
        {
            string finalUrl = sM.Groups[1].Value + "?nw=always";
            var html = await StaticObjects.HttpClient.GetStringAsync(finalUrl);

            // Getting tags
            List<string> allTags = new();
            string htmlTags = html.Split(new[] { "taglist" }, StringSplitOptions.None)[1].Split(new[] { "Showing" }, StringSplitOptions.None)[0];
            foreach (Match match in Regex.Matches(htmlTags, ">([^<]+)<\\/a><\\/div>"))
                allTags.Add(match.Groups[1].Value);

            // To get the cover image, we first must go the first image of the gallery then we get it
            string htmlCover = await StaticObjects.HttpClient.GetStringAsync(Regex.Match(html, "<a href=\"([^\"]+)\"><img alt=\"0*1\"").Groups[1].Value);
            string imageUrl = Regex.Match(htmlCover, "<img id=\"img\" src=\"([^\"]+)\"").Groups[1].Value;

            // Getting rating
            string finalRating = Regex.Match(html, "average_rating = ([0-9.]+)").Groups[1].Value;

            return (new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", allTags),
                Title = HttpUtility.HtmlDecode(Regex.Match(html, "<title>(.+) - E-Hentai Galleries<\\/title>").Groups[1].Value),
                Url = finalUrl,
                ImageUrl = $"attachment://image{Path.GetExtension(imageUrl)}",
                Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Rating",
                            Value = finalRating,
                            IsInline = true
                        }
                    }
            }.Build(), imageUrl);
        }
    }
}
