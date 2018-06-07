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
using System;
using System.IO;

namespace SanaraV2
{

    public static class Sentences
    {
        public struct TranslationData
        {
            public TranslationData(string language, string content)
            {
                this.language = language;
                this.content = content;
            }

            public string language;
            public string content;
        }

        private static string GetTranslation(ulong guildId, string id, params string[] args)
        {
            string language = Program.p.guildLanguages[guildId];
            if (Program.p.translations.ContainsKey(id))
            {
                TranslationData value = Program.p.translations[id].Find(x => x.language == language);
                string elem;
                if (value.language == null)
                    elem = Program.p.translations[id].Find(x => x.language == "en").content;
                else
                    elem = value.content;
                for (int i = 0; i < args.Length; i++)
                {
                    elem = elem.Replace("{" + i + "}", args[i]);
                }
                elem = elem.Replace("\\n", Environment.NewLine);
                return (elem);
            }
            return ("An error occured in the translation submodule: The id " + id + " doesn't exist.");
        }

        /// --------------------------- ID ---------------------------
        public readonly static ulong idPikyu = 352216646267437059; // Bot that collect informations for statistics
        public readonly static ulong myId = 329664361016721408;
        public readonly static ulong ownerId = 144851584478740481;

        /// --------------------------- General ---------------------------
        public static string introductionError(ulong guildId, string userId, string userName)
        {
            return (GetTranslation(guildId, "introductionError", "<@" + ownerId + ">", userId, userName));
        }
        public static string onlyMasterStr(ulong guildId) { return (GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(ownerId).ToString())); }
        public static string onlyOwnerStr(ulong guildId, ulong guildOwnerId) { return (GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(guildOwnerId).ToString())); }
        public static string needAttachFile(ulong guildId) { return (GetTranslation(guildId, "needAttachFile")); }
        public static string chanIsNotNsfw(ulong guildId) { return (GetTranslation(guildId, "chanIsNotNsfw")); }
        public static string nothingAfterXIterations(ulong guildId, int nbIterations) { return (GetTranslation(guildId, "nothingAfterXIterations", nbIterations.ToString())); }
        public static string tooManyRequests(ulong guildId, string apiName) { return (GetTranslation(guildId, "tooManyRequests", apiName)); }
        public static string tagsNotFound(string[] tags)
        {
            if (tags.Length == 1)
                return ("I didn't find anything with the tag '" + tags[0] + "'.");
            string finalStr = "";
            for (int i = 0; i < tags.Length - 1; i++)
                finalStr += "'" + tags[i] + "', ";
            return ("I didn't find anything with the tag '" + finalStr.Substring(0, finalStr.Length - 2) + " and '" + tags[tags.Length - 1] + "'.");
        }
        public static string noCorrespondingGuild(ulong guildId) { return (GetTranslation(guildId, "noCorrespondingGuild")); }
        public static string betaFeature(ulong guildId) { return (GetTranslation(guildId, "betaFeature")); }
        public static string dontPm(ulong guildId) { return (GetTranslation(guildId, "dontPm")); }

        /// --------------------------- Communication ---------------------------
        public static string introductionMsg(ulong guildId) { return (GetTranslation(guildId, "introductionMsg")); }
        public static string hiStr(ulong guildId) { return (GetTranslation(guildId, "hi")); }
        public static string whoIAmStr(ulong guildId) { return (GetTranslation(guildId, "whoIAm")); }
        public static string userNotExist(ulong guildId) { return (GetTranslation(guildId, "userNotExist")); }

        /// --------------------------- Booru ---------------------------
        public static string fileTooBig(ulong guildId) { return (GetTranslation(guildId, "fileTooBig")); }
        public static string prepareImage(ulong guildId) { return (GetTranslation(guildId, "prepareImage")); }

        /// --------------------------- Games ---------------------------
        public static string rulesShiritori(ulong guildId) { return (GetTranslation(guildId, "rulesShiritori")); }
        public static string rulesKancolle(ulong guildId) { return (GetTranslation(guildId, "rulesKancolle")); }
        public static string rulesBooru(ulong guildId) { return (GetTranslation(guildId, "rulesBooru")); }
        public static string invalidGameName(ulong guildId) { return (GetTranslation(guildId, "invalidGameName")); }
        public static string gameAlreadyRunning(ulong guildId) { return (GetTranslation(guildId, "gameAlreadyRunning")); }

        /// --------------------------- Settings ---------------------------
        public static string createArchiveStr(ulong guildId, string currTime)
        { return ( GetTranslation(guildId, "createArchive", currTime)); }
        public static string doneStr(ulong guildId) { return (GetTranslation(guildId, "done")); }
        public static string copyingFiles(ulong guildId) { return (GetTranslation(guildId, "copyingFiles")); }
        public static string needLanguage(ulong guildId) { return (GetTranslation(guildId, "needLanguage")); }

        /// --------------------------- Linguist ---------------------------
        public static string toHiraganaHelp(ulong guildId) { return (GetTranslation(guildId, "toHiraganaHelp")); }
        public static string toRomajiHelp(ulong guildId) { return (GetTranslation(guildId, "toRomajiHelp")); }
        public static string toKatakanaHelp(ulong guildId) { return (GetTranslation(guildId, "toKatakanaHelp")); }
        public static string japaneseHelp(ulong guildId) { return (GetTranslation(guildId, "japaneseHelp")); }
        public static string translateHelp(ulong guildId) { return (GetTranslation(guildId, "translateHelp")); }
        public static string invalidLanguage(ulong guildId) { return (GetTranslation(guildId, "invalidLanguage")); }

        /// --------------------------- KanColle---------------------------
        public static string kancolleHelp(ulong guildId) { return (GetTranslation(guildId, "kancolleHelp")); }
        public static string shipgirlDontExist(ulong guildId) { return (GetTranslation(guildId, "shipgirlDontExist")); }
        public static string dontDropOnMaps(ulong guildId) { return (GetTranslation(guildId, "dontDropOnMaps")); }
        public static string shipNotReferencedMap(ulong guildId) { return (GetTranslation(guildId, "shipNotReferencedMap")); }
        public static string shipNotReferencedConstruction(ulong guildId) { return (GetTranslation(guildId, "shipNotReferencedConstruction")); }
        public static string mapHelp(ulong guildId) { return (GetTranslation(guildId, "mapHelp")); }

        /// --------------------------- VNDB ---------------------------
        public static string vndbHelp(ulong guildId) { return (GetTranslation(guildId, "vndbHelp")); }
        public static string vndbNotFound(ulong guildId) { return (GetTranslation(guildId, "vndbNotFound")); }

        /// --------------------------- Code ---------------------------
        public static string indenteHelp(ulong guildId) { return (GetTranslation(guildId, "indenteHelp")); }

        /// --------------------------- MyAnimeList ---------------------------
        public static string mangaHelp(ulong guildId) { return (GetTranslation(guildId, "mangaHelp")); }
        public static string animeHelp(ulong guildId) { return (GetTranslation(guildId, "animeHelp")); }
        public static string mangaNotFound(ulong guildId) { return (GetTranslation(guildId, "mangaNotFound")); }
        public static string animeNotFound(ulong guildId) { return (GetTranslation(guildId, "animeNotFound")); }

        /// --------------------------- Youtube ---------------------------
        public static string youtubeHelp(ulong guildId) { return (GetTranslation(guildId, "youtubeHelp")); }
        public static string youtubeNotFound(ulong guildId) { return (GetTranslation(guildId, "youtubeNotFound")); }

        /// --------------------------- Radio ---------------------------
        public static string radioAlreadyStarted(ulong guildId) { return (GetTranslation(guildId, "radioAlreadyStarted")); }
        public static string radioNeedChannel(ulong guildId) { return (GetTranslation(guildId, "radioNeedChannel")); }
        public static string radioNeedArg(ulong guildId) { return (GetTranslation(guildId, "radioNeedArg")); }
        public static string radioNotStarted(ulong guildId) { return (GetTranslation(guildId, "radioNotStarted")); }
        public static string radioAlreadyInList(ulong guildId) { return (GetTranslation(guildId, "radioAlreadyInList")); }
        public static string radioTooMany(ulong guildId) { return (GetTranslation(guildId, "radioTooMany")); }
        public static string radioNoSong(ulong guildId) { return (GetTranslation(guildId, "radioNoSong")); }
        public static string cantDownload(ulong guildId) { return (GetTranslation(guildId, "cantDownload")); }
        public static string songSkipped(ulong guildId, string songName) { return (GetTranslation(guildId, "songSkipped", songName)); }


        /// --------------------------- XKCD ---------------------------
        public static string xkcdWrongArg(ulong guildId) { return (GetTranslation(guildId, "xkcdWrongArg")); }
        public static string xkcdWrongId(ulong guildId, int max) { return (GetTranslation(guildId, "xkcdWrongId", max.ToString())); }

        /// --------------------------- Help ---------------------------
        private static string noCommandAvailable(ulong guildId) { return (GetTranslation(guildId, "noCommandAvailable")); }
        public static Embed help(ulong guildId, bool isChanNsfw)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = GetTranslation(guildId, "help"),
                Color = Color.Purple
            };
            embed.AddField(GetTranslation(guildId, "animeMangaModuleName"), GetTranslation(guildId, "animeMangaModuleDescription"));
            embed.AddField(GetTranslation(guildId, "booruModuleName"), GetTranslation(guildId, "booruModuleDescription")
                + ((isChanNsfw) ? (GetTranslation(guildId, "booruModuleDescription2")) : ("")));
            embed.AddField(GetTranslation(guildId, "codeModuleName"), GetTranslation(guildId, "codeModuleDescription"));
            embed.AddField(GetTranslation(guildId, "communicationModuleName"), GetTranslation(guildId, "communicationModuleDescription"));
            embed.AddField(GetTranslation(guildId, "debugModuleName"), GetTranslation(guildId, "debugModuleDescription"));
            embed.AddField(GetTranslation(guildId, "doujinshiModuleName"),
                ((isChanNsfw) ? (GetTranslation(guildId, "doujinshiModuleDescription"))
                              : (noCommandAvailable(guildId))));
            embed.AddField(GetTranslation(guildId, "gameModuleName"), GetTranslation(guildId, "gameModuleDescription")
                + ((isChanNsfw) ? (GetTranslation(guildId, "gameModuleDescription2")) : ("")));
            embed.AddField(GetTranslation(guildId, "googleShortenerModuleName"),
                ((isChanNsfw) ? (GetTranslation(guildId, "googleShortenerDescription"))
                              : (noCommandAvailable(guildId))));
            embed.AddField(GetTranslation(guildId, "kantaiCollectionModuleName"), GetTranslation(guildId, "kantaiCollectionModuleDescription"));
            embed.AddField(GetTranslation(guildId, "linguisticModuleName"), GetTranslation(guildId, "linguisticModuleDescription"));
            embed.AddField(GetTranslation(guildId, "radioModuleName"), GetTranslation(guildId, "radioModuleDescription"));
            embed.AddField(GetTranslation(guildId, "settingsModuleName"), noCommandAvailable(guildId));
            embed.AddField(GetTranslation(guildId, "visualNovelModuleName"), GetTranslation(guildId, "visualNovelModuleDescription"));
            embed.AddField(GetTranslation(guildId, "xkcdModuleName"), GetTranslation(guildId, "xkcdModuleDescription"));
            embed.AddField(GetTranslation(guildId, "youtubeModuleName"), GetTranslation(guildId, "youtubeModuleDescription"));
            return (embed.Build());
        }
    }
}