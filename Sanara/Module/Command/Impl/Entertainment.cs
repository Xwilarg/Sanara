using DeepAI;
using Discord;
using HtmlAgilityPack;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Exception;
using Sanara.Module.Utility;
using System.Text.RegularExpressions;
using VndbSharp;
using VndbSharp.Models;

namespace Sanara.Module.Command.Impl
{
    public class Entertainment : ISubmodule
    {
        public string Name => "Entertainment";
        public string Description => "A collection of fun silly stuffs";

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
                    },
                    callback: InspireAsync,
                    aliases: []
                ),
                new CommandData(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "vnquote",
                        Description = "Get a quote from a random Visual Novel",
                        IsNsfw = true
                    },
                    callback: VNQuoteAsync,
                    aliases: Array.Empty<string>()
                )
            };
        }

        public async Task InspireAsync(IContext ctx)
        {
            await ctx.ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = await Inspire.GetInspireAsync(ctx.Provider.GetRequiredService<HttpClient>())
            }.Build());
        }

        public async Task VNQuoteAsync(IContext ctx)
        {
            var quoteTag = ctx.Provider.GetRequiredService<HtmlWeb>().Load("https://vndb.org").DocumentNode.SelectSingleNode("//footer/span/a");
            var id = quoteTag.Attributes["href"].Value;
            var vn = (await ctx.Provider.GetRequiredService<Vndb>().GetVisualNovelAsync(VndbFilters.Id.Equals(uint.Parse(id[2..])), VndbFlags.FullVisualNovel)).First();
            await ctx.ReplyAsync(embed: new EmbedBuilder
            {
                Title = $"From {vn.Name}",
                Url = $"https://vndb.org{id}",
                Description = quoteTag.InnerHtml,
                Color = Color.Blue
            }.Build());
        }
    }
}