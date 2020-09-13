using Discord;
using Discord.Commands;
using DiscordUtils;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadFunHelp()
        {
            _help.Add(new Help("Inspire", new Argument[0], "Get a random 'inspirational' quote using machine learning.", false));
            _help.Add(new Help("Complete", new[] { new Argument(ArgumentType.MANDATORY, "sentence") }, "Complete the given sentence using machine learning.", false));
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
                Utils.LogError(new LogMessage(LogSeverity.Error, e.Exception.Source, e.Message, e.Exception));
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
