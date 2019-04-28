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

namespace SanaraV2.Games
{
    public static class Constants
    {
        public static Tuple<Type, Type>[] allGames = new Tuple<Type, Type>[] // All games need to be added here!
        {
            new Tuple<Type, Type>(typeof(ShiritoriPreload), typeof(Shiritori)),
            new Tuple<Type, Type>(typeof(KanCollePreload), typeof(KanColle)),
            new Tuple<Type, Type>(typeof(AnimePreload), typeof(Anime)),
            new Tuple<Type, Type>(typeof(BooruPreload), typeof(Booru)),
            new Tuple<Type, Type>(typeof(AzurLanePreload), typeof(AzurLane))
        };

        public static List<string> shiritoriDictionnary = Shiritori.LoadDictionnary();
        public static List<string> kanColleDictionnary = KanColle.LoadDictionnary();
        public static Tuple<List<string>, List<string>> animeDictionnaries = Anime.LoadDictionnaries();
        public static List<string> booruDictionnary = Booru.LoadDictionnary();
        public static List<string> azurLaneDictionnary = AzurLane.LoadDictionnary();

        public static Tuple<Func<ulong, string>, List<string>>[] allDictionnaries = new Tuple<Func<ulong, string>, List<string>>[]
        {
            new Tuple<Func<ulong, string>, List<string>>(Sentences.ShiritoriGame, shiritoriDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.KancolleGame, kanColleDictionnary),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.AnimeGame, animeDictionnaries.Item1),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.AnimeFull, animeDictionnaries.Item2),
            new Tuple<Func<ulong, string>, List<string>>(Sentences.AzurLaneGame, azurLaneDictionnary),
        };
    }
}
