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

namespace SanaraV2.Modules.NSFW
{
    public static class Sentences
    {
        /// --------------------------- Booru ---------------------------
        public static string InvalidId(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidId")); }
        public static string HelpId (ulong guildId) { return (Translation.GetTranslation(guildId, "helpId")); }
        public static string Source(ulong guildId) { return (Translation.GetTranslation(guildId, "source")); }
        public static string Sources(ulong guildId) { return (Translation.GetTranslation(guildId, "sources")); }
        public static string Character(ulong guildId) { return (Translation.GetTranslation(guildId, "character")); }
        public static string Characters(ulong guildId) { return (Translation.GetTranslation(guildId, "characters")); }
        public static string Artist(ulong guildId) { return (Translation.GetTranslation(guildId, "artist")); }
        public static string Artists(ulong guildId) { return (Translation.GetTranslation(guildId, "artists")); }
        public static string ImageInfo(ulong guildId, string id) { return (Translation.GetTranslation(guildId, "imageInfo", id)); }
        public static string DownloadInfo(ulong guildId, string id) { return (Translation.GetTranslation(guildId, "downloadInfo", id)); }

        /// --------------------------- Doujinshi ---------------------------
        public static string ClickFull(ulong guildId) { return (Translation.GetTranslation(guildId, "clickFull")); }
        public static string PreparingDownload(ulong guildId) { return (Translation.GetTranslation(guildId, "preparingDownload")); }
        public static string DownloadHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "downloadHelp")); }
        public static string DownloadNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "downloadNotFound")); }
    }
}