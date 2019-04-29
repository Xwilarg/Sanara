﻿/// This file is part of Sanara.
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
                if (scores.ContainsKey(gameName))
                {
                    List<Tuple<string, int>> best = new List<Tuple<string, int>>();
                    var allScores = scores[gameName];
                    string bestServer;
                    int bestScore;
                    while (best.Count < 3 && allScores.Count > 0)
                    {
                        bestServer = null;
                        bestScore = -1;
                        foreach (var elem in allScores)
                        {
                            int score = int.Parse(elem.Value);
                            if (bestServer == null || score > bestScore)
                            {
                                bestServer = elem.Key;
                                bestScore = score;
                            }
                        }
                        allScores.Remove(bestServer);
                        best.Add(new Tuple<string, int>(Program.p.GetName(bestServer), bestScore));
                    }
                    finalStr.Append(string.Join("|", best.Select(x => x.Item1 + "|" + x.Item2)));
                }
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