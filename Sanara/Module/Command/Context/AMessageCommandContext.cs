using Discord;
using Sanara.Exception;
using Sanara.Module.Command.Context.Discord;
using System.Text.RegularExpressions;

namespace Sanara.Module.Command.Context
{
    public abstract class AMessageCommandContext
    {
        protected Dictionary<string, object> argsDict = new();

        protected abstract void ParseChannel(string data, string name);

        protected AMessageCommandContext(string arguments, CommandData command)
        {
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

            if (command.SlashCommand.Options != null && command.SlashCommand.Options.Any())
            {
                var last = command.SlashCommand.Options.Last().Name;
                foreach (var arg in command.SlashCommand.Options)
                {
                    if (argsArray.Count == 0)
                    {
                        if (arg.IsRequired.Value)
                        {
                            var errorMsg = $"Missing required argument {arg.Name} of type {arg.Type}";
                            if (arg.Choices?.Any() ?? false)
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

                            case ApplicationCommandOptionType.Attachment:
                                argsDict.Add(arg.Name, new UrlAttachment(data));
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
                                    ParseChannel(data, arg.Name);
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
    }
}
