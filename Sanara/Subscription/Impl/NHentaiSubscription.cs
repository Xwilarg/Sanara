using Discord;
/*
namespace Sanara.Subscription.Impl
{
    public class NHentaiSubscription : ISubscription
    {
        public async Task<FeedItem[]> GetFeedAsync(int current)
        {
            var datas = await SearchClient.SearchAsync();
            List<FeedItem> finalDatas = new();
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
}
*/