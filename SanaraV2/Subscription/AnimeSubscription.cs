/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.
using Discord;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SanaraV2.Subscription
{
    public class AnimeSubscription : ASubscription
    {
        public AnimeSubscription()
        {
            Program.p.db.InitSubscription("anime").GetAwaiter().GetResult();
            if (GetCurrent() == 0)
            {
                var feed = GetAnimeFeedAsync().GetAwaiter().GetResult();
                if (feed.Length > 0)
                    SetCurrent(GetAttribute(feed[0], "title").GetHashCode()).GetAwaiter().GetResult();
                else
                    SetCurrent(0).GetAwaiter().GetResult();
            }
        }

        public override async Task<(int, EmbedBuilder, string[])[]> GetFeed()
        {
            List<(int, EmbedBuilder, string[])> data = new List<(int, EmbedBuilder, string[])>();
            foreach (var node in await GetAnimeFeedAsync())
            {
                string title = GetAttribute(node, "title");
                if (title.GetHashCode() == GetCurrent())
                    break;
                string animeName = Regex.Match(title, "(^.+) #[1-9]+$").Groups[1].Value;
                string description = "";
                var result = await Features.Entertainment.AnimeManga.SearchAnime(Features.Entertainment.AnimeManga.SearchType.Anime, new[] { animeName }, null);
                if (result.error == Features.Entertainment.Error.AnimeManga.None
                    && result.answer.name.ToLower() == animeName.ToLower())
                {
                    description = result.answer.synopsis;
                }
                data.Add((title.GetHashCode(), new EmbedBuilder
                {
                    Color = Color.Blue,
                    Title = title,
                    Description = description,
                    Url = GetAttribute(node, "guid"),
                    ImageUrl = GetAttribute(node, "media:thumbnail", "url")
                }, new string[0]));
            }
            return data.ToArray();
        }

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

        private async Task<XmlNode[]> GetAnimeFeedAsync()
        {
            XmlDocument xml = new XmlDocument();
            using (HttpClient http = new HttpClient())
                xml.LoadXml(await http.GetStringAsync("https://www.livechart.me/feeds/episodes"));
            List<XmlNode> nodes = new List<XmlNode>();
            foreach (XmlNode node in xml.ChildNodes[1].FirstChild)
            {
                if (node.Name == "item")
                    nodes.Add(node);
            }
            return nodes.ToArray();
        }

        public override async Task SetCurrent(int value)
        {
            await Program.p.db.SetCurrent("anime", value);
        }

        public override int GetCurrent()
        {
            return Program.p.db.GetCurrent("anime");
        }

        public struct AnimeData
        {
            public string name;
            public string description;
            public string previewUrl;
            public string pageUrl;
        }
    }
}
