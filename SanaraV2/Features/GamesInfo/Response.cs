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
using System.Collections.Generic;

namespace SanaraV2.Features.GamesInfo
{
    public static class Response
    {
        public class ArknightsCharac
        {
            public string name;
            public string imgUrl;
            public string type;
            public string[] tags;
            public string wikiUrl;
            public ArknightsSkill[] skills;
            public string description;
        }

        public struct ArknightsSkill
        {
            public string name;
            public string description;
        }

        public class DropMap
        {
            public int? rarity;
            public Dictionary<string, DropMapLocation> dropMap;
        }

        public class DropConstruction
        {
            public ConstructionElem[] elems;
        }

        public class Charac
        {
            public string name;
            public string thumbnailUrl;
            public List<Tuple<string, string>> allCategories;
        }

        public enum DropMapLocation
        {
            Anywhere,
            BossOnly,
            NormalOnly
        }

        public struct ConstructionElem
        {
            public string chance;
            public string fuel;
            public string ammos;
            public string iron;
            public string bauxite;
            public string devMat;
        }
    }
}
