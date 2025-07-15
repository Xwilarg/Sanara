using Discord;
using Sanara.Compatibility;
using Sanara.Exception;

namespace Sanara.Module.Command.Context.Discord
{
    public class DiscordMessageCommandContext : AMessageCommandContext, IContext
    {
        public DiscordMessageCommandContext(IServiceProvider provider, IMessage message, string arguments, CommandData command) : base(arguments, command)
        {
            Provider = provider;
            _message = message;
        }

        protected override void ParseChannel(string data, string name)
        {
            var guild = (_message.Channel as ITextChannel)?.Guild;
            if (guild == null)
            {
                throw new CommandFailed("Command must be done is a guild");
            }
            if (ulong.TryParse(data, out var value))
            {
                argsDict.Add(name, guild.GetTextChannelAsync(value).GetAwaiter().GetResult());
            }
            else
            {
                throw new CommandFailed($"Argument {name} must be an ID to a text channel");
            }
        }

        public IServiceProvider Provider { private init; get; }

        private IMessage _message;
        private IUserMessage? _reply;

        public IMessageChannel Channel => _message.Channel;

        public CommonUser User => new(_message.Author);

        public DateTimeOffset CreatedAt => _message.CreatedAt;

        public T? GetArgument<T>(string key)
        {
            if (!argsDict.ContainsKey(key))
            {
                return default;
            }
            return (T)argsDict[key];
        }

        public async Task<IMessage> GetOriginalAnswerAsync()
        {
            return _message;
        }

        public async Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            if (_reply == null)
            {
                if (Channel is ITextChannel tChan && !(await tChan.Guild.GetCurrentUserAsync()).GuildPermissions.ReadMessageHistory && !tChan.PermissionOverwrites.Any(x => x.Permissions.ReadMessageHistory == PermValue.Allow))
                {
                    _reply = await _message.Channel.SendMessageAsync(text, embed: embed?.ToDiscord(), components: components);
                }
                else
                {
                    _reply = await _message.Channel.SendMessageAsync(text, embed: embed?.ToDiscord(), components: components, messageReference: new MessageReference(_message.Id));
                }
            }
            else
            {
                await _reply.ModifyAsync(x =>
                {
                    x.Content = text;
                    x.Embed = embed?.ToDiscord();
                    x.Components = components;
                });
            }
        }

        public async Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null)
        {
            _reply = await _message.Channel.SendFileAsync(new FileAttachment(file, fileName), text: text, embed: embed?.ToDiscord(), components: components);
        }

        public override string ToString()
        {
            return _message.Content;
        }

        public async Task AddReactionAsync(IEmote emote)
        {
            throw new NotImplementedException();
        }
    }
}
