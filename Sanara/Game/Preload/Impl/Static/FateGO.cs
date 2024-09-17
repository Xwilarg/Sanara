﻿using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;

namespace Sanara.Game.Preload.Impl.Static
{
    public static class FateGO
    {
        public static void Init(IServiceProvider provider)
        {
            var http = provider.GetRequiredService<HttpClient>();

            _characters = new();
            foreach (string servantClass in new[] { "Shielder", "Saber", "Archer", "Lancer", "Rider", "Caster", "Assassin", "Berserker", "Ruler", "Avenger", "Moon_Cancer", "Alter_Ego", "Foreigner" })
            {
                string html = http.GetStringAsync("https://fategrandorder.fandom.com/wiki/" + servantClass).GetAwaiter().GetResult();
                html = html.Split(new[] { "navbox mw-collapsible" }, StringSplitOptions.None)[0]; // Remove useless things at ending
                html = string.Join("", html.Split(new[] { "<table class=\"wikitable\"" }, StringSplitOptions.None).Skip(2));
                foreach (string s in html.Split(new[] { "<td>" }, StringSplitOptions.None))
                {
                    Match match = Regex.Match(s, "<a href=\"\\/wiki\\/([^\"]+)\"( |\t)*title=\"[^\"]+\">");
                    if (match.Success && !s.Contains("Unplayable")) // We only take servants that can be played
                    {
                        string name = match.Groups[1].Value;
                        if (!_characters.Contains(name) && name != "Event_Reward") // Some characters are here multiple times
                        {
                            _characters.Add(name);
                        }
                    }
                }

            }
        }

        public static List<string> GetCharacters()
            => _characters;

        private static List<string> _characters;
    }
}
