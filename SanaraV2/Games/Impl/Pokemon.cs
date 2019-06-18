/// This file is part of Sanara.
///
/// Sanara is free software: you can redistribute it and/or modify
/// it under the terms of the GNU General Public License as published by
/// the Free Software Foundation, either version 3 of the License, or
/// (at your option) any later version.
///
/// Sanara is distributed in the hope that it will be useful,
/// but WITHOUT ANY WARRANTY; without even the implied warranty of
/// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
/// GNU General Public License for more details.
///
/// You should have received a copy of the GNU General Public License
/// along with Sanara.  If not, see<http://www.gnu.org/licenses/>.

using Discord;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SanaraV2.Games.Impl
{
    public class PokemonPreload : APreload
    {
        public PokemonPreload() : base(new[] { "pokemon" }, 15, Sentences.PokemonGame)
        { }

        public override bool IsNsfw()
            => false;

        public override bool DoesAllowFull()
            => false;

        public override Multiplayer DoesAllowMultiplayer()
            => Multiplayer.SoloOnly;

        public override string GetRules(ulong guildId, bool _)
            => Sentences.RulesPokemon(guildId);
    }

    public class Pokemon : AQuizz
    {
        public Pokemon(ITextChannel chan, Config config, ulong playerId) : base(chan, Constants.pokemonDictionnary, config, playerId)
        { }

        protected override bool IsDictionnaryFull()
            => true;

        protected override bool DoesDisplayHelp()
            => false;

        protected override async Task<Tuple<string[], string[]>> GetPostInternalAsync(string curr)
        {
            string html;
            using (HttpClient hc = new HttpClient())
            {
                html = await hc.GetStringAsync("https://pokemondb.net/pokedex/" + curr);
            }
            string french = Regex.Match(html, "<th>French<\\/th>[^<]*<td>([^<]+)<\\/td>").Groups[1].Value;
            string japanese = Regex.Match(html, "<th>Japanese<\\/th>[^<]*<td>[^\\(]+\\(([^\\)]+)\\)<\\/td>").Groups[1].Value;
            string german = Regex.Match(html, "<th>German<\\/th>[^<]*<td>([^<]+)<\\/td>").Groups[1].Value;
            return (new Tuple<string[], string[]>(
                new[] { "https://img.pokemondb.net/artwork/" + curr + ".jpg" },
                new[] { curr, french, japanese, german }
            ));
        }

        public static ImmutableList<string> LoadDictionnary()
        {
            List<string> pokemons = new List<string>();
            using (HttpClient hc = new HttpClient())
            {
                string html = hc.GetStringAsync("https://pokemondb.net/pokedex/national").GetAwaiter().GetResult().Split(new[] { "Generation 1 Pokémon" }, StringSplitOptions.None)[1];
                foreach (Match m in Regex.Matches(html, "<a href=\"\\/pokedex\\/([^\"]+)\">"))
                    pokemons.Add(m.Groups[1].Value);
            }
            return (pokemons.ToImmutableList());
        }
    }
}
