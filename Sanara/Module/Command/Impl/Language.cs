using Discord;
using Google.Cloud.Translate.V3;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Compatibility;
using Sanara.Exception;
using Sanara.Module.Utility;
using Sanara.Service;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace Sanara.Module.Command.Impl;

public class Language : ISubmodule
{
    public string Name => "Language";
    public string Description => "Get information related to others languages";

    public CommandData[] GetCommands(IServiceProvider _)
    {
        return new[]
        {
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "translate",
                    Description = "Translate a sentence or an image",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "language",
                            Description = "Target language (ISO 639-1)",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        },
                        new SlashCommandOptionBuilder()
                        {
                            Name = "sentence",
                            Description = "Sentence to translate",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = false
                        },
                        new SlashCommandOptionBuilder()
                        {
                            Name = "image",
                            Description = "Image to translate",
                            Type = ApplicationCommandOptionType.Attachment,
                            IsRequired = false
                        }
                    },
                    IsNsfw = false
                },
                callback: TranslateAsync,
                aliases: [ "tr" ],
                discordSupport: Support.Supported,
                revoltSupport: Support.Unsupported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "kanji",
                    Description = "Get information about a kanji",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "kanji",
                            Description = "Kanji to get information about",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                    IsNsfw = false
                },
                callback: KanjiAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "japanese",
                    Description = "Get the definition of a word (Japanese only)",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "word",
                            Description = "Word to get the definition of",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                    IsNsfw = false
                },
                callback: JapaneseAsync,
                aliases: [ "ja" ],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "ocr",
                    Description = "Detect text on an image",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "image",
                            Description = "Image",
                            Type = ApplicationCommandOptionType.Attachment,
                            IsRequired = true
                        }
                    },
                    IsNsfw = false
                },
                callback: OCRAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
            )
        };
    }

    public async Task OCRAsync(IContext ctx)
    {
        var input = ctx.GetArgument<IAttachment>("image");
        var image = await Google.Cloud.Vision.V1.Image.FetchFromUriAsync(input.Url);
        TextAnnotation response;
        try
        {
            response = await ctx.Provider.GetRequiredService<ImageAnnotatorClient>().DetectDocumentTextAsync(image);
        }
        catch (AnnotateImageException)
        {
            throw new CommandFailed("The file given isn't a valid image.");
        }
        if (response == null)
            throw new CommandFailed("There is no text on the image.");

        var embed = new CommonEmbedBuilder();
        var img = SixLabors.ImageSharp.Image.Load(await ctx.Provider.GetRequiredService<HttpClient>().GetStreamAsync(input.Url));
        var pen = new SolidPen(SixLabors.ImageSharp.Color.Red, 2f);

        foreach (var page in response.Pages)
        {
            foreach (var block in page.Blocks)
            {
                foreach (var paragraph in block.Paragraphs)
                {
                    embed.AddField($"Confidence: {(paragraph.Confidence * 100):0.0}%", string.Join(" ", paragraph.Words.Select(x => string.Join("", x.Symbols.Select(s => s.Text)))));

                    // Draw all lines
                    var path = new PathBuilder();
                    path.AddLines(paragraph.BoundingBox.Vertices.Select(v => new SixLabors.ImageSharp.PointF(v.X, v.Y)).ToArray());
                    path.CloseFigure();

                    img.Mutate(x => x.Draw(pen, path.Build()));
                }
            }
        }

        await ctx.ReplyAsync(embed: embed);
        using var mStream = new MemoryStream();
        img.Save(mStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder());
        mStream.Position = 0;
        await ctx.ReplyAsync(mStream, "ocr.jpg");
    }

    public async Task TranslateAsync(IContext ctx)
    {
        var language = ctx.GetArgument<string>("language")!;
        var sentence = ctx.GetArgument<string>("sentence");
        var image = ctx.GetArgument<IAttachment>("image");

        var trClient = ctx.Provider.GetService<TranslationServiceClient>();
        var iClient = ctx.Provider.GetService<ImageAnnotatorClient>();

        if (trClient == null)
        {
            throw new CommandFailed("Translation client was not configured");
        }

        if (image == null && sentence == null)
        {
            throw new CommandFailed("You must either precise the sentence argument or the image one");
        }

        var res = await Utility.Language.GetTranslationEmbedAsync(ctx.Provider, sentence ?? image!.Url, language);
        await ctx.ReplyAsync(embed: res.embed, components: res.component.Build());
    }

    public async Task KanjiAsync(IContext ctx)
    {
        var kanji = ctx.GetArgument<string>("kanji");

        JObject json = JsonConvert.DeserializeObject<JObject>(await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync("https://jisho.org/api/v1/search/words?keyword=" + HttpUtility.UrlEncode(kanji)));
        if (json["data"].Value<JArray>().Count == 0 || json["data"][0]["japanese"][0]["word"] == null)
            throw new CommandFailed("I didn't find any kanji with this query.");

        char? finalKanji = null;

        // If player input is a kanji
        char playerChar = kanji[0];
        foreach (var elem in json["data"].Value<JArray>())
        {
            foreach (var elem2 in elem["japanese"].Value<JArray>())
            {
                if (elem2["word"] != null && elem2["word"].Value<string>() == playerChar.ToString())
                {
                    finalKanji = playerChar;
                }
            }
        }

        // Else we take the first result we got on our Jisho search
        if (!finalKanji.HasValue)
            finalKanji = json["data"][0]["japanese"][0]["word"].Value<string>()[0];

        string url = "https://jisho.org/search/" + finalKanji + "%20%23kanji";
        string html = await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync(url);

        // Radical of the kanji
        var radicalMatch = Regex.Match(html, "<span class=\"radical_meaning\">([^<]+)<\\/span>([^<]+)<\\/span>");

        if (radicalMatch.Length == 0)
        {
            throw new CommandFailed("I didn't find any kanji with this query.");
        }

        // Kanji meaning in english
        var meaning = Regex.Match(html, "<div class=\"kanji-details__main-meanings\">([^<]+)<\\/div>").Groups[1].Value.Trim();

        // All parts composing the kanji
        Dictionary<string, string> parts = new Dictionary<string, string>();
        foreach (var match in Regex.Matches(html, "<a href=\"(\\/\\/jisho\\.org\\/search\\/[^k]+kanji)\">([^<]+)<\\/a>").Cast<Match>())
        {
            string name = match.Groups[2].Value;
            if (name[0] == finalKanji.Value)
                parts.Add(name, meaning);
            else
                parts.Add(name, Regex.Match(await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync("https:" + match.Groups[1].Value), "<div class=\"kanji-details__main-meanings\">([^<]+)<\\/div>").Groups[1].Value.Trim());
        }

        var converter = ctx.Provider.GetRequiredService<JapaneseConverter>();

        // Onyomi and kunyomi (ways to read the kanji)
        Dictionary<string, string> onyomi = new();
        Dictionary<string, string> kunyomi = new();
        if (html.Contains("<dt>On:"))
            foreach (var match in Regex.Matches(html.Split(new[] { "<dt>On:" }, StringSplitOptions.None)[1]
                .Split(new[] { "</dd>" }, StringSplitOptions.None)[0],
                "<a[^>]+>([^<]+)<\\/a>").Cast<Match>())
                onyomi.Add(match.Groups[1].Value, converter.ToRomaji(match.Groups[1].Value));
        if (html.Contains("<dt>Kun:"))
            foreach (var match in Regex.Matches(html.Split(new[] { "<dt>Kun:" }, StringSplitOptions.None)[1]
                .Split(new string[] { "</dd>" }, StringSplitOptions.None)[0],
            "<a[^>]+>([^<]+)<\\/a>").Cast<Match>())
                kunyomi.Add(match.Groups[1].Value, converter.ToRomaji(match.Groups[1].Value));

        var embed = new CommonEmbedBuilder
        {
            Title = finalKanji.Value.ToString(),
            Url = url,
            Description = meaning
        };
        embed.AddField("Radical", radicalMatch.Groups[2].Value.Trim() + ": " + radicalMatch.Groups[1].Value.Trim());
        embed.AddField("Parts", string.Join("\n", parts.Select(x => x.Value == "" ? x.Key : x.Key + ": " + x.Value)));
        embed.AddField("Onyomi", onyomi.Count == 0 ? "None" : string.Join("\n", onyomi.Select(x => x.Key + " (" + x.Value + ")")));
        embed.AddField("Kunyomi", kunyomi.Count == 0 ? "None" : string.Join("\n", kunyomi.Select(x => x.Key + " (" + x.Value + ")")));
        await ctx.ReplyAsync(embed: embed);
    }

    public async Task JapaneseAsync(IContext ctx)
    {
        var word = ctx.GetArgument<string>("word");

        var data = System.Text.Json.JsonSerializer.Deserialize<Jisho>(
            await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync($"http://jisho.org/api/v1/search/words?keyword={HttpUtility.UrlEncode(word)}"),
            ctx.Provider.GetRequiredService<JsonSerializerOptions>()
        )!.Data;

        if (!data.Any())
            throw new CommandFailed("There is no result with this term search.");

        var target = data[0];

        var embed = new CommonEmbedBuilder
        {
            Color = Color.Blue,
            Title = target.Slug,
            Url = $"https://jisho.org/search/{HttpUtility.UrlEncode(word)}"
        };
        var conv = ctx.Provider.GetRequiredService<JapaneseConverter>();

        if (target.Jlpt.Any())
        {
            embed.AddField("JLPT", string.Join(", ", target.Jlpt));
        }
        embed.AddField("Reading", string.Join("\n", target.Japanese.Take(5).Select(x =>
        {
            if (x.Word == null) return $"{x.Reading} ({conv.ToRomaji(x.Reading)})";
            if (x.Reading == null) return x.Word;
            return $"{x.Word} - {x.Reading} ({conv.ToRomaji(x.Reading)})";
        })));
        embed.AddField("Meaning", string.Join("\n", target.Senses.Take(5).Select(x =>
        {
            StringBuilder str = new();
            str.AppendLine($"**{string.Join(", ", x.EnglishDefinitions)}**");
            if (x.Tags.Any()) str.AppendLine(string.Join(", ", x.Tags));
            if (x.Info.Any()) str.AppendLine(string.Join(", ", x.Info));
            return str.ToString();
        })));

        StringBuilder builder = new();
        builder.AppendLine("Other close meanings:");
        foreach (var elem in data.Skip(1).Take(5))
        {
            var title = string.Join(", ", elem.Senses.Select(x => x.EnglishDefinitions[0]).Distinct(StringComparer.InvariantCultureIgnoreCase));
            var jp = elem.Japanese[0];

            string content;
            if (jp.Word == null) content = $"{jp.Reading} ({conv.ToRomaji(jp.Reading)})";
            else if (jp.Reading == null) content = jp.Word;
            else content = $"{jp.Word} - {jp.Reading} ({conv.ToRomaji(jp.Reading)})";
            builder.AppendLine($"> {title}\n{content}");
        }
        embed.WithFooter(builder.ToString());

        await ctx.ReplyAsync(embed: embed);
    }
}