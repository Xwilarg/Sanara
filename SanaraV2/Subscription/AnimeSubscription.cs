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
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SanaraV2.Subscription
{
    public class AnimeSubscription
    {
        public AnimeSubscription()
        {
            var feed = GetAnimeFeedAsync().GetAwaiter().GetResult();
            currName = GetAttribute(feed[0], "title");
        }

        public async Task UpdateChannelAsync(ITextChannel[] channels)
        {
            List<AnimeData> data = new List<AnimeData>();
            foreach (var node in await GetAnimeFeedAsync())
            {
                string title = GetAttribute(node, "title");
                if (title == currName)
                    break;
                string animeName = Regex.Match(title, "(^.+) #[1-9]+$").Groups[1].Value;
                string description = "";
                var result = await Features.Entertainment.AnimeManga.SearchAnime(true, new[] { animeName }, null);
                if (result.error == Features.Entertainment.Error.AnimeManga.None
                    && result.answer.name == animeName)
                {
                    description = result.answer.synopsis;
                }
                data.Add(new AnimeData
                {
                    name = title,
                    pageUrl = GetAttribute(node, "guid"),
                    previewUrl = GetAttribute(node, "media:thumbnail", "url"),
                    description = description
                });
            }
            if (data.Count > 0)
            {
                currName = data[0].name;
                foreach (var chan in channels)
                {
                    try
                    {
                        foreach (var elem in data)
                        {
                            await chan.SendMessageAsync("", false, new EmbedBuilder
                            {
                                Color = Color.Blue,
                                Title = elem.name,
                                Description = elem.description,
                                Url = elem.pageUrl,
                                ImageUrl = elem.previewUrl
                            }.Build());
                        }
                    }
                    catch (Exception e)
                    {
                        await Program.p.LogError(new LogMessage(LogSeverity.Error, e.Source, e.Message, e));
                    }
                }
            }
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

        string currName;

        private struct AnimeData
        {
            public string name;
            public string description;
            public string previewUrl;
            public string pageUrl;
        }
    }
}
