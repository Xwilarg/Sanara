using Discord;
using Discord.WebSocket;

namespace Sanara.Module.Command.Context
{
    public class SlashCommandContext : ICommandContext
    {
        public SlashCommandContext(SocketSlashCommand ctx)
            => _ctx = ctx;

        private SocketSlashCommand _ctx;

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
            return (T?)(_ctx.Data.Options.FirstOrDefault(x => x.Name == key)?.Value ?? default);
        }

        public async Task<IMessage> GetOriginalAnswerAsync()
        {
            return await _ctx.GetOriginalResponseAsync();
        }

        public override string ToString()
        {
            return $"{_ctx.Data.Name} {string.Join(", ", _ctx.Data.Options.Select(x => $"{x.Name}: {x.Value}"))}";
        }
    }
}
