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

namespace SanaraV2.Modules.GamesInfo
{
    public static class Sentences
    {
        ///  --------------------------- Arknights ---------------------------
        public static string ArknightsHelp(IGuild guild) { return (Translation.GetTranslation(guild, "arknightsHelp")); }
        public static string OperatorDontExist(IGuild guild) { return (Translation.GetTranslation(guild, "operatorDontExist")); }

        /// --------------------------- KanColle ---------------------------
        public static string KancolleHelp(IGuild guild) { return (Translation.GetTranslation(guild, "kancolleHelp")); }
        public static string ShipgirlDontExist(IGuild guild) { return (Translation.GetTranslation(guild, "shipgirlDontExist")); }
        public static string DontDropOnMaps(IGuild guild) { return (Translation.GetTranslation(guild, "dontDropOnMaps")); }
        public static string ShipNotReferencedMap(IGuild guild) { return (Translation.GetTranslation(guild, "shipNotReferencedMap")); }
        public static string ShipNotReferencedConstruction(IGuild guild) { return (Translation.GetTranslation(guild, "shipNotReferencedConstruction")); }
        public static string OnlyNormalNodes(IGuild guild) { return (Translation.GetTranslation(guild, "onlyNormalNodes")); }
        public static string OnlyBossNode(IGuild guild) { return (Translation.GetTranslation(guild, "onlyBossNode")); }
        public static string AnyNode(IGuild guild) { return (Translation.GetTranslation(guild, "anyNode")); }
        public static string Rarity(IGuild guild) { return (Translation.GetTranslation(guild, "rarity")); }
        public static string Fuel(IGuild guild) { return (Translation.GetTranslation(guild, "fuel")); }
        public static string Ammos(IGuild guild) { return (Translation.GetTranslation(guild, "ammos")); }
        public static string Iron(IGuild guild) { return (Translation.GetTranslation(guild, "iron")); }
        public static string Bauxite(IGuild guild) { return (Translation.GetTranslation(guild, "bauxite")); }
        public static string DevMat(IGuild guild) { return (Translation.GetTranslation(guild, "devMat")); }
        public static string ConstructionDrop(IGuild guild) { return (Translation.GetTranslation(guild, "constructionDrop")); }
        public static string MapDrop(IGuild guild) { return (Translation.GetTranslation(guild, "mapDrop")); }
    }
}