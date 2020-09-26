using System.Collections.Generic;
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
            LoadDoujinshiHelp();
            LoadFunHelp();
            LoadGameHelp();
            LoadInformationHelp();
            LoadJapaneseHelp();
            LoadLanguageHelp();
            LoadMediaHelp();
            LoadRadioHelp();
            LoadScienceHelp();
            LoadSettingHelp();
        }
#pragma warning restore CS0649

        public List<(string, Help)> GetHelp(bool isNsfw, bool isAdmin, bool isOwner)
        {
            var help = _help;
            if (!isNsfw)
                help = help.Where(x => !x.Item2.Restriction.HasFlag(Restriction.Nsfw)).ToList();
            if (!isAdmin)
                help = help.Where(x => !x.Item2.Restriction.HasFlag(Restriction.AdminOnly)).ToList();
            if (!isOwner)
                help = help.Where(x => !x.Item2.Restriction.HasFlag(Restriction.ServerOwnerOnly)).ToList();
            return help;
        }

        public string GetSubmoduleHelp(string name)
            => _submoduleHelp[name];

        private List<(string, Help)> _help; // Module name, associate all help with it
        private Dictionary<string, string> _submoduleHelp;
    }
}
