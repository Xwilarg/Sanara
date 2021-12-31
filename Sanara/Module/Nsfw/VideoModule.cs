using Discord.Commands;
using System.Text.RegularExpressions;


            //_submoduleHelp.Add("Video", "Get pornographic videos");
            //_help.Add(("Nsfw", new Help("Video", "AdultVideo", new[] { new Argument(ArgumentType.Optional, "tags") }, "Search for an adult video given an optional tag.", new[] { "AV" }, Restriction.Nsfw, null)));


namespace Sanara.Module.Nsfw
{
    public class VideoModule : ModuleBase
    {
        public static async Task<string> DoJavmostHttpRequestAsync(string url)
        {
            int redirectCounter = 0;
            string html;
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod("GET"), url);
            do
            {
                request.Headers.Add("Host", "www5.javmost.com");
                html = await(await StaticObjects.HttpClient.SendAsync(request)).Content.ReadAsStringAsync();
                Match redirect = Regex.Match(html, "<p>The document has moved <a href=\"([^\"]+)\">");
                if (redirect.Success)
                    request = new HttpRequestMessage(new HttpMethod("GET"), redirect.Groups[1].Value);
                else
                    break;
                redirectCounter++;
            } while (redirectCounter < 10);
            return html;
        }
        /*
        [Command("AdultVideo", RunMode = RunMode.Async), RequireNsfw, Alias("AV")]
        public async Task AdultVideoAsync(string tag = "")
        {
            if (StaticObjects.JavmostCategories.Count == 0)
                throw new CommandFailed("Javmost categories aren't loaded yet, please retry later.");

            tag = tag.ToLowerInvariant();
            if (tag == "")
                tag = "all";
            else if (!StaticObjects.JavmostCategories.Contains(tag))
                throw new CommandFailed("This tag doesn't exist");

            string url = "https://www5.javmost.com/category/" + tag;
            string html = await DoJavmostHttpRequestAsync(url);
            int perPage = Regex.Matches(html, "<!-- begin card -->").Count; // Number of result per page
            int total = int.Parse(Regex.Match(html, "<input type=\"hidden\" id=\"page_total\" value=\"([0-9]+)\" \\/>").Groups[1].Value); // Total number of video
            int page = StaticObjects.Random.Next(total / perPage);
            if (page > 0) // If it's the first page, we already got the HTML
            {
                html = await DoJavmostHttpRequestAsync(url + "/page/" + (page + 1));
            }
            var arr = html.Split(new[] { "<!-- begin card -->" }, StringSplitOptions.None).Skip(1).ToList(); // We remove things life header and stuff
            Match videoMatch = null;
            string[] videoTags = null;
            string previewUrl = "";
            while (arr.Count > 0) // That shouldn't fail
            {
                string currHtml = arr[StaticObjects.Random.Next(arr.Count)];
                videoMatch = Regex.Match(currHtml, "<a href=\"(https:\\/\\/www5\\.javmost\\.com\\/([^\\/]+)\\/)\"");
                if (!videoMatch.Success)
                    continue;
                videoMatch = Regex.Match(currHtml, "<a href=\"(https:\\/\\/www5\\.javmost\\.com\\/([^\\/]+)\\/)\"");
                previewUrl = Regex.Match(currHtml, "data-src=\"([^\"]+)\"").Groups[1].Value;
                if (previewUrl.StartsWith("//"))
                    previewUrl = "https:" + previewUrl;
                videoTags = Regex.Matches(currHtml, "<a href=\"https:\\/\\/www5\\.javmost\\.com\\/category\\/([^\\/]+)\\/\"").Cast<Match>().Select(x => x.Groups[1].Value).ToArray();
                break;
            }
            await ReplyAsync(embed: new EmbedBuilder()
            {
                Color = new Color(255, 20, 147),
                Description = string.Join(", ", videoTags),
                Title = videoMatch.Groups[2].Value,
                Url = videoMatch.Groups[1].Value,
                ImageUrl = previewUrl
            }.Build());
        }
        */
    }
}
