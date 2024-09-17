using Discord;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Exception;
using Sanara.Module.Command;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Sanara.Module.Utility;

public static class EHentai
{
    public static async Task EHentaiDownloadAsync(IContext ctx, IServiceProvider provider, string urlId)
    {
        var web = provider.GetRequiredService<HtmlWeb>();
        var html = web.Load($"https://e-hentai.org/g/{urlId}/");

        // Get all pages
        var name = html.GetElementbyId("gn").InnerHtml;
        var pages = html.DocumentNode.SelectSingleNode("//table[contains(@class, 'ptt')]").FirstChild.ChildNodes;
        var count = int.Parse(pages[pages.Count - 2].SelectSingleNode("a").InnerHtml);
        var dirName = Guid.NewGuid();

        Directory.CreateDirectory("Saves/Download/" + dirName);
        var finalPath = $"Saves/Download/{Guid.NewGuid()}.zip";

        try
        {
            // We iterate on each page
            int pageIndex = 0;
            for (int i = 0; i < count; i++)
            {
                if (count > 1)
                {
                    await ctx.ReplyAsync($"Your file is being downloaded... {i}/{count}");
                }
                var images = html.GetElementbyId("gdt").ChildNodes;
                foreach (var img in images)
                {
                    if (img.HasClass("gdtm")) // Get all images to download them
                    {
                        var node = img.FirstChild.FirstChild.Attributes["href"].Value;
                        var image = web.Load(node).GetElementbyId("img").Attributes["src"].Value;
                        File.WriteAllBytes($"Saves/Download/{dirName}/{pageIndex:000}{Path.GetExtension(image)}",
                        await provider.GetRequiredService<HttpClient>().GetByteArrayAsync(image));
                        pageIndex++;
                    }
                }

                if (i < count - 1)
                {
                    html = web.Load($"https://e-hentai.org/g/{urlId}/?p={i + 1}");
                }
            }
            ZipFile.CreateFromDirectory($"Saves/Download/{dirName}", finalPath);
            FileInfo fi = new(finalPath);
            if (fi.Length < 25_000_000) // 25MB
            {
                string finalName = name;
                foreach (var c in Path.GetInvalidFileNameChars()) finalName = finalName.Replace(c.ToString(), string.Empty);
                finalName += ".zip";
                await ctx.ReplyAsync(new FileStream(finalPath, FileMode.Open), finalName);
                await (await ctx.GetOriginalAnswerAsync()).DeleteAsync();
            }
            else
            {
                var creds = provider.GetRequiredService<Credentials>();
                if (creds.UploadWebsiteUrl != null && creds.UploadWebsiteLocation != null)
                {
                    File.Copy(finalPath, creds.UploadWebsiteLocation + dirName + ".zip");
                    await ctx.ReplyAsync(creds.UploadWebsiteUrl + dirName + ".zip\n-# You file will be deleted after 10 minutes.");
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(600000); // 10 minutes
                        File.Delete(creds.UploadWebsiteLocation + dirName + ".zip");
                    });
                }
                await ctx.ReplyAsync("The result ZIP was too big to be sent on Discord");
            }
        }
        finally
        {
            Directory.Delete("Saves/Download/" + dirName, true);
            File.Delete(finalPath);
        }
    }

    public static async Task GetEHentaiAsync(IContext ctx, string tags, string name, int category)
    {
        int ratingInput = -1;
        int rand = -1;
        try
        {
            ratingInput = (int)(ctx.GetArgument<long?>("rating") ?? 3);

            var results = await GetEHentaiContentCountAsync(ctx.Provider.GetRequiredService<HtmlWeb>(), name, category, ratingInput, tags);
            if (results > 2475) results = 2475; // TODO: can't get a page over 99 somehow

            rand = ctx.Provider.GetRequiredService<Random>().Next(0, results);

            // Get a random page and result from the number we drew
            var matches = await GetAllResults(ctx.Provider.GetRequiredService<HtmlWeb>(), category, ratingInput, tags, rand / 25); // There are 25 results by page

            var finalMatches = matches.Where(x => x.ChildNodes.Count > 2); // Skip the header of the table and remove ads
            var target = finalMatches.ElementAt(rand % finalMatches.Count());

            // Get all data from our HTML element
            var url = target.ChildNodes[2].SelectSingleNode("a").Attributes["href"].Value;
            var title = target.ChildNodes[2].SelectSingleNode("a").FirstChild.InnerHtml;
            var resTags = string.Join(", ", target.ChildNodes[2].SelectSingleNode("a").ChildNodes[1].ChildNodes.Select(x => x.InnerHtml));
            var previewAttributes = target.ChildNodes[1].ChildNodes.First(x => x.HasClass("glthumb")).FirstChild.FirstChild.Attributes;
            var preview = previewAttributes.Contains("data-src") ? previewAttributes["data-src"].Value : previewAttributes["src"].Value;
            var pageCount = target.ChildNodes[3].ChildNodes[1].InnerHtml;
            var downloadUrl = GetEHentaiButton(url);

            var embed = new EmbedBuilder()
                .WithTitle(title)
                .WithUrl(url)
                .WithImageUrl(preview)
                .WithColor(Color.Blue)
                .WithFields(
                    new EmbedFieldBuilder()
                        .WithName("Tags")
                        .WithValue(resTags),
                    new EmbedFieldBuilder()
                        .WithName("Pages")
                        .WithValue(pageCount)
                );

            var component = new ComponentBuilder()
                .WithButton("Download", $"download-ehentai-{downloadUrl}");

            await ctx.ReplyAsync(embed: embed.Build(), components: component.Build());
        }
        catch (System.Exception e)
        {
            StringBuilder str = new();
            str.AppendLine($"Search for {GetUrl(category, ratingInput, tags, 0)} failed");
            str.AppendLine($"Looking for result {rand} (page {rand / 25} element {rand % 25})");
            throw new RuntimeCommandException(str.ToString(), e);
        }
    }

    public static async Task<IEnumerable<HtmlNode>> GetAllResults(HtmlWeb web, int category, int rating, string tags, int page)
    {
        var url = GetUrl(category, rating, tags, page);
        var html = web.Load(url);
        return html.DocumentNode.SelectSingleNode("//table[contains(@class, 'gltc')]").ChildNodes.Skip(1);
    }

    /// <summary>
    /// Get the amount of result for a e-hentai query
    /// </summary>
    public static async Task<int> GetEHentaiContentCountAsync(HtmlWeb web, string name, int category, int rating, string tags)
    {
        var url = GetUrl(category, rating, tags, 0);
        var html = web.Load(url);
        var searchText = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'searchtext')]");
        if (searchText == null) throw new CommandFailed($"There is no {name} with these tags{(rating != 0 ? ", this might be due to the rating given in parameter being too high" : string.Empty)}");
        var div = searchText.SelectSingleNode("p");
        Match m = Regex.Match(div.InnerHtml, "([0-9,]+)"); // Get number of results

        if (!m.Success) // Somehow e-hentai like to sometimes return weird values like "Found hundreds of results." instead of a clear result
        {
            if (div.InnerHtml.Contains("hundreds")) return 200;
            if (div.InnerHtml.Contains("thousands")) return 2000;
            // Else I guess I can let it crash and see the others values
        }

        return int.Parse(m.Groups[1].Value.Replace(",", "")); // Number is displayed like 10,000 so we remove the comma to parse it
    }

    /// <summary>
    /// Get e-hentai URL matching a query
    /// </summary>
    private static string GetUrl(int category, int rating, string tags, int page)
    {
        string url = $"https://e-hentai.org/?f_cats={category}&f_search=" + Uri.EscapeDataString(tags);
        if (rating > 1)
        {
            url += $"&advsearch=1&f_sname=on&f_stags=on&f_sr=on&f_srdd={rating}";
        }
        if (page > 1)
        {
            url += $"&range={page}";
        }
        return url;
    }

    public static string GetEHentaiButton(string url)
    {
        var downloadUrl = Regex.Match(url, "e-hentai\\.org\\/g\\/([0-9a-z]+\\/[0-9a-z]+)").Groups[1].Value;
        return $"download-ehentai-{downloadUrl}";
    }
}
