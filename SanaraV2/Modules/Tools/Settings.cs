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
using DynamicExpresso;
using SanaraV2.Modules.Base;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV2.Modules.Tools
{
    public class Settings : ModuleBase
    {
        Program p = Program.p;

        /// <summary>
        /// Only guild owner and users that have "Manage Guild" permission can modify bot settings for the guild
        /// </summary>
        public static bool CanModify(IUser user, IGuild guild)
        {
            if (guild == null)
                return true;
            if (user.Id == guild.OwnerId)
                return true;
            IGuildUser guildUser = (IGuildUser)user;
            return guildUser.GuildPermissions.ManageGuild;
        }

        [Command("Eval"), Summary("Allow to check the value of some variables in runtime")]
        public async Task EvalFct(params string[] args)
        {
            await p.DoAction(Context.User, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild));
            else if (args.Length == 0)
                await ReplyAsync(Sentences.EvalHelp(Context.Guild));
            else
            {
                Interpreter interpreter = new Interpreter()
                    .SetVariable("Context", Context)
                    .SetVariable("p", Program.p)
                    .EnableReflection();
                try
                {
                    await ReplyAsync(interpreter.Eval(string.Join(" ", args)).ToString());
                }
                catch (Exception e)
                {
                    await ReplyAsync(Sentences.EvalError(Context.Guild, e.Message));
                }
            }
        }

        [Command("Language"), Summary("Set the language of the bot for this server")]
        public async Task SetLanguage(params string[] language)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Base.Sentences.CommandDontPm(Context.Guild));
                return;
            }
            await p.DoAction(Context.User, Program.Module.Settings);
            if (!CanModify(Context.User, Context.Guild))
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild, Context.Guild.OwnerId));
            }
            else if (language.Length == 0)
                await ReplyAsync(Sentences.NeedLanguage(Context.Guild));
            else
            {
                string nextLanguage = Utilities.AddArgs(language);
                string lang = Utilities.GetLanguage(nextLanguage);
                if (lang == null)
                    await ReplyAsync(Sentences.InvalidLanguage(Context.Guild));
                else
                {
                    await p.db.SetLanguage(Context.Guild.Id, lang);
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
                }
            }
        }

        [Command("Prefix"), Summary("Set the prefix of the bot for this server")]
        public async Task SetPrefix(params string[] command)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Base.Sentences.CommandDontPm(Context.Guild));
                return;
            }
            await p.DoAction(Context.User, Program.Module.Settings);
            if (!CanModify(Context.User, Context.Guild))
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild, Context.Guild.OwnerId));
            }
            else
            {
                if (command.Length == 0)
                {
                    await p.db.SetPrefix(Context.Guild.Id, "");
                    await ReplyAsync(Sentences.PrefixRemoved(Context.Guild));
                }
                else
                {
                    await p.db.SetPrefix(Context.Guild.Id, Utilities.AddArgs(command));
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
                }
            }
        }

        [Command("Reload language"), Summary("Reload the language files")]
        public async Task ReloadLanguage()
        {
            await p.DoAction(Context.User, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
            {
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild));
            }
            else
            {
                p.UpdateLanguageFiles();
                await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
            }
        }

        [Command("Exit"), Summary("Leave the server")]
        public async Task Exit(params string[] args)
        {
            await p.DoAction(Context.User, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
            {
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild));
            }
            else
            {
                if (args.Length == 0)
                {
                    if (Context.Guild == null)
                        await ReplyAsync("I can't leave a private conversation.");
                    else
                        await Context.Guild.LeaveAsync();
                }
                else
                {
                    string serverName = string.Join(" ", args);
                    IGuild g = p.client.Guilds.ToList().Find(x => x.Name.ToUpper() == serverName.ToUpper() || x.Id.ToString() == serverName);
                    if (g == null)
                        await ReplyAsync(Base.Sentences.NoCorrespondingGuild(Context.Guild));
                    else
                    {
                        await g.LeaveAsync();
                        await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
                    }
                }
            }
        }

        [Command("ResetDb")]
        public async Task ResetDb(params string[] args)
        {
            await p.DoAction(Context.User, Program.Module.Settings);
            if (Context.User.Id != Base.Sentences.ownerId)
            {
                await ReplyAsync(Base.Sentences.OnlyMasterStr(Context.Guild));
            }
            else
            {
                if (args.Length == 0)
                {
                    await Program.p.db.ResetGuild(Context.Guild.Id);
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
                }
                else
                {
                    string serverName = string.Join(" ", args);
                    IGuild g = p.client.Guilds.ToList().Find(x => x.Name.ToUpper() == serverName.ToUpper() || x.Id.ToString() == serverName);
                    if (g == null)
                        await ReplyAsync(Base.Sentences.NoCorrespondingGuild(Context.Guild));
                    else
                    {
                        await Program.p.db.ResetGuild(g.Id);
                        await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
                    }
                }
            }
        }

        [Command("Anonymize")]
        public async Task Anonymize(params string[] args)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Base.Sentences.CommandDontPm(Context.Guild));
                return;
            }
            await p.DoAction(Context.User, Program.Module.Settings);
            if (!CanModify(Context.User, Context.Guild))
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild, Context.Guild.OwnerId));
            }
            if (args.Length == 0)
            {
                if (Program.p.db.IsAnonymized(Context.Guild.Id))
                    await ReplyAsync(Sentences.AnonymizeCurrentTrue(Context.Guild));
                else
                    await ReplyAsync(Sentences.AnonymizeCurrentFalse(Context.Guild));
            }
            else
            {
                if (bool.TryParse(string.Join(" ", args), out bool value))
                {
                    await Games.GameModule.Anonymize(Context.Guild.Id, value);
                    await ReplyAsync(Base.Sentences.DoneStr(Context.Guild));
                }
                else
                {
                    await ReplyAsync(Sentences.AnonymizeHelp(Context.Guild));
                }
            }
        }

        [Command("Enable"), Summary("Enable a module")]
        public async Task Enable(params string[] args)
            => await ManageModule(Context.Channel, args, 1);

        [Command("Disable"), Summary("Disable a module")]
        public async Task Disable(params string[] args)
            => await ManageModule(Context.Channel, args, 0);

        private async Task ManageModule(IMessageChannel chan, string[] args, int enable)
        {
            if (Context.Guild == null)
            {
                await ReplyAsync(Base.Sentences.CommandDontPm(Context.Guild));
                return;
            }
            await p.DoAction(Context.User, Program.Module.Settings);
            if (!CanModify(Context.User, Context.Guild))
            {
                await ReplyAsync(Base.Sentences.OnlyOwnerStr(Context.Guild, Context.Guild.OwnerId));
                return;
            }
            if (args.Length == 0)
            {
                await chan.SendMessageAsync(Sentences.ModuleManagementHelp(Context.Guild) + " " + GetModuleList());
                return;
            }
            Program.Module? module = null;
            string name = Utilities.AddArgs(args).Replace(" ", "");
            if (name == "all") // Enable/Disable all modules at once
            {
                if (enable == 1 && Program.p.db.AreAllAvailable(Context.Guild.Id))
                    await chan.SendMessageAsync(Sentences.AllModulesAlreadyEnabled(Context.Guild));
                else if (enable == 0 && Program.p.db.AreNoneAvailable(Context.Guild.Id))
                    await chan.SendMessageAsync(Sentences.AllModulesAlreadyDisabled(Context.Guild));
                else
                {
                    for (Program.Module i = 0; i <= Program.Module.Youtube; i++)
                    {
                        // We can't disable Settings and Information module
                        // We however can enable them, just in case
                        if (enable == 0 && (i == Program.Module.Settings || i == Program.Module.Information))
                           continue;
                        await Program.p.db.SetAvailability(Context.Guild.Id, i, enable);
                    }
                    if (enable == 1)
                        await chan.SendMessageAsync(Sentences.AllModulesEnabled(Context.Guild));
                    else
                        await chan.SendMessageAsync(Sentences.AllModulesDisabled(Context.Guild));
                }
                return;
            }
            for (Program.Module i = 0; i <= Enum.GetValues(typeof(Program.Module)).Cast<Program.Module>().Max(); i++)
            {
                if (i == Program.Module.Settings || i == Program.Module.Information)
                    continue;
                if (i.ToString().ToLower() == name.ToLower())
                {
                    module = i;
                    break;
                }
            }
            if (module == null)
            {
                await chan.SendMessageAsync(Sentences.ModuleManagementInvalid(Context.Guild) + " " + GetModuleList());
                return;
            }
            bool availability = Program.p.db.IsAvailable(Context.Guild.Id, module.Value);
            if (availability && enable == 1)
                await chan.SendMessageAsync(Sentences.ModuleAlreadyEnabled(Context.Guild, module.ToString()));
            else if (!availability && enable == 0)
                await chan.SendMessageAsync(Sentences.ModuleAlreadyDisabled(Context.Guild, module.ToString()));
            else
            {
                await Program.p.db.SetAvailability(Context.Guild.Id, module.Value, enable);
                if (enable == 1)
                    await chan.SendMessageAsync(Sentences.ModuleEnabled(Context.Guild, module.ToString()));
                else
                    await chan.SendMessageAsync(Sentences.ModuleDisabled(Context.Guild, module.ToString()));
            }
        }

        private string GetModuleList()
        {
            string finalStr = ((Program.Module)0).ToString();
            for (Program.Module i = (Program.Module)1; i <= (Program.Module.Youtube - 1); i++)
            {
                if (i == Program.Module.Settings || i == Program.Module.Information)
                    continue;
                finalStr += ", " + i.ToString();
            }
            finalStr += " " + Base.Sentences.OrStr(Context.Guild) + " " + Program.Module.Youtube.ToString();
            return finalStr;
        }
    }
}