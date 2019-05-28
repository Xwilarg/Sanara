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
using Discord.WebSocket;
using SanaraV2.Games;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Information : ModuleBase
    {
        Program p = Program.p;

        private struct Eval
        {
            public DiscordSocketClient Client { set; get; }
            public ICommandContext Context { set; get; }
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
            string booruModule = Sentences.BooruModuleName(Context.Guild.Id);
            string codeModule = Sentences.CodeModuleName(Context.Guild.Id);
            string communicationModule = Sentences.CommunicationModuleName(Context.Guild.Id);
            string doujinshiModule = Sentences.DoujinshiModuleName(Context.Guild.Id);
            string gameModule = Sentences.GameModuleName(Context.Guild.Id);
            string imageModule = Sentences.ImageModuleName(Context.Guild.Id);
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
                embed.Description = Sentences.AnimeMangaHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "2" || booruModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + booruModule + ")";
                embed.Description = Sentences.BooruHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "3" || codeModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + codeModule + ")";
                embed.Description = Sentences.CodeHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "4" || communicationModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + communicationModule + ")";
                embed.Description = Sentences.CommunicationHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "5" || doujinshiModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + doujinshiModule + ")";
                embed.Description = Sentences.DoujinshiHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "6" || gameModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + gameModule + ")";
                embed.Description = Sentences.GameHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "7" || imageModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + imageModule + ")";
                embed.Description = Sentences.ImageHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "8" || informationModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + informationModule + ")";
                embed.Description = Sentences.InformationHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "9" || kantaiCollectionModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + kantaiCollectionModule + ")";
                embed.Description = Sentences.KantaiCollectionHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "10" || linguisticModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + linguisticModule + ")";
                embed.Description = Sentences.LinguisticHelp(Context.Guild.Id, ((ITextChannel)Context.Channel).IsNsfw);
            }
            else if (page != "" && (page == "11" || radioModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + radioModule + ")";
                embed.Description = Sentences.RadioHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "12" || settingsModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + settingsModule + ")";
                embed.Description = Sentences.SettingsHelp(Context.Guild.Id, Context.User.Id == Context.Guild.OwnerId, Context.User.Id == Base.Sentences.ownerId);
            }
            else if (page != "" && (page == "13" || visualNovelModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + visualNovelModule + ")";
                embed.Description = Sentences.VisualNovelHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "14" || xkcdModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + xkcdModule + ")";
                embed.Description = Sentences.XkcdHelp(Context.Guild.Id);
            }
            else if (page != "" && (page == "15" || youtubeModule.ToLower().Contains(page)))
            {
                embed.Title += " (" + youtubeModule + ")";
                embed.Description = Sentences.YouTubeHelp(Context.Guild.Id);
            }
            else
            {
                embed.Description = Sentences.HelpHelp(Context.Guild.Id) + Environment.NewLine +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.AnimeManga) ? "**1**: " + animeMangaModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Booru) ? "**2**: " + booruModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Code) ? "**3**: " + codeModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Communication) ? "**4**: " + communicationModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Doujinshi) ? "**5**: " + doujinshiModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Game) ? "**6**: " + gameModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Image) ? "**7**: " + imageModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Information) ? "**8**: " + informationModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Kancolle) ? "**9**: " + kantaiCollectionModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Linguistic) ? "**10**: " + linguisticModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Radio) ? "**11**: " + radioModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Settings) ? "**12**: " + settingsModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Vn) ? "**13**: " + visualNovelModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Xkcd) ? "**14**: " + xkcdModule + Environment.NewLine : "") +
                    (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Youtube) ? "**15**: " + youtubeModule + Environment.NewLine: "");
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
                Title = "Datas saved about " + Context.Guild.Name,
                Description = await Program.p.db.GetGuild(Context.Guild.Id)
            }.Build());
        }

        [Command("Status"), Summary("Display which commands aren't available because of missing files")]
        public async Task Status()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Information);
            int yes = 0;
            int no = 0;
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Services availability"
            };
            string description = "";
            for (Program.Module i = 0; i < Program.Module.Youtube; i++)
                description += "**" + i.ToString() + "**: " + ((Program.p.db.IsAvailable(Context.Guild.Id, i)) ? (Sentences.Enabled(Context.Guild.Id)) : (Sentences.Disabled(Context.Guild.Id))) + Environment.NewLine;
            embed.Description = description;
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Radio))
            {
                embed.AddField("Radio Module",
                    "**Opus dll:** " + ((File.Exists("opus.dll") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**Lib Sodium dll:** " + ((File.Exists("libsodium.dll") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**Ffmpeg:** " + ((File.Exists("ffmpeg.exe") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**youtube-dl:** " + ((File.Exists("youtube-dl.exe") ? ("Yes") : ("No"))) + Environment.NewLine +
                    "**YouTube API key:** " + ((p.youtubeService != null ? ("Yes") : ("No"))));
                if (File.Exists("opus.dll") && File.Exists("libsodium.dll") && File.Exists("ffmpeg.exe") && p.youtubeService != null)
                    yes++;
                else
                    no++;
            }
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Game))
            {
                embed.AddField("Game Module", ScoreManager.GetInformation(Context.Guild.Id, ref yes, ref no));
            }
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Linguistic))
            {
                embed.AddField("Linguistic Module - Translations",
                "**Google Translate API key:** " + ((p.translationClient != null ? ("Yes") : ("No"))) + Environment.NewLine +
                "**Google Vision API key:** " + ((p.visionClient != null ? ("Yes") : ("No"))));
                if (p.translationClient != null)
                {
                    yes++;
                    if (p.visionClient != null)
                        yes++;
                    else
                        no++;
                }
                else
                    no += 2;
            }
            if (Program.p.db.IsAvailable(Context.Guild.Id, Program.Module.Youtube))
            {
                embed.AddField("YouTube Module", "**YouTube API key:** " + ((p.youtubeService != null) ? ("Yes") : ("No")));
                if (p.youtubeService != null)
                    yes++;
                else
                    no++;
            }
            if (yes + no == 0)
                yes++;
            int max = yes + no;
            embed.Color = new Color(no * 255 / max, yes * 255 / max, 0);
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
            embed.AddField("Translation availability", finalLanguage);
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