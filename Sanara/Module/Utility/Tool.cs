using Discord;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Exception;

namespace Sanara.Module.Utility;

public static class Tool
{
    public static async Task<CommonEmbedBuilder> GetSourceAsync(IServiceProvider provider, string paramUrl)
    {
        var web = provider.GetRequiredService<HtmlWeb>();
        var html = web.Load("https://saucenao.com/search.php?url=" + Uri.EscapeDataString(paramUrl));
        var answer = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'result')]");
        if (answer == null)
            throw new CommandFailed("I didn't find the source of this image.");

        var url = html.GetElementbyId("resImage0").Attributes["src"].Value;
        var compatibility = answer.SelectSingleNode("//div[contains(@class, 'resultsimilarityinfo')]").InnerHtml;
        var content = answer.SelectSingleNode("//div[contains(@class, 'resultcontentcolumn')]").InnerHtml;

        var link = answer.SelectSingleNode("//div[contains(@class, 'resultcontentcolumn')]").SelectSingleNode("a");
        var linkUrl = link == null ? null : link.Attributes["href"].Value;

        var embed = new CommonEmbedBuilder
        {
            Title = "Closest match",
            Url = linkUrl,
            ImageUrl = url.StartsWith("http") ? url : null,
            Description = Utils.CleanHtml(content),
            Color = Color.Blue
        };
        embed.WithFooter($"Certitude: {compatibility}");
        return embed;
    }
}
