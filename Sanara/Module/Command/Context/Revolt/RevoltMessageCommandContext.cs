using Discord;
using RevoltSharp;
using Sanara.Compatibility;
using Sanara.Exception;

namespace Sanara.Module.Command.Context.Revolt
{
    public class RevoltMessageCommandContext : AMessageCommandContext, IContext
    {
        private Dictionary<string, object> argsDict = new();

        public RevoltMessageCommandContext(IServiceProvider provider, UserMessage msg, string arguments, CommandData command) : base(arguments, command)
        {
            Provider = provider;
            _message = msg;
        }

        protected override void ParseChannel(string data, string name)
        {
            var guild = _message.Server;
            if (guild == null)
            {
                throw new CommandFailed("Command must be done is a guild");
            }
            var chan = guild.GetTextChannelAsync(data).GetAwaiter().GetResult();
            if (chan == null)
            {
                throw new CommandFailed($"Argument {name} must be an ID to a text channel");
            }
            argsDict.Add(name, chan);
        }

        private UserMessage _message;

        public IServiceProvider Provider { private init; get; }
        public DateTimeOffset CreatedAt => _message.CreatedAt;

        public IMessageChannel Channel => throw new NotImplementedException();
        public CommonUser User => new(_message.Author);

        public async Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            await _message.Channel.SendMessageAsync(text, embeds: embed == null ? null : [embed.ToRevolt()], replies: [ new MessageReply(_message.Id, true) ]);
        }

        public async Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null)
        {
            throw new NotImplementedException();
        }

        public Task AddReactionAsync(IEmote emote)
        {
            throw new NotImplementedException();
        }

        public T? GetArgument<T>(string key)
        {
            if (!argsDict.ContainsKey(key))
            {
                return default;
            }
            return (T)argsDict[key];
        }

        Task<global::Discord.IMessage> IContext.GetOriginalAnswerAsync()
        {
            throw new NotImplementedException();
        }
    }
}
