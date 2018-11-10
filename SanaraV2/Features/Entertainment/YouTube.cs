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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Features.Entertainment
{
    public static class YouTube
    {
        public static async Task<FeatureRequest<Response.YouTube, Error.YouTube>> SearchYouTube(string[] args, YouTubeService service)
        {
            if (service == null)
                return (new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.InvalidApiKey));
            if (args.Length == 0)
                return (new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.Help));
            SearchResource.ListRequest listRequest = service.Search.List("snippet");
            listRequest.Q = Utilities.AddArgs(args);
            listRequest.MaxResults = 10;
            IList<SearchResult> searchListResponse = (await listRequest.ExecuteAsync()).Items;
            if (searchListResponse.Count == 0)
                return (new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.NotFound));
            SearchResult biggest = null;
            DateTime publishTime = DateTime.MaxValue;
            List<string> argsList = args.ToList();
            foreach (SearchResult res in searchListResponse)
            {
                string cleanTitle = Utilities.CleanWord(res.Snippet.Title);
                if (res.Id.Kind == "youtube#video"
                    && ((res.Snippet.PublishedAt.HasValue && res.Snippet.PublishedAt < publishTime) || biggest == null))
                {
                    publishTime = res.Snippet.PublishedAt.Value;
                    biggest = res;
                }
            }
            if (biggest == null)
                return (new FeatureRequest<Response.YouTube, Error.YouTube>(null, Error.YouTube.NotFound));
            return (new FeatureRequest<Response.YouTube, Error.YouTube>(new Response.YouTube()
            {
                url = "https://www.youtube.com/watch?v=" + biggest.Id.VideoId,
                name = biggest.Snippet.Title,
                imageUrl = biggest.Snippet.Thumbnails.High.Url
            }, Error.YouTube.None));
        }
    }
}
