using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl.Static;
using SanaraV3.Game.Preload.Result;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class AzurLanePreload : IPreload
    {
        public AzurLanePreload()
        {
            var cache = StaticObjects.Db.GetCacheAsync(GetGameNames()[0]).GetAwaiter().GetResult().ToList();
            foreach (var elem in AzurLane.GetShips())
            {
                if (!cache.Any(x => x.id == elem.Item1))
                {
                    try
                    {
                        // Item1: href
                        // Item2: name

                        // Get URL
                        var htmlValue = Regex.Match(StaticObjects.HttpClient.GetStringAsync("https://azurlane.koumakan.jp/" + elem.Item1).GetAwaiter().GetResult(), "src=\"(\\/w\\/images\\/thumb\\/[^\\/]+\\/[^\\/]+\\/[^\\/]+\\/[0-9]+px-" + elem.Item1 + ".png)").Groups[1].Value;

                        // Names
                        List<string> names = new List<string> { elem.Item2 };
                        if (elem.Item2 == "HMS_Neptune" || elem.Item2 == "HDN_Neptune")
                            names.Add("Neptune"); // Both ship are named "Neptune" ingame
                        var escapeName = elem.Item2.Replace("µ", "mu").Replace('ö', 'o').Replace('Ö', 'O').Replace('é', 'e').Replace('É', 'E').Replace('â', 'a').Replace('Â', 'A').Replace('è', 'e');
                        if (escapeName != elem.Item2)
                            names.Add(escapeName);

                        var result = new QuizzPreloadResult("https://azurlane.koumakan.jp" + htmlValue, names.ToArray());
                        StaticObjects.Db.SetCacheAsync(GetGameNames()[0], result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, $"Error while preloading {elem.Item1}:\n" + e.Message, e));
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "azurlane", "al" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
