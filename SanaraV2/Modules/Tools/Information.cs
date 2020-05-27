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
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using SanaraV2.Games;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Information : ModuleBase
    {
        Program p = Program.p;

        [Command("Error")]
        public async Task Error(params string[] args)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            if (args.Length == 0)
            {
                await ReplyAsync(Sentences.ErrorHelp(Context.Guild.Id));
                return;
            }
            string id = string.Join("", args);
            if (!Program.p.exceptions.ContainsKey(id))
            {
                await ReplyAsync(Sentences.ErrorNotFound(Context.Guild.Id));
                return;
            }
            var elem = Program.p.exceptions[id];
            await ReplyAsync("", false, new EmbedBuilder
            {
                Color = Color.Purple,
                Title = elem.exception.InnerException.GetType().ToString(),
                Description = "```" + Environment.NewLine + elem.exception.InnerException.Message + Environment.NewLine + "```",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = Sentences.Command(Context.Guild.Id),
                        Value = elem.exception.Context.Message.ToString().Replace("@", "@\u200b")
                    },
                    new EmbedFieldBuilder
                    {
                        Name = Sentences.Date(Context.Guild.Id),
                        Value = elem.date.ToString(Base.Sentences.DateHourFormat(Context.Guild.Id))
                    }
                },
                Footer = new EmbedFooterBuilder
                {
                    Text = Sentences.ErrorGdpr(Context.Guild.Id, "https://sanara.zirk.eu/gdpr.html#collectedError")
                }
            }.Build());
        }

        [Command("Logs"), Alias("Log", "Changes", "Change")]
        public async Task Logs(params string[] args)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            if (p.GitHubKey == null)
            {
                await ReplyAsync(Base.Sentences.NoApiKey(Context.Guild.Id));
                return;
            }
            dynamic json;
            EmbedBuilder eb = new EmbedBuilder()
            {
                Title = Sentences.LatestChanges(Context.Guild.Id),
                Color = Color.Green
            };
            using (HttpClient hc = new HttpClient())
            {
                hc.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 Sanara");
                json = JsonConvert.DeserializeObject(await hc.GetStringAsync("https://api.github.com/repos/Xwilarg/Sanara/commits?per_page=5&access_token=" + p.GitHubKey));
            }
            foreach (var j in json)
            {
                eb.AddField(DateTime.ParseExact((string)j.commit.author.date, "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture).ToString(Base.Sentences.DateHourFormat(Context.Guild.Id))
                    + " " + Sentences.ByStr(Context.Guild.Id) + " " + j.commit.author.name, j.commit.message);
            }
            await ReplyAsync("", false, eb.Build());
        }

        [Command("Help"), Summary("Give the help"), Alias("Commands")]
        public async Task Help(params string[] args)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = Sentences.Help(Context.Guild.Id),
                Color = Color.Purple
            };
            string animeMangaModule = Sentences.AnimeMangaModuleName(Context.Guild.Id);
            string arknightsModule = Sentences.ArknightsModuleName(Context.Guild.Id);
            string booruModule = Sentences.BooruModuleName(Context.Guild.Id);
            string codeModule = Sentences.CodeModuleName(Context.Guild.Id);
            string communicationModule = Sentences.CommunicationModuleName(Context.Guild.Id);
            string communityModule = Sentences.CommunityModuleName(Context.Guild.Id);
            string doujinshiModule = Sentences.DoujinshiModuleName(Context.Guild.Id);
            string gameModule = Sentences.GameModuleName(Context.Guild.Id);
            string informationModule = Sentences.InformationModuleName(Context.Guild.Id);
            string kantaiCollectionModule = Sentences.KantaiCollectionModuleName(Context.Guild.Id);
            string linguisticModule = Sentences.LinguisticModuleName(Context.Guild.Id);
            string radioModule = Sentences.RadioModuleName(Context.Guild.Id);
            string settingsModule = Sentences.SettingsModuleName(Context.Guild.Id);
            string visualNovelModule = Sentences.VisualNovelModuleName(Context.Guild.Id);
            string xkcdModule = Sentences.XkcdModuleName(Context.Guild.Id);
            string youtubeModule = Sentences.YoutubeModuleName(Context.Guild.Id);
            string page = string.Join(" ", args).ToLower();
            if (page != "" && (page == "1" || animeMangaModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + animeMangaModule + ")";
                embed.Description = Sentences.AnimeMangaHelp(Context.Guild.Id, Settings.CanModify(Context.User, Base.Sentences.ownerId));
            }
            else if (page != "" && (page == "2" || arknightsModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + arknightsModule + ")";
                embed.Description = Sentences.ArknightsHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "3" || booruModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + booruModule + ")";
                embed.Description = Sentences.BooruHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "4" || codeModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + codeModule + ")";
                embed.Description = Sentences.CodeHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "5" || communicationModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + communicationModule + ")";
                embed.Description = Sentences.CommunicationHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "6" || communityModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + communityModule + ")";
                embed.Description = Sentences.CommunityHelp(Context.Guild.Id, Context.User.Id == Base.Sentences.ownerId);
            }
            else if (page != "" && (page == "7" || doujinshiModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + doujinshiModule + ")";
                embed.Description = Sentences.DoujinshiHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "8" || gameModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + gameModule + ")";
                embed.Description = Sentences.GameHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "9" || informationModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + informationModule + ")";
                embed.Description = Sentences.InformationHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "10" || kantaiCollectionModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + kantaiCollectionModule + ")";
                embed.Description = Sentences.KantaiCollectionHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "11" || linguisticModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + linguisticModule + ")";
                embed.Description = Sentences.LinguisticHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "12" || radioModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + radioModule + ")";
                embed.Description = Sentences.RadioHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "13" || settingsModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + settingsModule + ")";
                embed.Description = Sentences.SettingsHelp(Context.Guild.Id, Settings.CanModify(Context.User, Base.Sentences.ownerId), Context.User.Id == Base.Sentences.ownerId);
            }
            else if (page != "" && (page == "14" || visualNovelModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + visualNovelModule + ")";
                embed.Description = Sentences.VisualNovelHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "15" || xkcdModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + xkcdModule + ")";
                embed.Description = Sentences.XkcdHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "16" || youtubeModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + youtubeModule + ")";
                embed.Description = Sentences.YouTubeHelp(Context.Guild.Id);
            }
            else
            {
                embed.Description = Sentences.HelpHelp(Context.Guild.Id) + Environment.NewLine +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.AnimeManga) ? "**1**: " + animeMangaModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Arknights) ? "**2**: " + arknightsModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Booru) ? "**3**: " + booruModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Code) ? "**4**: " + codeModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Communication) ? "**5**: " + communicationModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Community) ? "**6**: " + communityModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Doujinshi) ? "**7**: " + doujinshiModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Game) ? "**8**: " + gameModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Information) ? "**9**: " + informationModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Kancolle) ? "**10**: " + kantaiCollectionModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Linguistic) ? "**11**: " + linguisticModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Radio) ? "**12**: " + radioModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Settings) ? "**13**: " + settingsModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Vn) ? "**14**: " + visualNovelModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Xkcd) ? "**15**: " + xkcdModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Youtube) ? "**16**: " + youtubeModule + Environment.NewLine: "");
            }
            await ReplyAsync("", false, embed.Build());
        }

        [Command("GDPR"), Summary("Show infos the bot have about the user and the guild")]
        public async Task GDPR(params string[] command)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            await ReplyAsync("", false, new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = Sentences.DataSaved(Context.Guild.Id, Context.Guild.Name),
                Description = await Program.p.db.GetGuild(Context.Guild.Id)
            }.Build());
            var me = Program.p.cm.GetProfile(Context.User.Id);
            if (me != null)
            {
                await Context.User.SendMessageAsync("", false, new EmbedBuilder()
                {
                    Color = Color.Blue,
                    Title = "Data saved about you",
                    Description = string.Join(Environment.NewLine, me.GetProfileToDb(Program.p.db.GetR(), true).Select(x => x.Key + ": " + x.Value)),
                    Footer = new EmbedFooterBuilder
                    {
                        Text = "Achievements progression is kept hidden"
                    }
                }.Build());
            }
        }

        [Command("Status"), Summary("Display which commands aren't available because of missing files")]
        public async Task Status()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = Sentences.ServicesAvailability(Context.Guild.Id)
            };
            List<string> disabledModules = new List<string>();
            for (Program.Module i = 0; i <= Enum.GetValues(typeof(Program.Module)).Cast<Program.Module>().Max(); i++)
                if (!Program.p.db.IsAvailable(Context.Guild.Id, i))
                    disabledModules.Add(i.ToString());
            embed.Description = disabledModules.Count == 0 ? "All modules are enabled" : "Disabled modules:" + string.Join(", ", disabledModules);
            string[] toCheck = new[]
            {
                "opus.dll", "libsodium.dll", "ffmpeg.exe", "youtube-dl.exe", 
            };
            List<string> missingFiles = new List<string>();
            foreach (string file in toCheck)
                if (!File.Exists(file))
                    missingFiles.Add(file);
            if (p.translationClient == null) missingFiles.Add("Google Translate API Key");
            if (p.visionClient == null) missingFiles.Add("Google Vision API Key");
            if (p.GitHubKey == null) missingFiles.Add("GitHub API Key");
            if (p.kitsuAuth == null) missingFiles.Add("Kitsu Logins");
            if (p.youtubeService == null) missingFiles.Add("YouTube API Key");
            embed.AddField("Missing Files", missingFiles.Count == 0 ? "None" : string.Join(", ", missingFiles));
            embed.AddField("Game Dictionnaries", ScoreManager.GetInformation(Context.Guild.Id));
            embed.AddField("Anime/Manga Subscription Channel", await p.db.GetMyChannelNameAnimeAsync(Context.Guild));
            embed.AddField("Doujinshi Subscription Channel", await p.db.GetMyChannelNameDoujinshiAsync(Context.Guild));
            embed.AddField("Profile Count", Program.p.cm.GetProfileCount(), true);
            embed.AddField("Anime Subscription Count", Program.p.db.AnimeSubscription.Count(), true);
            embed.AddField("Doujinshi Subscription Count", Program.p.db.NHentaiSubscription.Count(), true);
            embed.Color = Color.Blue;
            Dictionary<string, int> allTrads = new Dictionary<string, int>();
            foreach (var elem in Program.p.allLanguages)
                allTrads.Add(elem.Key, 0);
            foreach (var s in Program.p.translations)
            {
                if (s.Value.Any(x => x.language == "en"))
                {
                    foreach (var trad in s.Value)
                    {
                        allTrads[trad.language]++;
                    }
                }
            }
            string finalLanguage = "";
            int enRef = allTrads["en"];
            foreach (var s in allTrads)
            {
                finalLanguage += CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Program.p.allLanguages[s.Key][0]) + ": " + (s.Value * 100 / enRef) + "%" + Environment.NewLine;
            }
            embed.AddField(Sentences.TranslationsAvailability(Context.Guild.Id), finalLanguage);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Invite", RunMode = RunMode.Async), Summary("Get invitation link")]
        public async Task Invite()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            await ReplyAsync("<https://discordapp.com/oauth2/authorize?client_id=329664361016721408&permissions=3196928&scope=bot>");
        }
    }
}