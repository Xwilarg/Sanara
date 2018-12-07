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
using SanaraV2.Modules.Base;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Settings : ModuleBase
    {
        Program p = Program.p;

        [Command("Language"), Summary("Set the language of the bot for this server")]
        public async Task SetLanguage(params string[] language)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
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
                    p.db.SetLanguage(Context.Guild.Id, lang);
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Prefix"), Summary("Set the prefix of the bot for this server")]
        public async Task SetPrefix(params string[] command)
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Context.Guild.OwnerId)
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild.Id, Context.Guild.OwnerId));
            }
            else
            {
                if (command.Length == 0)
                {
                    p.db.SetPrefix(Context.Guild.Id, "");
                    await ReplyAsync(Sentences.PrefixRemoved(Context.Guild.Id));
                }
                else
                {
                    p.db.SetPrefix(Context.Guild.Id, Utilities.AddArgs(command));
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild.Id));
                }
            }
        }

        [Command("Reload language"), Summary("Reload the language files")]
        public async Task ReloadLanguage()
        {
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
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
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
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
            await p.DoAction(Context.User, Context.Guild.Id, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild.Id));
            else
                Environment.Exit(0);
        }
    }
}