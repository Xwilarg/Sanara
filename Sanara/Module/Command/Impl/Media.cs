using Discord;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Compatibility;
using Sanara.Exception;
using Sanara.Module.Utility;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Sanara.Module.Command.Impl;

public class Media : ISubmodule
{
    public string Name => "Media";
    public string Description => "Data coming from various medias";

    public CommandData[] GetCommands(IServiceProvider _)
    {
        return new[]
        {
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "inspire",
                    Description = "Get a random \"inspirational\" quote",
                    IsNsfw = false
                },
                callback: InspireAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "vnquote",
                    Description = "Get a quote from a random Visual Novel",
                    IsNsfw = true
                },
                callback: VNQuoteAsync,
                aliases: Array.Empty<string>(),
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported    
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "source",
                    Description = "Get the source of an image",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "image",
                            Description = "URL to the image",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                    IsNsfw = false
                },
                callback: SourceAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "anime",
                    Description = "Get information about an anime/manga/light novel",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "name",
                            Description = "Name",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder()
                        {
                            Name = "type",
                            Description = "What kind of media you are looking for",
                            Type = ApplicationCommandOptionType.Integer,
                            IsRequired = true,
                            Choices = new()
                            {
                                new ApplicationCommandOptionChoiceProperties()
                                {
                                    Name = "Anime",
                                    Value = (int)JapaneseMedia.Anime
                                },
                                new ApplicationCommandOptionChoiceProperties()
                                {
                                    Name = "Manga",
                                    Value = (int)JapaneseMedia.Manga
                                },
                                new ApplicationCommandOptionChoiceProperties()
                                {
                                    Name = "Light Novel",
                                    Value = (int)JapaneseMedia.LightNovel
                                }
                            }
                        }
                    },
                    IsNsfw = false
                },
                callback: AnimeAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "drama",
                    Description = "Get information about a drama",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "name",
                            Description = "Name",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                    IsNsfw = false
                },
                callback: DramaAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "visualnovel",
                    Description = "Get information about a visual novel",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "name",
                            Description = "Name",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                    IsNsfw = false
                },
                callback: VisualNovelAsync,
                aliases: [ "vn" ],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            )
        };
    }

    public async Task InspireAsync(IContext ctx)
    {
        await ctx.ReplyAsync(embed: new CommonEmbedBuilder
        {
            Color = Color.Blue,
            ImageUrl = await Inspire.GetInspireAsync(ctx.Provider.GetRequiredService<HttpClient>())
        });
    }

    private static HttpClient _vndbClient = new HttpClient();
    public async Task VNQuoteAsync(IContext ctx)
    {
        var c = ctx.Provider.GetRequiredService<Credentials>();
        if (c.VndbToken == null) throw new CommandFailed("VNDB token is missing");
        if (!_vndbClient.DefaultRequestHeaders.Any()) _vndbClient.DefaultRequestHeaders.Add("Authorization", $"Token {c.VndbToken}");

        var resp = await _vndbClient.PostAsync("https://api.vndb.org/kana/quote", new StringContent("""{"fields": "vn{id,title,image{url}},quote","filters": [ "random", "=", 1 ]}""", Encoding.UTF8, "application/json"));

        resp.EnsureSuccessStatusCode();

        var vnInfo = System.Text.Json.JsonSerializer.Deserialize<VndbReq<VnResult>>(
            await resp.Content.ReadAsStringAsync(),
            ctx.Provider.GetRequiredService<JsonSerializerOptions>()
        ).Results[0];

        await ctx.ReplyAsync(embed: new CommonEmbedBuilder
        {
            Title = $"From {vnInfo.Vn.Title}",
            Url = $"https://vndb.org/{vnInfo.Vn.Id}",
            Description = vnInfo.Quote,
            //ImageUrl = vnInfo.Vn.Image?.Url,
            Color = Color.Blue
        });
    }

    public async Task SourceAsync(IContext ctx)
    {
        var image = ctx.GetArgument<string>("image");

        await ctx.ReplyAsync(embed: await Tool.GetSourceAsync(ctx.Provider, image));
    }

    public async Task VisualNovelAsync(IContext ctx)
    {
        var name = ctx.GetArgument<string>("name");
        var cleanName = Utils.CleanWord(name);

        var c = ctx.Provider.GetRequiredService<Credentials>();
        if (c.VndbToken == null) throw new CommandFailed("VNDB token is missing");
        if (!_vndbClient.DefaultRequestHeaders.Any()) _vndbClient.DefaultRequestHeaders.Add("Authorization", $"Token {c.VndbToken}");

        var query = """{"fields": "id,title,image{url,sexual,violence},length,languages,platforms,rating,released","filters":["search","=","{0}"]}""".Replace("{0}", cleanName);
        var resp = await _vndbClient.PostAsync("https://api.vndb.org/kana/vn", new StringContent(query, Encoding.UTF8, "application/json"));

        resp.EnsureSuccessStatusCode();

        var res = System.Text.Json.JsonSerializer.Deserialize<VndbReq<VnInfo>>(
            await resp.Content.ReadAsStringAsync(),
            ctx.Provider.GetRequiredService<JsonSerializerOptions>()
        ).Results;

        if (res.Length == 0) throw new CommandFailed("No visual novel were found with this name");

        var vn = res[0];

        var embed = new CommonEmbedBuilder()
        {
            Title = vn.Title,
            Url = "https://vndb.org/" + vn.Id,
            ImageUrl =
#if NSFW_BUILD
            ctx.Channel is ITextChannel channel && vn.Image != null && !channel.IsNsfw && (vn.Image.Sexual >= 1 || vn.Image.Violence >= 1) ? null : vn.Image?.Url,
#else
        vn.Image != null && (vn.Image.Sexual >= 1 || vn.Image.Violence >= 1) ? null : vn.Image?.Url,
#endif
            Description = vn.Description == null ? null : Regex.Replace(vn.Description.Length > 1000 ? vn.Description[0..1000] + " [...]" : vn.Description, "\\[url=([^\\]]+)\\]([^\\[]+)\\[\\/url\\]", "[$2]($1)"),
            Color = Color.Blue
        };
        embed.AddField("Available in english?", vn.Languages.Contains("en") ? "Yes" : "No", true);
        embed.AddField("Available on Windows?", vn.Platforms.Contains("win") ? "Yes" : "No", true);
        string length = "???";
        switch (vn.Length)
        {
            case 1: length = "<2 Hours"; break;
            case 2: length = "2 - 10  Hours"; break;
            case 3: length = "10 - 30 Hours"; break;
            case 4: length = "30 - 50 Hours"; break;
            case 5: length = "\\> 50 Hours"; break;
        }
        embed.AddField("Length", length, true);
        embed.AddField("Vndb Rating", vn.Rating + " / 10", true);
        embed.AddField("Release Date", vn.Released ?? "TBA", true);
        await ctx.ReplyAsync(embed: embed);
    }

    public async Task DramaAsync(IContext ctx)
    {
        var apiKey = ctx.Provider.GetRequiredService<Credentials>().MyDramaListApiKey;
        if (apiKey == null)
        {
            throw new CommandFailed("Drama token is not available");
        }

        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://api.mydramalist.com/v1/search/titles?q=" + HttpUtility.UrlEncode(ctx.GetArgument<string>("name"))),
            Method = HttpMethod.Post
        };
        request.Headers.Add("mdl-api-key", apiKey);

        var response = await ctx.Provider.GetRequiredService<HttpClient>().SendAsync(request);
        var searchResults = JsonConvert.DeserializeObject<JArray>(await response.Content.ReadAsStringAsync());

        if (!searchResults.Any())
            throw new CommandFailed("Nothing was found with this name.");

        var id = searchResults.First().Value<int>("id");
        var drama = await GetDramaAsync(apiKey, ctx.Provider.GetRequiredService<HttpClient>(), id);

        var embed = new CommonEmbedBuilder()
        {
            Title = drama.Value<string>("original_title"),
            Description = drama.Value<string>("synopsis"),
            Color = new Color(0, 97, 157),
            Url = drama.Value<string>("permalink"),
            ImageUrl = drama["images"].Value<string>("poster")
        };

        embed.AddField("English Title", drama.Value<string>("title"), true);
        embed.AddField("Country", drama.Value<string>("country"), true);
        embed.AddField("Episode Count", drama.Value<int>("episodes").ToString(), true);

        if (drama["released"] != null)
        {
            embed.AddField("Released", drama.Value<string>("released"), true);
        }
        else
        {
            embed.AddField("Air date", drama.Value<string>("aired_start") + ((drama["aired_end"] != null) ? " - " + drama.Value<string>("aired_end") : ""), true);
        }
        embed.AddField("Audiance Warning", drama.Value<string>("certification"), true);

        if (drama["votes"] != null)
        {
            embed.AddField("MyDramaList User Rating", drama.Value<double>("rating") + "/10", true);
        }

        await ctx.ReplyAsync(embed: embed);
    }

    public static async Task<JObject> GetDramaAsync(string token, HttpClient client, int id)
    {
        var request = new HttpRequestMessage()
        {
            RequestUri = new Uri("https://api.mydramalist.com/v1/titles/" + id),
            Method = HttpMethod.Get
        };
        request.Headers.Add("mdl-api-key", token);

        var response = await client.SendAsync(request);
        return JsonConvert.DeserializeObject<JObject>(await response.Content.ReadAsStringAsync());
    }

    public async Task AnimeAsync(IContext ctx)
    {
        var name = ctx.GetArgument<string>("name");
        var media = (JapaneseMedia)ctx.GetArgument<long>("type");
        var answer = await AniList.SearchMediaAsync(ctx.Provider.GetRequiredService<HttpClient>(), media, name);

        if (answer == null)
            throw new CommandFailed("Nothing was found with this name.");

        if (ctx.Channel is ITextChannel channel && !channel.IsNsfw && answer.isAdult)
#if NSFW_BUILD
            throw new CommandFailed("The result of your search was NSFW and thus, can only be shown in a NSFW channel.");
#else
        throw new CommandFailed("Nothing was found with this name.");
#endif

        var description = "";
        if (!string.IsNullOrEmpty(answer.description))
            description = answer.description.Length > 1000 ? answer.description[..1000] + " [...]" : answer.description; // Description that fill the whole screen are a pain
        description = Utils.CleanHtml(description);

        var embed = new CommonEmbedBuilder
        {
            Title = answer.title.romaji,
            Color = answer.isAdult ? new Color(255, 20, 147) : Color.Green,
            Url = $"https://anilist.co/{(media == JapaneseMedia.Anime ? "anime" : "manga")}/{answer.id}",
            Description = description,
            ImageUrl = answer.coverImage.large
        };

        if (answer.tags.Any())
            embed.AddField("Tags", string.Join(", ", answer.tags.Where(x => x.rank > 50).Select(x => x.name)));
        if (!string.IsNullOrEmpty(answer.title.english)) // No use displaying this if it's the same as the embed title
            embed.AddField("English Title", answer.title.english, true);
        if (answer.averageScore != null)
            embed.AddField("User Average Rating", answer.averageScore?.ToString(), true);
        if (media == JapaneseMedia.Anime && answer.episodes != null)
            embed.AddField("Episode Count", answer.episodes + (answer.duration != null ? $" ({answer.duration} min per episode)" : ""), true);
        if (answer.startDate.year == null)
            embed.AddField("Release Date", "To Be Announced", true);
        else if (answer.endDate == answer.startDate)
            embed.AddField("Release Date", $"{answer.startDate.year}-{answer.startDate.month.Value:00}-{answer.startDate.day.Value:00}", true);
        else
            embed.AddField("Release Date", $"{answer.startDate.year.Value}-{answer.startDate.month.Value:00}-{answer.startDate.day.Value:00}" + " - " + (answer.endDate.year == null ? "???" : $"{answer.endDate.year}-{answer.endDate.month.Value:00}-{answer.endDate.day.Value:00}"), true);
        if (!string.IsNullOrEmpty(answer.source))
            embed.AddField("Source", Utils.ToWordCase(answer.source.Replace('_', ' ')), true);
        if (!string.IsNullOrEmpty(answer.type))
            embed.AddField("Type", Utils.ToWordCase(answer.type), true);
        if (!string.IsNullOrEmpty(answer.format))
            embed.AddField("Format", answer.format switch
            {
                "TV" => answer.format,
                "OVA" => answer.format,
                "ONA" => answer.format,
                "TV_SHORT" => "TV short",
                _ => Utils.ToWordCase(answer.format.Replace('_', ' '))
            }, true);
        if (answer.genres.Any())
            embed.AddField("Genres", string.Join(", ", answer.genres), true);

        await ctx.ReplyAsync(embed: embed);
    }
}