using Discord;
using Sanara.Exception;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Sanara.Module.Utility
{
    public static class Tool
    {
        public static async Task<Embed> GetSourceAsync(string paramUrl)
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://saucenao.com/search.php?db=999&url=" + Uri.EscapeDataString(paramUrl));
            if (!html.Contains("<div id=\"middle\">"))
                throw new CommandFailed("I didn't find the source of this image.");
            var subHtml = html.Split(new[] { "<td class=\"resulttablecontent\">" }, StringSplitOptions.None)[1];

            var compatibility = float.Parse(Regex.Match(subHtml, "<div class=\"resultsimilarityinfo\">([0-9]{2,3}\\.[0-9]{1,2})%<\\/div>").Groups[1].Value, CultureInfo.InvariantCulture);
            var content = Utils.CleanHtml(subHtml.Split(new[] { "<div class=\"resultcontentcolumn\">" }, StringSplitOptions.None)[1].Split(new[] { "</div>" }, StringSplitOptions.None)[0]);
            var url = Regex.Match(html, "<img title=\"Index #[^\"]+\"( raw-rating=\"[^\"]+\") src=\"(https:\\/\\/img[0-9]+.saucenao.com\\/[^\"]+)\"").Groups[2].Value;

            return new EmbedBuilder
            {
                ImageUrl = url,
                Description = content,
                Color = Color.Green,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Certitude: {compatibility}%"
                }
            }.Build();
        }
    }
}
