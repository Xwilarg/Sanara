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

using SanaraV2.Games.Impl;
using System;
using System.Collections.Immutable;

namespace SanaraV2.Games
{
    public static class Constants
    {
        // Order in Db: Shiritori, Anime, Booru, KanColle, AzurLane
        // The order matter and must be preserved
        public static ImmutableArray<Tuple<Type, Type, string>> allRankedGames = new ImmutableArray<Tuple<Type, Type, string>>
        {
            new Tuple<Type, Type, string>(typeof(ShiritoriPreload), typeof(Shiritori), "gameModuleShiritori"),
            new Tuple<Type, Type, string>(typeof(AnimePreload), typeof(Anime), "gameModuleAnime"),
            new Tuple<Type, Type, string>(typeof(BooruPreload), typeof(Booru), "gameModuleBooru"),
            new Tuple<Type, Type, string>(typeof(KanCollePreload), typeof(KanColle), "gameModuleKancolle"),
            new Tuple<Type, Type, string>(typeof(AzurLanePreload), typeof(AzurLane), "gameModuleAzurLane"),
            new Tuple<Type, Type, string>(typeof(FateGOPreload), typeof(FateGO), "gameModuleFateGO"),
            new Tuple<Type, Type, string>(typeof(PokemonPreload), typeof(Pokemon), "gameModulePokemon")
        };

        public static ImmutableArray<Tuple<Type, Type, string>> allGames = allRankedGames;

        public static ImmutableList<string> shiritoriDictionnary = Shiritori.LoadDictionnary();
        public static ImmutableList<string> kanColleDictionnary = KanColle.LoadDictionnary();
        public static Tuple<ImmutableList<string>, ImmutableList<string>> animeDictionnaries = Anime.LoadDictionnaries();
        public static ImmutableList<string> booruDictionnary = Booru.LoadDictionnary();
        public static ImmutableList<string> azurLaneDictionnary = AzurLane.LoadDictionnary();
        public static ImmutableList<string> fateGODictionnary = FateGO.LoadDictionnary();
        public static ImmutableList<string> pokemonDictionnary = Pokemon.LoadDictionnary();

        public static ImmutableArray<Tuple<Func<ulong, string>, ImmutableList<string>>> allDictionnaries = new ImmutableArray<Tuple<Func<ulong, string>, ImmutableList<string>>>
        {
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.ShiritoriGame, shiritoriDictionnary),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.KancolleGame, kanColleDictionnary),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.AnimeGame, animeDictionnaries.Item1),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.AnimeFull, animeDictionnaries.Item2),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.BooruGame, booruDictionnary),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.AzurLaneGame, azurLaneDictionnary),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.FateGOGame, fateGODictionnary),
            new Tuple<Func<ulong, string>, ImmutableList<string>>(Sentences.PokemonGame, pokemonDictionnary)
        };
    }
}
