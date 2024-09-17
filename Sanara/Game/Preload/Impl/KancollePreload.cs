using Discord;
using Microsoft.Extensions.DependencyInjection;
using Sanara.Database;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl
{
    public sealed class KancollePreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _provider = provider;
            var db = provider.GetRequiredService<Db>();
            var client = provider.GetRequiredService<HttpClient>();

            var cache = db.GetCacheAsync(Name).GetAwaiter().GetResult().ToList();
            foreach (string name in Kancolle.GetShips())
            {
                if (!cache.Any(x => x.id == name))
                {
                    try
                    {
                        // Get URL
                        string shipUrl = "https://kancolle.fandom.com/wiki/" + name + "/Gallery";
                        string html = client.GetStringAsync(shipUrl).GetAwaiter().GetResult();

                        // TODO: There are some issues for ships like Imuya that are called I-168 by the wikia (even if it's her "real" name we need to accept both)
                        var result = new QuizzPreloadResult(Regex.Match(html, "https:\\/\\/[^\\/]+\\/kancolle\\/images\\/[0-9a-z]+\\/[0-9a-z]+\\/" + name + "_Full\\.png").Value, new[] { name });
                        db.SetCacheAsync(Name, result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.LogErrorAsync(new System.Exception($"Error while preloading {name}:\n" + e.Message, e), null);
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "KanColle Quizz";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a shipgirl, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
