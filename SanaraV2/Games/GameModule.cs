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
using SanaraV2.Modules.Base;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public class GameModule : ModuleBase
    {
        [Command("Play")]
        public async Task Play(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Game);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            ITextChannel chan = (ITextChannel)Context.Channel;
            await Program.p.gm.Play(args, chan);
        }

        [Command("Cancel")]
        public async Task Cancel(params string[] _)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Game);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            if (Program.p.gm.Cancel(Context.Channel.Id))
                await ReplyAsync(Sentences.ResetDone(Context.Guild.Id));
            else
                await ReplyAsync(Sentences.ResetNone(Context.Guild.Id));
        }

        [Command("Score")]
        public async Task Score(params string[] _)
        {
            var scores = await Program.p.db.GetAllScores();
            if (!scores.Any(x => x.Key == Context.Guild.Id.ToString()))
            {
                await ReplyAsync(Sentences.NoScore(Context.Guild.Id));
                return;
            }
            var me = scores[Context.Guild.Id.ToString()];
            StringBuilder finalStr = new StringBuilder();
            foreach (var game in Constants.allGames)
            {
                APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                string gameName = preload.GetGameName();
                if (!me.ContainsKey(preload.GetGameName()))
                    continue;
                string[] myElems = me[gameName].Split('|');
                var users = await Context.Guild.GetUsersAsync();
                int myScore = int.Parse(myElems[0]);
                string[] contributors = myElems.Skip(1).Select(x => users.Where(y => y.Id.ToString() == x).ElementAt(0).ToString()).ToArray();
                int rankedNumber = scores.Where(x => Program.p.client.GetGuild(ulong.Parse(x.Key)) != null && x.Value.ContainsKey(gameName)).Count();
                int myRanking = scores.Where(x => Program.p.client.GetGuild(ulong.Parse(x.Key)) != null && x.Value.ContainsKey(gameName) && int.Parse(x.Value[gameName].Split('|')[0]) > myScore).Count() + 1;
                finalStr.Append("**" + preload.GetGameSentence(Context.Guild.Id) + "**:" + Environment.NewLine +
                    Sentences.ScoreText(Context.Guild.Id, myRanking, rankedNumber, myScore) + Environment.NewLine +
                    Sentences.ScoreContributors(Context.Guild.Id) + " " + string.Join(", ", contributors) + Environment.NewLine + Environment.NewLine);
            }
            await ReplyAsync(finalStr.ToString());
        }
    }
}
