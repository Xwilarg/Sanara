using Discord;

namespace Sanara.Module.Command.Context
{
    public class GameCommandContext : ICommandContext
    {
        public GameCommandContext(IMessage message)
        {
            _message = message;
        }

        private IMessage _message;
        private IUserMessage? _reply;

        public IMessageChannel Channel => _message.Channel;
        public IUser User => _message.Author;
        public DateTimeOffset CreatedAt => _message.CreatedAt;

        public T? GetArgument<T>(string key)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)_message.Content;
            }
            throw new NotImplementedException();
        }

        public async Task<IMessage> GetOriginalAnswerAsync()
        {
            return _message;
        }

        public async Task ReplyAsync(string text = "", Embed? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            if (_reply == null)
            {
                if (Channel is ITextChannel tChan && !(await tChan.Guild.GetCurrentUserAsync()).GuildPermissions.ReadMessageHistory && !tChan.PermissionOverwrites.Any(x => x.Permissions.ReadMessageHistory == PermValue.Allow))
                {
                    _reply = await _message.Channel.SendMessageAsync(text, embed: embed, components: components);
                }
                else
                {
                    _reply = await _message.Channel.SendMessageAsync(text, embed: embed, components: components, messageReference: new MessageReference(_message.Id));
                }
            }
            else
            {
                await _reply.ModifyAsync(x =>
                {
                    x.Content = text;
                    x.Embed = embed;
                    x.Components = components;
                });
            }
        }

        public async Task ReplyAsync(Stream file, string fileName)
        {
            _reply = await _message.Channel.SendFileAsync(new FileAttachment(file, fileName));
        }

        public override string ToString()
        {
            return _message.Content;
        }

        public async Task AddReactionAsync(IEmote emote)
        {
            await _message.AddReactionAsync(emote);
        }
    }
}
