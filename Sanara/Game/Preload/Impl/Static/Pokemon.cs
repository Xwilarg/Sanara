using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl.Static
{
    public static class Pokemon
    {
        public static void Init(IServiceProvider provider)
        {
            _pokemons = new();
            string html = provider.GetRequiredService<HttpClient>().GetStringAsync("https://pokemondb.net/pokedex/national").GetAwaiter().GetResult().Split(new[] { "Generation 1 Pokémon" }, StringSplitOptions.None)[1];
            foreach (Match m in Regex.Matches(html, "<a href=\"\\/pokedex\\/([^\"]+)\">"))
                _pokemons.Add(m.Groups[1].Value);
        }

        public static List<string> GetPokemons()
            => _pokemons;

        private static List<string> _pokemons;
    }
}
