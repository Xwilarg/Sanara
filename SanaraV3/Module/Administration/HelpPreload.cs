using System.Collections.Generic;

namespace SanaraV3.Module.Administration
{
    public sealed partial class HelpPreload
    {
#pragma warning disable CS0649 // Because _help is in fact assigned in the other partial class
        public HelpPreload()
        {
            _help = new List<Help>();
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

        public List<Help> GetHelp()
            => _help;

        private List<Help> _help;
    }
}
