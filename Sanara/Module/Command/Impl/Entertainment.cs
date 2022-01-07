using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Help;
using System.Text;
using System.Text.RegularExpressions;
using VndbSharp;
using VndbSharp.Models;

namespace Sanara.Module.Command.Impl
{
    public class Entertainment : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Entertainment", "Commands which main aim is to entertain");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "inspire",
                        Description = "Get a random \"inspirational\" quote"
                    }.Build(),
                    callback: InspireAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "vnquote",
                        Description = "Get a quote from a random Visual Novel"
                    }.Build(),
                    callback: VNQuoteAsync,
                    precondition: Precondition.NsfwOnly,
                    needDefer: false
                ),
                new CommandInfo(
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
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: CompleteAsync,
                    precondition: Precondition.None,
                    needDefer: true
                )
            };
        }

        public async Task InspireAsync(SocketSlashCommand ctx)
        {
            await ctx.RespondAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = await StaticObjects.HttpClient.GetStringAsync("https://inspirobot.me/api?generate=true")
            }.Build());
        }

        public async Task VNQuoteAsync(SocketSlashCommand ctx)
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://vndb.org");
            var match = Regex.Match(html, "footer\">\"<a href=\"\\/v([0-9]+)\"[^>]+>([^<]+)+<");
            var id = match.Groups[1].Value;
            var vn = (await StaticObjects.VnClient.GetVisualNovelAsync(VndbFilters.Id.Equals(uint.Parse(id)), VndbFlags.FullVisualNovel)).ToArray()[0];
            await ctx.RespondAsync(embed: new EmbedBuilder
            {
                Title = "From " + vn.Name,
                Url = "https://vndb.org/v" + id,
                Description = match.Groups[2].Value,
                Color = Color.Blue
            }.Build());
        }

        public async Task CompleteAsync(SocketSlashCommand ctx)
        {
            var sentence = (string)(ctx.Data.Options.FirstOrDefault(x => x.Name == "sentence")?.Value ?? "");

            var embed = new EmbedBuilder
            {
                Description = "Please wait, this can take up to a few minutes...",
                Color = Color.Blue
            };
            var timer = DateTime.Now;
            var resp = await StaticObjects.HttpClient.PostAsync("https://api.eleuther.ai/complete", new StringContent("{\"context\":\"" + sentence.Replace("\"", "\\\"").Replace("\n", "\\n") + "\",\"top_p\":0.9,\"temp\":1}", Encoding.UTF8, "application/json"));
            resp.EnsureSuccessStatusCode();
            embed.Footer = new EmbedFooterBuilder
            {
                Text = $"Time elapsed: {(DateTime.Now - timer).TotalSeconds:0.00}s"
            };
            var json = await resp.Content.ReadAsStringAsync();
            embed.Description = "**" + sentence + "**" + JsonConvert.DeserializeObject<JArray>(json)[0]["generated_text"].Value<string>();
            await ctx.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
        }
    }
}