using Discord;

namespace Sanara.Module.Command
{
    public interface ICommandContext
    {
        public Task ReplyAsync(string text = "", Embed? embed = null, MessageComponent? components = null, bool ephemeral = false);
        public Task ReplyAsync(Stream file, string fileName, string text = "", Embed? embed = null, MessageComponent? components = null);
        public Task AddReactionAsync(IEmote emote);
        public T? GetArgument<T>(string key);
        public Task<IMessage> GetOriginalAnswerAsync();
        public IMessageChannel Channel { get; }
        public IUser User { get; }
        public DateTimeOffset CreatedAt { get; }
    }
}
