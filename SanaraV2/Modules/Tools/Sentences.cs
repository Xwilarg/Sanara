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
using SanaraV2.Modules.Base;
using System;

namespace SanaraV2.Modules.Tools
{
    public static class Sentences
    {
        /// --------------------------- Communication ---------------------------
        public static string UserNotExist(IGuild guild) { return (Translation.GetTranslation(guild, "userNotExist")); }
        public static string Username(IGuild guild) { return (Translation.GetTranslation(guild, "username")); }
        public static string Nickname(IGuild guild) { return (Translation.GetTranslation(guild, "nickname")); }
        public static string AccountCreation(IGuild guild) { return (Translation.GetTranslation(guild, "accountCreation")); }
        public static string GuildJoined(IGuild guild) { return (Translation.GetTranslation(guild, "guildJoined")); }
        public static string Creator(IGuild guild) { return (Translation.GetTranslation(guild, "creator")); }
        public static string Uptime(IGuild guild) { return (Translation.GetTranslation(guild, "uptime")); }
        public static string Website(IGuild guild) { return (Translation.GetTranslation(guild, "website")); }
        public static string OfficialGuild(IGuild guild) { return (Translation.GetTranslation(guild, "officialGuild")); }
        public static string Roles(IGuild guild) { return (Translation.GetTranslation(guild, "roles")); }
        public static string NoRole(IGuild guild) { return (Translation.GetTranslation(guild, "noRole")); }
        public static string LatestVersion(IGuild guild) { return (Translation.GetTranslation(guild, "latestVersion")); }
        public static string NumberGuilds(IGuild guild) { return (Translation.GetTranslation(guild, "numberGuilds")); }
        public static string QuoteInvalidId(IGuild guild) { return (Translation.GetTranslation(guild, "quoteInvalidId")); }
        public static string QuoteNoMessage(IGuild guild) { return (Translation.GetTranslation(guild, "quoteNoMessage")); }
        public static string Enabled(IGuild guild) { return (Translation.GetTranslation(guild, "enabled")); }
        public static string Disabled(IGuild guild) { return (Translation.GetTranslation(guild, "disabled")); }
        public static string EvalHelp(IGuild guild) { return (Translation.GetTranslation(guild, "evalHelp")); }
        public static string EvalError(IGuild guild, string msg) { return (Translation.GetTranslation(guild, "evalError", msg)); }
        public static string InvitationLink(IGuild guild) { return (Translation.GetTranslation(guild, "invitationLink")); }
        public static string ProfilePicture(IGuild guild) { return (Translation.GetTranslation(guild, "profilePicture")); }
        public static string PollHelp(IGuild guild) { return (Translation.GetTranslation(guild, "pollHelp")); }
        public static string PollTooManyChoices(IGuild guild) { return (Translation.GetTranslation(guild, "pollTooManyChoices")); }
        public static string InvalidCalc(IGuild guild) { return (Translation.GetTranslation(guild, "invalidCalc")); }
        public static string CompleteHelp(IGuild guild) { return (Translation.GetTranslation(guild, "completeHelp")); }
        public static string CompleteWait(IGuild guild) { return (Translation.GetTranslation(guild, "completeWait")); }

