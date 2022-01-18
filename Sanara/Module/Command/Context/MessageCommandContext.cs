using Discord;
using Sanara.Exception;
using System.Text.RegularExpressions;

namespace Sanara.Module.Command.Context
{
    public class MessageCommandContext : ICommandContext
    {
        private Dictionary<string, string> argsDict = new();

        public MessageCommandContext(IMessage message, string arguments, CommandInfo command)
        {
            _message = message;
            var matches = Regex.Matches(arguments, @"[\""].+?[\""]|[^ ]+");
            List<string>? argsArray;
            if (matches.Count > 0)
            {
                argsArray = matches.Cast<Match>().Select(x => x.Value).ToList();
            }
            else
            {
                argsArray = new();
            }

            if (command.SlashCommand.Options.IsSpecified)
            {
                foreach (var arg in command.SlashCommand.Options.Value)
                {
                    if (argsArray.Count == 0)
                    {
                        if (arg.IsRequired.Value)
                        {
                            throw new CommandFailed("Missing required argument " + arg.Name);
                        }
                    }
                    else
                    {
                        argsDict.Add(arg.Name, argsArray[0]);
                        argsArray.RemoveAt(0);
                    }
                }
            }
        }

        private IMessage _message;
        private IUserMessage? _reply;

        public IMessageChannel Channel => _message.Channel;

        public IUser User => _message.Author;

        public DateTimeOffset CreatedAt => _message.CreatedAt;

        public T? GetArgument<T>(string key)
        {
            if (!argsDict.ContainsKey(key))
            {
                return default;
            }
            var a = argsDict[key];
            if (typeof(T) == typeof(string))
            {
                return (T)(object)a;
            }
            if (typeof(T) == typeof(long))
            {
                return (T)(object)long.Parse(a);
            }
            if (typeof(T) == typeof(ITextChannel))
            {
                var guild = ((ITextChannel)_message.Channel).Guild;
                return (T)(object)guild.GetTextChannelAsync(ulong.Parse(a)).GetAwaiter().GetResult();
            }
            throw new NotImplementedException($"Unknown type {typeof(T)}");
        }

        public async Task<IMessage> GetOriginalAnswerAsync()
        {
            return _message;
        }

        public async Task ReplyAsync(string text = "", Embed? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            if (_reply == null)
            {
                _reply = await _message.Channel.SendMessageAsync(text, embed: embed, components: components, messageReference: new MessageReference(_message.Id));
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
    }
}
