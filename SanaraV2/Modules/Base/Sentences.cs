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
using System;
using System.Linq;

namespace SanaraV2.Modules.Base
{
    public static class Sentences
    {
        /// --------------------------- ID ---------------------------
        public readonly static ulong myId = (Program.p.client == null) ? (0) : (Program.p.client.CurrentUser.Id);
        public readonly static ulong ownerId = 144851584478740481;

        /// --------------------------- General ---------------------------
        public static string IntroductionError(ulong guildId, string userId, string userName)
        {
            return (Translation.GetTranslation(guildId, "introductionError", "<@" + ownerId + ">", userId, userName));
        }
        public static string OnlyMasterStr(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(ownerId).ToString())); }
        public static string OnlyOwnerStr(ulong guildId, ulong guildOwnerId) { return (Translation.GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(guildOwnerId).ToString())); }
        public static string NeedAttachFile(ulong guildId) { return (Translation.GetTranslation(guildId, "needAttachFile")); }
        public static string ChanIsNotNsfw(ulong guildId) { return (Translation.GetTranslation(guildId, "chanIsNotNsfw")); }
        public static string NothingAfterXIterations(ulong guildId, int nbIterations) { return (Translation.GetTranslation(guildId, "nothingAfterXIterations", nbIterations.ToString())); }
        public static string TooManyRequests(ulong guildId, string apiName) { return (Translation.GetTranslation(guildId, "tooManyRequests", apiName)); }
        public static string TagsNotFound(string[] tags)
        {
            if (tags.Length == 1)
                return ("I didn't find anything with the tag '" + tags[0] + "'.");
            string finalStr = String.Join(", ", tags.ToList().Take(tags.Length - 1).Select(x => "'" + x + "'"));
            return ("I didn't find anything with the tag " + finalStr + " and '" + tags[tags.Length - 1] + "'.");
        }
        public static string NoCorrespondingGuild(ulong guildId) { return (Translation.GetTranslation(guildId, "noCorrespondingGuild")); }
        public static string BetaFeature(ulong guildId) { return (Translation.GetTranslation(guildId, "betaFeature")); }
        public static string DontPm(ulong guildId) { return (Translation.GetTranslation(guildId, "dontPm")); }
        public static string NoApiKey(ulong guildId) { return (Translation.GetTranslation(guildId, "noApiKey")); }
        public static string NoDictionnary(ulong guildId) { return (Translation.GetTranslation(guildId, "noDictionnary")); }
        public static string TimeSeconds(ulong guildId, string seconds) { return (Translation.GetTranslation(guildId, "timeSeconds", seconds)); }
        public static string TimeMinutes(ulong guildId, string minutes, string seconds) { return (Translation.GetTranslation(guildId, "timeMinutes", minutes, seconds)); }
        public static string TimeHours(ulong guildId, string hours, string minutes, string seconds) { return (Translation.GetTranslation(guildId, "timeHours", hours, minutes, seconds)); }
        public static string TimeDays(ulong guildId, string days, string hours, string minutes, string seconds) { return (Translation.GetTranslation(guildId, "timeDays", days, hours, minutes, seconds)); }
        public static string DoneStr(ulong guildId) { return (Translation.GetTranslation(guildId, "done")); }
        public static string HttpError(ulong guildId, string apiName) { return (Translation.GetTranslation(guildId, "httpError", apiName)); }
        public static string ExceptionThrown(ulong guildId, string details) { return (Translation.GetTranslation(guildId, "exceptionThrown", details)); }

        /// --------------------------- Parts ---------------------------
        public static string AndStr(ulong guildId) { return (Translation.GetTranslation(guildId, "and")); }
        public static string DateHourFormat(ulong guildId) { return (Translation.GetTranslation(guildId, "dateHourFormat")); }
        public static string DateHourFormatShort(ulong guildId) { return (Translation.GetTranslation(guildId, "dateHourFormatShort")); }
        public static string OrStr(ulong guildId) { return (Translation.GetTranslation(guildId, "or")); }
    }
}