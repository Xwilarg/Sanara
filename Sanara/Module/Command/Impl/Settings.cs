using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Database;
using Sanara.Module.Command.Context;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Sanara.Module.Command.Impl;

public class Settings : ISubmodule
{
    public string Name => "Settings";
    public string Description => "Configure and get information about the bot";

    public CommandData[] GetCommands()
    {
        return new[]
        {
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "ping",
                    Description = "Get the latency between the bot and Discord",
                    IsNsfw = false
                },
                callback: PingAsync,
                aliases: []
            ),
#if NSFW_BUILD
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "help",
                    Description = "Get the list of commands",
                    IsNsfw = false
                },
                callback: HelpAsync,
                aliases: []
            ),
#endif
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "botinfo",
                    Description = "Get various information about the bot",
                    IsNsfw = false
                },
                callback: BotInfoAsync,
                aliases: Array.Empty<string>()
            ),
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "configure",
                    Description = "Configure the bot for the current guild",
                    IsNsfw = false,
                    ContextTypes = [ InteractionContextType.Guild ],
                },
                callback: ConfigureAsync,
                adminOnly: true,
                aliases: []
            )
        };
    }

    public async Task HelpAsync(IContext ctx)
    {
        await ctx.ReplyAsync("Type / to see the full list of commands for all bots\nYou can also visit <https://sanara.zirk.eu/commands.html>");
    }

    public async Task ConfigureAsync(IContext ctx)
    {
        var guild = ((ITextChannel)ctx.Channel).Guild;
        var data = await Utility.Settings.GetSettingsDisplayAsync(guild);
        await ctx.ReplyAsync(embed: data.Embed, ephemeral: true, components: data.Components);
    }

    public async Task PingAsync(IContext ctx)
    {
        var content = ":ping_pong: Pong!";
        await ctx.ReplyAsync(content);
        if (ctx is SlashCommandContext)
        {
            var orMsg = await ctx.GetOriginalAnswerAsync();
            await ctx.ReplyAsync(orMsg.Content + "\nLatency: " + orMsg.CreatedAt.Subtract(ctx.CreatedAt).TotalMilliseconds + "ms");
        }
    }

    public async Task BotInfoAsync(IContext ctx)
    {
        var embed = new EmbedBuilder
        {
            Title = "Status",
            Color = Color.Purple
        };
        embed.AddField("Latest version", Utils.ToDiscordTimestamp(new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc, Utils.TimestampInfo.None), true);
        embed.AddField("Last command received", Utils.ToDiscordTimestamp(StaticObjects.LastMessage, Utils.TimestampInfo.TimeAgo), true);
        embed.AddField("Uptime", Utils.ToDiscordTimestamp(StaticObjects.Started, Utils.TimestampInfo.TimeAgo), true);
        embed.AddField("Guild count", ctx.Provider.GetRequiredService<DiscordSocketClient>().Client.Guilds.Count, true);

        var options = new ComponentBuilder();
        if (Program.IsBotOwner(ctx.User))
        {
            options.WithSelectMenu("delCache", StaticObjects.AllGameNames.Select(x => new SelectMenuOptionBuilder(x, ctx.Provider.GetRequiredService<Db>().GetCacheName(x))).ToList(), placeholder: "Select a game cache to delete (require bot restart)");
        }
        options.WithButton("Show Global Stats", "globalStats");

        await ctx.ReplyAsync(embed: embed.Build(), components: options.Build(), ephemeral: true);

        embed.AddField("Useful links",
#if NSFW_BUILD
            " - [Source Code](https://github.com/Xwilarg/Sanara)\n" +
            " - [Website](https://sanara.zirk.eu/)\n" +
#endif
            " - [Invitation Link](https://discord.com/api/oauth2/authorize?client_id=" + StaticObjects.ClientId + "&permissions=51264&scope=bot%20applications.commands)\n"
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
            "Profile Picture: [Uikoui](https://www.pixiv.net/en/users/11608780)"
#endif // TODO: Can prob use current pfp for SFW version
            );

#if NSFW_BUILD
        // Get latests commits
        StringBuilder str = new();
        var json = JsonConvert.DeserializeObject<JArray>(await ctx.Provider.GetRequiredService<HttpClient>().GetStringAsync("https://api.github.com/repos/Xwilarg/Sanara/commits?per_page=5"));
        foreach (var elem in json)
        {
            var time = Utils.ToDiscordTimestamp(DateTime.ParseExact(elem["commit"]["author"]["date"].Value<string>(), "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture), Utils.TimestampInfo.None);
            str.AppendLine($"{time}: [{elem["commit"]["message"].Value<string>()}](https://github.com/Xwilarg/Sanara/commit/{elem["sha"].Value<string>()})");
        }
        embed.AddField("Latest changes", str.ToString());

        await ctx.ReplyAsync(embed: embed.Build(), components: options.Build());
#endif
    }
}
