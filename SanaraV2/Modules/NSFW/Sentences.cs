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
        public static string InvalidExtension(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidExtension")); }
        public static string InvalidId(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidId")); }
        public static string Source(ulong guildId) { return (Translation.GetTranslation(guildId, "source")); }
        public static string Sources(ulong guildId) { return (Translation.GetTranslation(guildId, "sources")); }
        public static string Character(ulong guildId) { return (Translation.GetTranslation(guildId, "character")); }
        public static string Characters(ulong guildId) { return (Translation.GetTranslation(guildId, "characters")); }
        public static string Artist(ulong guildId) { return (Translation.GetTranslation(guildId, "artist")); }
        public static string Artists(ulong guildId) { return (Translation.GetTranslation(guildId, "artists")); }
        public static string ImageInfo(ulong guildId, string id) { return (Translation.GetTranslation(guildId, "imageInfo", id)); }
    }
}