using Discord;
using Sanara.Module.Command;

namespace Sanara.UnitTests
{
    public class TestCommandContext : ICommandContext
    {
        public TestCommandContext(Dictionary<string, object> args)
        {
            _args = args;
        }

        private Dictionary<string, object> _args;
        public Result Result { internal set; get; }

        public IMessageChannel Channel => new TestChannel(this);

        public IUser User => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public T? GetArgument<T>(string key)
        {
            if (!_args.ContainsKey(key))
            {
                return default;
            }
            return (T?)_args[key];
        }

        public Task<IMessage> GetOriginalAnswerAsync()
        {
            throw new NotImplementedException();
        }

        public Task ReplyAsync(string text = "", Embed? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            Result = new()
            {
                Text = text,
                Embed = embed
            };
            return Task.CompletedTask;
        }

        public Task ReplyAsync(Stream file, string fileName, string text = "", Embed? embed = null, MessageComponent? components = null)
        {
            Result = new()
            {
                Text = text,
                Embed = embed
            };
            return Task.CompletedTask;
        }

        public Task AddReactionAsync(IEmote emote)
        {
            throw new NotImplementedException();
        }
    }
}
