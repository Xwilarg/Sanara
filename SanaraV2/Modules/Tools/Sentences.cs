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

        /// --------------------------- Information ---------------------------
        public static string DataSaved(ulong guildId, string about) { return (Translation.GetTranslation(guildId, "dataSaved", about)); }
        public static string ServicesAvailability(ulong guildId) { return (Translation.GetTranslation(guildId, "servicesAvailability")); }
        public static string TranslationsAvailability(ulong guildId) { return (Translation.GetTranslation(guildId, "translationsAvailability")); }
        public static string LatestChanges(ulong guildId) { return (Translation.GetTranslation(guildId, "latestChanges")); }
        public static string ByStr(ulong guildId) { return (Translation.GetTranslation(guildId, "by")); }

        /// --------------------------- Code ---------------------------
        public static string ShellNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "shellNotFound")); }
        public static string ShellHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "shellHelp")); }

        /// --------------------------- Image ---------------------------
        public static string InvalidColor(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidColor")); }
        public static string HelpColor(ulong guildId) { return (Translation.GetTranslation(guildId, "helpColor")); }
        public static string Rgb(ulong guildId) { return (Translation.GetTranslation(guildId, "rgb")); }
        public static string Hex(ulong guildId) { return (Translation.GetTranslation(guildId, "hex")); }

        /// --------------------------- Linguist ---------------------------
        public static string JapaneseHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "japaneseHelp")); }
        public static string TranslateHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "translateHelp")); }
        public static string InvalidLanguage(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidLanguage")); }
        public static string NoJapaneseTranslation(ulong guildId) { return (Translation.GetTranslation(guildId, "noJapaneseTranslation")); }
        public static string NoTextOnImage(ulong guildId) { return (Translation.GetTranslation(guildId, "noTextOnImage")); }
        public static string NotAnImage(ulong guildId) { return (Translation.GetTranslation(guildId, "notAnImage")); }
        public static string UrbanHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "urbanHelp")); }
        public static string UrbanNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "urbanNotFound")); }
        public static string Definition(ulong guildId) { return (Translation.GetTranslation(guildId, "definition")); }
        public static string Example(ulong guildId) { return (Translation.GetTranslation(guildId, "example")); }

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

        /// --------------------------- Help ---------------------------
        public static string NoCommandAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "noCommandAvailable")); }
        public static string HelpHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "helpHelp")); }
        public static string Help(ulong guildId) { return (Translation.GetTranslation(guildId, "help")); }

        /// --------------------------- Help Module Name ---------------------------
        public static string AnimeMangaModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "animeMangaModuleName")); }
        public static string BooruModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "booruModuleName")); }
        public static string CodeModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "codeModuleName")); }
        public static string CommunicationModuleName(ulong guildId) { return (Translation.GetTranslation(guildId, "communicationModuleName")); }
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
        public static string AnimeMangaHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.AnimeManga))
                return Translation.GetTranslation(guildId, "animeMangaModuleAnime") + Environment.NewLine + Translation.GetTranslation(guildId, "animeMangaModuleManga");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string BooruHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Booru))
                return Translation.GetTranslation(guildId, "booruModuleSafebooru") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleE926")
                + Environment.NewLine + ((isChanNsfw) ? (Translation.GetTranslation(guildId, "booruModuleGelbooru") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleKonachan")
                + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleRule34") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleE621")) : ("*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*"));
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string CodeHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
                return Translation.GetTranslation(guildId, "codeModuleShell") + Environment.NewLine + Translation.GetTranslation(guildId, "codeModuleColor");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string CommunicationHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
                return Translation.GetTranslation(guildId, "communicationModuleInfos") + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleBotInfos")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleQuote");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string DoujinshiHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Doujinshi))
            {
                if (isChanNsfw)
                    return Translation.GetTranslation(guildId, "doujinshiModuleDoujinshi");
                else
                    return NoCommandAvailable(guildId) + Environment.NewLine + "*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*";
            }
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string GameHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Game))
                return Translation.GetTranslation(guildId, "gameModuleKancolle") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleAzurLane")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleFateGO")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleAnime") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleShiritori")
                     + Environment.NewLine + Translation.GetTranslation(guildId, "gameModulePokemon")
                    + ((isChanNsfw) ? (Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleBooru")) : (""))
                    + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleReset") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleScore")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleNote") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleNote2")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleNote3")
                    + ((!isChanNsfw) ? (Environment.NewLine + Environment.NewLine + "*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*") : (""));
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string InformationHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Information))
                return Translation.GetTranslation(guildId, "informationModuleHelp") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleGdpr")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleStatus") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleInvite")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleLogs");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string KantaiCollectionHelp(ulong guildId)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Kancolle))
                return Translation.GetTranslation(guildId, "kantaiCollectionModuleCharac") + Environment.NewLine + Translation.GetTranslation(guildId, "kantaiCollectionModuleDrop");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string LinguisticHelp(ulong guildId, bool isChanNsfw)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Linguistic))
                return Translation.GetTranslation(guildId, "linguisticModuleJapanese") + Environment.NewLine + Translation.GetTranslation(guildId, "linguisticModuleTranslation")
                 + Environment.NewLine + ((isChanNsfw) ? (Translation.GetTranslation(guildId, "linguisticModuleUrban")) : ("*" + Translation.GetTranslation(guildId, "nsfwForFull") + "*"));
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string RadioHelp(ulong guildId)
        {
            return Base.Sentences.NotWorking(guildId);
            if (Program.p.db.IsAvailable(guildId, Program.Module.Radio))
                return Translation.GetTranslation(guildId, "radioModuleLaunch") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleAdd")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "radioModulePlaylist") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleSkip")
                    + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleStop");
            return Base.Sentences.NotAvailable(guildId);
        }
        public static string SettingsHelp(ulong guildId, bool isServerOwner, bool isBotOwner)
        {
            if (Program.p.db.IsAvailable(guildId, Program.Module.Settings))
            {
                string finalStr = "";
                if (isServerOwner)
                    finalStr = Translation.GetTranslation(guildId, "settingsModuleLanguage") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModulePrefix")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleEnable") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleDisable");
                if (isBotOwner)
                {
                    if (finalStr != "")
                        finalStr += Environment.NewLine;
                    finalStr += Translation.GetTranslation(guildId, "settingsModuleReload") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleLeave")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleExit") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleResetDb")
                        + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleEval");
                }
                if (finalStr == "")
                    return NoCommandAvailable(guildId);
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