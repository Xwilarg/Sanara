using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Exception;
using Sanara.Help;
using System.Net;
using System.Web;

namespace Sanara.Module.Command.Impl
{
    public class Tool : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Tool", "Utility commands");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "photo",
                        Description = "Find a photo given an optional query",
                        Options = new()
                        {
                            new SlashCommandOptionBuilder()
                            {
                                Name = "query",
                                Description = "Filter the serach given a term",
                                Type = ApplicationCommandOptionType.String,
                                IsRequired = false
                            }
                        }
                    }.Build(),
                    callback: PhotoAsync,
                    precondition: Precondition.None,
                    needDefer: false
                )
            };
        }

        public async Task PhotoAsync(SocketSlashCommand ctx)
        {
            if (StaticObjects.UnsplashToken == null)
            {
                throw new CommandFailed("Photo token is not available");
            }

            string? query = (string?)ctx.Data.Options.FirstOrDefault(x => x.Name == "query")?.Value;

            JObject json;
            if (query == null)
            {
                json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://api.unsplash.com/photos/random?client_id=" + StaticObjects.UnsplashToken));
            }
            else
            {
                var resp = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos/random?query=" + HttpUtility.UrlEncode(query) + "&client_id=" + StaticObjects.UnsplashToken));
                if (resp.StatusCode == HttpStatusCode.NotFound)
                    throw new CommandFailed("There is no result with these search terms.");
                json = JsonConvert.DeserializeObject<JObject>(await resp.Content.ReadAsStringAsync());
            }
            await ctx.RespondAsync(embed: new EmbedBuilder
            {
                Title = "By " + json["user"]["name"].Value<string>(),
                Url = json["links"]["html"].Value<string>(),
                ImageUrl = json["urls"]["full"].Value<string>(),
                Footer = new EmbedFooterBuilder
                {
                    Text = json["description"].Value<string>()
                },
                Color = Color.Blue
            }.Build());
        }
    }
}
