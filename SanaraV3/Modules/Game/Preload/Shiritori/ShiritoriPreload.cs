using System.IO;
using System.Linq;

namespace SanaraV3.Modules.Game.Preload.Shiritori
{
    public sealed class ShiritoriPreload : IPreload
    {
        public ShiritoriPreload()
        {
            if (!File.Exists("Game/ShiritoriJapanese.txt"))
                _preload = null;
            else
            {
                string[] lines = File.ReadAllLines("Game/ShiritoriJapanese.txt");
                _preload = new ShiritoriPreloadResult[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] curr = lines[i].Split('$');
                    string word = curr[0];
                    _preload[i] = new ShiritoriPreloadResult(word, Tool.LanguageModule.ToRomaji(word), curr[1]);
                }
            }
        }

        public IPreloadResult[] Load()
        {
            return _preload.Cast<IPreloadResult>().ToArray();
        }

        private readonly ShiritoriPreloadResult[] _preload;
    }
}
