using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SanaraV3.Game.Preload.Impl.Static
{
    public static class Pokemon
    {
        static Pokemon()
        {
            _pokemons = new List<string>();
            string html = StaticObjects.HttpClient.GetStringAsync("https://pokemondb.net/pokedex/national").GetAwaiter().GetResult().Split(new[] { "Generation 1 Pokémon" }, StringSplitOptions.None)[1];
            foreach (Match m in Regex.Matches(html, "<a href=\"\\/pokedex\\/([^\"]+)\">"))
                _pokemons.Add(m.Groups[1].Value);
        }

        public static List<string> GetPokemons()
            => _pokemons;

        private static List<string> _pokemons;
    }
}
