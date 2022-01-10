using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl
{
    public sealed class AzurLanePreload : IPreload
    {
        public void Init()
        {
            var cache = StaticObjects.Db.GetCacheAsync(Name).GetAwaiter().GetResult().ToList();
            foreach (var elem in AzurLane.GetShips())
            {
                if (!cache.Any(x => x.id == elem.Item2))
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
                        var escapeName = Common.RemoveAccents(elem.Item2);
                        if (escapeName != elem.Item2)
                            names.Add(escapeName);

                        var result = new QuizzPreloadResult("https://azurlane.koumakan.jp" + htmlValue, names.ToArray());
                        StaticObjects.Db.SetCacheAsync(Name, result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.LogErrorAsync(new System.Exception($"Error while preloading {elem.Item1}:\n" + e.Message, e), null);
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Azur Lane Audio Quizz";
        public string Description => "Find the name of an Azur Lane character from an image";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a shipgirl, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
    }
}
