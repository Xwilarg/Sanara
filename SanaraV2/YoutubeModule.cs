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
using Discord.Commands;
using System;
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
            Tuple<string, string> url = (await GetYoutubeVideo(words, Context.Channel));
            if (url != null)
                await ReplyAsync(url.Item1);
        }

        public static async Task<Tuple<string, string> > GetYoutubeVideo(string[] words, IMessageChannel chan, int maxResult = 1)
        {
            if (words.Length == 0)
            {
                await chan.SendMessageAsync(Sentences.youtubeHelp);
                return (null);
            }
            var searchListRequest = Program.p.youtubeService.Search.List("snippet");
            searchListRequest.Q = Program.addArgs(words);
            searchListRequest.MaxResults = maxResult;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            if (searchListResponse.Items.Count < maxResult)
            {
                await chan.SendMessageAsync(Sentences.youtubeNotFound);
                return (null);
            }
            Google.Apis.YouTube.v3.Data.SearchResult sr = searchListResponse.Items[maxResult - 1];
            if (sr.Id.Kind != "youtube#video")
                return (await GetYoutubeVideo(words, chan, maxResult + 1));
            else
                return new Tuple<string, string>("https://www.youtube.com/watch?v=" + sr.Id.VideoId, sr.Snippet.Title);
        }
    }
}