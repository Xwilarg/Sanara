﻿using Discord;
using SanaraV3.Games.Impl;
using SanaraV3.Games.Preload.Result;
using SanaraV3.Modules.Tool;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SanaraV3.Games.Preload.Impl
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
                    _preload[i] = new ShiritoriPreloadResult(word, LanguageModule.ToRomaji(word), curr[1]);
                }
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