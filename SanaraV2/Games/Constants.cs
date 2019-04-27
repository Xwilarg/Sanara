using System;

namespace SanaraV2.Games
{
    public static class Constants
    {
        public static Tuple<Type, Type>[] allGames = new Tuple<Type, Type>[] // All games need to be added here!
        {
            new Tuple<Type, Type>(typeof(ShiritoriPreload), typeof(Shiritori))
        };
    }
}
