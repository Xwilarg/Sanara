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

namespace SanaraV2.Modules.Entertainment
{
    public static class Sentences
    {
        /// --------------------------- AnimeManga ---------------------------
        public static string MangaHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "mangaHelp")); }
        public static string AnimeHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "animeHelp")); }
        public static string SourceHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "sourceHelp")); }
        public static string MangaNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "mangaNotFound")); }
        public static string AnimeNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "animeNotFound")); }
        public static string AnimeEpisodes(ulong guildId) { return (Translation.GetTranslation(guildId, "animeEpisodes")); }
        public static string AnimeLength(ulong guildId, int length) { return (Translation.GetTranslation(guildId, "animeLength", length.ToString())); }
        public static string AnimeRating(ulong guildId) { return (Translation.GetTranslation(guildId, "animeRating")); }
        public static string AnimeAudiance(ulong guildId) { return (Translation.GetTranslation(guildId, "animeAudiance")); }
        public static string ToBeAnnounced(ulong guildId) { return (Translation.GetTranslation(guildId, "toBeAnnounced")); }
        public static string Unknown(ulong guildId) { return (Translation.GetTranslation(guildId, "unknown")); }
        public static string Certitude(ulong guildId) { return (Translation.GetTranslation(guildId, "certitude")); }
        public static string Episode(ulong guildId) { return (Translation.GetTranslation(guildId, "episode")); }
        public static string NotAnUrl(ulong guildId) { return (Translation.GetTranslation(guildId, "notAnUrl")); }
        public static string SubscribeHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "subscribeHelp")); }
        public static string InvalidChannel(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidChannel")); }
        public static string NoSubscription(ulong guildId) { return (Translation.GetTranslation(guildId, "noSubscription")); }
        public static string SubscribeDone(ulong guildId, string name, ITextChannel channel) { return (Translation.GetTranslation(guildId, "subscribeDone", name, channel.Mention)); }
        public static string UnsubscribeDone(ulong guildId, string name) { return (Translation.GetTranslation(guildId, "unsubscribeDone", name)); }

        /// --------------------------- Radio ---------------------------
        public static string RadioAlreadyStarted(ulong guildId) { return (Translation.GetTranslation(guildId, "radioAlreadyStarted")); }
        public static string RadioNeedChannel(ulong guildId) { return (Translation.GetTranslation(guildId, "radioNeedChannel")); }
        public static string RadioNeedArg(ulong guildId) { return (Translation.GetTranslation(guildId, "radioNeedArg")); }
        public static string RadioNotStarted(ulong guildId) { return (Translation.GetTranslation(guildId, "radioNotStarted")); }
        public static string RadioAlreadyInList(ulong guildId) { return (Translation.GetTranslation(guildId, "radioAlreadyInList")); }
        public static string RadioTooMany(ulong guildId) { return (Translation.GetTranslation(guildId, "radioTooMany")); }
        public static string RadioNoSong(ulong guildId) { return (Translation.GetTranslation(guildId, "radioNoSong")); }
        public static string SongSkipped(ulong guildId, string songName) { return (Translation.GetTranslation(guildId, "songSkipped", songName)); }
        public static string Current(ulong guildId) { return (Translation.GetTranslation(guildId, "current")); }
        public static string Downloading(ulong guildId) { return (Translation.GetTranslation(guildId, "downloading")); }
        public static string SongAdded(ulong guildId, string songName) { return (Translation.GetTranslation(guildId, "songAdded", songName)); }

        /// --------------------------- VN ---------------------------
        public static string VndbHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "vndbHelp")); }
        public static string VndbNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "vndbNotFound")); }
        public static string AvailableEnglish(ulong guildId) { return (Translation.GetTranslation(guildId, "availableEnglish")); }
        public static string AvailableWindows(ulong guildId) { return (Translation.GetTranslation(guildId, "availableWindows")); }
        public static string VndbRating(ulong guildId) { return (Translation.GetTranslation(guildId, "vndbRating")); }
        public static string Hours(ulong guildId, string length) { return (Translation.GetTranslation(guildId, "hours", length)); }
        public static string Length(ulong guildId) { return (Translation.GetTranslation(guildId, "length")); }
        public static string Tba(ulong guildId) { return (Translation.GetTranslation(guildId, "tba")); }
        public static string ReleaseDate(ulong guildId) { return (Translation.GetTranslation(guildId, "releaseDate")); }

        /// --------------------------- XKCD ---------------------------
        public static string XkcdWrongArg(ulong guildId) { return (Translation.GetTranslation(guildId, "xkcdWrongArg")); }
        public static string XkcdWrongId(ulong guildId, int max) { return (Translation.GetTranslation(guildId, "xkcdWrongId", max.ToString())); }

        /// --------------------------- Youtube ---------------------------
        public static string YoutubeHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "youtubeHelp")); }
        public static string YoutubeNotFound(ulong guildId) { return (Translation.GetTranslation(guildId, "youtubeNotFound")); }
    }
}