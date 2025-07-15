using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Compatibility;
using Sanara.Database;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl
{
    public sealed class GirlsFrontlinePreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _provider = provider;
            var db = provider.GetRequiredService<Db>();
            var client = provider.GetRequiredService<HttpClient>();

            var cache = db.GetCacheAsync(Name).GetAwaiter().GetResult().ToList();
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
                        string html = client.GetStringAsync(shipUrl).GetAwaiter().GetResult();
                        Match m = Regex.Match(html, "src=\"(\\/images\\/thumb\\/[^\"]+)\"");

                        var result = new QuizzPreloadResult("http://iopwiki.com" + m.Groups[1].Value, new[] { tDoll.Item2 }); // Not sure if the Replace is necessary but it was here in the V2
                        db.SetCacheAsync(Name, result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.LogErrorAsync(new System.Exception($"Error while preloading {tDoll.Item1}:\n" + e.Message, e), null);
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "Girls Frontline Quizz";

        public AGame CreateGame(CommonMessageChannel chan, CommonUser user, GameSettings settings)
            => new Quizz(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a t-doll, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
