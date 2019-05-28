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
        [Command("Play", RunMode = RunMode.Async)]
        public async Task Play(params string[] args)
        {
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Game);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            ITextChannel chan = (ITextChannel)Context.Channel;
            var error = await Program.p.gm.Play(args, chan);
            if (error != null)
                await ReplyAsync(error(Context.Guild.Id));
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
            Utilities.CheckAvailability(Context.Guild.Id, Program.Module.Game);
            await Program.p.DoAction(Context.User, Context.Guild.Id, Program.Module.Game);
            var scores = await Program.p.db.GetAllScores();
            if (!scores.Any(x => x.Key == Context.Guild.Id.ToString()))
            {
                await ReplyAsync(Sentences.NoScore(Context.Guild.Id));
                return;
            }
            var me = scores[Context.Guild.Id.ToString()];
            StringBuilder finalStr = new StringBuilder();
            float finalScore = 0;
            bool ranked = false;
            int nbGuilds = scores.Count(x => x.Value.Count > 0);
            foreach (var game in Constants.allGames)
            {
                APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                string gameName = preload.GetGameName();
                if (!me.ContainsKey(preload.GetGameName()))
                {
                    finalStr.Append("**" + preload.GetGameSentence(Context.Guild.Id) + "**:" + Environment.NewLine +
                       Sentences.NotRanked(Context.Guild.Id) + Environment.NewLine + Environment.NewLine);
                    continue;
                }
                ranked = true;
                string[] myElems = me[gameName].Split('|');
                var users = await Context.Guild.GetUsersAsync();
                int myScore = int.Parse(myElems[0]);
                string[] contributors = myElems.Skip(1).Select(x => users.Where(y => y.Id.ToString() == x).FirstOrDefault()?.ToString() ?? "(Unknown)").ToArray();
                int rankedNumber = scores.Where(x => Program.p.client.GetGuild(ulong.Parse(x.Key)) != null && x.Value.ContainsKey(gameName)).Count();
                int myRanking = scores.Where(x => Program.p.client.GetGuild(ulong.Parse(x.Key)) != null && x.Value.ContainsKey(gameName) && int.Parse(x.Value[gameName].Split('|')[0]) > myScore).Count() + 1;
                int bestScore = scores.Where(x => x.Value.ContainsKey(gameName)).Max(x => int.Parse(x.Value[gameName].Split('|')[0]));
                finalStr.Append("**" + preload.GetGameSentence(Context.Guild.Id) + "**:" + Environment.NewLine +
                    Sentences.ScoreText(Context.Guild.Id, myRanking, rankedNumber, myScore, bestScore) + Environment.NewLine +
                    Sentences.ScoreContributors(Context.Guild.Id) + " " + string.Join(", ", contributors) + Environment.NewLine + Environment.NewLine);
                finalScore += myScore * 100 / bestScore;
            }
            int myGlobalRanking = 1;
            if (ranked)
            {
                foreach (var s in scores)
                {
                    int sScore = 0;
                    foreach (var elem in s.Value)
                    {
                        int best = scores.Where(x => x.Value.ContainsKey(elem.Key)).Max(x => int.Parse(x.Value[elem.Key].Split('|')[0]));
                        sScore += int.Parse(elem.Value.Split('|')[0]) * 100 / best;
                    }
                    if (sScore > finalScore)
                        myGlobalRanking++;
                }
            }
            await ReplyAsync((ranked ? Sentences.GlobalRanking(Context.Guild.Id, myGlobalRanking, nbGuilds, finalScore / Constants.allGames.Length)
                : Sentences.NoGlobalRanking(Context.Guild.Id))+ Environment.NewLine + Environment.NewLine +
                finalStr.ToString());
        }
    }
}
