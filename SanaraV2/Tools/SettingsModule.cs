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
using SanaraV2.Base;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Tools
{
    public class SettingsModule : ModuleBase
    {
        Program p = Program.p;

        private void CopyContent(string source, string destination)
        {
            source = source.Replace('\\', '/');
            destination = destination.Replace('\\', '/');
            foreach (string f in Directory.GetFiles(source))
            {
                string f2 = f.Replace('\\', '/');
                File.Copy(f2, destination + "/" + f2.Split('/')[f2.Split('/').Length - 1]);
            }
            foreach (string d in Directory.GetDirectories(source))
            {
                string d2 = d.Replace('\\', '/');
                Directory.CreateDirectory(destination + "/" + d2.Split('/')[d2.Split('/').Length - 1]);
                CopyContent(d2, destination + "/" + d2.Split('/')[d2.Split('/').Length - 1]);
            }
        }

        [Command("Archive", RunMode = RunMode.Async), Summary("Create an archive for all datas")]
        public async Task Archive()
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild.Id));
            }
            else
            {
                await ReplyAsync(Sentences.CopyingFiles(Context.Guild.Id));
                if (!Directory.Exists("Archives"))
                    Directory.CreateDirectory("Archives");
                string currTime = DateTime.UtcNow.ToString("yy-MM-dd-HH-mm-ss");
                Directory.CreateDirectory("Archives/" + currTime);
                CopyContent("Saves", "Archives/" + currTime);
                await ReplyAsync(Sentences.CreateArchiveStr(Context.Guild.Id, currTime));
            }
        }

        [Command("Language"), Summary("Set the language of the bot for this server")]
        public async Task SetLanguage(params string[] language)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else if (language.Length == 0)
                await ReplyAsync(Sentences.NeedLanguage(Context.Guild.Id));
            else
            {
                string nextLanguage = Utilities.AddArgs(language);
                string lang = Utilities.GetLanguage(nextLanguage);
                if (lang == null)
                    await ReplyAsync(Sentences.NeedLanguage(Context.Guild.Id));
                else
                {
                    p.guildLanguages[Context.Guild.Id] = lang;
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/language.dat", lang);
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Prefix"), Summary("Set the prefix of the bot for this server")]
        public async Task SetPrefix(params string[] command)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else
            {
                if (command.Length == 0)
                {
                    p.prefixs[Context.Guild.Id] = "";
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/prefix.dat", "");
                    await ReplyAsync(Sentences.PrefixRemoved(Context.Guild.Id));
                }
                else
                {
                    string prefix = Utilities.AddArgs(command);
                    p.prefixs[Context.Guild.Id] = prefix;
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/prefix.dat", prefix);
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Status"), Summary("Display which commands aren't available because of missing files")]
        public async Task Status()
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            int yes = 0;
            int no = 0;
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = "Services availability"
            };
            embed.AddField("Radio Module",
                "**Opus dll:** " + ((File.Exists("opus.dll") ? ("Yes") : ("No"))) + Environment.NewLine +
                "**Lib Sodium dll:** " + ((File.Exists("libsodium.dll") ? ("Yes") : ("No"))) + Environment.NewLine +
                "**Ffmpeg:** " + ((File.Exists("ffmpeg.exe") ? ("Yes") : ("No"))) + Environment.NewLine +
                "**YouTube API key:** " + ((p.youtubeService != null ? ("Yes") : ("No"))));
            if (File.Exists("opus.dll") && File.Exists("libsodium.dll") && File.Exists("ffmpeg.exe") && p.youtubeService != null)
                yes++;
            else
                no++;
            embed.AddField("Game Module",
                "**Shiritori words file:** " + ((File.Exists("Saves/shiritoriWords.dat") ? ("Yes") : ("No"))) + Environment.NewLine +
                "**Booru guess tags file:** " + ((File.Exists("Saves/BooruTriviaTags.dat") ? ("Yes") : ("No"))) + Environment.NewLine +
                "**Anime guess animes file:** " + ((File.Exists("Saves/AnimeTags.dat") ? ("Yes") : ("No"))));
            if (File.Exists("Saves/shiritoriWords.dat"))
                yes++;
            else
                no++;
            if (File.Exists("Saves/BooruTriviaTags.dat"))
                yes++;
            else
                no++;
            if (File.Exists("Saves/AnimeTags.dat"))
                yes++;
            else
                no++;
            if (p.malClient != null)
            {
                embed.AddField("Anime/Manga module", "**MyAnimeList API key:** Yes");
                yes++;
            }
            else
            {
                embed.AddField("Anime/Manga module", "**MyAnimeList API key:** No");
                no++;
            }
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
            embed.AddField("YouTube Module", "**YouTube API key:** " + ((p.youtubeService != null) ? ("Yes") : ("No")));
            if (p.youtubeService != null)
                yes++;
            else
                no++;
            embed.AddField("Google Shortener Module", "**Google Shortener API key:** " + ((p.service != null) ? ("Yes") : ("No")));
            if (p.service != null)
                yes++;
            else
                no++;
            embed.Color = new Color(no * 255 / 8, yes * 255 / 8, 0);
            await ReplyAsync("", false, embed.Build());
        }

        [Command("Reload language"), Summary("Reload the language files")]
        public async Task ReloadLanguage()
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
            {
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild.Id));
            }
            else
            {
                p.UpdateLanguageFiles();
                await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
            }
        }

        [Command("Leave server"), Summary("Leave the server")]
        public async Task Leave(string serverName = null)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
            {
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild.Id));
            }
            else
            {
                if (serverName == null)
                    await Context.Guild.LeaveAsync();
                else
                {
                    IGuild g = p.client.Guilds.ToList().Find(x => x.Name.ToUpper() == serverName.ToUpper());
                    if (g == null)
                        await ReplyAsync(Base.Sentences.NoCorrespondingGuild(Context.Guild.Id));
                    else
                    {
                        await g.LeaveAsync();
                        await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
                    }
                }
            }
        }

        [Command("Exit"), Summary("Exit the program")]
        public async Task Exit(string serverName = null)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild.Id));
            else
                Environment.Exit(0);
        }
    }
}