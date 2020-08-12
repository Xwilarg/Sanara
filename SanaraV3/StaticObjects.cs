using BooruSharp.Booru;
using Google.Apis.YouTube.v3;
using SanaraV3.LanguageResources;
using SanaraV3.Modules.Game;
using SanaraV3.Modules.Game.PostMode;
using SanaraV3.Modules.Game.Preload;
using SanaraV3.Modules.Game.Preload.Shiritori;
using SanaraV3.Modules.Nsfw;
using SanaraV3.Modules.Radio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace SanaraV3
{
    /// <summary>
    /// Keep track of all the objects that must be keepen alive
    /// </summary>
    public static class StaticObjects
    {
        public static HttpClient HttpClient = new HttpClient();
        public static Random Random = new Random();

        // NSFW MODULE
        public static Safebooru Safebooru   = new Safebooru();
        public static Gelbooru  Gelbooru    = new Gelbooru();
        public static E621      E621        = new E621();
        public static E926      E926        = new E926();
        public static Rule34    Rule34      = new Rule34();
        public static Konachan  Konachan    = new Konachan();
        public static TagsManager Tags      = new TagsManager();

        // RADIO MODULE
        public static Dictionary<ulong, RadioChannel> Radios = new Dictionary<ulong, RadioChannel>();

        // ENTERTAINMENT MODULE
        public static YouTubeService YouTube = null;

        // GAME MODULE
        public static List<AGame>   Games = new List<AGame>();
        public static TextMode      ModeText = new TextMode();
        public static UrlMode       ModeUrl = new UrlMode();
        public static IPreload[]    Preloads;
        private static readonly GameManager _gm = new GameManager();

        // LANGUAGE MODULE
        public static Dictionary<string, string> RomajiToHiragana = new Dictionary<string, string>();
        public static Dictionary<string, string> HiraganaToRomaji = new Dictionary<string, string>();
        public static Dictionary<string, string> RomajiToKatakana = new Dictionary<string, string>();
        public static Dictionary<string, string> KatakanaToRomaji = new Dictionary<string, string>();

        static StaticObjects()
        {
            HttpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");

            Safebooru.HttpClient = HttpClient;
            Gelbooru.HttpClient = HttpClient;
            E621.HttpClient = HttpClient;
            E926.HttpClient = HttpClient;
            Rule34.HttpClient = HttpClient;
            Konachan.HttpClient = HttpClient;

            foreach (var elem in Hiragana.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true).Cast<DictionaryEntry>())
            {
                RomajiToHiragana.Add((string)elem.Key, (string)elem.Value);
                HiraganaToRomaji.Add((string)elem.Value, (string)elem.Key);
            }
            foreach (var elem in Katakana.ResourceManager.GetResourceSet(CultureInfo.CurrentUICulture, true, true).Cast<DictionaryEntry>())
            {
                RomajiToKatakana.Add((string)elem.Key, (string)elem.Value);
                KatakanaToRomaji.Add((string)elem.Value, (string)elem.Key);
            }

            Preloads = new[]
            {
                new ShiritoriPreload()
            };
        }
    }
}
