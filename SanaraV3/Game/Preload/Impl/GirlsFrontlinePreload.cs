using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl.Static;
using SanaraV3.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class GirlsFrontlinePreload : IPreload
    {
        public GirlsFrontlinePreload()
        {
            var cache = StaticObjects.Db.GetCacheAsync(GetGameNames()[0]).GetAwaiter().GetResult().ToList();
            // Item1 is name to be used in URL
            // Item2 is answer name
            foreach (var tDoll in GirlsFrontline.GetTDolls())
            {
                if (!cache.Any(x => x.id == tDoll.Item2))
                {
                    try
                    {
                        // Get URL
                        string shipUrl = "http://iopwiki.com/wiki/File:" + tDoll.Item1 + ".png";
                        string html = StaticObjects.HttpClient.GetStringAsync(shipUrl).GetAwaiter().GetResult();
                        Match m = Regex.Match(html, "src=\"(\\/images\\/thumb\\/[^\"]+)\"");

                        var result = new QuizzPreloadResult("http://iopwiki.com" + m.Groups[1].Value, new[] { tDoll.Item2 }); // Not sure if the Replace is necessary but it was here in the V2
                        StaticObjects.Db.SetCacheAsync(GetGameNames()[0], result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, $"Error while preloading {tDoll.Item1}:\n" + e.Message, e));
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "girlsfrontline", "gf" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a t-doll, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private readonly QuizzPreloadResult[] _preload;
    }
}