        /// --------------------------- Information ---------------------------
        public static string DataSaved(IGuild guild, string about) { return (Translation.GetTranslation(guild, "dataSaved", about)); }
        public static string ServicesAvailability(IGuild guild) { return (Translation.GetTranslation(guild, "servicesAvailability")); }
        public static string TranslationsAvailability(IGuild guild) { return (Translation.GetTranslation(guild, "translationsAvailability")); }
        public static string LatestChanges(IGuild guild) { return (Translation.GetTranslation(guild, "latestChanges")); }
        public static string ByStr(IGuild guild) { return (Translation.GetTranslation(guild, "by")); }
        public static string ErrorHelp(IGuild guild) { return (Translation.GetTranslation(guild, "errorHelp")); }
        public static string ErrorNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "errorNotFound")); }
        public static string ErrorGdpr(IGuild guild, string link) { return (Translation.GetTranslation(guild, "errorGdpr", link)); }
        public static string ErrorDetails(IGuild guild, string error) { return (Translation.GetTranslation(guild, "errorDetails", error)); }
        public static string Command(IGuild guild) { return (Translation.GetTranslation(guild, "command")); }
        public static string Date(IGuild guild) { return (Translation.GetTranslation(guild, "date")); }
        public static string CantPM(IGuild guild) { return (Translation.GetTranslation(guild, "cantPM")); }

        /// --------------------------- Code ---------------------------
        public static string ShellNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "shellNotFound")); }
        public static string ShellHelp(IGuild guild) { return (Translation.GetTranslation(guild, "shellHelp")); }
        public static string IncreaseHelp(IGuild guild) { return (Translation.GetTranslation(guild, "increaseHelp")); }

        /// --------------------------- Image ---------------------------
        public static string InvalidColor(IGuild guild) { return (Translation.GetTranslation(guild, "invalidColor")); }
        public static string HelpColor(IGuild guild) { return (Translation.GetTranslation(guild, "helpColor")); }
        public static string Rgb(IGuild guild) { return (Translation.GetTranslation(guild, "rgb")); }
        public static string Hex(IGuild guild) { return (Translation.GetTranslation(guild, "hex")); }

        /// --------------------------- Linguist ---------------------------
        public static string JapaneseHelp(IGuild guild) { return (Translation.GetTranslation(guild, "japaneseHelp")); }
        public static string KanjiHelp(IGuild guild) { return (Translation.GetTranslation(guild, "kanjiHelp")); }
        public static string TranslateHelp(IGuild guild) { return (Translation.GetTranslation(guild, "translateHelp")); }
        public static string InvalidLanguage(IGuild guild) { return (Translation.GetTranslation(guild, "invalidLanguage")); }
        public static string NoJapaneseTranslation(IGuild guild) { return (Translation.GetTranslation(guild, "noJapaneseTranslation")); }
        public static string NoTextOnImage(IGuild guild) { return (Translation.GetTranslation(guild, "noTextOnImage")); }
        public static string NotAnImage(IGuild guild) { return (Translation.GetTranslation(guild, "notAnImage")); }
        public static string UrbanHelp(IGuild guild) { return (Translation.GetTranslation(guild, "urbanHelp")); }
        public static string UrbanNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "urbanNotFound")); }
        public static string Definition(IGuild guild) { return (Translation.GetTranslation(guild, "definition")); }
        public static string Example(IGuild guild) { return (Translation.GetTranslation(guild, "example")); }
        public static string Radical(IGuild guild) { return (Translation.GetTranslation(guild, "radical")); }
        public static string Parts(IGuild guild) { return (Translation.GetTranslation(guild, "parts")); }

        /// --------------------------- Settings ---------------------------
        public static string NeedLanguage(IGuild guild) { return (Translation.GetTranslation(guild, "needLanguage")); }
        public static string PrefixRemoved(IGuild guild) { return (Translation.GetTranslation(guild, "prefixRemoved")); }
        public static string ModuleManagementHelp(IGuild guild) { return (Translation.GetTranslation(guild, "moduleManagementHelp")); }
        public static string ModuleManagementInvalid(IGuild guild) { return (Translation.GetTranslation(guild, "moduleManagementInvalid")); }
        public static string ModuleEnabled(IGuild guild, string moduleName) { return (Translation.GetTranslation(guild, "moduleEnabled", moduleName)); }
        public static string ModuleDisabled(IGuild guild, string moduleName) { return (Translation.GetTranslation(guild, "moduleDisabled", moduleName)); }
        public static string ModuleAlreadyEnabled(IGuild guild, string moduleName) { return (Translation.GetTranslation(guild, "moduleAlreadyEnabled", moduleName)); }
        public static string ModuleAlreadyDisabled(IGuild guild, string moduleName) { return (Translation.GetTranslation(guild, "moduleAlreadyDisabled", moduleName)); }
        public static string AllModulesEnabled(IGuild guild) { return (Translation.GetTranslation(guild, "allModulesEnabled")); }
        public static string AllModulesDisabled(IGuild guild) { return (Translation.GetTranslation(guild, "allModulesDisabled")); }
        public static string AllModulesAlreadyEnabled(IGuild guild) { return (Translation.GetTranslation(guild, "allModulesAlreadyEnabled")); }
        public static string AllModulesAlreadyDisabled(IGuild guild) { return (Translation.GetTranslation(guild, "allModulesAlreadyDisabled")); }
        public static string AnonymizeCurrentTrue(IGuild guild) { return (Translation.GetTranslation(guild, "anonymizeCurrentTrue")); }
        public static string AnonymizeCurrentFalse(IGuild guild) { return (Translation.GetTranslation(guild, "anonymizeCurrentFalse")); }
        public static string AnonymizeHelp(IGuild guild) { return (Translation.GetTranslation(guild, "anonymizeHelp")); }

        /// --------------------------- Help ---------------------------
        public static string NoCommandAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "noCommandAvailable")); }
        public static string HelpHelp(IGuild guild) { return (Translation.GetTranslation(guild, "helpHelp")); }
        public static string Help(IGuild guild) { return (Translation.GetTranslation(guild, "help")); }

        /// --------------------------- Help Module Name ---------------------------
        public static string AnimeMangaModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "animeMangaModuleName")); }
        public static string ArknightsModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "arknightsModuleName")); }
        public static string BooruModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "booruModuleName")); }
        public static string CodeModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "codeModuleName")); }
        public static string CommunicationModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "communicationModuleName")); }
        public static string CommunityModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "communityModuleName")); }
        public static string DoujinshiModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "doujinshiModuleName")); }
        public static string GameModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "gameModuleName")); }
        public static string ImageModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "imageModuleName")); }
        public static string InformationModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "informationModuleName")); }
        public static string KantaiCollectionModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "kantaiCollectionModuleName")); }
        public static string LinguisticModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "linguisticModuleName")); }
        public static string RadioModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "radioModuleName")); }
        public static string SettingsModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "settingsModuleName")); }
        public static string VisualNovelModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "visualNovelModuleName")); }
        public static string XkcdModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "xkcdModuleName")); }
        public static string YoutubeModuleName(IGuild guild) { return (Translation.GetTranslation(guild, "youtubeModuleName")); }
        /// --------------------------- Help Module Content ---------------------------
        public static string ArknightsHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Arknights))
                return Translation.GetTranslation(guild, "arknightsModuleCharac");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string AnimeMangaHelp(IGuild guild, bool isServerOwner)
        {
            ulong guildId = guild?.Id ?? 0;
            string res;
            if (Program.p.db.IsAvailable(guildId, Program.Module.AnimeManga))
            {
                res = Translation.GetTranslation(guild, "animeMangaModuleAnime") + Environment.NewLine + Translation.GetTranslation(guild, "animeMangaModuleManga") + Environment.NewLine + Translation.GetTranslation(guild, "animeMangaModuleLN");
                if (isServerOwner)
                    res += Environment.NewLine + Translation.GetTranslation(guild, "animeMangaModuleSubscribe") + Environment.NewLine + Translation.GetTranslation(guild, "animeMangaModuleUnsubscribe");
                else
                    res += Environment.NewLine + "*" + Translation.GetTranslation(guild, "ownerForFull") + "*";
                return res;
            }
            return Base.Sentences.NotAvailable(guild);
        }
        public static string BooruHelp(IGuild guild, bool isChanNsfw)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Booru))
                return Translation.GetTranslation(guild, "booruModuleSource") + Environment.NewLine + Translation.GetTranslation(guild, "booruModuleSafebooru") + Environment.NewLine + Translation.GetTranslation(guild, "booruModuleE926")
                + ((isChanNsfw) ? (Environment.NewLine + Translation.GetTranslation(guild, "booruModuleGelbooru") + Environment.NewLine + Translation.GetTranslation(guild, "booruModuleKonachan")
                + Environment.NewLine + Translation.GetTranslation(guild, "booruModuleRule34") + Environment.NewLine + Translation.GetTranslation(guild, "booruModuleE621")) : (""))
                + Environment.NewLine + Translation.GetTranslation(guild, "booruModuleTags")
                + (isChanNsfw ? "" : Environment.NewLine + "*" + Translation.GetTranslation(guild, "nsfwForFull") + "*");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string CodeHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
                return Translation.GetTranslation(guild, "codeModuleShell") + Environment.NewLine + Translation.GetTranslation(guild, "codeModuleColor") + Environment.NewLine + Translation.GetTranslation(guild, "codeModuleIncrease");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string CommunicationHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
            {
                string str = Translation.GetTranslation(guild, "communicationModuleInfos") + Environment.NewLine + Translation.GetTranslation(guild, "communicationModuleBotInfos")
                    + Environment.NewLine + Translation.GetTranslation(guild, "communicationModuleQuote") + Environment.NewLine + Translation.GetTranslation(guild, "communicationModulePoll")
                    + Environment.NewLine + Translation.GetTranslation(guild, "communicationModuleCalc") + Environment.NewLine + Translation.GetTranslation(guild, "communicationModuleComplete")
                    + Environment.NewLine + Translation.GetTranslation(guild, "communicationModuleInspire");
                return str;
            }
            return Base.Sentences.NotAvailable(guild);
        }
        public static string CommunityHelp(IGuild guild, bool isServerOwner)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Community))
            {
                string str = Translation.GetTranslation(guild, "communityModuleGet") + Environment.NewLine + Translation.GetTranslation(guild, "communityModuleDescription")
                    + Environment.NewLine + Translation.GetTranslation(guild, "communityModuleColor") + Environment.NewLine + Translation.GetTranslation(guild, "communityModuleVisibility")
                    + Environment.NewLine + Translation.GetTranslation(guild, "communityModuleFriend") + Environment.NewLine + Translation.GetTranslation(guild, "communityModuleUnfriend");
                if (isServerOwner)
                    str += Environment.NewLine + Translation.GetTranslation(guild, "communityModuleSaveAll");
                str += Environment.NewLine + Environment.NewLine + "See https://sanara.zirk.eu/achievements.html for more information about the achievements";
                return str;
            }
            return Base.Sentences.NotAvailable(guild);
        }
        public static string DoujinshiHelp(IGuild guild, bool isChanNsfw)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Doujinshi))
            {
                if (isChanNsfw)
                    return Translation.GetTranslation(guild, "doujinshiModuleDoujinshi") + Environment.NewLine + Translation.GetTranslation(guild, "doujinshiModuleDoujinshiPopularity") + Environment.NewLine
                        + Translation.GetTranslation(guild, "doujinshiModuleCosplay")
                        + Environment.NewLine + Translation.GetTranslation(guild, "doujinshiModuleDownloadDoujinshi") + Environment.NewLine + Translation.GetTranslation(guild, "doujinshiModuleDownloadCosplay")
                        + Environment.NewLine + Translation.GetTranslation(guild, "doujinshiModuleAdultVideo") + Environment.NewLine + Translation.GetTranslation(guild, "doujinshiModuleSubscribe")
                        + Environment.NewLine + Translation.GetTranslation(guild, "doujinshiModuleUnsubscribe") + Environment.NewLine + Environment.NewLine + Translation.GetTranslation(guild, "blacklistExplanationsIntro")
                        + Environment.NewLine + Translation.GetTranslation(guild, "blacklistExplanations") + Environment.NewLine + Translation.GetTranslation(guild, "blacklistExplanations2")
                        + Environment.NewLine + Translation.GetTranslation(guild, "blacklistExplanations3") + Environment.NewLine + Translation.GetTranslation(guild, "blacklistExplanations4");
                else
                    return NoCommandAvailable(guild) + Environment.NewLine + "*" + Translation.GetTranslation(guild, "nsfwForFull") + "*";
            }
            return Base.Sentences.NotAvailable(guild);
        }
        public static string GameHelp(IGuild guild, bool isChanNsfw)
        {
            return Games.GameModule.DisplayHelp(guild, isChanNsfw);
        }
        public static string InformationHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Information))
                return Translation.GetTranslation(guild, "informationModuleHelp") + Environment.NewLine + Translation.GetTranslation(guild, "informationModuleGdpr")
                    + Environment.NewLine + Translation.GetTranslation(guild, "informationModuleStatus") + Environment.NewLine + Translation.GetTranslation(guild, "informationModuleInvite")
                    + Environment.NewLine + Translation.GetTranslation(guild, "informationModuleLogs") + Environment.NewLine + Translation.GetTranslation(guild, "informationModuleError");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string KantaiCollectionHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Kancolle))
                return Translation.GetTranslation(guild, "kantaiCollectionModuleCharac");// + Environment.NewLine + Translation.GetTranslation(guild, "kantaiCollectionModuleDrop");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string LinguisticHelp(IGuild guild, bool isChanNsfw)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Linguistic))
                return Translation.GetTranslation(guild, "linguisticModuleJapanese") + Environment.NewLine + Translation.GetTranslation(guild, "linguisticModuleKanji")
                    + Environment.NewLine + Translation.GetTranslation(guild, "linguisticModuleTranslation")
                 + Environment.NewLine + ((isChanNsfw) ? (Translation.GetTranslation(guild, "linguisticModuleUrban")) : ("*" + Translation.GetTranslation(guild, "nsfwForFull") + "*"));
            return Base.Sentences.NotAvailable(guild);
        }
        public static string RadioHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Radio))
                return Translation.GetTranslation(guild, "radioModuleLaunch") + Environment.NewLine + Translation.GetTranslation(guild, "radioModuleAdd")
                    + Environment.NewLine + Translation.GetTranslation(guild, "radioModulePlaylist") + Environment.NewLine + Translation.GetTranslation(guild, "radioModuleSkip")
                    + Environment.NewLine + Translation.GetTranslation(guild, "radioModuleRemove") + Environment.NewLine + Translation.GetTranslation(guild, "radioModuleStop");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string SettingsHelp(IGuild guild, bool isServerOwner, bool isBotOwner)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Settings))
            {
                string finalStr = "";
                if (isServerOwner)
                    finalStr = Translation.GetTranslation(guild, "settingsModuleLanguage") + Environment.NewLine + Translation.GetTranslation(guild, "settingsModulePrefix")
                        + Environment.NewLine + Translation.GetTranslation(guild, "settingsModuleEnable") + Environment.NewLine + Translation.GetTranslation(guild, "settingsModuleDisable")
                         + Environment.NewLine + Translation.GetTranslation(guild, "settingsModuleAnonymize");
                if (isBotOwner)
                {
                    if (finalStr != "")
                        finalStr += Environment.NewLine;
                    finalStr += Translation.GetTranslation(guild, "settingsModuleReload")
                        + Environment.NewLine + Translation.GetTranslation(guild, "settingsModuleExit") + Environment.NewLine + Translation.GetTranslation(guild, "settingsModuleEval");
                }
                if (finalStr == "")
                    return NoCommandAvailable(guild) + Environment.NewLine + "*" + Translation.GetTranslation(guild, "ownerForFull") + "*"; ;
                return finalStr;
            }
            return Base.Sentences.NotAvailable(guild);
        }
        public static string VisualNovelHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Vn))
                return Translation.GetTranslation(guild, "visualNovelModuleVn");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string XkcdHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Xkcd))
                return Translation.GetTranslation(guild, "xkcdModuleXkcd");
            return Base.Sentences.NotAvailable(guild);
        }
        public static string YouTubeHelp(IGuild guild)
        {
            ulong guildId = guild?.Id ?? 0;
            if (Program.p.db.IsAvailable(guildId, Program.Module.Youtube))
                return Translation.GetTranslation(guild, "youtubeModuleYoutube");
            return Base.Sentences.NotAvailable(guild);
        }
    }
}