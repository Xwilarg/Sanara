﻿using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SanaraV3.Exception;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadGameInfoHelp()
        {
            _submoduleHelp.Add("GameInfo", "Information about various video games");
            _help.Add(("Entertainment", new Help("GameInfo", "Kancolle", new[] { new Argument(ArgumentType.MANDATORY, "shipgirl") }, "Get information about a shipgirl.", new string[0], Restriction.None, "Kancolle Ikazuchi")));
        }
    }
}

namespace SanaraV3.Module.Entertainment
{
    public sealed class GameInfoModule : ModuleBase
    {
        [Command("Kancolle", RunMode = RunMode.Async), Alias("KC", "Shipgirl")]
        public async Task Kancolle(string shipgirl)
        {
            var json = JsonConvert.DeserializeObject<JObject>(await StaticObjects.HttpClient.GetStringAsync("https://kancolle.fandom.com/api.php?action=query&prop=revisions&rvprop=content&titles=" + HttpUtility.UrlEncode(shipgirl) + "&format=json"));
            var dict = json["query"]["pages"].ToObject<Dictionary<int, JObject>>();
            // A dictionary of search results
            // Key is the id of the page, value a json with its content
            // If the search, there will be a key with the value of -1
            var first = dict.First();
            if (first.Key == -1)
                throw new CommandFailed("This shipgirl doesn't exist");
            var content = first.Value["revisions"][0]["*"].Value<string>();
            var match = Regex.Match(content, "#REDIRECT \\[\\[([^\\]]+)\\]\\]");
            if (match.Success) // Redirections (for example Imuya redirect to I-168)
                await Kancolle(match.Groups[1].Value);
            else
            {
                var title = first.Value["title"].Value<string>();
                var shipUrl = "https://kancolle.fandom.com/wiki/" + title + "/Gallery";
                var html = StaticObjects.HttpClient.GetStringAsync(shipUrl).GetAwaiter().GetResult();
                var img = Regex.Match(html, "https:\\/\\/[^\\/]+\\/kancolle\\/images\\/[0-9a-z]+\\/[0-9a-z]+\\/" + title + "_Full\\.png").Value;

                var libraryIntro = Regex.Match(CleanWikiText(content), "\\|Library\\/En = ([^\\|]+)").Groups[1].Value; // Library english introduction

                content = Regex.Replace(content, "\\(\\[[^\\]]+\\]\\)", "");
                content = Regex.Replace(content, "\\[https?:\\/\\/[^ ]+ ([^\\]]+)\\]", "$1");
                var appearance = CleanWikiText(Regex.Match(content, "=== ?Appearance ?===([^=]+)").Groups[1].Value);
                var personality = CleanWikiText(Regex.Match(content, "=== ?Personality ?===([^=]+)").Groups[1].Value);
                var trivia = CleanWikiText(Regex.Match(content, "== ?Trivia ?==([^=]+)").Groups[1].Value);

                var embed = new EmbedBuilder
                {
                    Title = title,
                    Color = Color.Blue,
                    ImageUrl = img,
                    Footer = new EmbedFooterBuilder
                    {
                        Text = libraryIntro
                    }
                };
                if (!string.IsNullOrEmpty(appearance)) embed.AddField("Appearance", SplitApproprietly(appearance));
                if (!string.IsNullOrEmpty(personality)) embed.AddField("Personality", SplitApproprietly(personality));
                if (!string.IsNullOrEmpty(trivia)) embed.AddField("Trivia", SplitApproprietly(trivia));

                await ReplyAsync(embed: embed.Build());
            }
        }

        private string CleanWikiText(string input)
        {
            input = input.Split("{{ShipPageFooter}}", StringSplitOptions.None)[0].Split("{{Ship/Footer}}", StringSplitOptions.None)[0];   
            input = input.Replace("\\n", "\n").Replace("*", "\\*").Replace("<br>", "\n").Trim('\n', ' ');
            input = Regex.Replace(input, "<!--[^-]+-->", "");
            return Regex.Replace(input, "\\[\\[(:[^:]+:)?([^(\\||\\])]+)(\\||\\])[^\\]]*\\]?\\]", "$2");
        }

        private string SplitApproprietly(string input)
        {
            if (input.Length > 1024)
            {
                var split = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                return SplitApproprietly(string.Join("\n", split.Take(split.Length - 1)));
            }
            return input;
        }
    }
}
