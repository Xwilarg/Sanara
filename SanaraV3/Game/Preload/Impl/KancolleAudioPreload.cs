using Discord;
using DiscordUtils;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl.Static;
using SanaraV3.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class KancolleAudioPreload : IPreload
    {
        public KancolleAudioPreload()
        {
            var cache = StaticObjects.Db.GetCacheAsync(GetGameNames()[0] + "_" + GetNameArg()).GetAwaiter().GetResult().ToList();
            foreach (string name in Kancolle.GetShips())
            {
                if (!cache.Any(x => x.id == name))
                {
                    try
                    {
                        // Get URL
                        string shipUrl = "https://kancolle.fandom.com/wiki/" + name;
                        string html = StaticObjects.HttpClient.GetStringAsync(shipUrl).GetAwaiter().GetResult();

                        var result = new QuizzPreloadResult(Regex.Match(html, "https:\\/\\/vignette\\.wikia\\.nocookie\\.net\\/kancolle\\/images\\/[0-9a-z]+\\/[0-9a-z]+\\/[^-]*-Battle_Start\\.ogg").Value, new[] { name });
                        StaticObjects.Db.SetCacheAsync(GetGameNames()[0] + "_" + GetNameArg(), result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, $"Error while preloading {name}:\n" + e.Message, e));
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "kancolle", "kc", "kantaicollection" };

        public string GetNameArg()
            => "audio";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new QuizzAudio(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
