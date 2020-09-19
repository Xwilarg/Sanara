using Discord;
using Discord.Commands;
using SanaraV3.Exception;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadCommunicationHelp()
        {
            _help.Add(new Help("Quote", new[] { new Argument(ArgumentType.OPTIONAL, "user/message") }, "Quote the message if an user.", false));
            _help.Add(new Help("Poll", new[] { new Argument(ArgumentType.MANDATORY, "name"), new Argument(ArgumentType.MANDATORY, "choices - 1 to 9") }, "Create a poll for users to choose between various choices.", false));
            _help.Add(new Help("Info", new[] { new Argument(ArgumentType.OPTIONAL, "user") }, "Get information about an user from this server.", false));
            _help.Add(new Help("Invite", new Argument[0], "Get the bot invitation link.", false));
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

        [Command("Info")]
        public async Task InfoAsync(IUser user = null)
        {
            if (user == null) // If user is null, that means we want information about the one that sent the command
                user = Context.User;

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
