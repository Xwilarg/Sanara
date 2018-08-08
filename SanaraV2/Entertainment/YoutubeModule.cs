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
using SanaraV2.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Entertainment
{
    public class YoutubeModule : ModuleBase
    {
        Program p = Program.p;
        [Command("Youtube"), Summary("Get a random video given a playlist")]
        public async Task YoutubeVideo(params string[] words)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Youtube);
            if (p.youtubeService == null)
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
            else
            {
                Tuple<string, string> url = (await GetYoutubeMostPopular(words, Context.Channel));
                if (url != null)
                    await ReplyAsync(url.Item1);
            }
        }

        private static async Task<IList<Google.Apis.YouTube.v3.Data.SearchResult> > GetVideos(string[] words, IMessageChannel chan, int maxResult)
        {
            if (words.Length == 0)
            {
                await chan.SendMessageAsync(Sentences.YoutubeHelp((chan as ITextChannel).GuildId));
                return (null);
            }
            var searchListRequest = Program.p.youtubeService.Search.List("snippet");
            searchListRequest.Q = Utilities.AddArgs(words);
            searchListRequest.MaxResults = maxResult;
            var searchListResponse = await searchListRequest.ExecuteAsync();
            if (searchListResponse.Items.Count < maxResult)
            {
                await chan.SendMessageAsync(Sentences.YoutubeNotFound((chan as ITextChannel).GuildId));
                return (null);
            }
            return (searchListResponse.Items);
        }

        public static async Task<Tuple<string, string>> GetYoutubeMostPopular(string[] words, IMessageChannel chan, int maxResult = 10)
        {
            var results = await GetVideos(words, chan, maxResult);
            Google.Apis.YouTube.v3.Data.SearchResult biggest = null;
            DateTime publishTime = DateTime.MaxValue;
            foreach (var res in results)
            {
                string cleanTitle = Utilities.CleanWord(res.Snippet.Title);
                if (res.Id.Kind == "youtube#video" && words.ToList().All(x => cleanTitle.Contains(Utilities.CleanWord(x))))
                {
                    if (res.Snippet.PublishedAt.HasValue && res.Snippet.PublishedAt < publishTime)
                    {
                        publishTime = res.Snippet.PublishedAt.Value;
                        biggest = res;
                    }
                }
            }
            if (biggest == null)
            {
                foreach (var res in results)
                {
                    if (res.Id.Kind == "youtube#video")
                    {
                        if (res.Snippet.PublishedAt.HasValue && res.Snippet.PublishedAt < publishTime)
                        {
                            publishTime = res.Snippet.PublishedAt.Value;
                            biggest = res;
                        }
                    }
                }
            }
            return new Tuple<string, string>("https://www.youtube.com/watch?v=" + biggest.Id.VideoId, biggest.Snippet.Title);
        }
    }
}