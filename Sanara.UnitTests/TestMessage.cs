using Discord;

namespace Sanara.UnitTests
{
    public class TestMessage : IUserMessage
    {
        public IUserMessage ReferencedMessage => throw new NotImplementedException();

        public MessageType Type => throw new NotImplementedException();

        public MessageSource Source => throw new NotImplementedException();

        public bool IsTTS => throw new NotImplementedException();

        public bool IsPinned => throw new NotImplementedException();

        public bool IsSuppressed => throw new NotImplementedException();

        public bool MentionedEveryone => throw new NotImplementedException();

        public string Content => throw new NotImplementedException();

        public string CleanContent => throw new NotImplementedException();

        public DateTimeOffset Timestamp => throw new NotImplementedException();

        public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();

        public IMessageChannel Channel => throw new NotImplementedException();

        public IUser Author => throw new NotImplementedException();

        public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();

        public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();

        public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedChannelIds => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedRoleIds => throw new NotImplementedException();

        public IReadOnlyCollection<ulong> MentionedUserIds => throw new NotImplementedException();

        public MessageActivity Activity => throw new NotImplementedException();

        public MessageApplication Application => throw new NotImplementedException();

        public MessageReference Reference => throw new NotImplementedException();

        public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();

        public IReadOnlyCollection<IMessageComponent> Components => throw new NotImplementedException();

        public IReadOnlyCollection<IStickerItem> Stickers => throw new NotImplementedException();

        public MessageFlags? Flags => throw new NotImplementedException();

        public IMessageInteraction Interaction => throw new NotImplementedException();

        public DateTimeOffset CreatedAt => throw new NotImplementedException();

        public ulong Id => throw new NotImplementedException();

        public IThreadChannel Thread => throw new NotImplementedException();

        public MessageRoleSubscriptionData RoleSubscriptionData => throw new NotImplementedException();

        public Task AddReactionAsync(IEmote emote, RequestOptions options = null)
        {
            throw new NotImplementedException();
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
