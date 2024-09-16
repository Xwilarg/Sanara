using DeepAI;
using Discord;
using Sanara.Exception;
using Sanara.Help;
using Sanara.Module.Utility;
using System.Text.RegularExpressions;
using VndbSharp;
using VndbSharp.Models;

namespace Sanara.Module.Command.Impl
{
    public class Entertainment : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Entertainment", "A collection of fun silly stuffs");
        }

        public CommandData[] GetCommands()
        {
            return new[]
            {
                new CommandData(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "inspire",
                        Description = "Get a random \"inspirational\" quote",
                        IsNsfw = false
                    }.Build(),
                    callback: InspireAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandData(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "vnquote",
                        Description = "Get a quote from a random Visual Novel",
                        IsNsfw = true
                    }.Build(),
                    callback: VNQuoteAsync,
                    precondition: Precondition.NsfwOnly,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandData(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "complete",
                        Description = "Complete the given sentence using machine learning",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "sentence",
                                Description = "Start of the sentence",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: CompleteAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                ),
                new CommandData(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "generate",
                        Description = "Generate an image from a sentence",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "sentence",
                                Description = "Sentence to make the image from",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = true
                            }
                        },
                        IsNsfw = false
                    }.Build(),
                    callback: GenerateAsync,
                    precondition: Precondition.None,
                    aliases: Array.Empty<string>(),
                    needDefer: true
                )
            };
        }

        public async Task InspireAsync(IContext ctx)
        {
            await ctx.ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = await Inspire.GetInspireAsync()
            }.Build());
        }

        public async Task VNQuoteAsync(IContext ctx)
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://vndb.org");
            var match = Regex.Match(html, "footer><span>\"<a href=\"\\/v([0-9]+)\">([^<]+)+<");
            var id = match.Groups[1].Value;
            var vn = (await StaticObjects.VnClient.GetVisualNovelAsync(VndbFilters.Id.Equals(uint.Parse(id)), VndbFlags.FullVisualNovel)).ToArray()[0];
            await ctx.ReplyAsync(embed: new EmbedBuilder
            {
                Title = "From " + vn.Name,
                Url = "https://vndb.org/v" + id,
                Description = match.Groups[2].Value,
                Color = Color.Blue
            }.Build());
        }

        public async Task CompleteAsync(IContext ctx)
        {
            if (StaticObjects.DeepAI == null)
            {
                throw new CommandFailed("Machine Learning client is not available");
            }

            var sentence = ctx.GetArgument<string>("sentence");

            StandardApiResponse resp = StaticObjects.DeepAI.callStandardApi("text-generator", new
            {
                text = sentence,
            });
            var output = (string)resp.output;
            await ctx.ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Description = $"**{output[..sentence.Length]}**{output[sentence.Length..]}"
            }.Build());
        }

        public async Task GenerateAsync(IContext ctx)
        {
            if (StaticObjects.DeepAI == null)
            {
                throw new CommandFailed("Machine Learning client is not available");
            }

            var sentence = ctx.GetArgument<string>("sentence");

            StandardApiResponse resp = StaticObjects.DeepAI.callStandardApi("text2img", new
            {
                text = sentence,
            });
            await ctx.ReplyAsync(await StaticObjects.HttpClient.GetStreamAsync(resp.output_url), $"image{Path.GetExtension(resp.output_url)}", embed: new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = $"attachment://image{Path.GetExtension(resp.output_url)}"
            }.Build());
        }
    }
}