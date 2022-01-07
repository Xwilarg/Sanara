using Discord;
using Newtonsoft.Json.Linq;
using Sanara.Exception;
using Sanara.Module.Command.Impl;
using Sanara.Module.Utility;
using System.Text.RegularExpressions;
using System.Xml;

namespace Sanara.Subscription.Impl
{
    public class AnimeSubscription : ISubscription
    {
        public async Task<FeedItem[]> GetFeedAsync(int current)
        {
            List<FeedItem> items = new();
            foreach (var node in await GetFeedInternalAsync())
            {
                string title = GetAttribute(node, "title");
                if (title.GetHashCode() == current)
                    break;
                string animeName = Regex.Match(title, "(^.+) #[0-9]+$").Groups[1].Value; // We get only the title (and remove things such as the episode name)
                string description = "";
                try
                {
                    var result = await Tool.SearchMediaAsync(JapaneseMedia.Anime, animeName, true);
                    if (result.Attributes.Synopsis != null)
                        description = result.Attributes.Synopsis.Length > 1000 ? result.Attributes.Synopsis[..1000] + " [...]" : result.Attributes.Synopsis;
                }
                catch (CommandFailed) // Can't find an anime with this name
                { }
                items.Add(new FeedItem(title.GetHashCode(), new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = title,
                    Description = description,
                    Url = GetAttribute(node, "guid"),
                    ImageUrl = GetAttribute(node, "media:thumbnail", "url")
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

        private async Task<XmlNode[]> GetFeedInternalAsync()
        {
            XmlDocument xml = new();
            xml.LoadXml(await StaticObjects.HttpClient.GetStringAsync("https://www.livechart.me/feeds/episodes"));
            List<XmlNode> nodes = new();
            foreach (XmlNode node in xml.ChildNodes[1].FirstChild)
            {
                if (node.Name == "item")
                    nodes.Add(node);
            }
            return nodes.ToArray();
        }
    }
}
