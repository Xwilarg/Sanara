using Discord;
using Discord.Commands;
using SanaraV3.Exception;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
        public void LoadCommunicationHelp()
        {
            _help.Add(new Help("Quote", new[] { new Argument(ArgumentType.OPTIONAL, "user/message") }, "Quote the message if an user.", false));
        }
    }
}

namespace SanaraV3.Module.Tool
{
    public sealed class CommunicationModule : ModuleBase
    {
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
