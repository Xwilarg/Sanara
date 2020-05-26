using Discord;
using NHentaiSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Subscription
{
    public class NHentaiSubscription : ASubscription
    {
        public NHentaiSubscription()
        {
            var feed = GetFeed().GetAwaiter().GetResult();
            if (feed.Length > 0)
                Current = feed[0].Item1;
            else
                Current = 0;
        }

        public override async Task<(int, EmbedBuilder, string[])[]> GetFeed()
        {
            var datas = await SearchClient.SearchAsync();
            List<(int, EmbedBuilder, string[])> allDoujins = new List<(int, EmbedBuilder, string[])>();
            foreach (var x in datas.elements)
            {
                Console.WriteLine(x.id + " == " + Current);
                if (x.id == Current)
                    break;
                allDoujins.Add(((int)x.id, new EmbedBuilder
                {
                    Color = new Color(255, 20, 147),
                    Title = x.prettyTitle,
                    Description = string.Join(", ", x.tags.Select(y => y.name)),
                    Url = x.url.ToString(),
                    ImageUrl = x.pages[0].imageUrl.ToString(),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = Modules.NSFW.Sentences.ClickFull(0) + "\n\n" + Modules.NSFW.Sentences.DownloadDoujinshiInfo(0, x.id.ToString())
                    }
                }, x.tags.Select(y => y.name).ToArray()));
            }
            allDoujins.Reverse();
            return allDoujins.ToArray();
        }

        public struct NHentaiData
        {
            public long id;
            public string name;
            public string[] tags;
            public Uri urlDoujinshi;
            public Uri urlImage;
        }
    }
}
