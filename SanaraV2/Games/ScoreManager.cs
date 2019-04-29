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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SanaraV2.Games
{
    public static class ScoreManager
    {
        public static async Task<string> GetBestScores()
        {
            var scores = await Program.p.db.GetAllScores();
            StringBuilder finalStr = new StringBuilder();
            // Output format: Game1Place1Server | Game1Place1Score | Game1Place2Server..... Game1Place3Score $ Game2Place1Server | Game2Place1Score
            foreach (var game in Constants.allGames)
            {
                APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                string gameName = preload.GetGameName();

                // Prepare array containing all games serverId/score
                List<Tuple<string, string>> allScores = new List<Tuple<string, string>>();
                foreach (var elem in scores)
                {
                    if (elem.Value.ContainsKey(gameName))
                        allScores.Add(new Tuple<string, string>(elem.Key, elem.Value[gameName].Split('|')[0]));
                }
                List<Tuple<string, int>> best = new List<Tuple<string, int>>();
                string bestServer;
                int bestScore;
                List<string> alreadySaid = new List<string>();
                while (best.Count < 3 && allScores.Count > alreadySaid.Count)
                {
                    bestServer = null;
                    bestScore = -1;
                    foreach (var elem in allScores)
                    {
                        int score = int.Parse(elem.Item2);
                        if (!alreadySaid.Contains(elem.Item1) && (bestServer == null || score > bestScore))
                        {
                            bestServer = elem.Item1;
                            bestScore = score;
                        }
                    }
                    alreadySaid.Add(bestServer);
                    IGuild guild = Program.p.client.GetGuild(ulong.Parse(bestServer));
                    if (guild != null)
                        best.Add(new Tuple<string, int>(Program.p.GetName(Program.p.client.GetGuild(ulong.Parse(bestServer)).Name), bestScore));
                }
                finalStr.Append(string.Join("|", best.Select(x => x.Item1 + "|" + x.Item2)) + "$");
            }
            return (finalStr.ToString());
        }

        public static string GetInformation(ulong guildId, ref int yes, ref int no)
        {
            StringBuilder finalStr = new StringBuilder();
            foreach (var game in Constants.allDictionnaries)
            {
                finalStr.Append("**" + game.Item1(guildId) + ":** ");
                if (game.Item2 == null)
                {
                    finalStr.Append(Sentences.NotLoaded(guildId));
                    no++;
                }
                else
                {
                    finalStr.Append(game.Item2.Count + " " + Sentences.Words(guildId));
                    yes++;
                }
                finalStr.Append(Environment.NewLine);
            }
            return finalStr.ToString();
        }
    }
}
