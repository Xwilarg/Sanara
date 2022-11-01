/*using Discord;
using Sanara.Module.Utility;

namespace Sanara.Subscription.Impl
{
    public class NHentaiSubscription : ISubscription
    {
        public bool DeleteOldMessage => false;

        public async Task<FeedItem[]> GetFeedAsync(int current, bool _)
        {
            var matches = await EHentai.GetAllMatchesAsync(1021, 0, string.Empty, 0); // There are 25 results by page
            var target = matches[rand % 25];
            await EHentai.SendEmbedAsync(ctx, name, target);
            foreach (var x in datas.elements)
            {
                if (x.id == current)
                    break;
                finalDatas.Add(new FeedItem((int)x.id, new EmbedBuilder
                {
                    Color = new Color(255, 20, 147),
                    Title = x.prettyTitle,
                    Description = string.Join(", ", x.tags.Select(y => y.name)),
                    Url = x.url.ToString(),
                    ImageUrl = x.pages[0].imageUrl.ToString(),
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Do the 'Download doujinshi' command with the id '{x.id}' to download the doujinshi."
                    }
                }.Build(), x.tags.Select(y => y.name).ToArray()));
            }
            return finalDatas.ToArray();
        }

        public string GetName()
            => "nhentai";
    }
}*/