using Discord;
using System.IO;
using System.Linq;

namespace SanaraV3.Modules.Game.Preload.Shiritori
{
    public sealed class ShiritoriPreload : IPreload
    {
        public ShiritoriPreload()
        {
            if (!File.Exists("Saves/Game/ShiritoriJapanese.txt"))
                _preload = null;
            else
            {
                string[] lines = File.ReadAllLines("Saves/Game/ShiritoriJapanese.txt");
                _preload = new ShiritoriPreloadResult[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    string[] curr = lines[i].Split('$');
                    string word = curr[0];
                    System.Console.WriteLine("A");
                    System.Console.WriteLine(word);
                    System.Console.WriteLine(Tool.LanguageModule.ToRomaji(word));
                    System.Console.WriteLine(curr[1]);
                    System.Console.WriteLine("A END");
                    _preload[i] = new ShiritoriPreloadResult(word, Tool.LanguageModule.ToRomaji(word), curr[1]);
                }
            }
        }

        public IPreloadResult[] Load()
            => _preload.Cast<IPreloadResult>().ToArray();

        public string[] GetGameNames()
            => new[] { "shiritori" };

        public AGame CreateGame(IMessageChannel chan)
            => new Impl.Shiritori(chan, this);

        private readonly ShiritoriPreloadResult[] _preload;
    }
}
