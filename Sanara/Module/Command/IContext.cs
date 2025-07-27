using Discord;
using Sanara.Compatibility;
using Sanara.Module.Command.Context;

namespace Sanara.Module.Command
{
    public interface IContext
    {
        public IServiceProvider Provider { get; }

        public ContextSourceType SourceType { get; }

        public Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false);
        public Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null);
        public Task AddReactionAsync(IEmote emote);
        public T? GetArgument<T>(string key);
        public Task<CommonMessage> GetOriginalAnswerAsync();
        public Task DeleteAnswerAsync();
        public CommonMessageChannel Channel { get; }
        public CommonUser User { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}
