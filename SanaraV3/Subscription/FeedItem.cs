using Discord;

namespace SanaraV3.Subscription
{
    public struct FeedItem
    {
        public FeedItem(int id, Embed embed, string[] tags)
        {
            Id = id;
            Embed = embed;
            Tags = tags;
        }

        public int Id { get; } // Unique identifier of this item
        public Embed Embed { get; } // Embed to be sent containing all item info
        public string[] Tags { get; } // All tags associated to the item (null if not relevant)
    }
}
