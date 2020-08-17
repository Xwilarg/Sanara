using Discord;
using Discord.Commands;
using DiscordUtils;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exceptions;
using System;
using System.Collections.Generic;
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
        public string ModuleName { get { return "Entertainment"; } }

        [Command("Reddit hot", RunMode = RunMode.Async)]
        public async Task RedditHotAsync([Remainder]string name)
        {
            await GetRedditEmbedAsync(name, "hot");
        }

        [Command("Reddit top", RunMode = RunMode.Async)]
        public async Task RedditTopAsync([Remainder]string name)
        {
            await GetRedditEmbedAsync(name, "hot");
        }

        [Command("Reddit new", RunMode = RunMode.Async)]
        public async Task RedditNewAsync([Remainder]string name)
        {
            await GetRedditEmbedAsync(name, "new");
        }

        [Command("Reddit random", RunMode = RunMode.Async), Alias("Reddit"), Priority(-1)]
        public async Task RedditRandomAsync([Remainder]string name)
        {
            name = name.ToLowerInvariant();
            var arr = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync($"https://api.reddit.com/r/{name}/random"));
            if (!(arr is JArray))
            {
                if (arr["data"]["children"].Value<JArray>().Count == 0)
                    throw new CommandFailed("There is no post available in this subreddit");
                throw new CommandFailed("This post subreddit doesn't handle random post");
            }
            JToken token = arr[0]["data"]["children"][0]["data"];
            var post = GetRedditEmbed(token);
            if (post == null) // If the post returned is null that means we weren't able to send it because the chan is not NSFW
                throw new CommandFailed("There is no safe post available in this subreddit");
            await ReplyAsync(embed: Diaporama.ReactionManager.Post(post, 1, 1));
        }

        private async Task GetRedditEmbedAsync(string name, string filter)
        {
            name = name.ToLowerInvariant();
            JArray arr = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync($"https://api.reddit.com/r/{name}/{filter}"))["data"]["children"].Value<JArray>();
            List<Diaporama.Reddit> elems = new List<Diaporama.Reddit>();
            foreach (var e in arr)
            {
                var data = GetRedditEmbed(e["data"]);
                if (data != null)
                    elems.Add(data);
            }
            if (elems.Count == 0)
            {
                if (arr.Count == 0)
                    throw new CommandFailed("There is no post available in this subreddit");
                throw new CommandFailed("There is no safe post available in this subreddit");
            }
            var msg = await ReplyAsync(embed: Diaporama.ReactionManager.Post(elems[0], 1, elems.Count));
            await msg.AddReactionsAsync(new[] { new Emoji("⏪"), new Emoji("◀️"), new Emoji("▶️"), new Emoji("⏩") });
            StaticObjects.Diaporamas.Add(msg.Id, new Diaporama.Diaporama(elems.ToArray()));
        }

        private Diaporama.Reddit GetRedditEmbed(JToken elem) // Parse a JSON and get a Reddit object out of it
        {
            var isSafe = !Utils.CanSendNsfw(Context.Channel);
            var elemNsfw = elem["over_18"].Value<bool>();
            if (isSafe && elemNsfw) // Result is NSFW and we aren't in a safe channel
                return null;
            string preview = elem["url"].Value<string>();
            if (!Utils.IsImage(preview.Split('.').Last()))
                preview = elem["thumbnail"].Value<string>();
            if (preview == "spoiler") // We don't want to display spoilers
                return null;
            return new Diaporama.Reddit(elem["title"].Value<string>(), preview == "self" ? null : new Uri(preview), new Uri("https://reddit.com" + elem["permalink"].Value<string>()),
                elem["ups"].Value<int>(), elem["link_flair_text"].Value<string>(), elemNsfw, elem["selftext"].Value<string>());
        }

        [Command("Youtube", RunMode = RunMode.Async)]
        public async Task YoutubeAsync([Remainder]string search)
        {
            await ReplyAsync(embed: GetEmbedFromVideo(await GetYoutubeVideoAsync(search), out _).Build());
        }

        public static EmbedBuilder GetEmbedFromVideo(Video video, out string duration)
        {
            var description = video.Snippet.Description.Split('\n');
            // We make likes/dislikes easier to read: 4000 -> 4k
            var finalViews = Utils.MakeNumberReadable(video.Statistics.ViewCount.ToString());
            var finalLikes = Utils.MakeNumberReadable(video.Statistics.LikeCount.ToString());
            var finalDislikes = Utils.MakeNumberReadable(video.Statistics.DislikeCount.ToString());
            var match = Regex.Match(video.ContentDetails.Duration, "PT([0-9]+)M([0-9]+)S");
            duration = match.Groups[1] + ":" + match.Groups[2];
            return new EmbedBuilder
            {
                ImageUrl = video.Snippet.Thumbnails.High.Url,
                Title = video.Snippet.Title,
                Url = "https://youtu.be/" + video.Id,
                Description = description.Length > Constants.YOUTUBE_DESC_MAX_SIZE ? (string.Join("\n", video.Snippet.Description.Split('\n').Take(Constants.YOUTUBE_DESC_MAX_SIZE)) + "\n[...]") : video.Snippet.Description,
                Color = Color.Blue,
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Duration: {duration}\nViews: {finalViews}\nLikes: {finalLikes}\n" +
                    $"Dislikes: {finalDislikes}\nRatio: {((float)video.Statistics.LikeCount / video.Statistics.DislikeCount).Value:0.0}"
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
                VideosResource.ListRequest r = StaticObjects.YouTube.Videos.List("snippet,statistics,contentDetails");
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
                VideosResource.ListRequest videoRequest = StaticObjects.YouTube.Videos.List("snippet,statistics,contentDetails");
                videoRequest.Id = string.Join(",", correctVideos.Select(x => x.Id.VideoId));
                var videoResponse = (await videoRequest.ExecuteAsync()).Items;
                ulong likes = ulong.MinValue;
                // Sometimes the first result isn't the one we want, so compare the differents results and take the one with the best like/dislike ratio
                bool isExactSearch = false;
                var lowerSearch = search.ToLowerInvariant();
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
                        var lowerTitle = res.Snippet.Title.ToLowerInvariant();
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
        public async Task XkcdAsync(uint? nb = null)
        {
            JToken json;
            int max;
            json = JsonConvert.DeserializeObject<JToken>(await StaticObjects.HttpClient.GetStringAsync("https://xkcd.com/info.0.json"));
            max = json["num"].Value<int>();
            if (nb.HasValue && nb > max)
                throw new CommandFailed($"The latest comic available is the number {nb}.");
            nb ??= (uint)StaticObjects.Random.Next(max) + 1;
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
        public async Task XkcdLastAsync()
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
