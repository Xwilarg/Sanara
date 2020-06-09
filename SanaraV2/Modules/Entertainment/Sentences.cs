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
        public static string MangaHelp(IGuild guild) { return (Translation.GetTranslation(guild, "mangaHelp")); }
        public static string AnimeHelp(IGuild guild) { return (Translation.GetTranslation(guild, "animeHelp")); }
        public static string LNHelp(IGuild guild) { return (Translation.GetTranslation(guild, "LNHelp")); }
        public static string SourceHelp(IGuild guild) { return (Translation.GetTranslation(guild, "sourceHelp")); }
        public static string MangaNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "mangaNotFound")); }
        public static string AnimeNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "animeNotFound")); }
        public static string LNNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "LNNotFound")); }
        public static string AnimeEpisodes(IGuild guild) { return (Translation.GetTranslation(guild, "animeEpisodes")); }
        public static string AnimeLength(IGuild guild, int length) { return (Translation.GetTranslation(guild, "animeLength", length.ToString())); }
        public static string AnimeRating(IGuild guild) { return (Translation.GetTranslation(guild, "animeRating")); }
        public static string AnimeAudiance(IGuild guild) { return (Translation.GetTranslation(guild, "animeAudiance")); }
        public static string ToBeAnnounced(IGuild guild) { return (Translation.GetTranslation(guild, "toBeAnnounced")); }
        public static string Unknown(IGuild guild) { return (Translation.GetTranslation(guild, "unknown")); }
        public static string Certitude(IGuild guild) { return (Translation.GetTranslation(guild, "certitude")); }
        public static string NotAnUrl(IGuild guild) { return (Translation.GetTranslation(guild, "notAnUrl")); }
        public static string SubscribeHelp(IGuild guild) { return (Translation.GetTranslation(guild, "subscribeHelp")); }
        public static string InvalidChannel(IGuild guild) { return (Translation.GetTranslation(guild, "invalidChannel")); }
        public static string NoSubscription(IGuild guild) { return (Translation.GetTranslation(guild, "noSubscription")); }
        public static string SubscribeDone(IGuild guild, string name, ITextChannel channel) { return (Translation.GetTranslation(guild, "subscribeDone", name, channel.Mention)); }
        public static string UnsubscribeDone(IGuild guild, string name) { return (Translation.GetTranslation(guild, "unsubscribeDone", name)); }

        /// --------------------------- Radio ---------------------------
        public static string RadioAlreadyStarted(IGuild guild) { return (Translation.GetTranslation(guild, "radioAlreadyStarted")); }
        public static string RadioNeedChannel(IGuild guild) { return (Translation.GetTranslation(guild, "radioNeedChannel")); }
        public static string RadioNeedArg(IGuild guild) { return (Translation.GetTranslation(guild, "radioNeedArg")); }
        public static string RadioNotStarted(IGuild guild) { return (Translation.GetTranslation(guild, "radioNotStarted")); }
        public static string RadioAlreadyInList(IGuild guild) { return (Translation.GetTranslation(guild, "radioAlreadyInList")); }
        public static string RadioTooMany(IGuild guild) { return (Translation.GetTranslation(guild, "radioTooMany")); }
        public static string RadioNoSong(IGuild guild) { return (Translation.GetTranslation(guild, "radioNoSong")); }
        public static string SongSkipped(IGuild guild, string songName) { return (Translation.GetTranslation(guild, "songSkipped", songName)); }
        public static string SongsSkipped(IGuild guild, int nb) { return (Translation.GetTranslation(guild, "songSkipped", nb.ToString())); }
        public static string InvalidSong(IGuild guild) { return (Translation.GetTranslation(guild, "invalidSong")); }
        public static string Current(IGuild guild) { return (Translation.GetTranslation(guild, "current")); }
        public static string Downloading(IGuild guild) { return (Translation.GetTranslation(guild, "downloading")); }
        public static string SongAdded(IGuild guild, string songName) { return (Translation.GetTranslation(guild, "songAdded", songName)); }

        /// --------------------------- VN ---------------------------
        public static string VndbHelp(IGuild guild) { return (Translation.GetTranslation(guild, "vndbHelp")); }
        public static string VndbNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "vndbNotFound")); }
        public static string AvailableEnglish(IGuild guild) { return (Translation.GetTranslation(guild, "availableEnglish")); }
        public static string AvailableWindows(IGuild guild) { return (Translation.GetTranslation(guild, "availableWindows")); }
        public static string VndbRating(IGuild guild) { return (Translation.GetTranslation(guild, "vndbRating")); }
        public static string Hours(IGuild guild, string length) { return (Translation.GetTranslation(guild, "hours", length)); }
        public static string Length(IGuild guild) { return (Translation.GetTranslation(guild, "length")); }
        public static string Tba(IGuild guild) { return (Translation.GetTranslation(guild, "tba")); }
        public static string ReleaseDate(IGuild guild) { return (Translation.GetTranslation(guild, "releaseDate")); }

        /// --------------------------- XKCD ---------------------------
        public static string XkcdWrongArg(IGuild guild) { return (Translation.GetTranslation(guild, "xkcdWrongArg")); }
        public static string XkcdWrongId(IGuild guild, int max) { return (Translation.GetTranslation(guild, "xkcdWrongId", max.ToString())); }

        /// --------------------------- Youtube ---------------------------
        public static string YoutubeHelp(IGuild guild) { return (Translation.GetTranslation(guild, "youtubeHelp")); }
        public static string YoutubeNotFound(IGuild guild) { return (Translation.GetTranslation(guild, "youtubeNotFound")); }
    }
}