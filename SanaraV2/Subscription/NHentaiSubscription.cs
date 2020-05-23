using Discord;
using NHentaiSharp.Core;
using System;
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

        protected override async Task<(int, EmbedBuilder)[]> GetFeed()
        {
            var datas = await SearchClient.SearchAsync();
            return datas.elements.Select(x => ((int)x.id, new EmbedBuilder
            {
                Color = new Color(255, 20, 147),
                Title = x.prettyTitle,
                Description = string.Join(", ", x.tags),
                Url = x.url.ToString(),
                ImageUrl = x.pages[0].imageUrl.ToString(),
                Footer = new EmbedFooterBuilder()
                {
                    Text = Modules.NSFW.Sentences.ClickFull(0) + "\n\n" + Modules.NSFW.Sentences.DownloadDoujinshiInfo(0, x.id.ToString())
                }
            })).ToArray();
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
