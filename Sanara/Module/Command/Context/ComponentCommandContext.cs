using Discord;
using Discord.WebSocket;

namespace Sanara.Module.Command.Context
{
    public class ComponentCommandContext : ICommandContext
    {
        public ComponentCommandContext(SocketMessageComponent ctx)
            => _ctx = ctx;

        private SocketMessageComponent _ctx;

        public IMessageChannel Channel => _ctx.Channel;
        public IUser User => _ctx.User;
        public DateTimeOffset CreatedAt => _ctx.CreatedAt;

        public async Task ReplyAsync(string text = "", Embed? embed = null, MessageComponent? components = null, bool ephemeral = false)
        {
            if (_ctx.HasResponded)
            {
                await _ctx.ModifyOriginalResponseAsync(x =>
                {
                    x.Content = text;
                    x.Embed = embed;
                    x.Components = components;
                });
            }
            else
            {
                await _ctx.RespondAsync(text, embed: embed, components: components, ephemeral: ephemeral);
            }
        }

        public async Task ReplyAsync(Stream file, string fileName)
        {
            await _ctx.RespondWithFileAsync(
                fileStream: file,
                fileName: fileName,
                text: "",
                embeds: null,
                isTTS: false,
                ephemeral: false,
                allowedMentions: null,
                components: null,
                embed: null,
                options: null
            );
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
