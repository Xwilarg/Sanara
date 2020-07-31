using Discord;
using Discord.Commands;
using DiscordUtils;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exceptions;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV3.Modules.Entertainment
{
    /// <summary>
    /// Commands that are centered around a media (such as YouTube) and not a specific feature
    /// For example "Video" would be a feature, "YouTube" is a media it's not just about getting a video, it's about getting a **YouTube** video
    /// </summary>
    public sealed class MediaModule : ModuleBase, IModule
    {
        public string GetModuleName()
            => "Entertainment";

        [Command("Youtube", RunMode = RunMode.Async)]
        public async Task Youtube([Remainder]string search)
        {
            await ReplyAsync(embed: GetEmbedFromVideo(await GetYoutubeVideoAsync(search)).Build());
        }

        public static EmbedBuilder GetEmbedFromVideo(Video video)
        {
            var description = video.Snippet.Description.Split('\n');
            // We make likes/dislikes easier to read: 4000 -> 4k
            var finalViews = Utils.MakeNumberReadable(video.Statistics.ViewCount.ToString());
            var finalLikes = Utils.MakeNumberReadable(video.Statistics.LikeCount.ToString());
            var finalDislikes = Utils.MakeNumberReadable(video.Statistics.DislikeCount.ToString());
            return new EmbedBuilder
            {
                ImageUrl = video.Snippet.Thumbnails.High.Url,
                Title = video.Snippet.Title,
                Url = "https://youtu.be/" + video.Id,
                Description = description.Length > 10 ? (string.Join("\n", video.Snippet.Description.Split('\n').Take(10)) + "\n[...]") : video.Snippet.Description,
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Views: {finalViews}\nLikes: {finalLikes}\nDislikes: {finalDislikes}\nRatio: {((float)video.Statistics.LikeCount / video.Statistics.DislikeCount).Value:0.0}"
                }
            };
        }

        // We split this into another function because it's also used by the Radio
        public static async Task<Video> GetYoutubeVideoAsync(string search)
        {
            // Check if the search given in an URL
            string id = null;
            Match match = Regex.Match(search, "https:\\/\\/www.youtube.com\\/watch\\?v=([^&]+)");
            if (match.Success)
                id = match.Groups[1].Value;
            else
            {
                match = Regex.Match(search, "https:\\/\\/youtu.be\\/([^&]+)");
                if (match.Success)
                    id = match.Groups[1].Value;
                else // If the search is an ID
                {
                    match = Regex.Match(search, "^[0-9a-zA-Z_-]{11}$");
                    if (match.Success && match.Value.Length == 11)
                        id = search;
                }
            }

            Video result = null;
            if (id != null) // If managed to get the Id of the video thanks to the previous REGEX
            {
                VideosResource.ListRequest r = StaticObjects.YouTube.Videos.List("snippet,statistics");
                r.Id = id;
                var resp = (await r.ExecuteAsync()).Items;
                if (resp.Count == 0)
                    throw new CommandFailed($"There is no video with the ID {id}.");
                result = resp[0];
            }
            else
            {
                SearchResource.ListRequest listRequest = StaticObjects.YouTube.Search.List("snippet");
                listRequest.Q = search;
                listRequest.MaxResults = 5;
                var searchListResponse = (await listRequest.ExecuteAsync()).Items;
                if (searchListResponse.Count == 0) // The search returned no result
                    throw new CommandFailed($"There is no video with these search terms.");

                var correctVideos = searchListResponse.Where(x => x.Id.Kind == "youtube#video"); // We remove things that aren't videos from the search (like playlists)
                if (correctVideos.Count() == 0)
                    throw new CommandFailed($"There is no video with these search terms.");

                // For each video, we contact the statistics endpoint
                VideosResource.ListRequest videoRequest = StaticObjects.YouTube.Videos.List("snippet,statistics");
                videoRequest.Id = string.Join(",", correctVideos.Select(x => x.Id.VideoId));
                var videoResponse = (await videoRequest.ExecuteAsync()).Items;
                ulong likes = ulong.MinValue;
                // Sometimes the first result isn't the one we want, so compare the differents results and take the one with the best like/dislike ratio
                bool isExactSearch = false;
                var lowerSearch = search.ToLower();
                foreach (Video res in videoResponse)
                {
                    ulong likeCount = ulong.MinValue;
                    if (res.Statistics.LikeCount != null)
                        likeCount = res.Statistics.LikeCount.Value;
                    if (res.Statistics.DislikeCount != null)
                        likeCount -= res.Statistics.DislikeCount.Value;
                    if (likeCount > likes || result == null)
                    {
                        // We get the best ratio if possible, but if the title match then it's more important
                        var lowerTitle = res.Snippet.Title.ToLower();
                        if (isExactSearch && !lowerTitle.Contains(lowerSearch))
                            continue;
                        likes = likeCount;
                        result = res;
                        if (!isExactSearch && lowerTitle.Contains(lowerSearch))
                            isExactSearch = true;
                    }
                }
            }
            return result;
        }

        [Command("Xkcd")]
        public async Task Xkcd(uint? nb = null)
        {
            JToken json;
            int max;
            json = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync("https://xkcd.com/info.0.json"));
            max = json["num"].Value<int>();
            if (nb.HasValue && nb > max)
                throw new CommandFailed($"The latest comic available is the number {nb}.");
            if (nb == null)
                nb = (uint)StaticObjects.Random.Next(max) + 1;
            json = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync("https://xkcd.com/" + nb.Value + "/info.0.json"));
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Title = json["title"].Value<string>(),
                Url = $"https://xkcd.com/{nb}/",
                ImageUrl = json["img"].Value<string>(),
                Footer = new EmbedFooterBuilder
                {
                    Text = json["alt"].Value<string>()
                }
            }.Build());
        }

        /// <summary>
        /// Get the last xkcd comic
        /// </summary>
        [Command("Xkcd last")]
        public async Task XkcdLast()
        {
            JToken json;
            json = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync("https://xkcd.com/info.0.json"));
            int max = json["num"].Value<int>();
            json = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync("https://xkcd.com/" + max + "/info.0.json"));
            await ReplyAsync(embed: new EmbedBuilder
            {
                Color = Color.Blue,
                Title = json["title"].Value<string>(),
                Url = "https://xkcd.com/" + max,
                ImageUrl = json["img"].Value<string>(),
                Footer = new EmbedFooterBuilder
                {
                    Text = json["alt"].Value<string>()
                }
            }.Build());
        }
    }
}
