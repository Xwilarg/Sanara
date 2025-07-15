using Discord;
using Discord.WebSocket;
using Sanara.Compatibility;

namespace Sanara.Module.Command.Context.Discord
{
    public class ComponentCommandContext : IContext
    {
        public ComponentCommandContext(IServiceProvider provider, SocketMessageComponent ctx)
            => (Provider, _ctx) = (provider, ctx);

        private SocketMessageComponent _ctx;

        public IServiceProvider Provider { private init; get; }
        public IMessageChannel Channel => _ctx.Channel;
        public CommonUser User => new(_ctx.User);
        public DateTimeOffset CreatedAt => _ctx.CreatedAt;

        public async Task ReplyAsync(string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            if (_ctx.HasResponded)
            {
                await _ctx.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = text;
                    x.Embed = embed?.ToDiscord();
                    x.Components = components;
                });
            }
            else
            {
                await _ctx.RespondAsync(text, embed: embed?.ToDiscord(), components: components, ephemeral: ephemeral);
            }
        }

        public async Task ReplyAsync(Stream file, string fileName, string text = "", CommonEmbedBuilder? embed = null, MessageComponent? components = null)
        {
            if (_ctx.HasResponded)
            {
                await _ctx.FollowupWithFileAsync(file, fileName, text: text, embed: embed?.ToDiscord(), components: components);
            }
            else
            {
                await _ctx.RespondWithFileAsync(
                    fileStream: file,
                    fileName: fileName,
                    text: text,
                    embeds: null,
                    isTTS: false,
                    ephemeral: false,
                    allowedMentions: null,
                    components: components,
                    embed: embed?.ToDiscord(),
                    options: null
                );
            }
        }

        public T? GetArgument<T>(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<IMessage> GetOriginalAnswerAsync()
        {
            return await _ctx.GetOriginalResponseAsync();
        }

        public override string ToString()
        {
            return $"{_ctx.Data.CustomId}";
        }

        public Task AddReactionAsync(IEmote emote)
        {
            throw new NotImplementedException();
        }
    }
}
