using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Help;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Sanara.Module.Command.Impl
{
    public class Settings : ISubmodule
    {
        public SubmoduleInfo GetInfo()
        {
            return new("Settings", "Configure and get information about the bot");
        }

        public CommandInfo[] GetCommands()
        {
            return new[]
            {
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "ping",
                        Description = "Get the latency between the bot and Discord"
                    }.Build(),
                    callback: PingAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "botinfo",
                        Description = "Get various information about the bot"
                    }.Build(),
                    callback: BotInfoAsync,
                    precondition: Precondition.None,
                    needDefer: false
                ),
                new CommandInfo(
                    slashCommand: new SlashCommandBuilder()
                    {
                        Name = "configure",
                        Description = "Configure the bot for the current guild"
                    }.Build(),
                    callback: ConfigureAsync,
                    precondition: Precondition.AdminOnly | Precondition.GuildOnly,
                    needDefer: false
                )
            };
        }

        public async Task ConfigureAsync(SocketSlashCommand ctx)
        {
            var guild = ((ITextChannel)ctx.Channel).Guild;
            var subs = await StaticObjects.GetSubscriptionsAsync(guild.Id);
            var mySubs = subs?.Select(x => $"**{Utils.ToWordCase(x.Key)}**: {(x.Value == null ? "None" : x.Value.Mention)}");
            var button = new ComponentBuilder();
            if (subs != null)
            {
                foreach (var sub in subs)
                {
                    if (sub.Value != null)
                    {
                        button.WithButton($"Remove {sub.Key} subscription", $"delSub-{sub.Key}", style: ButtonStyle.Danger);
                    }
                }
            }
            button.WithButton("Database dump", "dump", style: ButtonStyle.Secondary);
            await ctx.RespondAsync(embed: new EmbedBuilder
            {
                Title = guild.ToString(),
                Color = Color.Purple,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Subscriptions",
                        Value = subs == null ? "Not yet initialized" : (mySubs.Any() ?  string.Join("\n", mySubs) : "None")
                    }
                }
            }.Build(), ephemeral: true, components: button.Build());
        }

        public async Task PingAsync(SocketSlashCommand ctx)
        {
            var content = ":ping_pong: Pong!";
            await ctx.RespondAsync(content);
            var orMsg = await ctx.GetOriginalResponseAsync();
            await ctx.ModifyOriginalResponseAsync(x => x.Content = orMsg.Content + "\nLatency: " + orMsg.CreatedAt.Subtract(ctx.CreatedAt).TotalMilliseconds + "ms");
        }

        public async Task BotInfoAsync(SocketSlashCommand ctx)
        {
            var embed = new EmbedBuilder
            {
                Title = "Status",
                Color = Color.Purple
            };
            embed.AddField("Latest version", Utils.ToDiscordTimestamp(new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc, Utils.TimestampInfo.None), true);
            embed.AddField("Last command received", Utils.ToDiscordTimestamp(StaticObjects.LastMessage, Utils.TimestampInfo.TimeAgo), true);
            embed.AddField("Uptime", Utils.ToDiscordTimestamp(StaticObjects.Started, Utils.TimestampInfo.TimeAgo), true);
            embed.AddField("Guild count", StaticObjects.Client.Guilds.Count, true);

            // Get informations about games
            StringBuilder str = new();
            List<string> gameNames = new();
            foreach (var elem in StaticObjects.Preloads)
            {
                // We only get games once so we skip when we get the "others" versions (like audio)
                //if (elem.GetNameArg() != null && elem.GetNameArg() != "hard")
                //    continue;
                // var fullName = name + (elem.GetNameArg() != null ? $" {elem.GetNameArg()}" : "");
                var loadInfo = elem.Load();
                if (loadInfo != null)
                    str.AppendLine($"**{Utils.ToWordCase(elem.Name)}**: {elem.Load().Count} words.");
                else // Get information at runtime
                    str.AppendLine($"**{Utils.ToWordCase(elem.Name)}**: None");
            }
            embed.AddField("Games", str.ToString());

            // Get information about subscriptions
            var subs = StaticObjects.GetSubscriptionCount();
            embed.AddField("Subscriptions",
                subs == null ?
                    "Not yet initialized" :
#if NSFW_BUILD
                    string.Join("\n", subs.Select(x => "**" + char.ToUpper(x.Key[0]) + string.Join("", x.Key.Skip(1)) + "**: " + x.Value)));
#else
                    "**Anime**: " + subs["anime"]);
#endif

            embed.AddField("Useful links",
#if NSFW_BUILD
                " - [Source Code](https://github.com/Xwilarg/Sanara)\n" +
                " - [Website](https://sanara.zirk.eu/)\n" +
#endif
                " - [Invitation Link](https://discord.com/api/oauth2/authorize?client_id=" + StaticObjects.ClientId + "&scope=bot%20applications.commands)\n"
#if NSFW_BUILD
                +
                " - [Support Server](https://discordapp.com/invite/H6wMRYV)\n" +
                " - [Top.gg](https://discordbots.org/bot/329664361016721408)"
#endif
                );
            embed.AddField("Credits",
                "Programming: [Zirk#0001](https://zirk.eu/)\n" +
                "With the help of [TheIndra](https://theindra.eu/)\n"
#if NSFW_BUILD
                +
                "Profile Picture: [BlankSensei](https://www.pixiv.net/en/users/23961764)"
#endif // TODO: Can prob use current pfp for SFW version
                );

            await ctx.RespondAsync(embed: embed.Build(), ephemeral: true);
#if NSFW_BUILD
            // Get latests commits
            str = new();
            var json = JsonConvert.DeserializeObject<JArray>(await StaticObjects.HttpClient.GetStringAsync("https://api.github.com/repos/Xwilarg/Sanara/commits?per_page=5"));
            foreach (var elem in json)
            {
                var time = Utils.ToDiscordTimestamp(DateTime.ParseExact(elem["commit"]["author"]["date"].Value<string>(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture), Utils.TimestampInfo.None);
                str.AppendLine($"{time}: [{elem["commit"]["message"].Value<string>()}](https://github.com/Xwilarg/Sanara/commit/{elem["sha"].Value<string>()})");
            }
            embed.AddField("Latest changes", str.ToString());

            await ctx.ModifyOriginalResponseAsync(x => x.Embed = embed.Build());
#endif
        }
    }
}
