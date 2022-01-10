using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Web;

namespace Sanara.Game.Preload.Impl
{
    public sealed class FateGOPreload : IPreload
    {
        public void Init()
        {
            var cache = StaticObjects.Db.GetCacheAsync(Name).GetAwaiter().GetResult().ToList();
            foreach (var tmp in FateGO.GetCharacters())
            {
                string elem = tmp;
                elem = HttpUtility.UrlDecode(elem).Replace("&amp;", "&").Replace("&#39;", "'");
                if (!cache.Any(x => x.id == elem))
                {
                    try
                    {
                        string html = StaticObjects.HttpClient.GetStringAsync("https://fategrandorder.fandom.com/wiki/" + elem).GetAwaiter().GetResult();

                        List<string> allAnswer = new();
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
                        StaticObjects.Db.SetCacheAsync(Name, result).GetAwaiter().GetResult();
                        cache.Add(result);
                    }
                    catch (System.Exception e)
                    {
                        _ = Log.LogErrorAsync(new System.Exception($"Error while preloading {elem}:\n" + e.Message, e), null);
                    }
                    Thread.Sleep(250); // We wait a bit to not spam the HTTP requests
                }
            }
            _preload = cache.ToArray();
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string Name => "FateGO Quizz";
        public string Description => "Find the name of a FateGO character from an image";

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a servant, you'll have to give his/her name.";

        public bool IsSafe()
            => true;

        private QuizzPreloadResult[] _preload;
    }
}
