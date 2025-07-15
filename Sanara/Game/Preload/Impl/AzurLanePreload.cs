﻿using Discord;
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
    public sealed class AzurLanePreload : IPreload
    {
        public void Init(IServiceProvider provider)
        {
            _provider = provider;
            var db = provider.GetRequiredService<Db>();
            var client = provider.GetRequiredService<HttpClient>();

            var cache = db.GetCacheAsync(Name).GetAwaiter().GetResult().ToList();
            foreach (var elem in AzurLane.GetShips())
            {
                if (!cache.Any(x => x.id == elem.Item2))
                {
                    try
                    {
                        // Item1: href
                        // Item2: name

                        // Get URL
                        var regexData = elem.Item1.Replace("(", "%28").Replace(")", "%29");
                        var htmlValue = Regex.Match(client.GetStringAsync("https://azurlane.koumakan.jp/wiki/" + elem.Item1).GetAwaiter().GetResult(), "class=\"mw-file-description\"><img src=\"([^\"]+)\"").Groups[1].Value;
                        if (string.IsNullOrWhiteSpace(htmlValue))
                        {
                            throw new NullReferenceException("No image found in page");
                        }
                        // Names
                        List<string> names = new() { elem.Item2 };
                        if (elem.Item2 == "HMS_Neptune" || elem.Item2 == "HDN_Neptune")
                            names.Add("Neptune"); // Both ship are named "Neptune" ingame
                        var escapeName = Common.RemoveAccents(elem.Item2);
                        if (escapeName != elem.Item2)
                            names.Add(escapeName);

                        var result = new QuizzPreloadResult(htmlValue, names.ToArray());
                        db.SetCacheAsync(Name, result).GetAwaiter().GetResult();
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

        public string Name => "Azur Lane Quizz";

        public AGame CreateGame(IMessageChannel chan, CommonUser user, GameSettings settings)
            => new Quizz(_provider, chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a shipgirl, you'll have to give her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
        private IServiceProvider _provider;
    }
}
