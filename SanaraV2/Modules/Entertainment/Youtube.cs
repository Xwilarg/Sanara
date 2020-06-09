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
using System;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Entertainment
{
    public class Youtube : ModuleBase
    {
        Program p = Program.p;
        [Command("Youtube"), Summary("Get a random video given some keywords")]
        public async Task YoutubeVideo(params string[] args)
        {
            Base.Utilities.CheckAvailability(Context.Guild, Program.Module.Youtube);
            await p.DoAction(Context.User, Program.Module.Youtube);
            var result = await Features.Entertainment.YouTube.SearchYouTube(args, Program.p.youtubeService);
            switch (result.error)
            {
                case Features.Entertainment.Error.YouTube.InvalidApiKey:
                    await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild));
                    break;

                case Features.Entertainment.Error.YouTube.Help:
                    await ReplyAsync(Sentences.YoutubeHelp(Context.Guild));
                    break;

                case Features.Entertainment.Error.YouTube.NotFound:
                    await ReplyAsync(Sentences.YoutubeNotFound(Context.Guild));
                    break;

                case Features.Entertainment.Error.YouTube.None:
                    await ReplyAsync(result.answer.url);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
    }
}