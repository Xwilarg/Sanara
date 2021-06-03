using Discord;
using Discord.Commands;
using SanaraV3.Exception;
using SanaraV3.Help;
using SanaraV3.Module.Administration;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadCommunicationHelp()
        {
            _submoduleHelp.Add("Communication", "Commands to interact with Discord");
            _help.Add(("Tool", new Help("Communication", "Quote", new[] { new Argument(ArgumentType.OPTIONAL, "user/message") }, "Quote the message if an user.", new string[0], Restriction.None, "Quote Sanara#1537")));
            _help.Add(("Tool", new Help("Communication", "Poll", new[] { new Argument(ArgumentType.MANDATORY, "name"), new Argument(ArgumentType.MANDATORY, "choices - 1 to 9") }, "Create a poll for users to choose between various choices.", new string[0], Restriction.None, "Poll \"Is cereal a soup or a salad\" Soup Salad")));
            _help.Add(("Tool", new Help("Communication", "Info", new[] { new Argument(ArgumentType.OPTIONAL, "user") }, "Get information about an user from this server.", new string[0], Restriction.None, "Info Sanara#1537")));
            _help.Add(("Tool", new Help("Communication", "BotInfo", new Argument[0], "Get information about the bot.", new string[0], Restriction.None, null)));
            _help.Add(("Tool", new Help("Communication", "ServerInfo", new Argument[0], "Get information about the server.", new[] { "GuildInfo" }, Restriction.None, null)));
            _help.Add(("Tool", new Help("Communication", "Invite", new Argument[0], "Get the bot invitation link.", new string[0], Restriction.None, null)));
            _help.Add(("Tool", new Help("Communication", "RandomCmd", new Argument[0], "Suggest a random command.", new string[0], Restriction.None, null)));
        }
    }
}

namespace SanaraV3.Module.Tool
{
    public sealed class CommunicationModule : ModuleBase
    {
        [Command("Invite")]
        public async Task InviteAsync()
        {
            // Permissions:
            // Send Messages, Embed Links, Attach Files - Basic Usage
            // Connect, Speak - Radio and audio games
            await ReplyAsync("https://discord.com/oauth2/authorize?client_id=" + StaticObjects.ClientId + "&permissions=3196928&scope=bot");
        }

