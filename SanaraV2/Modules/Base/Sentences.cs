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
using System.Linq;

namespace SanaraV2.Modules.Base
{
    public static class Sentences
    {
        /// --------------------------- ID ---------------------------
        public static string ownerStr;
        public static ulong ownerId;

        /// --------------------------- General ---------------------------
        public static string OnlyMasterStr(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyMaster", ownerStr)); }
        public static string OnlyOwnerStr(ulong guildId, ulong guildOwnerId) { return (Translation.GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(guildOwnerId).ToString())); }
        public static string ChanIsNotNsfw(ulong guildId) { return (Translation.GetTranslation(guildId, "chanIsNotNsfw")); }
        private static string TagNotFoundInternal(ulong guildId, string tag) { return Translation.GetTranslation(guildId, "tagNotFound", tag); }
        private static string TagsNotFoundInternal(ulong guildId, string[] tags) {
            string finalStr = string.Join(", ", tags.ToList().Take(tags.Length - 1).Select(x => "'" + x + "'"));
            return Translation.GetTranslation(guildId, "tagsNotFound", finalStr, tags[tags.Length - 1]);
        }
        public static string TagsNotFound(ulong guildId, string[] tags)
        {
            if (tags.Length == 1)
                return (TagNotFoundInternal(guildId, tags[0]));
            return (TagsNotFoundInternal(guildId, tags));
        }
        public static string NoCorrespondingGuild(ulong guildId) { return (Translation.GetTranslation(guildId, "noCorrespondingGuild")); }
        public static string DontPm(ulong guildId) { return (Translation.GetTranslation(guildId, "dontPm")); }
        public static string NoApiKey(ulong guildId) { return (Translation.GetTranslation(guildId, "noApiKey")); }
        public static string TimeSeconds(ulong guildId, string seconds) { return (Translation.GetTranslation(guildId, "timeSeconds", seconds)); }
        public static string TimeMinutes(ulong guildId, string minutes, string seconds) { return (Translation.GetTranslation(guildId, "timeMinutes", minutes, seconds)); }
        public static string TimeHours(ulong guildId, string hours, string minutes, string seconds) { return (Translation.GetTranslation(guildId, "timeHours", hours, minutes, seconds)); }
        public static string TimeDays(ulong guildId, string days, string hours, string minutes, string seconds) { return (Translation.GetTranslation(guildId, "timeDays", days, hours, minutes, seconds)); }
        public static string DoneStr(ulong guildId) { return (Translation.GetTranslation(guildId, "done")); }
        public static string ExceptionThrown(ulong guildId, string details) { return (Translation.GetTranslation(guildId, "exceptionThrown", details)); }
        public static string NeedEmbedLinks(ulong guildId) { return (Translation.GetTranslation(guildId, "needEmbedLinks"));  }
        public static string NotAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "notAvailable")); }

        /// --------------------------- Parts ---------------------------
        public static string DateHourFormat(ulong guildId) { return (Translation.GetTranslation(guildId, "dateHourFormat")); }
        public static string DateHourFormatShort(ulong guildId) { return (Translation.GetTranslation(guildId, "dateHourFormatShort")); }
        public static string OrStr(ulong guildId) { return (Translation.GetTranslation(guildId, "or")); }
        public static string FromStr(ulong guildId, string source) { return (Translation.GetTranslation(guildId, "from", source)); }
        public static string YesStr(ulong guildId) { return (Translation.GetTranslation(guildId, "yes")); }
        public static string NoStr(ulong guildId) { return (Translation.GetTranslation(guildId, "no")); }
    }
}