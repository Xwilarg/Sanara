using Discord;
using Sanara.Compatibility;

namespace Sanara.Module.Command
{
    public interface IContext
    {
        public IServiceProvider Provider { get; }

        public Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false);
        public Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null);
        public Task AddReactionAsync(IEmote emote);
        public T? GetArgument<T>(string key);
        public Task<IMessage> GetOriginalAnswerAsync();
        public CommonMessageChannel Channel { get; }
        public CommonUser User { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}
