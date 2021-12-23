using Discord;
using Sanara.Game.Impl;
using Sanara.Game.Preload.Impl.Static;
using Sanara.Game.Preload.Result;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl
{
    public sealed class PokemonPreload : IPreload
    {
        public PokemonPreload()
        {
            var cache = StaticObjects.Db.GetCacheAsync(GetGameNames()[0]).GetAwaiter().GetResult().ToList();
            foreach (var elem in Pokemon.GetPokemons())
            {
                if (!cache.Any(x => x.id == elem))
                {
                    try
                    {
                        string html;
                        html = StaticObjects.HttpClient.GetStringAsync("https://pokemondb.net/pokedex/" + elem).GetAwaiter().GetResult();
                        string french = Regex.Match(html, "<th>French<\\/th>[^<]*<td>([^<]+)<\\/td>").Groups[1].Value;
                        string japanese = Regex.Match(html, "<th>Japanese<\\/th>[^<]*<td>[^\\(]+\\(([^\\)]+)\\)<\\/td>").Groups[1].Value;
                        string german = Regex.Match(html, "<th>German<\\/th>[^<]*<td>([^<]+)<\\/td>").Groups[1].Value;

                        var result = new QuizzPreloadResult("https://img.pokemondb.net/artwork/" + elem + ".jpg", new[] { elem, french, japanese, german });
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
            => new[] { "pokemon" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Quizz(chan, user, this, settings);

        public string GetRules()
            => "I'll post an image of a pokemon, you'll have to give its name.";

        public bool IsSafe()
            => true;

        private readonly QuizzPreloadResult[] _preload;
    }
}
