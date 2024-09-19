using Discord;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Exception;

namespace Sanara.Module.Utility;

public static class Tool
{
    public static async Task<Embed> GetSourceAsync(IServiceProvider provider, string paramUrl)
    {
        var web = provider.GetRequiredService<HtmlWeb>();
        var html = web.Load("https://saucenao.com/search.php?url=" + Uri.EscapeDataString(paramUrl));
        var answers = html.DocumentNode.SelectNodes("//div[contains(@class, 'result')]");
        if (!answers.Any())
            throw new CommandFailed("I didn't find the source of this image.");

        var answer = answers[0];
        var url = html.GetElementbyId("resImage0").Attributes["src"].Value;
        var compatibility = answer.SelectSingleNode("//div[contains(@class, 'resultsimilarityinfo')]").InnerHtml;
        var content = answer.SelectSingleNode("//div[contains(@class, 'resultcontentcolumn')]").InnerHtml;

        return new EmbedBuilder
        {
            ImageUrl = url,
            Description = Utils.CleanHtml(content),
            Color = Color.Blue,
            Footer = new EmbedFooterBuilder
            {
                Text = $"Certitude: {compatibility}"
            }
        }.Build();
    }
}
