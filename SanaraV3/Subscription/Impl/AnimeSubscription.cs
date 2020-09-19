using Discord;
using Newtonsoft.Json.Linq;
using SanaraV3.Exception;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SanaraV3.Subscription.Impl
{
    public class AnimeSubscription : ISubscription
    {
        public async Task<FeedItem[]> GetFeedAsync(int current)
        {
            List<FeedItem> items = new List<FeedItem>();
            foreach (var node in await GetFeedInternalAsync())
            {
                string title = GetAttribute(node, "title");
                if (title.GetHashCode() == current)
                    break;
                string animeName = Regex.Match(title, "(^.+) #[0-9]+$").Groups[1].Value; // We get only the title (and remove things such as the episode name)
                string description = "";
                try
                {
                    var result = await Module.Entertainment.JapaneseModule.SearchMediaAsync(Module.Entertainment.JapaneseMedia.ANIME, animeName, true);
                    description = result["attributes"]["synopsis"].Value<string>().Length > 1000 ? result["attributes"]["synopsis"].Value<string>().Substring(0, 1000) + " [...]" : result["attributes"]["synopsis"].Value<string>();
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
                }.Build(), new string[0]));
            }
            return items.ToArray();
        }

        public string GetName()
            => "anime";

        private string GetAttribute(XmlNode node, string name, string attribute = null)
        {
            foreach (XmlNode elem in node)
            {
                if (elem.Name == name)
                {
                    if (attribute == null)
                        return elem.InnerText;
                    return elem.Attributes.GetNamedItem(attribute).InnerText;
                }
            }
            return null;
        }

        private async Task<XmlNode[]> GetFeedInternalAsync()
        {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(await StaticObjects.HttpClient.GetStringAsync("https://www.livechart.me/feeds/episodes"));
            List<XmlNode> nodes = new List<XmlNode>();
            foreach (XmlNode node in xml.ChildNodes[1].FirstChild)
            {
                if (node.Name == "item")
                    nodes.Add(node);
            }
            return nodes.ToArray();
        }
    }
}
