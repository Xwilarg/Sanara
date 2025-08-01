﻿using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sanara.Compatibility;
using Sanara.Database;
using Sanara.Game;
using Sanara.Service;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Sanara.Module.Command.Impl;

public class Settings : ISubmodule
{
    public string Name => "Settings";
    public string Description => "Configure and get information about the bot";

    public CommandData[] GetCommands(IServiceProvider _)
    {
        return new[]
        {
#if NSFW_BUILD
            new CommandData(
                slashCommand: new SlashCommandBuilder()
                {
                    Name = "help",
                    Description = "Get the list of commands",
                    IsNsfw = false
                },
                callback: HelpAsync,
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Supported
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
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Partial
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
                aliases: [],
                discordSupport: Support.Supported,
                revoltSupport: Support.Unsupported
            )
        };
    }

    public async Task HelpAsync(IContext ctx)
    {
        if (ctx.SourceType == Context.ContextSourceType.Discord)
        {
            await ctx.ReplyAsync("Type / to see the full list of commands for all bots\nYou can also visit <https://sanara.zirk.eu/commands.html>");
        }
        else
        {
            await ctx.ReplyAsync("To use Sanara, tag her followed by your command\nExample: <@01JWZMD7W3D14NG8846QB1YD0Z> booru 0 sheep_horns sword\nWill get an image from Safebooru with the tags \"sheep_horns\" and \"sword\"\n\nYou can also visit <https://sanara.zirk.eu/commands.html>");
        }
    }

    public async Task ConfigureAsync(IContext ctx)
    {
        var guild = ((ITextChannel)ctx.Channel).Guild;
        var data = await Utility.Settings.GetSettingsDisplayAsync(ctx.Provider, guild);
        await ctx.ReplyAsync(embed: data.Embed, ephemeral: true, components: data.Components);
    }

    public async Task BotInfoAsync(IContext ctx)
    {
        var embed = new CommonEmbedBuilder
        {
            Title = "Status",
            Color = Color.Purple
        };
        embed.AddField("Latest version", Utils.ToDiscordTimestamp(new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc, Utils.TimestampInfo.None), true);
        embed.AddField("Last command received", Utils.ToDiscordTimestamp(ctx.Provider.GetRequiredService<StatData>().LastMessage, Utils.TimestampInfo.TimeAgo), true);
        embed.AddField("Uptime", Utils.ToDiscordTimestamp(ctx.Provider.GetRequiredService<StatData>().Started, Utils.TimestampInfo.TimeAgo), true);
        embed.AddField("Guild count", ctx.Provider.GetRequiredService<DiscordSocketClient>().Guilds.Count.ToString(), true);

        var options = new ComponentBuilder();
        if (Program.IsBotOwner(ctx.User))
        {
            options.WithSelectMenu("delCache", ctx.Provider.GetRequiredService<GameManager>().AllGameNames.Select(x => new SelectMenuOptionBuilder(x, ctx.Provider.GetRequiredService<Db>().GetCacheName(x))).ToList(), placeholder: "Select a game cache to delete (require bot restart)");
        }
        options.WithButton("Show Global Stats", "globalStats");

        await ctx.ReplyAsync(embed: embed, components: options.Build(), ephemeral: true);

        var orMsg = await ctx.GetOriginalAnswerAsync();
        embed.AddField("Ping", $"Latency: {orMsg.CreatedAt.Subtract(ctx.CreatedAt).TotalMilliseconds}ms", true);

        embed.AddField("Useful links",
#if NSFW_BUILD
            " - [Source Code](https://github.com/Xwilarg/Sanara)\n" +
            " - [Website](https://sanara.zirk.eu/)\n" +
#endif
            (
            ctx.SourceType == Context.ContextSourceType.Discord
            ? " - [Invitation Link](https://discord.com/api/oauth2/authorize?client_id=" + Program.ClientId + "&permissions=51264&scope=bot%20applications.commands)\n"
            : " - [Invitation Link](https://app.revolt.chat/bot/01JWZMD7W3D14NG8846QB1YD0Z)\n"
            )
#if NSFW_BUILD
            +
            " - [Support Server](https://discordapp.com/invite/H6wMRYV)\n" +
            " - [Top.gg](https://top.gg/bot/329664361016721408)"
#endif
            );
        embed.AddField("Credits",
            "Programming: [Zirk](https://zirk.eu/)\n" +
            "With the help of [TheIndra](https://theindra.eu/)\n"
#if NSFW_BUILD
            +
            "Profile Picture: [Fractal](https://x.com/FractalStella)"
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

        await ctx.ReplyAsync(embed: embed, components: options.Build());
#endif
    }
}
