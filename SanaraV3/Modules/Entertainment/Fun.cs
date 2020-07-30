using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;
using WebSocketSharp;

namespace SanaraV3.Modules.Entertainment
{
    /// <summary>
    /// All "Fun" commands that have no real purposes
    /// </summary>
    public sealed class Fun : ModuleBase, IModule
    {
        public string GetModuleName()
            => "Entertainment";

        [Command("Complete", RunMode = RunMode.Async)]
        public async Task Complete([Remainder]string sentence)
        {
            IUserMessage msg = null;
            string content = sentence;
            string oldContent = content;

            var ws = new WebSocket("ws://163.172.76.10:8080");
            ws.Origin = "http://textsynth.org";

            // Since we may receive a lot of messages from the website, we don't update the embed everytimes we get one
            // Instead we store it in "content"
            ws.OnMessage += (sender, e) =>
            {
                // For some reasons, there are some weird spaces around punctuation
                content += " " + e.Data.Replace(" .", ".").Replace(" '", "'").Replace(" ,", ",");
            };

            ws.OnError += (sender, e) =>
            {
                content = null;
                throw new Exception(e.Message);
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
            ws.Send("g," + sentence);

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
