using Discord;

namespace Sanara.UnitTests.Impl
{
    public sealed class UnitTestUserMessage : IUserMessage
    {
        public UnitTestUserMessage(Func<UnitTestUserMessage, Task> callback)
        {
            Channel = new UnitTestMessageChannel(callback);
            Content = "";
            Embeds = new List<Embed>();
        }

        public UnitTestUserMessage(IMessageChannel channel, string message, Embed embed)
        {
            Channel = channel;
            Content = message;
            var embeds = new List<Embed>();
            if (embed != null)
                embeds.Add(embed);
            Embeds = embeds;
        }

        public MessageType Type => MessageType.Default;

        public MessageSource Source => MessageSource.User;

        public bool IsTTS => false;

        public bool IsPinned => false;

        public bool IsSuppressed => false;

        public string Content { get; }

        public DateTimeOffset Timestamp => DateTimeOffset.MinValue;

        public DateTimeOffset? EditedTimestamp => null;

        public IMessageChannel Channel { get; }

        public IUser Author => null;

        public IReadOnlyCollection<IAttachment> Attachments => new List<IAttachment>();

        public IReadOnlyCollection<IEmbed> Embeds { get; }

        public IReadOnlyCollection<ITag> Tags => new List<ITag>();

        public IReadOnlyCollection<ulong> MentionedChannelIds => new List<ulong>();

        public IReadOnlyCollection<ulong> MentionedRoleIds => new List<ulong>();

        public IReadOnlyCollection<ulong> MentionedUserIds => new List<ulong>();

        public MessageActivity Activity => new MessageActivity();

        public MessageApplication Application => new MessageApplication();

        public MessageReference Reference => new MessageReference();

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => new Dictionary<IEmote, ReactionMetadata>();

        public DateTimeOffset CreatedAt => DateTimeOffset.MinValue;

        public ulong Id => ulong.Parse(DateTime.Now.ToString("mmssff"));

        public IUserMessage ReferencedMessage => throw new NotImplementedException();

        public bool MentionedEveryone => throw new NotImplementedException();

        public MessageFlags? Flags => throw new NotImplementedException();

        public IReadOnlyCollection<ISticker> Stickers => throw new NotImplementedException();

        public string CleanContent => throw new NotImplementedException();

        public IReadOnlyCollection<IMessageComponent> Components => throw new NotImplementedException();

        public IMessageInteraction Interaction => throw new NotImplementedException();

        IReadOnlyCollection<IStickerItem> IMessage.Stickers => throw new NotImplementedException();

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
        {
            return Task.CompletedTask;
        }

        public Task CrosspostAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task ModifyAsync(Action<MessageProperties> func, RequestOptions options = null)
        {
            return Task.CompletedTask;
        }

        public Task ModifySuppressionAsync(bool suppressEmbeds, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task PinAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions options = null)
        {
            throw new NotImplementedException();
        }

        public string Resolve(TagHandling userHandling = TagHandling.Name, TagHandling channelHandling = TagHandling.Name, TagHandling roleHandling = TagHandling.Name, TagHandling everyoneHandling = TagHandling.Ignore, TagHandling emojiHandling = TagHandling.Name)
        {
            throw new NotImplementedException();
        }

        public Task UnpinAsync(RequestOptions options = null)
        {
            throw new NotImplementedException();
        }
    }
}
