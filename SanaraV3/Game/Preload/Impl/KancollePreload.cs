using Discord;
using DiscordUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public sealed class KancollePreload : IPreload
    {
        public KancollePreload()
        {
            var cache = StaticObjects.Db.GetCacheAsync(GetGameNames()[0]).GetAwaiter().GetResult().ToList();
            foreach (string name in Kancolle.GetShips())
            {
                if (!cache.Any(x => x.id == name))
                {
                    try
                    {
                        // Get URL
                        string shipUrl = "https://kancolle.fandom.com/wiki/" + name + "/Gallery";
                        string html = StaticObjects.HttpClient.GetStringAsync(shipUrl).GetAwaiter().GetResult();

                        // TODO: There are some issues for ships like Imuya that are called I-168 by the wikia (even if it's her "real" name we need to accept both)
                        var result = new QuizzPreloadResult(Regex.Match(html, "https:\\/\\/vignette\\.wikia\\.nocookie\\.net\\/kancolle\\/images\\/[0-9a-z]+\\/[0-9a-z]+\\/" + name + "_Full\\.png").Value, new[] { name });
                        StaticObjects.Db.SetCacheAsync(GetGameNames()[0], result).GetAwaiter().GetResult();
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
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
