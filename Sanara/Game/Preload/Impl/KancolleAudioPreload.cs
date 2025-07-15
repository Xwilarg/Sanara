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
    public sealed class KancolleAudioPreload : IPreload
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
                        string shipUrl = "https://kancolle.fandom.com/wiki/" + name;
                        string html = client.GetStringAsync(shipUrl).GetAwaiter().GetResult();

                        var result = new QuizzPreloadResult(Regex.Match(html, "https:\\/\\/vignette\\.wikia\\.nocookie\\.net\\/kancolle\\/images\\/[0-9a-z]+\\/[0-9a-z]+\\/[^-]*-Battle_Start\\.ogg").Value, new[] { name });
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

        public string Name => "KanColle Audio Quizz";

        public AGame CreateGame(CommonMessageChannel chan, CommonUser user, GameSettings settings)
            => new QuizzAudio(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll play a vocal line of a shipgirl, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
