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

        private void copyContent(string source, string destination)
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
                copyContent(d2, destination + "/" + d2.Split('/')[d2.Split('/').Length - 1]);
            }
        }

        [Command("Archive", RunMode = RunMode.Async), Summary("Create an archive for all datas")]
        public async Task archive()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != 144851584478740481)
            {
                await ReplyAsync(Sentences.onlyMasterStr(Context.Guild.Id));
            }
            else
            {
                await ReplyAsync(Sentences.copyingFiles(Context.Guild.Id));
                if (!Directory.Exists("Archives"))
                    Directory.CreateDirectory("Archives");
                string currTime = DateTime.UtcNow.ToString("yy-MM-dd-HH-mm-ss");
                Directory.CreateDirectory("Archives/" + currTime);
                copyContent("Saves", "Archives/" + currTime);
                await ReplyAsync(Sentences.createArchiveStr(Context.Guild.Id, currTime));
            }
        }

        [Command("Language"), Summary("Set the language of the bot for this server")]
        public async Task setLanguage(params string[] language)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Sentences.onlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else if (language.Length == 0)
                await ReplyAsync(Sentences.needLanguage(Context.Guild.Id));
            else
            {
                string nextLanguage = Program.addArgs(language);
                string lang = null;
                if (p.allLanguages.ContainsKey(nextLanguage))
                    lang = nextLanguage;
                foreach (var key in p.allLanguages)
                {
                    if (key.Value.Contains(nextLanguage))
                    {
                        lang = key.Key;
                        break;
                    }
                }
                if (lang == null)
                    await ReplyAsync(Sentences.needLanguage(Context.Guild.Id));
                else
                {
                    p.guildLanguages[Context.Guild.Id] = lang;
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/language.dat", lang);
                    await ReplyAsync(Sentences.doneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Prefix"), Summary("Set the prefix of the bot for this server")]
        public async Task setPrefix(params string[] command)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Sentences.onlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else
            {
                if (command.Length == 0)
                {
                    p.prefixs[Context.Guild.Id] = "";
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/prefix.dat", "");
                    await ReplyAsync(Sentences.prefixRemoved(Context.Guild.Id));
                }
                else
                {
                    string prefix = Program.addArgs(command);
                    p.prefixs[Context.Guild.Id] = prefix;
                    File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/prefix.dat", prefix);
                    await ReplyAsync(Sentences.doneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Reload language"), Summary("Reload the language files")]
        public async Task reloadLanguage()
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.onlyMasterStr(Context.Guild.Id));
            }
            else
            {
                p.UpdateLanguageFiles();
                await ReplyAsync(Sentences.doneStr(Context.Guild.Id));
            }
        }

        [Command("Leave server"), Summary("Leave the server")]
        public async Task leave(string serverName = null)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.onlyMasterStr(Context.Guild.Id));
            }
            else
            {
                if (serverName == null)
                    await Context.Guild.LeaveAsync();
                else
                {
                    IGuild g = p.client.Guilds.ToList().Find(x => x.Name.ToUpper() == serverName.ToUpper());
                    if (g == null)
                        await ReplyAsync(Sentences.noCorrespondingGuild(Context.Guild.Id));
                    else
                    {
                        await g.LeaveAsync();
                        await ReplyAsync(Sentences.doneStr(Context.Guild.Id));
                    }
                }
            }
        }
    }
}