using Discord;
using Sanara.Module.Utility;
using System.Xml;

namespace Sanara.Subscription.Impl
{
    public class AnimeSubscription : ISubscription
    {
        public bool DeleteOldMessage => false;

        public async Task<FeedItem[]> GetFeedAsync(int current, bool _)
        {
            List<FeedItem> items = new();
            foreach (var info in await GetFeedInternalAsync())
            {
                items.Add(new FeedItem($"{info.id},{info.episode}".GetHashCode(), new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = info.media.title.romaji,
                    Description = info.media.description,
                    Url = $"https://anilist.co/anime/{info.id}",
                    ImageUrl = info.media.coverImage.large
                }.Build(), Array.Empty<string>()));
            }
            return items.ToArray();
        }

        public string GetName()
            => "anime";

        private static string? GetAttribute(XmlNode node, string name, string? attribute = null)
        {
            foreach (XmlNode elem in node)
            {
                if (elem.Name == name)
                {
                    if (attribute == null)
                        return elem.InnerText;
                    return elem.Attributes?.GetNamedItem(attribute)?.InnerText;
                }
            }
            return null;
        }

        private async Task<AiringSchedule[]> GetFeedInternalAsync()
        {
            return await AniList.GetAnimeFeedAsync();
        }
    }
}
