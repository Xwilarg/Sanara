using Discord;
using Sanara.Compatibility;
using Sanara.Module.Command;
using Sanara.Module.Command.Context;

namespace Sanara.UnitTests
{
    public class TestCommandContext : IContext
    {
        public TestCommandContext(IServiceProvider provider, Dictionary<string, object> args)
        {
            Provider = provider;
            _args = args;
        }

        public ContextSourceType SourceType => ContextSourceType.Discord;

        public IServiceProvider Provider { init; get; }

        private Dictionary<string, object> _args;
        public Result Result { internal set; get; }

        public CommonMessageChannel Channel => new(new TestChannel(this));
        public CommonTextChannel? TextChannel => null; // TODO

        public CommonUser User => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public T? GetArgument<T>(string key)
        {
            if (!_args.ContainsKey(key))
            {
                return default;
            }
            return (T?)_args[key];
        }

        public Task AddReactionAsync(IEmote emote)
        {
            throw new NotImplementedException();
        }

        public Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            Result = new()
            {
                Text = text,
                Embed = embed.ToDiscord()
            };
            return Task.CompletedTask;
        }

        public Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null)
        {
            Result = new()
            {
                Text = text,
                Embed = embed.ToDiscord()
            };
            return Task.CompletedTask;
        }

        Task<CommonMessage> IContext.GetOriginalAnswerAsync()
        {
            throw new NotImplementedException();
        }

        public Task DeleteAnswerAsync()
        {
            throw new NotImplementedException();
        }
    }
}
