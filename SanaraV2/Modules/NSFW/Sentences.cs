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

namespace SanaraV2.Modules.NSFW
{
    public static class Sentences
    {
        /// --------------------------- Booru ---------------------------
        public static string InvalidId(IGuild guild) { return (Translation.GetTranslation(guildId, "invalidId")); }
        public static string HelpId (IGuild guild) { return (Translation.GetTranslation(guildId, "helpId")); }
        public static string Source(IGuild guild) { return (Translation.GetTranslation(guildId, "source")); }
        public static string Sources(IGuild guild) { return (Translation.GetTranslation(guildId, "sources")); }
        public static string Character(IGuild guild) { return (Translation.GetTranslation(guildId, "character")); }
        public static string Characters(IGuild guild) { return (Translation.GetTranslation(guildId, "characters")); }
        public static string Artist(IGuild guild) { return (Translation.GetTranslation(guildId, "artist")); }
        public static string Artists(IGuild guild) { return (Translation.GetTranslation(guildId, "artists")); }
        public static string ImageInfo(IGuild guild, string id) { return (Translation.GetTranslation(guildId, "imageInfo", id)); }
        public static string DownloadDoujinshiInfo(IGuild guild, string id) { return (Translation.GetTranslation(guildId, "downloadDoujinshiInfo", id)); }
        public static string DownloadCosplayInfo(IGuild guild, string id) { return (Translation.GetTranslation(guildId, "downloadCosplayInfo", id)); }
        public static string DownloadHelp(IGuild guild) { return (Translation.GetTranslation(guildId, "downloadHelp")); }

        /// --------------------------- Doujinshi ---------------------------
        public static string ClickFull(IGuild guild) { return (Translation.GetTranslation(guildId, "clickFull")); }
        public static string PreparingDownload(IGuild guild) { return (Translation.GetTranslation(guildId, "preparingDownload")); }
        public static string DownloadDoujinshiHelp(IGuild guild) { return (Translation.GetTranslation(guildId, "downloadDoujinshiHelp")); }
        public static string DownloadCosplayHelp(IGuild guild) { return (Translation.GetTranslation(guildId, "downloadCosplayHelp")); }
        public static string DownloadDoujinshiNotFound(IGuild guild) { return (Translation.GetTranslation(guildId, "downloadDoujinshiNotFound")); }
        public static string DownloadCosplayNotFound(IGuild guild) { return (Translation.GetTranslation(guildId, "downloadCosplayNotFound")); }
        public static string DeleteTime(IGuild guild, string time) { return (Translation.GetTranslation(guildId, "deleteTime", time)); }
        public static string SubscribeNHentaiHelp(IGuild guild) { return (Translation.GetTranslation(guildId, "subscribeNHentaiHelp")); }
        public static string SubscribeSafeDestination(IGuild guild) { return (Translation.GetTranslation(guildId, "subscribeSafeDestination")); }
        public static string Blacklist(IGuild guild) { return (Translation.GetTranslation(guildId, "blacklist")); }
        public static string Whitelist(IGuild guild) { return (Translation.GetTranslation(guildId, "whitelist")); }
    }
}