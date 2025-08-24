using Discord;
using Microsoft.Extensions.DependencyInjection;
using RevoltSharp;
using RevoltSharp.Rest;
using Sanara.Compatibility;
using Sanara.Exception;

namespace Sanara.Module.Command.Context.Revolt;

public class RevoltMessageCommandContext : AMessageCommandContext, IContext
{
    public RevoltMessageCommandContext(IServiceProvider provider, UserMessage msg, string arguments, CommandData command) : base(arguments, command)
    {
        Provider = provider;
        _message = msg;
    }

    public ContextSourceType SourceType => ContextSourceType.Revolt;

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
    private UserMessage? _sentMessage = null;

    public IServiceProvider Provider { private init; get; }
    public DateTimeOffset CreatedAt => _message.CreatedAt;

    public CommonMessageChannel Channel => new(_message.Channel);
    public CommonTextChannel? TextChannel => _message.Channel is RevoltSharp.TextChannel tChan ? new(tChan) : null;
    public CommonUser User => new(_message.Author);

    public async Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false)
    {
        RevoltSharp.FileAttachment att = null;
        if (!string.IsNullOrWhiteSpace(embed?.ImageUrl))
        {
            att = await Provider.GetService<RevoltClient>().Rest.UploadFileAsync(await Provider.GetRequiredService<HttpClient>().GetByteArrayAsync(embed.ImageUrl), $"attachment{Path.GetExtension(embed.ImageUrl)}", UploadFileType.Attachment);
            embed.ImageUrl = null;
        }
        var revoltEmbed = embed?.ToRevolt();

        if (_sentMessage == null)
        {
            _sentMessage = await _message.Channel.SendMessageAsync(text, embeds: revoltEmbed == null ? null : [revoltEmbed], replies: [new MessageReply(_message.Id, true)], attachments: att == null ? null : [att.Id]);
        }
        else
        {
            await _sentMessage.EditMessageAsync(content: new(text), embeds: revoltEmbed == null ? null : new([revoltEmbed]));
        }
    }

    public async Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null)
    {
        if (_sentMessage != null) throw new NotImplementedException();

        _sentMessage = await _message.Channel.SendFileAsync(await Provider.GetRequiredService<HttpClient>().GetByteArrayAsync(embed.ImageUrl), fileName, text);
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

    public Task<CommonMessage> GetOriginalAnswerAsync()
    {
        return Task.FromResult(new CommonMessage(_sentMessage));
    }

    public async Task DeleteAnswerAsync()
    {
        await _sentMessage.DeleteAsync();
    }
}
