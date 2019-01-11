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

        /// --------------------------- Help ---------------------------
        private static string NoCommandAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "noCommandAvailable")); }
        public static Embed Help(ulong guildId, bool isChanNsfw, bool isOwner)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = Translation.GetTranslation(guildId, "help"),
                Color = Color.Purple
            };
            if (Program.p.db.IsAvailable(guildId, Program.Module.AnimeManga))
                embed.AddField(Translation.GetTranslation(guildId, "animeMangaModuleName"), Translation.GetTranslation(guildId, "animeMangaModuleAnime") + Environment.NewLine + Translation.GetTranslation(guildId, "animeMangaModuleManga"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "animeMangaModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Booru))
                embed.AddField(Translation.GetTranslation(guildId, "booruModuleName"), Translation.GetTranslation(guildId, "booruModuleSafebooru") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleE926")
                + ((isChanNsfw) ? (Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleGelbooru") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleKonachan") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleRule34") + Environment.NewLine + Translation.GetTranslation(guildId, "booruModuleE621")) : ("")));
            else
                embed.AddField(Translation.GetTranslation(guildId, "booruModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Communication))
                embed.AddField(Translation.GetTranslation(guildId, "communicationModuleName"), Translation.GetTranslation(guildId, "communicationModuleInfos") + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleBotInfos") + Environment.NewLine + Translation.GetTranslation(guildId, "communicationModuleQuote"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "communicationModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Doujinshi))
                embed.AddField(Translation.GetTranslation(guildId, "doujinshiModuleName"),
                ((isChanNsfw) ? (Translation.GetTranslation(guildId, "doujinshiModuleDoujinshi"))
                              : (NoCommandAvailable(guildId))));
            else
                embed.AddField(Translation.GetTranslation(guildId, "doujinshiModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Game))
                embed.AddField(Translation.GetTranslation(guildId, "gameModuleName"), Translation.GetTranslation(guildId, "gameModuleKancolle") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleAnime") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleShiritori") + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleFireEmblem")
                + ((isChanNsfw) ? (Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleBooru")) : (""))
                + Environment.NewLine + Translation.GetTranslation(guildId, "gameModuleNote"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "gameModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Image))
                embed.AddField(Translation.GetTranslation(guildId, "imageModuleName"), Translation.GetTranslation(guildId, "imageModuleColor"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "imageModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Information))
                embed.AddField(Translation.GetTranslation(guildId, "informationModuleName"), Translation.GetTranslation(guildId, "informationModuleGdpr") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleStatus") + Environment.NewLine + Translation.GetTranslation(guildId, "informationModuleInvite"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "informationModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Kancolle))
                embed.AddField(Translation.GetTranslation(guildId, "kantaiCollectionModuleName"), Translation.GetTranslation(guildId, "kantaiCollectionModuleCharac") + Environment.NewLine + Translation.GetTranslation(guildId, "kantaiCollectionModuleDrop"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "kantaiCollectionModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Linguistic))
                embed.AddField(Translation.GetTranslation(guildId, "linguisticModuleName"),Translation.GetTranslation(guildId, "linguisticModuleJapanese") + Environment.NewLine + Translation.GetTranslation(guildId, "linguisticModuleTranslation") + Environment.NewLine + Translation.GetTranslation(guildId, "linguisticModuleUrban")
                 + ((isChanNsfw) ? (Environment.NewLine + Translation.GetTranslation(guildId, "linguisticModuleUrban")) : ("")));
            else
                embed.AddField(Translation.GetTranslation(guildId, "linguisticModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Radio))
                embed.AddField(Translation.GetTranslation(guildId, "radioModuleName"), Translation.GetTranslation(guildId, "radioModuleLaunch") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleAdd") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModulePlaylist") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleSkip") + Environment.NewLine + Translation.GetTranslation(guildId, "radioModuleStop"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "radioModuleName"), Base.Sentences.NotAvailable(guildId));
            embed.AddField(Translation.GetTranslation(guildId, "settingsModuleName"),
                ((isOwner) ? (Translation.GetTranslation(guildId, "settingsModuleLanguage") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModulePrefix") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleReload") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleLeave") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleExit")
                 + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleEnable") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleDisable") + Environment.NewLine + Translation.GetTranslation(guildId, "settingsModuleResetDb")) : (NoCommandAvailable(guildId))));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Vn))
                embed.AddField(Translation.GetTranslation(guildId, "visualNovelModuleName"), Translation.GetTranslation(guildId, "visualNovelModuleVn"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "visualNovelModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Xkcd))
                embed.AddField(Translation.GetTranslation(guildId, "xkcdModuleName"), Translation.GetTranslation(guildId, "xkcdModuleXkcd"));
            else
                embed.AddField(Translation.GetTranslation(guildId, "xkcdModuleName"), Base.Sentences.NotAvailable(guildId));
            if (Program.p.db.IsAvailable(guildId, Program.Module.Youtube))
                embed.AddField(Translation.GetTranslation(guildId, "youtubeModuleName"), Translation.GetTranslation(guildId, "youtubeModuleYoutube") + Environment.NewLine + Environment.NewLine
                + ((isChanNsfw) ? ("") : (Translation.GetTranslation(guildId, "nsfwForFull"))));
            else
                embed.AddField(Translation.GetTranslation(guildId, "youtubeModuleName"), Base.Sentences.NotAvailable(guildId));
            return (embed.Build());
        }
    }
}