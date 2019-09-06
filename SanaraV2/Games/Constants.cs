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
using System.Collections.Generic;
using System.Linq;

namespace SanaraV2.Games
{
    public static class Constants
    {
        // Order in Db: Shiritori, Anime, Booru, KanColle, AzurLane
        // The order matter and must be preserved
        public static readonly Tuple<Type, Type, string>[] allRankedGames = new Tuple<Type, Type, string>[]
        {
            new Tuple<Type, Type, string>(typeof(ShiritoriPreload), typeof(Shiritori), "gameModuleShiritori"),
            new Tuple<Type, Type, string>(typeof(AnimePreload), typeof(Anime), "gameModuleAnime"),
            new Tuple<Type, Type, string>(typeof(BooruPreload), typeof(Booru), "gameModuleBooru"),
            new Tuple<Type, Type, string>(typeof(KanCollePreload), typeof(KanColle), "gameModuleKancolle"),
            new Tuple<Type, Type, string>(typeof(AzurLanePreload), typeof(AzurLane), "gameModuleAzurLane"),
            new Tuple<Type, Type, string>(typeof(FateGOPreload), typeof(FateGO), "gameModuleFateGO"),
            new Tuple<Type, Type, string>(typeof(PokemonPreload), typeof(Pokemon), "gameModulePokemon"),
            new Tuple<Type, Type, string>(typeof(GirlsFrontlinePreload), typeof(GirlsFrontline), "gameModuleGirlsFrontline")
        };

        public static readonly Tuple<Type, Type, string>[] allGames = InitAllGames();

        private static Tuple<Type, Type, string>[] InitAllGames()
        {
            List<Tuple<Type, Type, string>> allRanked = allRankedGames.ToList();
            allRanked.Add(new Tuple<Type, Type, string>(typeof(ReversiPreload), typeof(Reversi), "gameModuleReversi"));
            return allRanked.ToArray();
        }

        public static readonly List<string> shiritoriDictionnary = Shiritori.LoadDictionnary();
        public static readonly List<string> kanColleDictionnary = KanColle.LoadDictionnary();
        public static readonly Tuple<List<string>, List<string>> animeDictionnaries = Anime.LoadDictionnaries();
        public static readonly List<string> booruDictionnary = Booru.LoadDictionnary();
        public static readonly List<string> azurLaneDictionnary = AzurLane.LoadDictionnary();
        public static readonly List<string> fateGODictionnary = FateGO.LoadDictionnary();
        public static readonly List<string> pokemonDictionnary = Pokemon.LoadDictionnary();
        public static readonly List<string> girlsfrontlineDictionnary = GirlsFrontline.LoadDictionnary();

        // Used by information module to display stats
        public static readonly Tuple<Func<ulong, string>, List<string>>[] allDictionnaries = new Tuple<Func<ulong, string>, List<string>>[]
        {
            new Tuple<Func<ulong, string>, List<string>>(Sentences.ShiritoriGame, shiritoriDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.KancolleGame, kanColleDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.AnimeGame, animeDictionnaries.Item1),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.AnimeFull, animeDictionnaries.Item2),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.BooruGame, booruDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.AzurLaneGame, azurLaneDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.FateGOGame, fateGODictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.PokemonGame, pokemonDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.GirlsFrontlineGame, girlsfrontlineDictionnary)
        };
    }
}
