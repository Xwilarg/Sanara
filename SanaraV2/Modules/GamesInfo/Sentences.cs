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

namespace SanaraV2.Modules.GamesInfo
{
    public static class Sentences
    {
        ///  --------------------------- Arknights ---------------------------
        public static string ArknightsHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "arknightsHelp")); }
        public static string OperatorDontExist(ulong guildId) { return (Translation.GetTranslation(guildId, "operatorDontExist")); }

        /// --------------------------- KanColle ---------------------------
        public static string KancolleHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "kancolleHelp")); }
        public static string ShipgirlDontExist(ulong guildId) { return (Translation.GetTranslation(guildId, "shipgirlDontExist")); }
        public static string DontDropOnMaps(ulong guildId) { return (Translation.GetTranslation(guildId, "dontDropOnMaps")); }
        public static string ShipNotReferencedMap(ulong guildId) { return (Translation.GetTranslation(guildId, "shipNotReferencedMap")); }
        public static string ShipNotReferencedConstruction(ulong guildId) { return (Translation.GetTranslation(guildId, "shipNotReferencedConstruction")); }
        public static string OnlyNormalNodes(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyNormalNodes")); }
        public static string OnlyBossNode(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyBossNode")); }
        public static string AnyNode(ulong guildId) { return (Translation.GetTranslation(guildId, "anyNode")); }
        public static string Rarity(ulong guildId) { return (Translation.GetTranslation(guildId, "rarity")); }
        public static string Fuel(ulong guildId) { return (Translation.GetTranslation(guildId, "fuel")); }
        public static string Ammos(ulong guildId) { return (Translation.GetTranslation(guildId, "ammos")); }
        public static string Iron(ulong guildId) { return (Translation.GetTranslation(guildId, "iron")); }
        public static string Bauxite(ulong guildId) { return (Translation.GetTranslation(guildId, "bauxite")); }
        public static string DevMat(ulong guildId) { return (Translation.GetTranslation(guildId, "devMat")); }
        public static string ConstructionDrop(ulong guildId) { return (Translation.GetTranslation(guildId, "constructionDrop")); }
        public static string MapDrop(ulong guildId) { return (Translation.GetTranslation(guildId, "mapDrop")); }
    }
}