        [Command("Serverinfo"), Alias("Guildinfo")]
        public async Task ServerinfoAsync()
        {
            var subs = await StaticObjects.GetSubscriptionsAsync(Context.Guild.Id);
            var mySubs = subs?.Select(x => "**" + char.ToUpper(x.Key[0]) + string.Join("", x.Key.Skip(1)) + "**: " + (x.Value == null ? "None" : x.Value.Mention));
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = Context.Guild.ToString(),
                Color = Color.Purple,
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Subscriptions",
                        Value = subs == null ? "Not yet initialized" : (mySubs.Count() == 0 ? "None" : string.Join("\n", mySubs))
                    }
                }
            }.Build());
        }

        [Command("Botinfo")]
        public async Task BotinfoAsync() // TODO: Combine command with next one
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Title = StaticObjects.Client.CurrentUser.ToString(),
                Color = Color.Purple,
                ImageUrl = StaticObjects.Client.CurrentUser.GetAvatarUrl(),
                Description = "**List of useful links:**\n" +
                " - [Source Code](https://github.com/Xwilarg/Sanara)\n" +
                " - [Website](https://sanara.zirk.eu/)\n" +
                " - [Invitation Link](https://discord.com/oauth2/authorize?client_id=" + StaticObjects.ClientId + "&permissions=3196928&scope=bot)\n" +
                " - [Support Server](https://discordapp.com/invite/H6wMRYV)\n" +
                " - [Upcoming Features](https://trello.com/b/dVoVeadz/sanara)\n" +
                " - [Top.gg](https://discordbots.org/bot/329664361016721408)\n\n" +
                "**Credits:**\n" +
                "Programming: [Zirk#0001](https://zirk.eu/)\n" +
                "Picture Profile: [BlankSensei](https://www.pixiv.net/en/users/23961764)",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Id",
                        Value = StaticObjects.ClientId,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Account Creation",
                        Value = StaticObjects.Client.CurrentUser.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Guild Joined",
                        Value = (await Context.Guild.GetCurrentUserAsync()).JoinedAt.Value.ToString("dd/MM/yyyy HH:mm:ss"),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Latest Version",
                        Value = new FileInfo(Assembly.GetEntryAssembly().Location).LastWriteTimeUtc.ToString("dd/MM/yyyy HH:mm:ss"),
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Guild Count",
                        Value = StaticObjects.Client.Guilds.Count,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Last message received",
                        Value = StaticObjects.LastMessage.ToString("HH:mm:ss UTC+0"),
                        IsInline = true
                    }
                }
            }.Build());
        }

        [Command("Info")]
        public async Task InfoAsync(IUser user = null)
        {
            if (user == null) // If user is null, that means we want information about the one that sent the command
                user = Context.User;
            else if (user.Id == StaticObjects.ClientId)
            {
                await BotinfoAsync();
                return;
            }

            var embed = new EmbedBuilder
            {
                Title = user.ToString(),
                Color = Color.Purple,
                ImageUrl = user.GetAvatarUrl(),
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Id",
                        Value = user.Id,
                        IsInline = true
                    },
                    new EmbedFieldBuilder
                    {
                        Name = "Account Creation",
                        Value = user.CreatedAt.ToString("dd/MM/yyyy HH:mm:ss"),
                        IsInline = true
                    }
                }
            };
            if (user is IGuildUser guildUser)
            {
                embed.AddField("Guild Joined", guildUser.JoinedAt.Value.ToString("dd/MM/yyyy HH:mm:ss"), true);
            }
            await ReplyAsync(embed: embed.Build());
        }

        [Command("Poll", RunMode = RunMode.Async)]
        public async Task PollAsync(string title, params string[] choices)
        {
            if (choices.Length < 2)
                throw new CommandFailed("You must provide at least 1 choice.");
            if (choices.Length > 9)
                throw new CommandFailed("You can't provide more than 9 choices.");

            // All emotes to be added as reactions
            var emotes = new[] { new Emoji("1️⃣"), new Emoji("2️⃣"), new Emoji("3️⃣"), new Emoji("4️⃣"), new Emoji("5️⃣"), new Emoji("6️⃣"), new Emoji("7️⃣"), new Emoji("8️⃣"), new Emoji("9️⃣") };

            StringBuilder desc = new StringBuilder();
            int i = 0;
            foreach (var c in choices)
            {
                desc.AppendLine(emotes[i] + ": " + c);
                i++;
            }

            var msg = await ReplyAsync(embed: new EmbedBuilder
            {
                Title = title,
                Color = Color.Blue,
                Description = desc.ToString()
            }.Build());
            await msg.AddReactionsAsync(emotes.Take(i).ToArray());
        }

        [Command("Quote", RunMode = RunMode.Async)]
        public async Task QuoteAsync(IMessage msg)
        {
            await QuoteInternalAsync(msg);
        }

        [Command("Quote", RunMode = RunMode.Async)]
        public async Task QuoteAsync(IUser user)
        {
            var msgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var msg = msgs.First(x => x.Author.Id == user.Id);
            if (msg == null)
                throw new CommandFailed("This user didn't post any message here recently.");
            await QuoteInternalAsync(msg);
        }

        [Command("RandomCmd"), Alias("RandomCommand")]
        public async Task RandomAsync()
        {
            var help = StaticObjects.Help.GetHelp(Context.Guild?.Id ?? 0, Context.Channel is ITextChannel chan ? chan.IsNsfw : true, false, false);
            var random = help[StaticObjects.Random.Next(0, help.Count)];

            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Title = "Why not trying \"" + random.Item2.CommandName + "\"",
                Description = "**" + random.Item2.CommandName + " " + string.Join(" ", random.Item2.Arguments.Select(x => x.Type == ArgumentType.MANDATORY ? $"[{x.Content}]" : $"({x.Content})")) + $"**: {random.Item2.Description}" +
                        (random.Item2.Example != null ? $"\n*Example: {random.Item2.Example}*" : ""),
                Footer = new EmbedFooterBuilder
                {
                    Text = "[argument]: Mandatory argument\n" +
                        "(argument): Optional argument"
                }
            }.Build());
        }

        [Command("Quote", RunMode = RunMode.Async), Priority(-1)]
        public async Task QuoteAsync()
        {
            var msgs = await Context.Channel.GetMessagesAsync().FlattenAsync();
            var msg = msgs.First(x => x.Author.Id == Context.User.Id);
            if (msg == null)
                throw new CommandFailed("You didn't post any message here recently.");
            await QuoteInternalAsync(msg);
        }

        private async Task QuoteInternalAsync(IMessage msg)
        {
            await ReplyAsync(embed: new EmbedBuilder
            {
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = msg.Author.GetAvatarUrl(),
                    Name = msg.Author.ToString()
                },
                Description = msg.Content.Length == 0 && msg.Embeds.Count == 1 ? msg.Embeds.ToArray()[0].Description : msg.Content,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"The {msg.CreatedAt.ToString("dd/MM/yy at HH:mm:ss")} in #{msg.Channel.Name}"
                }
            }.Build());
        }
    }
}
