using Discord;
using Sanara.Compatibility;

namespace Sanara.Module.Command.Context.Discord
{
    public class GameCommandContext : IContext
    {
        public GameCommandContext(IServiceProvider provider, IMessage message)
        {
            Provider = provider;
            _message = message;
        }

        public IServiceProvider Provider { private init; get; }
        private IMessage _message;
        private IUserMessage? _reply;

        public CommonMessageChannel Channel => new(_message.Channel);
        public CommonUser User => new(_message.Author);
        public DateTimeOffset CreatedAt => _message.CreatedAt;

        public T? GetArgument<T>(string key)
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)_message.Content;
            }
            throw new NotImplementedException();
        }

        public async Task<CommonMessage> GetOriginalAnswerAsync()
        {
            return new(_message);
        }

        public async Task DeleteAnswerAsync()
        {
            await _message.DeleteAsync();
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
            await _message.AddReactionAsync(emote);
        }
    }
}
