using Discord;
using Sanara.Exception;
using System.Text.RegularExpressions;

namespace Sanara.Module.Command.Context
{
    public class MessageCommandContext : ICommandContext
    {
        private Dictionary<string, object> argsDict = new();

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

            if (command.SlashCommand.Options.IsSpecified && command.SlashCommand.Options.Value.Any())
            {
                var last = command.SlashCommand.Options.Value.Last().Name;
                foreach (var arg in command.SlashCommand.Options.Value)
                {
                    if (argsArray.Count == 0)
                    {
                        if (arg.IsRequired.Value)
                        {
                            var errorMsg = $"Missing required argument {arg.Name} of type {arg.Type}";
                            if (arg.Choices.Any())
                            {
                                errorMsg += $"\n\nAvailable choices:\n{string.Join("\n", arg.Choices.Select(x => $"{x.Value}: {x.Name}"))}";
                            }
                            throw new CommandFailed(errorMsg);
                        }
                    }
                    else
                    {
                        var data = argsArray[0];
                        switch (arg.Type)
                        {
                            case ApplicationCommandOptionType.String:
                                if (last == arg.Name)
                                {
                                    argsDict.Add(arg.Name, string.Join(" ", argsArray));
                                }
                                else
                                {
                                    argsDict.Add(arg.Name, data);
                                }
                                break;

                            case ApplicationCommandOptionType.Integer:
                                {
                                    if (long.TryParse(data, out var value))
                                    {
                                        argsDict.Add(arg.Name, value);
                                    }
                                    else
                                    {
                                        var errorMsg = $"Argument {arg.Name} must be a number";
                                        if (arg.Choices.Any())
                                        {
                                            errorMsg += $"\n\nAvailable choices:\n{string.Join("\n", arg.Choices.Select(x => $"{x.Value}: {x.Name}"))}";
                                        }
                                        throw new CommandFailed(errorMsg);
                                    }
                                }
                                break;

                            case ApplicationCommandOptionType.Channel:
                                {
                                    var guild = (_message.Channel as ITextChannel)?.Guild;
                                    if (guild == null)
                                    {
                                        throw new CommandFailed("Command must be done is a guild");
                                    }
                                    if (ulong.TryParse(data, out var value))
                                    {
                                        argsDict.Add(arg.Name, guild.GetTextChannelAsync(value).GetAwaiter().GetResult());
                                    }
                                    else
                                    {
                                        throw new CommandFailed($"Argument {arg.Name} must be an ID to a text channel");
                                    }
                                }
                                break;

                            default:
                                throw new NotImplementedException($"Unknown type {arg.Type}");
                        }
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
            return (T)argsDict[key];
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
            throw new NotImplementedException();
        }
    }
}
