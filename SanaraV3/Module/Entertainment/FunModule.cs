using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exception;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using VndbSharp;
using VndbSharp.Models;
using WebSocketSharp;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadFunHelp()
        {
            _submoduleHelp.Add("Fun", "Various small entertainement commands");
            _help.Add(("Entertainment", new Help("Fun", "Inspire", new Argument[0], "Get a random 'inspirational' quote using machine learning.", new string[0], Restriction.None, null)));
            _help.Add(("Entertainment", new Help("Fun", "Complete", new[] { new Argument(ArgumentType.MANDATORY, "sentence") }, "Complete the given sentence using machine learning.", new string[0], Restriction.None, "Complete Why can't cats just")));
            _help.Add(("Entertainment", new Help("Fun", "Photo", new[] { new Argument(ArgumentType.OPTIONAL, "query") }, "Get a photo given some search terms, if none is provided, get a random one.", new string[0], Restriction.None, "Photo France")));
            _help.Add(("Entertainment", new Help("Fun", "VNQuote", new Argument[0], "Get a quote from a random Visual Novel.", new string[0], Restriction.Nsfw, null)));
        }
    }
}

namespace SanaraV3.Module.Entertainment
{
    /// <summary>
    /// All "Fun" commands that have no real purposes
    /// </summary>
    public sealed class FunModule : ModuleBase
    {
        [Command("VNQuote"), RequireNsfw]
        public async Task VNQuote()
        {
            var html = await StaticObjects.HttpClient.GetStringAsync("https://vndb.org");
            var match = Regex.Match(html, "footer\">\"<a href=\"\\/v([0-9]+)\"[^>]+>([^<]+)+<");
            var id = match.Groups[1].Value;
            var vn = (await StaticObjects.VnClient.GetVisualNovelAsync(VndbFilters.Id.Equals(uint.Parse(id)), VndbFlags.FullVisualNovel)).ToArray()[0];
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = "From " + vn.Name,
                Url = "https://vndb.org/v" + id,
                Description = match.Groups[2].Value,
                Color = Color.Blue
            }.Build());
        }

        [Command("Photo", RunMode = RunMode.Async)]
        public async Task PhotoAsync()
        {
            var json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://api.unsplash.com/photos/random?client_id=" + StaticObjects.UnsplashToken));
            await PhotoAsync(json);
        }

        [Command("Photo", RunMode = RunMode.Async)]
        public async Task PhotoAsync([Remainder]string query)
        {
            var resp = await StaticObjects.HttpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://api.unsplash.com/photos/random?query=" + HttpUtility.UrlEncode(query) + "&client_id=" + StaticObjects.UnsplashToken));
            if (resp.StatusCode == HttpStatusCode.NotFound)
                throw new CommandFailed("There is no result with these search terms.");
            var json = JsonConvert.DeserializeObject<JObject>(await resp.Content.ReadAsStringAsync());
            await PhotoAsync(json);
        }

        private async Task PhotoAsync(JToken json)
        {
            await ReplyAsync(embed: new EmbedBuilder
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

        [Command("Inspire")]
        public async Task InspireAsync()
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                ImageUrl = await StaticObjects.HttpClient.GetStringAsync("https://inspirobot.me/api?generate=true")
            }.Build());
        }

        [Command("Complete", RunMode = RunMode.Async)]
        public async Task CompleteAsync([Remainder]string sentence)
        {
            IUserMessage msg = null;
            string content = sentence;
            string oldContent = content;

            var ws = new WebSocket("wss://bellard.org/textsynth/ws");
            ws.Origin = "https://bellard.org";

            // Since we may receive a lot of messages from the website, we don't update the embed everytimes we get one
            // Instead we store it in "content"
            ws.OnMessage += (sender, e) =>
            {
                // We need to escape things because of Discord
                content += e.Data.Replace("*", "\\*").Replace("_", "\\_");
            };

            ws.OnError += (sender, e) =>
            {
                content = null;
                _ = Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Exception.Source, e.Message, e.Exception));
            };

            ws.OnClose += (sender, e) =>
            {
                // Sometimes the connection close before we even send the message
                if (msg != null)
                {
                    msg.ModifyAsync(x => x.Embed = new EmbedBuilder
                    {
                        Color = Color.Blue,
                        Description = content
                    }.Build()).GetAwaiter().GetResult();
                }
                content = null;
            };

            ws.Connect();
            ws.Send("g,1558M,40,0.9,1," + StaticObjects.Random.Next(0, int.MaxValue) + "," + sentence);

            msg = await ReplyAsync("", false, new EmbedBuilder
            {
                Color = Color.Blue,
                Description = content,
                Footer = new EmbedFooterBuilder
                {
                    Text = "Please wait for the bot to update the embed"
                }
            }.Build());
            
            // We keep waiting for update until the connection close or an error occur
            await Task.Run(async () =>
            {
                while (content != null)
                {
                    await Task.Delay(2000);
                    if (content != null && oldContent != content)
                    {
                        await msg.ModifyAsync(x => x.Embed = new EmbedBuilder
                        {
                            Color = Color.Blue,
                            Description = content,
                            Footer = new EmbedFooterBuilder
                            {
                                Text = "Please wait for the bot to update the embed"
                            }
                        }.Build());
                    }
                    oldContent = content;
                }
            });
        }
    }
}
