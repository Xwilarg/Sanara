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

using Discord.Commands;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2
{
    public class YoutubeModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Youtube"), Summary("Get a random video given a playlist")]
        public async Task youtubeVideo(params string[] words)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Youtube);
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = File.ReadAllText("Keys/YoutubeAPIKey.dat")
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = Program.addArgs(words);
            searchListRequest.MaxResults = 1;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            if (searchListResponse.Items.Count == 0)
            {
                await ReplyAsync(Sentences.youtubeNotFound);
                return;
            }
            Google.Apis.YouTube.v3.Data.SearchResult sr;
            /// TODO: Search for second video if first isn't one
            if (!searchListResponse.Items.Any(x => x.Id.Kind == "youtube#video"))
                await ReplyAsync(Sentences.youtubeBadVideo);
            else
            {
                do
                {
                    sr = searchListResponse.Items[p.rand.Next(0, searchListResponse.Items.Count)];
                } while (sr.Id.Kind != "youtube#video");
                await ReplyAsync("https://www.youtube.com/watch?v=" + sr.Id.VideoId);
            }
        }
    }
}