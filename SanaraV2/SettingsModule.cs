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
        public async Task setLanguage(string language = null)
        {
            p.doAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Sentences.ownerId)
            {
                await ReplyAsync(Sentences.onlyMasterStr(Context.Guild.Id));
            }
            else if (language != "en" && language != "fr")
                await ReplyAsync(Sentences.needLanguage);
            else
            {
                p.guildLanguages[Context.User.Id] = language;
                File.WriteAllText("Saves/Servers/" + Context.Guild.Id + "/language.dat", language);
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