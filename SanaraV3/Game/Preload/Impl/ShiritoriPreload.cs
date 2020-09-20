using Discord;
using SanaraV3.Game.Impl;
using SanaraV3.Game.Preload.Result;
using SanaraV3.Module.Tool;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SanaraV3.Game.Preload.Impl
{
    public sealed class ShiritoriPreload : IPreload
    {
        public ShiritoriPreload()
        {
            if (!File.Exists("Saves/Game/ShiritoriJapanese.txt"))
                File.WriteAllBytes("Saves/Game/ShiritoriJapanese.txt", StaticObjects.HttpClient.GetByteArrayAsync("https://files.zirk.eu/Sanara/shiritoriJapanese.txt").GetAwaiter().GetResult());
            string[] lines = File.ReadAllLines("Saves/Game/ShiritoriJapanese.txt");
            _preload = new ShiritoriPreloadResult[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                string[] curr = lines[i].Split('$');
                string word = curr[0];
                _preload[i] = new ShiritoriPreloadResult(word, LanguageModule.ToRomaji(word), curr[1]);
            }
        }

        public ReadOnlyCollection<IPreloadResult> Load()
            => _preload.Cast<IPreloadResult>().ToList().AsReadOnly();

        public string[] GetGameNames()
            => new[] { "shiritori" };

        public string GetNameArg()
            => null;

        public AGame CreateGame(IMessageChannel chan, IUser user, GameSettings settings)
            => new Shiritori(chan, user, this, settings);

        private readonly ShiritoriPreloadResult[] _preload;
    }
}
