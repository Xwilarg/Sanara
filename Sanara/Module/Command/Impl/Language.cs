﻿using Discord;
using Google.Cloud.Translate.V3;
using Google.Cloud.Vision.V1;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Exception;
using Sanara.Service;
using System.Text.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Sanara.Module.Utility;

namespace Sanara.Module.Command.Impl;

public class Language : ISubmodule
{
    public string Name => "Language";
    public string Description => "Get information related to others languages";

    public CommandData[] GetCommands()
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
                aliases: [ "tr" ]
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "urban",
                    Description = "Get the slang definition of a word",
                    Options = new()
                    {
                        new SlashCommandOptionBuilder()
                        {
                            Name = "word",
                            Description = "Word to get the meaning of",
                            Type = ApplicationCommandOptionType.String,
                            IsRequired = true
                        }
                    },
                    IsNsfw = true
                },
                callback: UrbanAsync,
                aliases: []
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
                aliases: []
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
                aliases: [ "ja" ]
            ),
        };
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

        var res = await Utility.Language.GetTranslationEmbedAsync(trClient, iClient, ctx.Provider.GetRequiredService<Credentials>().GoogleProjectId, sentence ?? image!.Url, language);
        await ctx.ReplyAsync(embed: res.embed, components: res.component.Build());
    }

    public async Task UrbanAsync(IContext ctx)
    {
        var word = ctx.GetArgument<string>("word");

        JObject json;
        try
        {
            json = JsonConvert.DeserializeObject<JObject>(await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync("http://api.urbandictionary.com/v0/define?term=" + HttpUtility.UrlEncode(word)));
        }
        catch (HttpRequestException re)
        {
            if (re.StatusCode == System.Net.HttpStatusCode.InternalServerError) // Somehow for some invalid query urbandictionary throws a 500
            {
                throw new CommandFailed("There is no definition for this query.");
            }
            throw;
        }
        if (json["list"].Value<JArray>().Count == 0)
            throw new CommandFailed("There is no definition for this query.");

        int up = json["list"][0]["thumbs_up"].Value<int>();
        int down = json["list"][0]["thumbs_down"].Value<int>();

        if (up <= down)
        {
            throw new CommandFailed("There is no definition having a positive vote ratio for this query");
        }

        int gcd = Utils.GCD(up, down);

        string definition = json["list"][0]["definition"].Value<string>();
        if (definition.Length > 1000)
            definition = definition[..1000] + " [...]";
        string example = json["list"][0]["example"].Value<string>();
        if (example.Length > 1000)
            example = example[..1000] + " [...]";

        var outWord = json["list"][0]["word"].Value<string>();
        var embed = new EmbedBuilder
        {
            Color = Color.Blue,
            Title = Utils.ToWordCase(outWord),
            Url = json["list"][0]["permalink"].Value<string>(),
            Fields = new()
            {
                new EmbedFieldBuilder
                {
                    Name = "Definition",
                    Value = definition
                }
            },
            Footer = new EmbedFooterBuilder
            {
                Text = $"Up/Down vote ratio: {up / gcd} : {down / gcd}"
            }
        };
        if (example != "")
        {
            embed.AddField("Example", example);
        }
        await ctx.ReplyAsync(embed: embed.Build());
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

        await ctx.ReplyAsync(embed: new EmbedBuilder
        {
            Title = finalKanji.Value.ToString(),
            Url = url,
            Description = meaning,
            Fields =
            [
                new EmbedFieldBuilder
                {
                    Name = "Radical",
                    Value = radicalMatch.Groups[2].Value.Trim() + ": " + radicalMatch.Groups[1].Value.Trim()
                },
                new EmbedFieldBuilder
                {
                    Name = "Parts",
                    Value = string.Join("\n", parts.Select(x => x.Value == "" ? x.Key : x.Key + ": " + x.Value))
                },
                new EmbedFieldBuilder
                {
                    Name = "Onyomi",
                    Value = onyomi.Count == 0 ? "None" : string.Join("\n", onyomi.Select(x => x.Key + " (" + x.Value + ")"))
                },
                new EmbedFieldBuilder
                {
                    Name = "Kunyomi",
                    Value = kunyomi.Count == 0 ? "None" : string.Join("\n", kunyomi.Select(x => x.Key + " (" + x.Value + ")"))
                },
            ]
        }.Build());
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

        var embed = new EmbedBuilder
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

        await ctx.ReplyAsync(embed: embed.Build());
    }
}