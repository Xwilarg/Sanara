
using SanaraV2.Base;
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
namespace SanaraV2.NSFW
{
    public static class Sentences
    {
        /// --------------------------- Booru ---------------------------
        public static string FileTooBig(ulong guildId) { return (Translation.GetTranslation(guildId, "fileTooBig")); }
        public static string PrepareImage(ulong guildId) { return (Translation.GetTranslation(guildId, "prepareImage")); }
        public static string MoreNotTagged(ulong guildId) { return (Translation.GetTranslation(guildId, "moreNotTagged")); }
        public static string AnimeFromOriginal(ulong guildId) { return (Translation.GetTranslation(guildId, "animeFromOriginal")); }
        public static string AnimeNotTagged(ulong guildId) { return (Translation.GetTranslation(guildId, "animeNotTagged")); }
        public static string AnimeFrom(ulong guildId) { return (Translation.GetTranslation(guildId, "animeFrom")); }
        public static string AnimeTagUnknowed(ulong guildId) { return (Translation.GetTranslation(guildId, "animeTagUnknowed")); }
        public static string CharacterTagUnknowed(ulong guildId) { return (Translation.GetTranslation(guildId, "characterTagUnknowed")); }
        public static string CharacterNotTagged(ulong guildId) { return (Translation.GetTranslation(guildId, "characterNotTagged")); }
        public static string CharacterIs(ulong guildId) { return (Translation.GetTranslation(guildId, "characterIs")); }
        public static string CharacterAre(ulong guildId) { return (Translation.GetTranslation(guildId, "characterAre")); }
        public static string ArtistFrom(ulong guildId) { return (Translation.GetTranslation(guildId, "artistFrom")); }
        public static string ArtistNotTagged(ulong guildId) { return (Translation.GetTranslation(guildId, "artistNotTagged")); }
    }
}