using System.IO;
using System.Linq;

namespace SanaraV3.Modules.Game.Preload.Shiritori
{
    public sealed class ShiritoriPreload
    {
        public ShiritoriPreload()
        {
            if (!File.Exists("Game/ShiritoriJapanese.txt"))
                preload = null;
            else
                preload = File.ReadAllLines("Game/ShiritoriJapanese.txt").Select(x => new ShiritoriPreloadResult()).ToArray();
        }

        private ShiritoriPreloadResult[] preload;
    }
}
