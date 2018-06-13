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
            string language = (guildId == 0) ? ("en") : (Program.p.guildLanguages[guildId]);
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
        public readonly static ulong myId = 455059707313258499;
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
        public static string noApiKey(ulong guildId) { return (GetTranslation(guildId, "noApiKey")); }
        public static string timeSeconds(ulong guildId, string seconds) { return (GetTranslation(guildId, "timeSeconds", seconds)); }
        public static string timeMinutes(ulong guildId, string minutes, string seconds) { return (GetTranslation(guildId, "timeMinutes", minutes, seconds)); }
        public static string timeHours(ulong guildId, string hours, string minutes, string seconds) { return (GetTranslation(guildId, "timeHours", hours, minutes, seconds)); }
        public static string timeDays(ulong guildId, string days, string hours, string minutes, string seconds) { return (GetTranslation(guildId, "timeDays", days, hours, minutes, seconds)); }

        /// --------------------------- Parts ---------------------------
        public static string andStr(ulong guildId) { return (GetTranslation(guildId, "and")); }
        public static string dateHourFormat(ulong guildId) { return (GetTranslation(guildId, "dateHourFormat")); }
        public static string orStr(ulong guildId) { return (GetTranslation(guildId, "or")); }

        /// --------------------------- Communication ---------------------------
        public static string introductionMsg(ulong guildId) { return (GetTranslation(guildId, "introductionMsg")); }
        public static string hiStr(ulong guildId) { return (GetTranslation(guildId, "hi")); }
        public static string whoIAmStr(ulong guildId) { return (GetTranslation(guildId, "whoIAm")); }
        public static string userNotExist(ulong guildId) { return (GetTranslation(guildId, "userNotExist")); }
        public static string username(ulong guildId) { return (GetTranslation(guildId, "username")); }
        public static string nickname(ulong guildId) { return (GetTranslation(guildId, "nickname")); }
        public static string accountCreation(ulong guildId) { return (GetTranslation(guildId, "accountCreation")); }
        public static string guildJoined(ulong guildId) { return (GetTranslation(guildId, "guildJoined")); }
        public static string creator(ulong guildId) { return (GetTranslation(guildId, "creator")); }
        public static string uptime(ulong guildId) { return (GetTranslation(guildId, "uptime")); }
        public static string website(ulong guildId) { return (GetTranslation(guildId, "website")); }
        public static string officialGuild(ulong guildId) { return (GetTranslation(guildId, "officialGuild")); }
        public static string roles(ulong guildId) { return (GetTranslation(guildId, "roles")); }
        public static string noRole(ulong guildId) { return (GetTranslation(guildId, "noRole")); }
        public static string latestVersion(ulong guildId) { return (GetTranslation(guildId, "latestVersion")); }
        public static string numberGuilds(ulong guildId) { return (GetTranslation(guildId, "numberGuilds")); }

        /// --------------------------- Booru ---------------------------
        public static string fileTooBig(ulong guildId) { return (GetTranslation(guildId, "fileTooBig")); }
        public static string prepareImage(ulong guildId) { return (GetTranslation(guildId, "prepareImage")); }
        public static string moreNotTagged(ulong guildId) { return (GetTranslation(guildId, "moreNotTagged")); }
        public static string animeFromOriginal(ulong guildId) { return (GetTranslation(guildId, "animeFromOriginal")); }
        public static string animeNotTagged(ulong guildId) { return (GetTranslation(guildId, "animeNotTagged")); }
        public static string animeFrom(ulong guildId) { return (GetTranslation(guildId, "animeFrom")); }
        public static string animeTagUnknowed(ulong guildId) { return (GetTranslation(guildId, "animeTagUnknowed")); }
        public static string characterTagUnknowed(ulong guildId) { return (GetTranslation(guildId, "characterTagUnknowed")); }
        public static string characterNotTagged(ulong guildId) { return (GetTranslation(guildId, "characterNotTagged")); }
        public static string characterIs(ulong guildId) { return (GetTranslation(guildId, "characterIs")); }
        public static string characterAre(ulong guildId) { return (GetTranslation(guildId, "characterAre")); }

        /// --------------------------- Games ---------------------------
        public static string rulesShiritori(ulong guildId) { return (GetTranslation(guildId, "rulesShiritori")); }
        public static string rulesKancolle(ulong guildId) { return (GetTranslation(guildId, "rulesKancolle")); }
        public static string rulesBooru(ulong guildId) { return (GetTranslation(guildId, "rulesBooru")); }
        public static string invalidGameName(ulong guildId) { return (GetTranslation(guildId, "invalidGameName")); }
        public static string gameAlreadyRunning(ulong guildId) { return (GetTranslation(guildId, "gameAlreadyRunning")); }
        public static string timeoutGame(ulong guildId, string answer) { return (GetTranslation(guildId, "timeoutGame", answer)); }
        public static string newBestScore(ulong guildId, string lastScore, string newScore) { return (GetTranslation(guildId, "newBestScore", lastScore, newScore)); }
        public static string equalizedScore(ulong guildId, string score) { return (GetTranslation(guildId, "equalizedScore", score)); }
        public static string didntBeatScore(ulong guildId, string lastScore, string newScore) { return (GetTranslation(guildId, "didntBeatScore", lastScore, newScore)); }
        public static string shiritoriNoWord(ulong guildId) { return (GetTranslation(guildId, "shiritoriNoWord")); }
        public static string waitPlay(ulong guildId) { return (GetTranslation(guildId, "waitPlay")); }
        public static string onlyHiraganaKatakanaRomaji(ulong guildId) { return (GetTranslation(guildId, "onlyHiraganaKatakanaRomaji")); }
        public static string shiritoriNotNoun(ulong guildId) { return (GetTranslation(guildId, "shiritoriNotNoun")); }
        public static string shiritoriDoesntExist(ulong guildId) { return (GetTranslation(guildId, "shiritoriDoesntExist")); }
        public static string shiritoriMustBegin(ulong guildId, string beginHiragana, string beginRomaji) { return (GetTranslation(guildId, "shiritoriMustBegin", beginHiragana, beginRomaji)); }
        public static string shiritoriAlreadySaid(ulong guildId) { return (GetTranslation(guildId, "shiritoriAlreadySaid")); }
        public static string shiritoriEndWithN(ulong guildId) { return (GetTranslation(guildId, "shiritoriEndWithN")); }
        public static string shiritoriNoMoreWord(ulong guildId) { return (GetTranslation(guildId, "shiritoriNoMoreWord")); }
        public static string shiritoriSuggestion(ulong guildId, string suggestionHiragana, string suggestionRomaji, string suggestionTranslation) { return (GetTranslation(guildId, "shiritoriSuggestion", suggestionHiragana, suggestionRomaji, suggestionTranslation)); }
        public static string waitImage(ulong guildId) { return (GetTranslation(guildId, "waitImage")); }
        public static string kancolleGuessDontExist(ulong guildId) { return (GetTranslation(guildId, "kancolleGuessDontExist")); }
        public static string guessGood(ulong guildId) { return (GetTranslation(guildId, "guessGood")); }
        public static string kancolleGuessBad(ulong guildId, string attempt) { return (GetTranslation(guildId, "kancolleGuessBad", attempt)); }
        public static string waitImages(ulong guildId) { return (GetTranslation(guildId, "waitImages")); }
        public static string booruGuessClose(ulong guildId, string attempt) { return (GetTranslation(guildId, "booruGuessClose", attempt)); }
        public static string booruGuessBad(ulong guildId, string attempt) { return (GetTranslation(guildId, "booruGuessBad", attempt)); }
        public static string lostStr(ulong guildId) { return (GetTranslation(guildId, "lost")); }
        public static string invalidDifficulty(ulong guildId) { return (GetTranslation(guildId, "invalidDifficulty")); }

        /// --------------------------- Settings ---------------------------
        public static string createArchiveStr(ulong guildId, string currTime)
        { return ( GetTranslation(guildId, "createArchive", currTime)); }
        public static string doneStr(ulong guildId) { return (GetTranslation(guildId, "done")); }
        public static string copyingFiles(ulong guildId) { return (GetTranslation(guildId, "copyingFiles")); }
        public static string needLanguage(ulong guildId) { return (GetTranslation(guildId, "needLanguage")); }
        public static string prefixRemoved(ulong guildId) { return (GetTranslation(guildId, "prefixRemoved")); }

        /// --------------------------- Linguist ---------------------------
        public static string toHiraganaHelp(ulong guildId) { return (GetTranslation(guildId, "toHiraganaHelp")); }
        public static string toRomajiHelp(ulong guildId) { return (GetTranslation(guildId, "toRomajiHelp")); }
        public static string toKatakanaHelp(ulong guildId) { return (GetTranslation(guildId, "toKatakanaHelp")); }
        public static string japaneseHelp(ulong guildId) { return (GetTranslation(guildId, "japaneseHelp")); }
        public static string translateHelp(ulong guildId) { return (GetTranslation(guildId, "translateHelp")); }
        public static string invalidLanguage(ulong guildId) { return (GetTranslation(guildId, "invalidLanguage")); }
        public static string giveJapaneseTranslations(ulong guildId, string word) { return (GetTranslation(guildId, "giveJapaneseTranslations", word)); }
        public static string noJapaneseTranslation(ulong guildId, string word) { return (GetTranslation(guildId, "noJapaneseTranslation", word)); }
        public static string meaning(ulong guildId) { return (GetTranslation(guildId, "meaning")); }

        /// --------------------------- KanColle---------------------------
        public static string kancolleHelp(ulong guildId) { return (GetTranslation(guildId, "kancolleHelp")); }
        public static string shipgirlDontExist(ulong guildId) { return (GetTranslation(guildId, "shipgirlDontExist")); }
        public static string dontDropOnMaps(ulong guildId) { return (GetTranslation(guildId, "dontDropOnMaps")); }
        public static string shipNotReferencedMap(ulong guildId) { return (GetTranslation(guildId, "shipNotReferencedMap")); }
        public static string shipNotReferencedConstruction(ulong guildId) { return (GetTranslation(guildId, "shipNotReferencedConstruction")); }
        public static string mapHelp(ulong guildId) { return (GetTranslation(guildId, "mapHelp")); }
        public static string onlyNormalNodes(ulong guildId) { return (GetTranslation(guildId, "onlyNormalNodes")); }
        public static string onlyBossNode(ulong guildId) { return (GetTranslation(guildId, "onlyBossNode")); }
        public static string anyNode(ulong guildId) { return (GetTranslation(guildId, "anyNode")); }
        public static string defaultNode(ulong guildId) { return (GetTranslation(guildId, "defaultNode")); }
        public static string rarity(ulong guildId) { return (GetTranslation(guildId, "rarity")); }
        public static string shipConstruction(ulong guildId) { return (GetTranslation(guildId, "shipConstruction")); }
        public static string fuel(ulong guildId) { return (GetTranslation(guildId, "fuel")); }
        public static string ammos(ulong guildId) { return (GetTranslation(guildId, "ammos")); }
        public static string iron(ulong guildId) { return (GetTranslation(guildId, "iron")); }
        public static string bauxite(ulong guildId) { return (GetTranslation(guildId, "bauxite")); }
        public static string devMat(ulong guildId) { return (GetTranslation(guildId, "devMat")); }
        public static string personality(ulong guildId) { return (GetTranslation(guildId, "personality")); }
        public static string appearance(ulong guildId) { return (GetTranslation(guildId, "appearance")); }
        public static string secondRemodel(ulong guildId) { return (GetTranslation(guildId, "secondRemodel")); }
        public static string trivia(ulong guildId) { return (GetTranslation(guildId, "trivia")); }

        /// --------------------------- VNDB ---------------------------
        public static string vndbHelp(ulong guildId) { return (GetTranslation(guildId, "vndbHelp")); }
        public static string vndbNotFound(ulong guildId) { return (GetTranslation(guildId, "vndbNotFound")); }
        public static string availableEnglish(ulong guildId) { return (GetTranslation(guildId, "availableEnglish")); }
        public static string notAvailableEnglish(ulong guildId) { return (GetTranslation(guildId, "notAvailableEnglish")); }
        public static string availableWindows(ulong guildId) { return (GetTranslation(guildId, "availableWindows")); }
        public static string notAvailableWindows(ulong guildId) { return (GetTranslation(guildId, "notAvailableWindows")); }
        public static string vndbRating(ulong guildId, string score) { return (GetTranslation(guildId, "vndbRating", score)); }

        /// --------------------------- Code ---------------------------
        public static string indenteHelp(ulong guildId) { return (GetTranslation(guildId, "indenteHelp")); }

        /// --------------------------- MyAnimeList ---------------------------
        public static string mangaHelp(ulong guildId) { return (GetTranslation(guildId, "mangaHelp")); }
        public static string animeHelp(ulong guildId) { return (GetTranslation(guildId, "animeHelp")); }
        public static string mangaNotFound(ulong guildId) { return (GetTranslation(guildId, "mangaNotFound")); }
        public static string animeNotFound(ulong guildId) { return (GetTranslation(guildId, "animeNotFound")); }
        public static string animeInfos(ulong guildId, string type, string status, string episodes) { return (GetTranslation(guildId, "animeInfos", type, status, episodes)); }
        public static string animeScore(ulong guildId, string score) { return (GetTranslation(guildId, "animeScore", score)); }
        public static string synopsis(ulong guildId) { return (GetTranslation(guildId, "synopsis")); }

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
        public static string current(ulong guildId) { return (GetTranslation(guildId, "current")); }
        public static string downloading(ulong guildId) { return (GetTranslation(guildId, "downloading")); }
        public static string songAdded(ulong guildId, string song) { return (GetTranslation(guildId, "songAdded", song)); }


        /// --------------------------- XKCD ---------------------------
        public static string xkcdWrongArg(ulong guildId) { return (GetTranslation(guildId, "xkcdWrongArg")); }
        public static string xkcdWrongId(ulong guildId, int max) { return (GetTranslation(guildId, "xkcdWrongId", max.ToString())); }

        /// --------------------------- Debug ---------------------------
        public static string general(ulong guildId) { return (GetTranslation(guildId, "general")); }
        public static string creationDate(ulong guildId) { return (GetTranslation(guildId, "creationDate")); }
        public static string messagesReceived(ulong guildId) { return (GetTranslation(guildId, "messagesReceived")); }
        public static string userMoreMessages(ulong guildId, string username, string nbMsgs) { return (GetTranslation(guildId, "userMoreMessages", username, nbMsgs)); }
        public static string userKnown(ulong guildId) { return (GetTranslation(guildId, "userKnown")); }
        public static string alreadySpoke(ulong guildId, string nbSpoke) { return (GetTranslation(guildId, "alreadySpoke", nbSpoke)); }
        public static string guildsAvailable(ulong guildId) { return (GetTranslation(guildId, "guildsAvailable")); }
        public static string translation(ulong guildId) { return (GetTranslation(guildId, "translation")); }
        public static string definition(ulong guildId) { return (GetTranslation(guildId, "definition")); }
        public static string randomURL(ulong guildId) { return (GetTranslation(guildId, "randomURL")); }
        public static string errorStr(ulong guildId) { return (GetTranslation(guildId, "error")); }
        public static string okStr(ulong guildId) { return (GetTranslation(guildId, "ok")); }
        public static string unitTests(ulong guildId) { return (GetTranslation(guildId, "unitTests")); }

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
                + ((isChanNsfw) ? (GetTranslation(guildId, "gameModuleDescription2")) : (""))
                + Environment.NewLine + GetTranslation(guildId, "gameModuleDescription3"));
            embed.AddField(GetTranslation(guildId, "googleShortenerModuleName"),
                ((isChanNsfw) ? (GetTranslation(guildId, "googleShortenerModuleDescription"))
                              : (noCommandAvailable(guildId))));
            embed.AddField(GetTranslation(guildId, "kantaiCollectionModuleName"), GetTranslation(guildId, "kantaiCollectionModuleDescription"));
            embed.AddField(GetTranslation(guildId, "linguisticModuleName"), GetTranslation(guildId, "linguisticModuleDescription"));
            embed.AddField(GetTranslation(guildId, "radioModuleName"), GetTranslation(guildId, "radioModuleDescription"));
            embed.AddField(GetTranslation(guildId, "settingsModuleName"), GetTranslation(guildId, "settingsModuleDescription"));
            embed.AddField(GetTranslation(guildId, "visualNovelModuleName"), GetTranslation(guildId, "visualNovelModuleDescription"));
            embed.AddField(GetTranslation(guildId, "xkcdModuleName"), GetTranslation(guildId, "xkcdModuleDescription"));
            embed.AddField(GetTranslation(guildId, "youtubeModuleName"), GetTranslation(guildId, "youtubeModuleDescription") + Environment.NewLine + Environment.NewLine
                + ((isChanNsfw) ? ("") : (GetTranslation(guildId, "nsfwForFull"))));
            return (embed.Build());
        }
    }
}