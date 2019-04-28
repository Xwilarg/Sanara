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
            List<Tuple<int, string>[]> ranking = new List<Tuple<int, string>[]>();
            for (int i = 0; i < 3; i++) // The 3 best scores...
            {
                Dictionary<string, Tuple<int, string>> biggests = new Dictionary<string, Tuple<int, string>>();
                foreach (var elem in scores) // ...in all scores saved...
                {
                    foreach (var game in Constants.allGames) // ...for all games...
                    {
                        APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                        string gameName = preload.GetGameName();
                        if (elem.Value.ContainsKey(gameName))
                        {
                            string[] content = elem.Value[gameName].Split('|');
                            IGuild guild = Program.p.client.GetGuild(ulong.Parse(elem.Key));
                            if (guild == null || ranking.Any(x => x[0] != null && x[0].Item2 == Program.p.GetName(guild.Name))) { }
                            else
                            {
                                int score = int.Parse(content[0]);
                                if (!biggests.ContainsKey(gameName))
                                    biggests.Add(gameName, new Tuple<int, string>(score, Program.p.GetName(guild.Name)));
                                else if (score > biggests[gameName].Item1)
                                    biggests[gameName] = new Tuple<int, string>(score, Program.p.GetName(guild.Name));
                            }
                        }
                    }
                }
                ranking.Add(biggests.Select(x => x.Value).ToArray());
            }
            return (string.Join("$", ranking.Select(x => string.Join("|", x.Select(y => y?.Item2 + "|" + y?.Item1)))));
        }

        public static string GetInformation(ulong guildId, ref int yes, ref int no)
        {
            StringBuilder finalStr = new StringBuilder();
            foreach (var game in Constants.allGames)
            {
                APreload preload = (APreload)Activator.CreateInstance(game.Item1);
                finalStr.Append("**" + preload.GetGameSentence(guildId) + ":** ");
                if (game.Item3 == null)
                {
                    finalStr.Append(Sentences.NotLoaded(guildId));
                    no++;
                }
                else
                {
                    finalStr.Append(game.Item3.Count + " " + Sentences.Words(guildId));
                    yes++;
                }
                finalStr.Append(Environment.NewLine);
            }
            return finalStr.ToString();
        }
    }
}
