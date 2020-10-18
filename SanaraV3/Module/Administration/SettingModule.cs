using Discord;
using Discord.Commands;
using SanaraV3.Attribute;
using SanaraV3.Exception;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
        public void LoadSettingHelp()
        {
            _submoduleHelp.Add("Setting", "Modify the bot behavior for your server");
            _help.Add(("Administration", new Help("Setting", "Prefix", new[] { new Argument(ArgumentType.OPTIONAL, "prefix") }, "Change the bot prefix. Is no information is provided, display the current one.", new string[0], Restriction.AdminOnly, "Prefix s.")));
            _help.Add(("Administration", new Help("Setting", "Anonymize", new[] { new Argument(ArgumentType.OPTIONAL, "value") }, "Set if your guild name can be displayed on Sanara stats page. Is no information is provided, display the current value.", new string[0], Restriction.AdminOnly, "Anonymize true")));
            _help.Add(("Administration", new Help("Setting", "Flag translation", new[] { new Argument(ArgumentType.OPTIONAL, "value") }, "Set if your guild can use the translate command by reacting with flags on messages. Is no information is provided, display the current value.", new string[0], Restriction.AdminOnly, "Flag translation true")));
            _help.Add(("Administration", new Help("Setting", "Leave", new[] { new Argument(ArgumentType.MANDATORY, "server name") }, "Leave the server given in parameter.", new string[0], Restriction.OwnerOnly, null)));
            _help.Add(("Administration", new Help("Setting", "Exit", new Argument[0], "Stop the bot executable.", new string[0], Restriction.OwnerOnly, null)));
            _help.Add(("Administration", new Help("Setting", "Disable", new[] { new Argument(ArgumentType.MANDATORY, "module name") }, "Disable a module for this server.", new string[0], Restriction.AdminOnly, "Disable nsfw")));
            _help.Add(("Administration", new Help("Setting", "Enable", new[] { new Argument(ArgumentType.MANDATORY, "module name") }, "Enable a module for this server.", new string[0], Restriction.AdminOnly, "Enable media")));
        }
    }
}

namespace SanaraV3.Module.Administration
{
    public class SettingModule : ModuleBase
    {
        [Command("Enable"), RequireAdmin]
        public async Task EnableAsync(string moduleName)
        {
            moduleName = moduleName.ToLower();
            if (!StaticObjects.Help.IsModuleNameValid(moduleName))
                throw new CommandFailed("This module doesn't exist.");
            foreach (var m in StaticObjects.Help.GetSubmodulesFromModule(moduleName))
                await StaticObjects.Db.AddAvailabilityAsync(Context.Guild.Id, m);
            await ReplyAsync("Your availability was updated.");
        }

        [Command("Disable"), RequireAdmin]
        public async Task DisableAsync(string moduleName)
        {
            moduleName = moduleName.ToLower();
            if (!StaticObjects.Help.IsModuleNameValid(moduleName))
                throw new CommandFailed("This module doesn't exist.");
            foreach (var m in StaticObjects.Help.GetSubmodulesFromModule(moduleName))
                await StaticObjects.Db.RemoveAvailabilityAsync(Context.Guild.Id, m);
            await ReplyAsync("Your availability was updated.");
        }

        [Command("Leave"), RequireOwner]
        public async Task LeaveAsync([Remainder]string guildName)
        {
            IGuild g = StaticObjects.Client.Guilds.ToList().Find(x => x.Name.ToUpper() == guildName.ToUpper() || x.Id.ToString() == guildName);
            if (g == null)
                throw new CommandFailed("I don't know any server with this name.");
            await g.LeaveAsync();
            await ReplyAsync("Done");
        }

        [Command("Exit"), RequireOwner]
        public async Task ExitAsync()
        {
            await ReplyAsync("Done");
            Environment.Exit(1);
        }

        [Command("Prefix"), RequireAdmin]
        public async Task Prefix()
        {
            await ReplyAsync("Your current prefix is " + StaticObjects.Db.GetGuild(Context.Guild.Id).Prefix);
        }

        [Command("Prefix"), RequireAdmin]
        public async Task Prefix(string prefix)
        {
            await StaticObjects.Db.UpdatePrefixAsync(Context.Guild.Id, prefix);
            await ReplyAsync("Your prefix was updated to " + prefix);
        }

        [Command("Anonymize"), RequireAdmin]
        public async Task Anonymize()
        {
            await ReplyAsync("Your current anonymize setting is set to " + StaticObjects.Db.GetGuild(Context.Guild.Id).Anonymize.ToString().ToLower());
        }

        [Command("Anonymize"), RequireAdmin]
        public async Task Anonymize(bool value)
        {
            await StaticObjects.Db.UpdateAnonymizeAsync(Context.Guild.Id, value);
            await ReplyAsync("Your anonymize setting was updated to " + value.ToString().ToLower());
        }

        [Command("Flag translation"), RequireAdmin]
        public async Task FlagTranslation()
        {
            await ReplyAsync("Your current flag translation setting is set to " + StaticObjects.Db.GetGuild(Context.Guild.Id).TranslateUsingFlags.ToString().ToLower());
        }

        [Command("Flag translation"), RequireAdmin]
        public async Task FlagTranslation(bool value)
        {
            await StaticObjects.Db.UpdateFlagAsync(Context.Guild.Id, value);
            await ReplyAsync("Your translation flag setting was updated to " + value.ToString().ToLower());
        }
    }
}
