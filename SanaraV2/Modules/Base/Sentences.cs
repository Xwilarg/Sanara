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
using Discord.WebSocket;
using System.Linq;

namespace SanaraV2.Modules.Base
{
    public static class Sentences
    {
        /// --------------------------- ID ---------------------------
        public static string ownerStr;
        public static ulong ownerId;

        /// --------------------------- General ---------------------------
        public static string OnlyMasterStr(IGuild guild) { return (Translation.GetTranslation(guild, "onlyMaster")); }
        public static string OnlyOwnerStr(IGuild guild, ulong guildOwnerId) { return (Translation.GetTranslation(guild, "onlyOwner", ((SocketGuild)guild).GetUser(guildOwnerId).ToString())); }
        public static string ChanIsNotNsfw(IGuild guild) { return (Translation.GetTranslation(guild, "chanIsNotNsfw")); }
        public static string AnswerNsfw(IGuild guild) { return (Translation.GetTranslation(guild, "answerNsfw")); }
        private static string TagNotFoundInternal(IGuild guild, string tag) { return Translation.GetTranslation(guild, "tagNotFound", tag); }
        private static string TagsNotFoundInternal(IGuild guild, string[] tags) {
            string finalStr = string.Join(", ", tags.ToList().Take(tags.Length - 1).Select(x => "'" + x + "'"));
            return Translation.GetTranslation(guild, "tagsNotFound", finalStr, "'" + tags[tags.Length - 1] + "'");
        }
        public static string TagsNotFound(IGuild guild, string[] tags)
        {
            if (tags.Length == 1)
                return (TagNotFoundInternal(guild, tags[0]));
            return (TagsNotFoundInternal(guild, tags));
        }
        public static string NoCorrespondingGuild(IGuild guild) { return (Translation.GetTranslation(guild, "noCorrespondingGuild")); }
        public static string DontPm(IGuild guild) { return (Translation.GetTranslation(guild, "dontPm")); }
        public static string NoApiKey(IGuild guild) { return (Translation.GetTranslation(guild, "noApiKey")); }
        public static string TimeSeconds(IGuild guild, string seconds) { return (Translation.GetTranslation(guild, "timeSeconds", seconds)); }
        public static string TimeMinutes(IGuild guild, string minutes, string seconds) { return (Translation.GetTranslation(guild, "timeMinutes", minutes, seconds)); }
        public static string TimeHours(IGuild guild, string hours, string minutes, string seconds) { return (Translation.GetTranslation(guild, "timeHours", hours, minutes, seconds)); }
        public static string TimeDays(IGuild guild, string days, string hours, string minutes, string seconds) { return (Translation.GetTranslation(guild, "timeDays", days, hours, minutes, seconds)); }
        public static string DoneStr(IGuild guild) { return (Translation.GetTranslation(guild, "done")); }
        public static string ExceptionThrown(IGuild guild, string details) { return (Translation.GetTranslation(guild, "exceptionThrown", details)); }
        public static string ExceptionReported(IGuild guild) { return (Translation.GetTranslation(guild, "exceptionReported")); }
        public static string NeedEmbedLinks(IGuild guild) { return (Translation.GetTranslation(guild, "needEmbedLinks"));  }
        public static string NotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "notAvailable")); }
        public static string NotWorking(IGuild guild) { return (Translation.GetTranslation(guild, "notWorking")); }
        public static string ErrorIntro(IGuild guild) { return (Translation.GetTranslation(guild, "errorIntro")); }
        public static string ErrorBody(IGuild guild, string id) { return (Translation.GetTranslation(guild, "errorBody", id)); }
        public static string CommandDontPm(IGuild guild) { return (Translation.GetTranslation(guild, "commandDontPm")); }

        /// --------------------------- Parts ---------------------------
        public static string DateHourFormat(IGuild guild) { return (Translation.GetTranslation(guild, "dateHourFormat")); }
        public static string DateHourFormatShort(IGuild guild) { return (Translation.GetTranslation(guild, "dateHourFormatShort")); }
        public static string AtStr(IGuild guild) { return (Translation.GetTranslation(guild, "at")); }
        public static string OrStr(IGuild guild) { return (Translation.GetTranslation(guild, "or")); }
        public static string FromStr(IGuild guild, string source) { return (Translation.GetTranslation(guild, "from", source)); }
        public static string YesStr(IGuild guild) { return (Translation.GetTranslation(guild, "yes")); }
        public static string NoStr(IGuild guild) { return (Translation.GetTranslation(guild, "no")); }
        public static string None(IGuild guild) { return (Translation.GetTranslation(guild, "none")); }
    }
}