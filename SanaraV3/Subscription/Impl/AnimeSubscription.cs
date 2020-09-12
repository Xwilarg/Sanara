using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace SanaraV3.Subscription.Impl
{
    public class AnimeSubscription : ISubscription
    {
        public Task<FeedItem[]> GetFeedAsync(int current)
        {
            throw new NotImplementedException();
        }

        public string GetName()
            => "name";

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
