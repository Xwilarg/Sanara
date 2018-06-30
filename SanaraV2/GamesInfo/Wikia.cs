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
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace SanaraV2.GamesInfo
{
    public static class Wikia
    {
        public enum WikiaType
        {
            KanColle,
            GirlsFrontline
        }

        /// <summary>
        /// Download thumbnail given an URL
        /// </summary>
        /// <param name="fullUrl">The cropped URL given byGetShipInfos</param>
        /// <returns>The path to the file downloaded</returns>
        public static string DownloadCharacThumbnail(string fullUrl)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                int currentTime = Convert.ToInt32(DateTime.Now.ToString("HHmmss"));
                string shipName = "shipgirl" + currentTime + ".jpg";
                wc.DownloadFile(fullUrl, shipName);
                return (shipName);
            }
        }

        public struct CharacInfo
        {
            public CharacInfo(string id, string thumbnail, string infos, string name)
            {
                this.id = id;
                this.thumbnail = thumbnail;
                this.infos = infos;
                this.name = name;
            }
            public string id;
            public string thumbnail;
            public string infos;
            public string name;
        }

        /// <summary>
        /// Return informations about a character
        /// </summary>
        /// <param name="shipName">Name of the ship</param>
        /// <param name="id">Return the id of the ship on KanColle wikia</param>
        /// <param name="thumbnail">Return the thumbnail of the ship</param>
        public static CharacInfo? GetCharacInfos(string name, WikiaType wikia)
        {
            try
            {
                using (WebClient w = new WebClient())
                {
                    w.Encoding = Encoding.UTF8;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    List<string> finalStr = new List<string> { "" };
                    string json = w.DownloadString("https://" + WikiaTypeToString(wikia) + ".wikia.com/api/v1/Search/List?query=" + name + "&limit=1");
                    string id = Utilities.GetElementXml("\"id\":", json, ',');
                    string title = Utilities.GetElementXml("\"title\":\"", json, '"');
                    json = w.DownloadString("http://" + WikiaTypeToString(wikia) + ".wikia.com/api/v1/Articles/Details?ids=" + id);
                    string thumbnail = Utilities.GetElementXml("\"thumbnail\":\"", json, '"');
                    bool isJpg = thumbnail.Contains(".jpg");
                    thumbnail = thumbnail.Split(new string[] { ((isJpg) ? (".jpg") : (".png")) }, StringSplitOptions.None)[0] + ((isJpg) ? (".jpg") : (".png"));
                    thumbnail = thumbnail.Replace("\\", "");
                    if (Utilities.CleanWord(title.ToUpper()) != Utilities.CleanWord(name.ToUpper()))
                        return (null);
                    string url = "http://" + WikiaTypeToString(wikia) + ".wikia.com/wiki/" + title + "?action=raw";
                    json = w.DownloadString(url);
                    if (!CheckPageHeader(wikia, Utilities.GetElementXml("{{", json.Split('\n', '|')[0], '}')))
                        return (null);
                    return (new CharacInfo(id, thumbnail, json, title));
                }
            }
            catch (WebException ex)
            {
                HttpWebResponse code = ex.Response as HttpWebResponse;
                if (code.StatusCode == HttpStatusCode.NotFound)
                    return (null);
                else
                    throw ex;
            }
        }

        public static List<string> FillWikiaInfos(string shipId, ulong guildId, Dictionary<string, Func<ulong, string> > categories, WikiaType wikia)
        {
            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                List<string> finalStr = new List<string>() {
                    ""
                };
                string url = "http://" + WikiaTypeToString(wikia) + ".wikia.com/api/v1/Articles/AsSimpleJson?id=" + shipId;
                string json = wc.DownloadString(url);
                string[] jsonInside = json.Split(new string[] { "\"title\"" }, StringSplitOptions.None);
                int currI = 0;
                foreach (var s in categories)
                    GetWikiaInfo(s.Key, ref currI, ref finalStr, jsonInside, s.Value(guildId));
                return (finalStr.Select(x => Regex.Replace(x, @"\\[Uu]([0-9A-Fa-f]{4})", m => char.ToString((char)ushort.Parse(m.Groups[1].Value, NumberStyles.AllowHexSpecifier)))).ToList());
            }
        }

        /// <summary>
        /// Return  informations about a ship on a categorie from KanColle wikia
        /// </summary>
        /// <param name="categorie">Name of the categorie on the JSON</param>
        /// <param name="currI">Counter for finalStr</param>
        /// <param name="finalStr">List containing the informations</param>
        /// <param name="jsonInside">Informations to parse</param>
        /// <param name="relatedSentence">Name of the categorie to display</param>
        private static void GetWikiaInfo(string categorie, ref int currI, ref List<string> finalStr, string[] jsonInside, string relatedSentence)
        {
            foreach (string s in jsonInside)
            {
                if (s.Contains(categorie))
                {
                    finalStr[currI] += Environment.NewLine + "**" + relatedSentence + "**" + Environment.NewLine;
                    string[] allExplanations = s.Split(new string[] { "\"te" }, StringSplitOptions.None);
                    foreach (string str in allExplanations)
                    {
                        string per = Utilities.GetElementXml("xt\":\"", str, '"');
                        if (per != "")
                        {
                            if (finalStr[currI].Length + per.Length > 1500)
                            {
                                currI++;
                                finalStr.Add("");
                            }
                            finalStr[currI] += per + Environment.NewLine;
                        }
                    }
                    break;
                }
            }
        }

        private static bool CheckPageHeader(WikiaType wikia, string header)
        {
            switch (wikia)
            {
                case WikiaType.KanColle:
                    return (header == "ShipPageHeader" || header == "Ship/Header");

                case WikiaType.GirlsFrontline:
                    return (header == "TDOLL PAGE");

                default:
                    throw new ArgumentException("Invalid WikiaType.");
            }
        }

        private static string WikiaTypeToString(WikiaType wikia)
        {
            return (wikia.ToString().ToLower());
        }
    }
}
