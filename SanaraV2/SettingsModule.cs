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
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2
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
                await ReplyAsync(Sentences.OnlyMasterStr(Context.Guild.Id));
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
                await ReplyAsync(Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
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
                    await ReplyAsync(Sentences.DoneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Prefix"), Summary("Set the prefix of the bot for this server")]
        public async Task SetPrefix(params string[] command)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
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
                    await ReplyAsync(Sentences.DoneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Reload language"), Summary("Reload the language files")]
        public async Task ReloadLanguage()
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.OnlyMasterStr(Context.Guild.Id));
            }
            else
            {
                p.UpdateLanguageFiles();
                await ReplyAsync(Sentences.DoneStr(Context.Guild.Id));
            }
        }

        [Command("Leave server"), Summary("Leave the server")]
        public async Task Leave(string serverName = null)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.OnlyMasterStr(Context.Guild.Id));
            }
            else
            {
                if (serverName == null)
                    await Context.Guild.LeaveAsync();
                else
                {
                    IGuild g = p.client.Guilds.ToList().Find(x => x.Name.ToUpper() == serverName.ToUpper());
                    if (g == null)
                        await ReplyAsync(Sentences.NoCorrespondingGuild(Context.Guild.Id));
                    else
                    {
                        await g.LeaveAsync();
                        await ReplyAsync(Sentences.DoneStr(Context.Guild.Id));
                    }
                }
            }
        }

        [Command("Exit"), Summary("Exit the program")]
        public async Task Exit(string serverName = null)
        {
            p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Sentences.ownerId)
                await ReplyAsync(Sentences.OnlyMasterStr(Context.Guild.Id));
            else
                Environment.Exit(0);
        }
    }
}