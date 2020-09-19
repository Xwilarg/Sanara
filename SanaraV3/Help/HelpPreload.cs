using System.Collections.Generic;

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

        public List<(string, Help)> GetHelp()
            => _help;

        public string GetSubmoduleHelp(string name)
            => _submoduleHelp[name];

        private List<(string, Help)> _help; // Module name, associate all help with it
        private Dictionary<string, string> _submoduleHelp;
    }
}
