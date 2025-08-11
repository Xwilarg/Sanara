using Discord;

namespace Sanara.Compatibility;

public class CommonMessageChannel
{
    public CommonMessageChannel(Discord.IMessageChannel chan)
    {
        Id = chan.Id.ToString();
        _dChan = chan;
    }

    public CommonMessageChannel(RevoltSharp.Channel chan)
    {
        Id = chan.Id;
    }

    public string Id { private set; get; }

    public async Task<IUserMessage> SendMessageAsync(string? content = null, CommonEmbedBuilder? embed = null, MessageComponent? components = null)
    {
        if (_dChan == null) throw new NotImplementedException();

        return await _dChan.SendMessageAsync(content, embed: embed?.ToDiscord(), components: components);
    }

    public async Task<IUserMessage> SendFileAsync(Stream stream, string name)
    {
        if (_dChan == null) throw new NotImplementedException();

        return await _dChan.SendFileAsync(stream, name);
    }

    public T As<T>() where T : class, IMessageChannel => _dChan as T;

    private IMessageChannel? _dChan;
}
