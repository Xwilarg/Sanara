using Discord;
using Discord.WebSocket;
using Sanara.Compatibility;

namespace Sanara.Module.Command.Context.Discord
{
    public class SlashCommandContext : IContext
    {
        public SlashCommandContext(IServiceProvider provider, SocketSlashCommand ctx)
            => (Provider, _ctx) = (provider, ctx);

        public ContextSourceType SourceType => ContextSourceType.Discord;

        private SocketSlashCommand _ctx;

        public IServiceProvider Provider { private init; get; }
        public CommonMessageChannel Channel => new(_ctx.Channel);
        public CommonTextChannel? TextChannel => _ctx.Channel is ITextChannel tChan ? new(tChan) : null;
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
            return (T?)(_ctx.Data.Options.FirstOrDefault(x => x.Name == key)?.Value ?? default);
        }

        public async Task<CommonMessage> GetOriginalAnswerAsync()
        {
            return new(await _ctx.GetOriginalResponseAsync());
        }

        public async Task DeleteAnswerAsync()
        {
            await (await _ctx.GetOriginalResponseAsync()).DeleteAsync();
        }

        public override string ToString()
        {
            return $"{_ctx.Data.Name} {string.Join(", ", _ctx.Data.Options.Select(x => $"{x.Name}: {x.Value}"))}";
        }

        public Task AddReactionAsync(IEmote emote)
        {
            throw new NotImplementedException();
        }
    }
}
