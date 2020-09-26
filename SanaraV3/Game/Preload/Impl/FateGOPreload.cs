using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Impl.Static;
using SanaraV3.Game.Preload.Result;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class FateGOPreload : IPreload
    {
        public FateGOPreload()
        {
            var cache = StaticObjects.Db.GetCacheAsync(GetGameNames()[0]).GetAwaiter().GetResult().ToList();
            foreach (var tmp in FateGO.GetCharacters())
            {
                string elem = tmp;
                if (!cache.Any(x => x.id == elem))
                {
                    try
                    {
                        elem = HttpUtility.UrlDecode(elem).Replace("&amp;", "&").Replace("&#39;", "'");
                        string html = StaticObjects.HttpClient.GetStringAsync("https://fategrandorder.fandom.com/wiki/" + elem).GetAwaiter().GetResult();

                        List<string> allAnswer = new List<string>();
                        allAnswer.Add(elem);
                        string cleanAnswer = Common.RemoveAccents(elem);
                        if (elem != cleanAnswer)
                            allAnswer.Add(cleanAnswer);
                        if (html.Contains("AKA:")) // Alternative answers
                        {
                            foreach (string s in Regex.Replace(html.Split(new[] { "AKA:</b></span>" }, StringSplitOptions.None)[1].Split(new[] { "</td>" }, StringSplitOptions.None)[0], "\\([^\\)]+\\)", "")
                                .Split(new[] { ",", "<br />" }, StringSplitOptions.RemoveEmptyEntries))
                            {
                                string name = s;
                                Match m = Regex.Match(name, "<[^>]+>([^<]+)<\\/[^>]+>");
                                if (m.Success)
                                    name = m.Groups[1].Value;
                                name = Regex.Replace(name, "<[^>]+>", "");
                                name = Regex.Replace(name, "<\\/[^>]+>", "");
                                foreach (string sName in name.Split(','))
                                {
                                    if (!string.IsNullOrWhiteSpace(sName))
                                    {
                                        string akaName = sName.Trim();
                                        allAnswer.Add(akaName);
                                        string cleanAka = Common.RemoveAccents(akaName);
                                        if (akaName != cleanAka)
                                            allAnswer.Add(cleanAka);
                                    }
                                }
                            }
                        }

                        var result = new QuizzPreloadResult(Regex.Match(html.Split(new[] { "pi-image-collection-tab-content current" }, StringSplitOptions.None)[1], "<a href=\"([^\"]+)\"").Groups[1].Value.Split(new string[] { "/revision" }, StringSplitOptions.None)[0],
                            allAnswer.ToArray());
                        StaticObjects.Db.SetCacheAsync(GetGameNames()[0], result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.ErrorAsync(new LogMessage(LogSeverity.Error, e.Source, $"Error while preloading {elem}:\n" + e.Message, e));
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "fatego", "fate", "fgo" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        private readonly QuizzPreloadResult[] _preload;
    }
}
