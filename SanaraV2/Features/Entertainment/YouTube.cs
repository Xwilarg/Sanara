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
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Features.Entertainment
{
    public static class YouTube
    {
        public static async Task<FeatureRequest<Response.YouTube, Error.YouTube>> SearchYouTube(string[] args, YouTubeService service)
        {
            if (service == null)
                return new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.InvalidApiKey);
            if (args.Length == 0)
                return new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.Help);
            string id = null;
            Match match = Regex.Match(args[0], "https:\\/\\/www.youtube.com\\/watch\\?v=([^&]+)");
            if (match.Success)
                id = match.Groups[1].Value;
            match = Regex.Match(args[0], "https:\\/\\/youtu.be\\/([^&]+)");
            if (match.Success)
                id = match.Groups[1].Value;
            else if (Regex.Match(args[0], "^[0-9a-zA-Z_-]{11}$").Success)
                id = args[0];
            if (id != null)
            {
                VideosResource.ListRequest r = service.Videos.List("snippet");
                r.Id = id;
                var resp = (await r.ExecuteAsync()).Items;
                if (resp.Count() == 0)
                    return (new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.NotFound));
                return new FeatureRequest<Response.YouTube, Error.YouTube>(new Response.YouTube()
                {
                    url = "https://www.youtube.com/watch?v=" + resp[0].Id,
                    name = resp[0].Snippet.Title,
                    imageUrl = resp[0].Snippet.Thumbnails.High.Url
                }, Error.YouTube.None);
            }
            SearchResource.ListRequest listRequest = service.Search.List("snippet");
            listRequest.Q = Utilities.AddArgs(args);
            listRequest.MaxResults = 5;
            IList<SearchResult> searchListResponse = (await listRequest.ExecuteAsync()).Items;
            if (searchListResponse.Count == 0)
                return new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.NotFound);
            IEnumerable<SearchResult> correctVideos = searchListResponse.Where(x => x.Id.Kind == "youtube#video");
            if (correctVideos.Count() == 0)
                return new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.NotFound);
            VideosResource.ListRequest videoRequest = service.Videos.List("snippet,statistics");
            videoRequest.Id = string.Join(",", correctVideos.Select(x => x.Id.VideoId));
            IList<Video> videoResponse = (await videoRequest.ExecuteAsync()).Items;
            Video biggest = null;
            ulong likes = ulong.MinValue;
            foreach (Video res in videoResponse)
            {
                ulong likeCount = ulong.MinValue;
                if (res.Statistics.LikeCount != null)
                    likeCount = res.Statistics.LikeCount.Value;
                if (res.Statistics.DislikeCount != null)
                    likeCount -= res.Statistics.DislikeCount.Value;
                if (likeCount > likes || biggest == null)
                {
                    likes = likeCount;
                    biggest = res;
                }
            }
            return new FeatureRequest<Response.YouTube, Error.YouTube>(new Response.YouTube()
            {
                url = "https://www.youtube.com/watch?v=" + biggest.Id,
                name = biggest.Snippet.Title,
                imageUrl = biggest.Snippet.Thumbnails.High.Url
            }, Error.YouTube.None);
        }
    }
}
