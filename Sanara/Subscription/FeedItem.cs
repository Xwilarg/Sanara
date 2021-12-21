using Discord;

namespace Sanara.Subscription
{
    public struct FeedItem
    {
        public FeedItem(int id, Embed embed, string[] tags)
        {
            Id = id;
            Embed = embed;
            Tags = tags;
        }

        /// <summary>
        /// Unique identifier of this item
        /// </summary>
        public int Id { get; }
        /// <summary>
        /// Embed to be sent containing all item info
        /// </summary>
        public Embed Embed { get; }
        /// <summary>
        /// All tags associated to the item (null if not relevant)
        /// </summary>
        public string[] Tags { get; }
    }
}
