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
using SanaraV2.Base;

namespace SanaraV2.GamesInfo
{
    public static class Sentences
    {
        /// --------------------------- Girls Frontline ---------------------------
        public static string GirlsFrontlineHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "girlsFrontlineHelp")); }
        public static string TDollDontExist(ulong guildId) { return (Translation.GetTranslation(guildId, "tdollDontExist")); }
        public static string ModelAndHistoricalInformation(ulong guildId) { return (Translation.GetTranslation(guildId, "modelAndHistoricalInformation")); }
        public static string Basic(ulong guildId) { return (Translation.GetTranslation(guildId, "basic")); }
        public static string Technical(ulong guildId) { return (Translation.GetTranslation(guildId, "technical")); }
        public static string ClassStr(ulong guildId) { return (Translation.GetTranslation(guildId, "class")); }
        public static string ManufTime(ulong guildId) { return (Translation.GetTranslation(guildId, "manufTime")); }
        public static string GrowthGrade(ulong guildId) { return (Translation.GetTranslation(guildId, "growthGrade")); }
        public static string OperationalEffectiveness(ulong guildId) { return (Translation.GetTranslation(guildId, "operationalEffectiveness")); }
        public static string Health(ulong guildId) { return (Translation.GetTranslation(guildId, "health")); }
        public static string Evasion(ulong guildId) { return (Translation.GetTranslation(guildId, "evasion")); }
        public static string Speed(ulong guildId) { return (Translation.GetTranslation(guildId, "speed")); }
        public static string Damage(ulong guildId) { return (Translation.GetTranslation(guildId, "damage")); }
        public static string Accuracy(ulong guildId) { return (Translation.GetTranslation(guildId, "accuracy")); }
        public static string RateOfFire(ulong guildId) { return (Translation.GetTranslation(guildId, "rateOfFire")); }
        public static string AmmoConsumption(ulong guildId) { return (Translation.GetTranslation(guildId, "ammoConsumption")); }
        public static string RationConsumption(ulong guildId) { return (Translation.GetTranslation(guildId, "rationConsumption")); }
        public static string GirlsFrontlineCompareHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "girlsFrontlineCompareHelp")); }
        public static string TooManyTDolls(ulong guildId) { return (Translation.GetTranslation(guildId, "tooManyTdolls")); }
        public static string TDollDontExistSpecify(ulong guildId, string tdoll) { return (Translation.GetTranslation(guildId, "tdollDontExistSpecify", tdoll)); }

        /// --------------------------- KanColle ---------------------------
        public static string KancolleHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "kancolleHelp")); }
        public static string ShipgirlDontExist(ulong guildId) { return (Translation.GetTranslation(guildId, "shipgirlDontExist")); }
        public static string DontDropOnMaps(ulong guildId) { return (Translation.GetTranslation(guildId, "dontDropOnMaps")); }
        public static string ShipNotReferencedMap(ulong guildId) { return (Translation.GetTranslation(guildId, "shipNotReferencedMap")); }
        public static string ShipNotReferencedConstruction(ulong guildId) { return (Translation.GetTranslation(guildId, "shipNotReferencedConstruction")); }
        public static string MapHelp(ulong guildId) { return (Translation.GetTranslation(guildId, "mapHelp")); }
        public static string OnlyNormalNodes(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyNormalNodes")); }
        public static string OnlyBossNode(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyBossNode")); }
        public static string AnyNode(ulong guildId) { return (Translation.GetTranslation(guildId, "anyNode")); }
        public static string DefaultNode(ulong guildId) { return (Translation.GetTranslation(guildId, "defaultNode")); }
        public static string Rarity(ulong guildId) { return (Translation.GetTranslation(guildId, "rarity")); }
        public static string ShipConstruction(ulong guildId) { return (Translation.GetTranslation(guildId, "shipConstruction")); }
        public static string Fuel(ulong guildId) { return (Translation.GetTranslation(guildId, "fuel")); }
        public static string Ammos(ulong guildId) { return (Translation.GetTranslation(guildId, "ammos")); }
        public static string Iron(ulong guildId) { return (Translation.GetTranslation(guildId, "iron")); }
        public static string Bauxite(ulong guildId) { return (Translation.GetTranslation(guildId, "bauxite")); }
        public static string DevMat(ulong guildId) { return (Translation.GetTranslation(guildId, "devMat")); }
        public static string Personality(ulong guildId) { return (Translation.GetTranslation(guildId, "personality")); }
        public static string Appearance(ulong guildId) { return (Translation.GetTranslation(guildId, "appearance")); }
        public static string SecondRemodel(ulong guildId) { return (Translation.GetTranslation(guildId, "secondRemodel")); }
        public static string Trivia(ulong guildId) { return (Translation.GetTranslation(guildId, "trivia")); }
        public static string InGame(ulong guildId) { return (Translation.GetTranslation(guildId, "inGame")); }
        public static string Historical(ulong guildId) { return (Translation.GetTranslation(guildId, "historical")); }
        public static string LibeccioCG(ulong guildId) { return (Translation.GetTranslation(guildId, "libeccioCG")); }
        public static string LibeccioAsAShip(ulong guildId) { return (Translation.GetTranslation(guildId, "libeccioAsAShip")); }
    }
}