using Discord;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Exception;
using Sanara.Module.Utility;
using System.Web;

namespace Sanara.Module.Command.Impl;

public class Music : ISubmodule
{
    public string Name => "Settings";
    public string Description => "Configure and get information about the bot";

    public CommandData[] GetCommands(IServiceProvider _)
    {
        return new[]
        {
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "lyrics",
                    Description = "Find a song lyrics",
                    IsNsfw = false,
                    Options = [
                        new SlashCommandOptionBuilder()
                        {
                            Name = "song",
                            Description = "Name of the song you're looking for",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    ]
                },
                callback: LyricsAsync,
                adminOnly: false,
                aliases: []
            )
        };
    }

    public async Task LyricsAsync(IContext ctx)
    {
        var web = ctx.Provider.GetRequiredService<HtmlWeb>();

        // Get main page
        var targetUrl = $"https://utaten.com/search?sort=popular_sort_asc&artist_name=&title={HttpUtility.UrlEncode(ctx.GetArgument<string>("song"))}";
        var html = web.Load(targetUrl);
        var searchTarget = html.DocumentNode.SelectSingleNode("//p[contains(@class, 'searchResult__title')]");
        if (searchTarget == null)
        {
            throw new CommandFailed("Not song of that name was found", ephemeral: true);
        }
        var href = searchTarget.ChildNodes[1].Attributes["href"].Value;

        html = web.Load($"https://utaten.com{href}");
        
        var comp = new ComponentBuilder()
            .WithButton("Kanji", $"lyrics-kanji-{href}")
            .WithButton("Hiragana", $"lyrics-hiragana-{href}")
            .WithButton("Romaji", $"lyrics-romaji-{href}")
            .Build();
        await ctx.ReplyAsync(embed: new CommonEmbedBuilder()
        {
            Title = html.DocumentNode.SelectSingleNode("//h2[contains(@class, 'newLyricTitle__main')]").ChildNodes[0].InnerHtml,
            ImageUrl = html.DocumentNode.SelectSingleNode("//div[contains(@class, 'lyricData__sub')]//img").Attributes["src"].Value,
            Description = await Lyrics.GetRawLyricsAsync(html, Lyrics.DisplayMode.Kanji),
        }, components: comp);
    }
}
