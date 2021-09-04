using Newtonsoft.Json;
using SanaraV3.Exception;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SanaraV3.Help
{
    public sealed partial class HelpPreload
    {
#pragma warning disable CS0649 // Because _help is in fact assigned in the other partial class
        public HelpPreload()
        {
            _help = new List<(string, Help)>();
            _submoduleHelp = new Dictionary<string, string>();
            LoadBooruHelp();
            LoadCommunicationHelp();
            LoadCosplayHelp();
            LoadDoujinHelp();
            LoadFunHelp();
            LoadGameHelp();
            LoadGameInfoHelp();
            LoadInformationHelp();
            LoadJapaneseHelp();
            LoadLanguageHelp();
            LoadMediaHelp();
            LoadRadioHelp();
            LoadScienceHelp();
            LoadSettingHelp();
            LoadVideoHelp();

#if !NSFW_BUILD
            _help.RemoveAll(x => (x.Item2.Restriction & Restriction.Nsfw) != 0);
#endif

            File.WriteAllText("Saves/Help.json", JsonConvert.SerializeObject(_help));
        }
#pragma warning restore CS0649

        public bool IsModuleNameValid(string name)
        {
            if (name == "administration" || name == "information" || name == "setting")
                throw new CommandFailed("You can change the availability of this module.");
            return IsModuleNameValidInternal(name);
        }

        public bool IsModuleNameValidInternal(string name)
            => _help.Any(x => x.Item1.ToLower() == name || x.Item2.SubmoduleName.ToLower() == name);

        public bool IsModuleAvailable(ulong guildId, string name)
        {
            // TODO: We can probably find a faster way to do that
            foreach (var h in _help.Where(x => name.StartsWith(x.Item2.CommandName.ToLower()) || x.Item2.Aliases.Any(y => name.StartsWith(y.ToLower()))))
            {
                if (!StaticObjects.Db.IsAvailable(guildId, h.Item2.SubmoduleName.ToLower()))
                    return false;
            }
            return true;
        }

        public string[] GetSubmodulesFromModule(string name)
        {
            List<string> names = new List<string>();
            foreach (var h in _help.Where(x => x.Item1.ToLower().StartsWith(name)))
            {
                if (!names.Contains(h.Item2.SubmoduleName.ToLower()))
                    names.Add(h.Item2.SubmoduleName.ToLower());
            }
            if (names.Count == 0)
                names.Add(name);
            return names.ToArray();
        }

        public List<(string, Help)> GetHelp(ulong guildId, bool isNsfw, bool isAdmin, bool isOwner)
        {
            var help = guildId == 0 ? _help : _help.Where(x => StaticObjects.Db.IsAvailable(guildId, x.Item1.ToLower()) && StaticObjects.Db.IsAvailable(guildId, x.Item2.SubmoduleName.ToLower()));
            if (!isNsfw)
                help = help.Where(x => !x.Item2.Restriction.HasFlag(Restriction.Nsfw));
            if (!isAdmin)
                help = help.Where(x => !x.Item2.Restriction.HasFlag(Restriction.AdminOnly));
            if (!isOwner)
                help = help.Where(x => !x.Item2.Restriction.HasFlag(Restriction.OwnerOnly));
            return help.ToList();
        }

        public string GetSubmoduleHelp(string name)
            => _submoduleHelp[name];

        private List<(string, Help)> _help; // Module name, associate all help with it
        private Dictionary<string, string> _submoduleHelp;
    }
}
