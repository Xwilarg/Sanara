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
using SanaraV2.Modules.Base;
using System;

namespace SanaraV2.Modules.Tools
{
    public static class Sentences
    {
        /// --------------------------- Communication ---------------------------
        public static string UserNotExist(ulong guildId) { return (Translation.GetTranslation(guildId, "userNotExist")); }
        public static string Username(ulong guildId) { return (Translation.GetTranslation(guildId, "username")); }
        public static string Nickname(ulong guildId) { return (Translation.GetTranslation(guildId, "nickname")); }
        public static string AccountCreation(ulong guildId) { return (Translation.GetTranslation(guildId, "accountCreation")); }
        public static string GuildJoined(ulong guildId) { return (Translation.GetTranslation(guildId, "guildJoined")); }
        public static string Creator(ulong guildId) { return (Translation.GetTranslation(guildId, "creator")); }
        public static string Uptime(ulong guildId) { return (Translation.GetTranslation(guildId, "uptime")); }
        public static string Website(ulong guildId) { return (Translation.GetTranslation(guildId, "website")); }
        public static string OfficialGuild(ulong guildId) { return (Translation.GetTranslation(guildId, "officialGuild")); }
        public static string Roles(ulong guildId) { return (Translation.GetTranslation(guildId, "roles")); }
        public static string NoRole(ulong guildId) { return (Translation.GetTranslation(guildId, "noRole")); }
        public static string LatestVersion(ulong guildId) { return (Translation.GetTranslation(guildId, "latestVersion")); }
        public static string NumberGuilds(ulong guildId) { return (Translation.GetTranslation(guildId, "numberGuilds")); }
        public static string QuoteInvalidId(ulong guildId) { return (Translation.GetTranslation(guildId, "quoteInvalidId")); }
        public static string QuoteNoMessage(ulong guildId) { return (Translation.GetTranslation(guildId, "quoteNoMessage")); }
        public static string Enabled(ulong guildId) { return (Translation.GetTranslation(guildId, "enabled")); }
        public static string Disabled(ulong guildId) { return (Translation.GetTranslation(guildId, "disabled")); }
        public static string EvalHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "evalHelp")); }
        public static string EvalError(ulong guildId, string msg) { return (Translation.GetTranslation(guildId, "evalError", msg)); }
        public static string InvitationLink(ulong guildId) { return (Translation.GetTranslation(guildId, "invitationLink")); }
        public static string ProfilePicture(ulong guildId) { return (Translation.GetTranslation(guildId, "profilePicture")); }
        public static string PollHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "pollHelp")); }
        public static string PollTooManyChoices(ulong guildId) { return (Translation.GetTranslation(guildId, "pollTooManyChoices")); }
        public static string InvalidCalc(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidCalc")); }
        public static string CompleteHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "completeHelp")); }
        public static string CompleteWait(ulong guildId) { return (Translation.GetTranslation(guildId, "completeWait")); }

        /// --------------------------- Information ---------------------------
        public static string DataSaved(ulong guildId, string about) { return (Translation.GetTranslation(guildId, "dataSaved", about)); }
        public static string ServicesAvailability(ulong guildId) { return (Translation.GetTranslation(guildId, "servicesAvailability")); }
        public static string TranslationsAvailability(ulong guildId) { return (Translation.GetTranslation(guildId, "translationsAvailability")); }
        public static string LatestChanges(ulong guildId) { return (Translation.GetTranslation(guildId, "latestChanges")); }
        public static string ByStr(ulong guildId) { return (Translation.GetTranslation(guildId, "by")); }
        public static string ErrorHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "errorHelp")); }
        public static string ErrorNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "errorNotFound")); }
        public static string ErrorGdpr(ulong guildId, string link) { return (Translation.GetTranslation(guildId, "errorGdpr", link)); }
        public static string ErrorDetails(ulong guildId, string error) { return (Translation.GetTranslation(guildId, "errorDetails", error)); }
        public static string Command(ulong guildId) { return (Translation.GetTranslation(guildId, "command")); }
        public static string Date(ulong guildId) { return (Translation.GetTranslation(guildId, "date")); }
        public static string CantPM(ulong guildId) { return (Translation.GetTranslation(guildId, "cantPM")); }

        /// --------------------------- Code ---------------------------
        public static string ShellNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "shellNotFound")); }
        public static string ShellHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "shellHelp")); }
        public static string IncreaseHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "increaseHelp")); }

        /// --------------------------- Image ---------------------------
        public static string InvalidColor(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidColor")); }
        public static string HelpColor(ulong guildId) { return (Translation.GetTranslation(guildId, "helpColor")); }
        public static string Rgb(ulong guildId) { return (Translation.GetTranslation(guildId, "rgb")); }
        public static string Hex(ulong guildId) { return (Translation.GetTranslation(guildId, "hex")); }

        /// --------------------------- Linguist ---------------------------
        public static string JapaneseHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "japaneseHelp")); }
        public static string KanjiHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "kanjiHelp")); }
        public static string TranslateHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "translateHelp")); }
        public static string InvalidLanguage(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidLanguage")); }
        public static string NoJapaneseTranslation(ulong guildId) { return (Translation.GetTranslation(guildId, "noJapaneseTranslation")); }
        public static string NoTextOnImage(ulong guildId) { return (Translation.GetTranslation(guildId, "noTextOnImage")); }
        public static string NotAnImage(ulong guildId) { return (Translation.GetTranslation(guildId, "notAnImage")); }
        public static string UrbanHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "urbanHelp")); }
        public static string UrbanNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "urbanNotFound")); }
        public static string Definition(ulong guildId) { return (Translation.GetTranslation(guildId, "definition")); }
        public static string Example(ulong guildId) { return (Translation.GetTranslation(guildId, "example")); }
        public static string Radical(ulong guildId) { return (Translation.GetTranslation(guildId, "radical")); }
        public static string Parts(ulong guildId) { return (Translation.GetTranslation(guildId, "parts")); }

        /// --------------------------- Settings ---------------------------
        public static string NeedLanguage(ulong guildId) { return (Translation.GetTranslation(guildId, "needLanguage")); }
        public static string PrefixRemoved(ulong guildId) { return (Translation.GetTranslation(guildId, "prefixRemoved")); }
        public static string ModuleManagementHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "moduleManagementHelp")); }
        public static string ModuleManagementInvalid(ulong guildId) { return (Translation.GetTranslation(guildId, "moduleManagementInvalid")); }
        public static string ModuleEnabled(ulong guildId, string moduleName) { return (Translation.GetTranslation(guildId, "moduleEnabled", moduleName)); }
        public static string ModuleDisabled(ulong guildId, string moduleName) { return (Translation.GetTranslation(guildId, "moduleDisabled", moduleName)); }
        public static string ModuleAlreadyEnabled(ulong guildId, string moduleName) { return (Translation.GetTranslation(guildId, "moduleAlreadyEnabled", moduleName)); }
        public static string ModuleAlreadyDisabled(ulong guildId, string moduleName) { return (Translation.GetTranslation(guildId, "moduleAlreadyDisabled", moduleName)); }
        public static string AllModulesEnabled(ulong guildId) { return (Translation.GetTranslation(guildId, "allModulesEnabled")); }
        public static string AllModulesDisabled(ulong guildId) { return (Translation.GetTranslation(guildId, "allModulesDisabled")); }
        public static string AllModulesAlreadyEnabled(ulong guildId) { return (Translation.GetTranslation(guildId, "allModulesAlreadyEnabled")); }
        public static string AllModulesAlreadyDisabled(ulong guildId) { return (Translation.GetTranslation(guildId, "allModulesAlreadyDisabled")); }
        public static string AnonymizeCurrentTrue(ulong guildId) { return (Translation.GetTranslation(guildId, "anonymizeCurrentTrue")); }
        public static string AnonymizeCurrentFalse(ulong guildId) { return (Translation.GetTranslation(guildId, "anonymizeCurrentFalse")); }
        public static string AnonymizeHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "anonymizeHelp")); }

        /// --------------------------- Help ---------------------------
        public static string NoCommandAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "noCommandAvailable")); }
        public static string HelpHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "helpHelp")); }
        public static string Help(ulong guildId) { return (Translation.GetTranslation(guildId, "help")); }

        /// --------------------------- Help Module Name ---------------------------
        public static string AnimeMangaModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "animeMangaModuleName")); }
        public static string ArknightsModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "arknightsModuleName")); }
        public static string BooruModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "booruModuleName")); }
        public static string CodeModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "codeModuleName")); }
        public static string CommunicationModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "communicationModuleName")); }
        public static string CommunityModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "communityModuleName")); }
        public static string DoujinshiModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "doujinshiModuleName")); }
        public static string GameModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "gameModuleName")); }
        public static string ImageModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "imageModuleName")); }
        public static string InformationModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "informationModuleName")); }
        public static string KantaiCollectionModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "kantaiCollectionModuleName")); }
        public static string LinguisticModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "linguisticModuleName")); }
        public static string RadioModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "radioModuleName")); }
        public static string SettingsModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "settingsModuleName")); }
        public static string VisualNovelModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "visualNovelModuleName")); }
        public static string XkcdModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "xkcdModuleName")); }
        public static string YoutubeModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "youtubeModuleName")); }
        /// --------------------------- Help Module Content ---------------------------
        public static string ArknightsHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Arknights))
                return Translation.GetTranslation(guildId, "arknightsModuleCharac");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string AnimeMangaHelp(ulong guildId, bool isServerOwner)
        {
            string res;
            if (Program.p.db.IsAvailable(guildId, Program.Module.AnimeManga))
            {
                res = Translation.GetTranslation(guildId, "animeMangaModuleAnime") + Environment.NewLine + Translation.GetTranslation(guildId, "animeMangaModuleManga") + Environment.NewLine + Translation.GetTranslation(guildId, "animeMangaModuleLN");
                if (isServerOwner)
                    res += Environment.NewLine + Translation.GetTranslation(guildId, "animeMangaModuleSubscribe") + Environment.NewLine + Translation.GetTranslation(guildId, "animeMangaModuleUnsubscribe");
                else
                    res += Environment.NewLine + "*" + Translation.GetTranslation(guildId, "ownerForFull") + "*";
                return res;
            }
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string BooruHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Booru))
                return Translation.GetTranslation(guildId, "booruModuleSource") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleSafebooru") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleE926")
                + ((isChanNsfw) ? (Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleGelbooru") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleKonachan")
                + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleRule34") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleE621")) : (""))
                + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleTags")
                + (isChanNsfw ? "" : Environment.NewLine + "*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string CodeHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
                return Translation.GetTranslation(guildId, "codeModuleShell") + Environment.NewLine + Translation.GetTranslation(guildId, "codeModuleColor") + Environment.NewLine + Translation.GetTranslation(guildId, "codeModuleIncrease");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string CommunicationHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
            {
                string str = Translation.GetTranslation(guildId, "communicationModuleInfos") + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleBotInfos")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleQuote") + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModulePoll")
                     + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleCalc") + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleComplete");
                return str;
            }
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string CommunityHelp(ulong guildId, bool isServerOwner)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Community))
            {
                string str = Translation.GetTranslation(guildId, "communityModuleGet") + Environment.NewLine + Translation.GetTranslation(guildId, "communityModuleDescription")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "communityModuleColor") + Environment.NewLine + Translation.GetTranslation(guildId, "communityModuleVisibility")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "communityModuleFriend") + Environment.NewLine + Translation.GetTranslation(guildId, "communityModuleUnfriend");
                if (isServerOwner)
                    str += Environment.NewLine + Translation.GetTranslation(guildId, "communityModuleSaveAll");
                return str;
            }
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string DoujinshiHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Doujinshi))
            {
                if (isChanNsfw)
                    return Translation.GetTranslation(guildId, "doujinshiModuleDoujinshi") + Environment.NewLine + Translation.GetTranslation(guildId, "doujinshiModuleCosplay")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "doujinshiModuleDownloadDoujinshi") + Environment.NewLine + Translation.GetTranslation(guildId, "doujinshiModuleDownloadCosplay")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "doujinshiModuleAdultVideo") + Environment.NewLine + Translation.GetTranslation(guildId, "doujinshiModuleSubscribe")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "doujinshiModuleUnsubscribe") + Environment.NewLine + Environment.NewLine + Translation.GetTranslation(guildId, "blacklistExplanationsIntro")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "blacklistExplanations") + Environment.NewLine + Translation.GetTranslation(guildId, "blacklistExplanations2")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "blacklistExplanations3") + Environment.NewLine + Translation.GetTranslation(guildId, "blacklistExplanations4");
                else
                    return NoCommandAvailable(guildId) + Environment.NewLine + "*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*";
            }
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string GameHelp(ulong guildId, bool isChanNsfw)
        {
            return Games.GameModule.DisplayHelp(guildId, isChanNsfw);
        }
        public static string InformationHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Information))
                return Translation.GetTranslation(guildId, "informationModuleHelp") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleGdpr")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleStatus") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleInvite")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleLogs") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleError");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string KantaiCollectionHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Kancolle))
                return Translation.GetTranslation(guildId, "kantaiCollectionModuleCharac");// + Environment.NewLine + Translation.GetTranslation(guildId, "kantaiCollectionModuleDrop");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string LinguisticHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Linguistic))
                return Translation.GetTranslation(guildId, "linguisticModuleJapanese") + Environment.NewLine + Translation.GetTranslation(guildId, "linguisticModuleKanji")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "linguisticModuleTranslation")
                 + Environment.NewLine + ((isChanNsfw) ? (Translation.GetTranslation(guildId, "linguisticModuleUrban")) : ("*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*"));
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string RadioHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Radio))
                return Translation.GetTranslation(guildId, "radioModuleLaunch") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleAdd")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "radioModulePlaylist") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleSkip")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleRemove") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleStop");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string SettingsHelp(ulong guildId, bool isServerOwner, bool isBotOwner)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Settings))
            {
                string finalStr = "";
                if (isServerOwner)
                    finalStr = Translation.GetTranslation(guildId, "settingsModuleLanguage") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModulePrefix")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleEnable") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleDisable")
                         + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleAnonymize");
                if (isBotOwner)
                {
                    if (finalStr != "")
                        finalStr += Environment.NewLine;
                    finalStr += Translation.GetTranslation(guildId, "settingsModuleReload")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleExit") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleResetDb")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleEval");
                }
                if (finalStr == "")
                    return NoCommandAvailable(guildId) + Environment.NewLine + "*" + Translation.GetTranslation(guildId, "ownerForFull") + "*"; ;
                return finalStr;
            }
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string VisualNovelHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Vn))
                return Translation.GetTranslation(guildId, "visualNovelModuleVn");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string XkcdHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Xkcd))
                return Translation.GetTranslation(guildId, "xkcdModuleXkcd");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string YouTubeHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Youtube))
                return Translation.GetTranslation(guildId, "youtubeModuleYoutube");
            return Base.Sentences.NotAvailable(guildId);
        }
    }
}