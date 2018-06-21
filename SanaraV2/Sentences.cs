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
using System.Linq;

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
            if (guildId == 0) // GuildId is equal to 0 for unit tests
                return (id);
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
        public readonly static ulong myId = (Program.p.client == null) ? (0) : (Program.p.client.CurrentUser.Id);
        public readonly static ulong ownerId = 144851584478740481;

        /// --------------------------- General ---------------------------
        public static string IntroductionError(ulong guildId, string userId, string userName)
        {
            return (GetTranslation(guildId, "introductionError", "<@" + ownerId + ">", userId, userName));
        }
        public static string OnlyMasterStr(ulong guildId) { return (GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(ownerId).ToString())); }
        public static string OnlyOwnerStr(ulong guildId, ulong guildOwnerId) { return (GetTranslation(guildId, "onlyMaster", Program.p.client.GetGuild(guildId).GetUser(guildOwnerId).ToString())); }
        public static string NeedAttachFile(ulong guildId) { return (GetTranslation(guildId, "needAttachFile")); }
        public static string ChanIsNotNsfw(ulong guildId) { return (GetTranslation(guildId, "chanIsNotNsfw")); }
        public static string NothingAfterXIterations(ulong guildId, int nbIterations) { return (GetTranslation(guildId, "nothingAfterXIterations", nbIterations.ToString())); }
        public static string TooManyRequests(ulong guildId, string apiName) { return (GetTranslation(guildId, "tooManyRequests", apiName)); }
        public static string TagsNotFound(string[] tags)
        {
            if (tags.Length == 1)
                return ("I didn't find anything with the tag '" + tags[0] + "'.");
            string finalStr = String.Join(", ", tags.ToList().Skip(1).Select(x => "'" + x + "'"));
            return ("I didn't find anything with the tag '" + finalStr + " and '" + tags[tags.Length - 1] + "'.");
        }
        public static string NoCorrespondingGuild(ulong guildId) { return (GetTranslation(guildId, "noCorrespondingGuild")); }
        public static string BetaFeature(ulong guildId) { return (GetTranslation(guildId, "betaFeature")); }
        public static string DontPm(ulong guildId) { return (GetTranslation(guildId, "dontPm")); }
        public static string NoApiKey(ulong guildId) { return (GetTranslation(guildId, "noApiKey")); }
        public static string TimeSeconds(ulong guildId, string seconds) { return (GetTranslation(guildId, "timeSeconds", seconds)); }
        public static string TimeMinutes(ulong guildId, string minutes, string seconds) { return (GetTranslation(guildId, "timeMinutes", minutes, seconds)); }
        public static string TimeHours(ulong guildId, string hours, string minutes, string seconds) { return (GetTranslation(guildId, "timeHours", hours, minutes, seconds)); }
        public static string TimeDays(ulong guildId, string days, string hours, string minutes, string seconds) { return (GetTranslation(guildId, "timeDays", days, hours, minutes, seconds)); }

        /// --------------------------- Parts ---------------------------
        public static string AndStr(ulong guildId) { return (GetTranslation(guildId, "and")); }
        public static string DateHourFormat(ulong guildId) { return (GetTranslation(guildId, "dateHourFormat")); }
        public static string OrStr(ulong guildId) { return (GetTranslation(guildId, "or")); }

        /// --------------------------- Communication ---------------------------
        public static string IntroductionMsg(ulong guildId) { return (GetTranslation(guildId, "introductionMsg")); }
        public static string HiStr(ulong guildId) { return (GetTranslation(guildId, "hi")); }
        public static string WhoIAmStr(ulong guildId) { return (GetTranslation(guildId, "whoIAm")); }
        public static string UserNotExist(ulong guildId) { return (GetTranslation(guildId, "userNotExist")); }
        public static string Username(ulong guildId) { return (GetTranslation(guildId, "username")); }
        public static string Nickname(ulong guildId) { return (GetTranslation(guildId, "nickname")); }
        public static string AccountCreation(ulong guildId) { return (GetTranslation(guildId, "accountCreation")); }
        public static string GuildJoined(ulong guildId) { return (GetTranslation(guildId, "guildJoined")); }
        public static string Creator(ulong guildId) { return (GetTranslation(guildId, "creator")); }
        public static string Uptime(ulong guildId) { return (GetTranslation(guildId, "uptime")); }
        public static string Website(ulong guildId) { return (GetTranslation(guildId, "website")); }
        public static string OfficialGuild(ulong guildId) { return (GetTranslation(guildId, "officialGuild")); }
        public static string Roles(ulong guildId) { return (GetTranslation(guildId, "roles")); }
        public static string NoRole(ulong guildId) { return (GetTranslation(guildId, "noRole")); }
        public static string LatestVersion(ulong guildId) { return (GetTranslation(guildId, "latestVersion")); }
        public static string NumberGuilds(ulong guildId) { return (GetTranslation(guildId, "numberGuilds")); }

        /// --------------------------- Booru ---------------------------
        public static string FileTooBig(ulong guildId) { return (GetTranslation(guildId, "fileTooBig")); }
        public static string PrepareImage(ulong guildId) { return (GetTranslation(guildId, "prepareImage")); }
        public static string MoreNotTagged(ulong guildId) { return (GetTranslation(guildId, "moreNotTagged")); }
        public static string AnimeFromOriginal(ulong guildId) { return (GetTranslation(guildId, "animeFromOriginal")); }
        public static string AnimeNotTagged(ulong guildId) { return (GetTranslation(guildId, "animeNotTagged")); }
        public static string AnimeFrom(ulong guildId) { return (GetTranslation(guildId, "animeFrom")); }
        public static string AnimeTagUnknowed(ulong guildId) { return (GetTranslation(guildId, "animeTagUnknowed")); }
        public static string CharacterTagUnknowed(ulong guildId) { return (GetTranslation(guildId, "characterTagUnknowed")); }
        public static string CharacterNotTagged(ulong guildId) { return (GetTranslation(guildId, "characterNotTagged")); }
        public static string CharacterIs(ulong guildId) { return (GetTranslation(guildId, "characterIs")); }
        public static string CharacterAre(ulong guildId) { return (GetTranslation(guildId, "characterAre")); }

        /// --------------------------- Games ---------------------------
        public static string RulesShiritori(ulong guildId) { return (GetTranslation(guildId, "rulesShiritori")); }
        public static string RulesKancolle(ulong guildId) { return (GetTranslation(guildId, "rulesKancolle")); }
        public static string RulesBooru(ulong guildId) { return (GetTranslation(guildId, "rulesBooru")); }
        public static string InvalidGameName(ulong guildId) { return (GetTranslation(guildId, "invalidGameName")); }
        public static string GameAlreadyRunning(ulong guildId) { return (GetTranslation(guildId, "gameAlreadyRunning")); }
        public static string TimeoutGame(ulong guildId, string answer) { return (GetTranslation(guildId, "timeoutGame", answer)); }
        public static string NewBestScore(ulong guildId, string lastScore, string newScore) { return (GetTranslation(guildId, "newBestScore", lastScore, newScore)); }
        public static string EqualizedScore(ulong guildId, string score) { return (GetTranslation(guildId, "equalizedScore", score)); }
        public static string DidntBeatScore(ulong guildId, string lastScore, string newScore) { return (GetTranslation(guildId, "didntBeatScore", lastScore, newScore)); }
        public static string ShiritoriNoWord(ulong guildId) { return (GetTranslation(guildId, "shiritoriNoWord")); }
        public static string WaitPlay(ulong guildId) { return (GetTranslation(guildId, "waitPlay")); }
        public static string OnlyHiraganaKatakanaRomaji(ulong guildId) { return (GetTranslation(guildId, "onlyHiraganaKatakanaRomaji")); }
        public static string ShiritoriNotNoun(ulong guildId) { return (GetTranslation(guildId, "shiritoriNotNoun")); }
        public static string ShiritoriDoesntExist(ulong guildId) { return (GetTranslation(guildId, "shiritoriDoesntExist")); }
        public static string ShiritoriMustBegin(ulong guildId, string beginHiragana, string beginRomaji) { return (GetTranslation(guildId, "shiritoriMustBegin", beginHiragana, beginRomaji)); }
        public static string ShiritoriAlreadySaid(ulong guildId) { return (GetTranslation(guildId, "shiritoriAlreadySaid")); }
        public static string ShiritoriEndWithN(ulong guildId) { return (GetTranslation(guildId, "shiritoriEndWithN")); }
        public static string ShiritoriNoMoreWord(ulong guildId) { return (GetTranslation(guildId, "shiritoriNoMoreWord")); }
        public static string ShiritoriSuggestion(ulong guildId, string suggestionHiragana, string suggestionRomaji, string suggestionTranslation) { return (GetTranslation(guildId, "shiritoriSuggestion", suggestionHiragana, suggestionRomaji, suggestionTranslation)); }
        public static string WaitImage(ulong guildId) { return (GetTranslation(guildId, "waitImage")); }
        public static string KancolleGuessDontExist(ulong guildId) { return (GetTranslation(guildId, "kancolleGuessDontExist")); }
        public static string GuessGood(ulong guildId) { return (GetTranslation(guildId, "guessGood")); }
        public static string KancolleGuessBad(ulong guildId, string attempt) { return (GetTranslation(guildId, "kancolleGuessBad", attempt)); }
        public static string WaitImages(ulong guildId) { return (GetTranslation(guildId, "waitImages")); }
        public static string BooruGuessClose(ulong guildId, string attempt) { return (GetTranslation(guildId, "booruGuessClose", attempt)); }
        public static string BooruGuessBad(ulong guildId, string attempt) { return (GetTranslation(guildId, "booruGuessBad", attempt)); }
        public static string LostStr(ulong guildId) { return (GetTranslation(guildId, "lost")); }
        public static string InvalidDifficulty(ulong guildId) { return (GetTranslation(guildId, "invalidDifficulty")); }

        /// --------------------------- Settings ---------------------------
        public static string CreateArchiveStr(ulong guildId, string currTime)
        { return ( GetTranslation(guildId, "createArchive", currTime)); }
        public static string DoneStr(ulong guildId) { return (GetTranslation(guildId, "done")); }
        public static string CopyingFiles(ulong guildId) { return (GetTranslation(guildId, "copyingFiles")); }
        public static string NeedLanguage(ulong guildId) { return (GetTranslation(guildId, "needLanguage")); }
        public static string PrefixRemoved(ulong guildId) { return (GetTranslation(guildId, "prefixRemoved")); }

        /// --------------------------- Linguist ---------------------------
        public static string ToHiraganaHelp(ulong guildId) { return (GetTranslation(guildId, "toHiraganaHelp")); }
        public static string ToRomajiHelp(ulong guildId) { return (GetTranslation(guildId, "toRomajiHelp")); }
        public static string ToKatakanaHelp(ulong guildId) { return (GetTranslation(guildId, "toKatakanaHelp")); }
        public static string JapaneseHelp(ulong guildId) { return (GetTranslation(guildId, "japaneseHelp")); }
        public static string TranslateHelp(ulong guildId) { return (GetTranslation(guildId, "translateHelp")); }
        public static string InvalidLanguage(ulong guildId) { return (GetTranslation(guildId, "invalidLanguage")); }
        public static string GiveJapaneseTranslations(ulong guildId, string word) { return (GetTranslation(guildId, "giveJapaneseTranslations", word)); }
        public static string NoJapaneseTranslation(ulong guildId, string word) { return (GetTranslation(guildId, "noJapaneseTranslation", word)); }
        public static string Meaning(ulong guildId) { return (GetTranslation(guildId, "meaning")); }

        /// --------------------------- KanColle---------------------------
        public static string KancolleHelp(ulong guildId) { return (GetTranslation(guildId, "kancolleHelp")); }
        public static string ShipgirlDontExist(ulong guildId) { return (GetTranslation(guildId, "shipgirlDontExist")); }
        public static string DontDropOnMaps(ulong guildId) { return (GetTranslation(guildId, "dontDropOnMaps")); }
        public static string ShipNotReferencedMap(ulong guildId) { return (GetTranslation(guildId, "shipNotReferencedMap")); }
        public static string ShipNotReferencedConstruction(ulong guildId) { return (GetTranslation(guildId, "shipNotReferencedConstruction")); }
        public static string MapHelp(ulong guildId) { return (GetTranslation(guildId, "mapHelp")); }
        public static string OnlyNormalNodes(ulong guildId) { return (GetTranslation(guildId, "onlyNormalNodes")); }
        public static string OnlyBossNode(ulong guildId) { return (GetTranslation(guildId, "onlyBossNode")); }
        public static string AnyNode(ulong guildId) { return (GetTranslation(guildId, "anyNode")); }
        public static string DefaultNode(ulong guildId) { return (GetTranslation(guildId, "defaultNode")); }
        public static string Rarity(ulong guildId) { return (GetTranslation(guildId, "rarity")); }
        public static string ShipConstruction(ulong guildId) { return (GetTranslation(guildId, "shipConstruction")); }
        public static string Fuel(ulong guildId) { return (GetTranslation(guildId, "fuel")); }
        public static string Ammos(ulong guildId) { return (GetTranslation(guildId, "ammos")); }
        public static string Iron(ulong guildId) { return (GetTranslation(guildId, "iron")); }
        public static string Bauxite(ulong guildId) { return (GetTranslation(guildId, "bauxite")); }
        public static string DevMat(ulong guildId) { return (GetTranslation(guildId, "devMat")); }
        public static string Personality(ulong guildId) { return (GetTranslation(guildId, "personality")); }
        public static string Appearance(ulong guildId) { return (GetTranslation(guildId, "appearance")); }
        public static string SecondRemodel(ulong guildId) { return (GetTranslation(guildId, "secondRemodel")); }
        public static string Trivia(ulong guildId) { return (GetTranslation(guildId, "trivia")); }
        public static string InGame(ulong guildId) { return (GetTranslation(guildId, "inGame")); }
        public static string Historical(ulong guildId) { return (GetTranslation(guildId, "historical")); }

        /// --------------------------- VNDB ---------------------------
        public static string VndbHelp(ulong guildId) { return (GetTranslation(guildId, "vndbHelp")); }
        public static string VndbNotFound(ulong guildId) { return (GetTranslation(guildId, "vndbNotFound")); }
        public static string AvailableEnglish(ulong guildId) { return (GetTranslation(guildId, "availableEnglish")); }
        public static string NotAvailableEnglish(ulong guildId) { return (GetTranslation(guildId, "notAvailableEnglish")); }
        public static string AvailableWindows(ulong guildId) { return (GetTranslation(guildId, "availableWindows")); }
        public static string NotAvailableWindows(ulong guildId) { return (GetTranslation(guildId, "notAvailableWindows")); }
        public static string VndbRating(ulong guildId, string score) { return (GetTranslation(guildId, "vndbRating", score)); }

        /// --------------------------- Code ---------------------------
        public static string IndenteHelp(ulong guildId) { return (GetTranslation(guildId, "indenteHelp")); }

        /// --------------------------- MyAnimeList ---------------------------
        public static string MangaHelp(ulong guildId) { return (GetTranslation(guildId, "mangaHelp")); }
        public static string AnimeHelp(ulong guildId) { return (GetTranslation(guildId, "animeHelp")); }
        public static string MangaNotFound(ulong guildId) { return (GetTranslation(guildId, "mangaNotFound")); }
        public static string AnimeNotFound(ulong guildId) { return (GetTranslation(guildId, "animeNotFound")); }
        public static string AnimeInfos(ulong guildId, string type, string status, string episodes) { return (GetTranslation(guildId, "animeInfos", type, status, episodes)); }
        public static string AnimeScore(ulong guildId, string score) { return (GetTranslation(guildId, "animeScore", score)); }
        public static string Synopsis(ulong guildId) { return (GetTranslation(guildId, "synopsis")); }

        /// --------------------------- Youtube ---------------------------
        public static string YoutubeHelp(ulong guildId) { return (GetTranslation(guildId, "youtubeHelp")); }
        public static string YoutubeNotFound(ulong guildId) { return (GetTranslation(guildId, "youtubeNotFound")); }

        /// --------------------------- Radio ---------------------------
        public static string RadioAlreadyStarted(ulong guildId) { return (GetTranslation(guildId, "radioAlreadyStarted")); }
        public static string RadioNeedChannel(ulong guildId) { return (GetTranslation(guildId, "radioNeedChannel")); }
        public static string RadioNeedArg(ulong guildId) { return (GetTranslation(guildId, "radioNeedArg")); }
        public static string RadioNotStarted(ulong guildId) { return (GetTranslation(guildId, "radioNotStarted")); }
        public static string RadioAlreadyInList(ulong guildId) { return (GetTranslation(guildId, "radioAlreadyInList")); }
        public static string RadioTooMany(ulong guildId) { return (GetTranslation(guildId, "radioTooMany")); }
        public static string RadioNoSong(ulong guildId) { return (GetTranslation(guildId, "radioNoSong")); }
        public static string CantDownload(ulong guildId) { return (GetTranslation(guildId, "cantDownload")); }
        public static string SongSkipped(ulong guildId, string songName) { return (GetTranslation(guildId, "songSkipped", songName)); }
        public static string Current(ulong guildId) { return (GetTranslation(guildId, "current")); }
        public static string Downloading(ulong guildId) { return (GetTranslation(guildId, "downloading")); }
        public static string SongAdded(ulong guildId, string song) { return (GetTranslation(guildId, "songAdded", song)); }

        /// --------------------------- XKCD ---------------------------
        public static string XkcdWrongArg(ulong guildId) { return (GetTranslation(guildId, "xkcdWrongArg")); }
        public static string XkcdWrongId(ulong guildId, int max) { return (GetTranslation(guildId, "xkcdWrongId", max.ToString())); }

        /// --------------------------- Image ---------------------------
        public static string HelpTransparency(ulong guildId) { return (GetTranslation(guildId, "helpTransparency")); }
        public static string HelpConvert(ulong guildId) { return (GetTranslation(guildId, "helpConvert")); }
        public static string InvalidColor(ulong guildId) { return (GetTranslation(guildId, "invalidColor")); }
        public static string HelpRgb(ulong guildId) { return (GetTranslation(guildId, "helpRgb")); }
        public static string InvalidStep(ulong guildId) { return (GetTranslation(guildId, "invalidStep")); }
        public static string InvalidFormat(ulong guildId) { return (GetTranslation(guildId, "invalidFormat")); }

        /// --------------------------- Help ---------------------------
        private static string NoCommandAvailable(ulong guildId) { return (GetTranslation(guildId, "noCommandAvailable")); }
        public static Embed Help(ulong guildId, bool isChanNsfw)
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
                              : (NoCommandAvailable(guildId))));
            embed.AddField(GetTranslation(guildId, "gameModuleName"), GetTranslation(guildId, "gameModuleDescription")
                + ((isChanNsfw) ? (GetTranslation(guildId, "gameModuleDescription2")) : (""))
                + Environment.NewLine + GetTranslation(guildId, "gameModuleDescription3"));
            embed.AddField(GetTranslation(guildId, "googleShortenerModuleName"),
                ((isChanNsfw) ? (GetTranslation(guildId, "googleShortenerModuleDescription"))
                              : (NoCommandAvailable(guildId))));
            embed.AddField(GetTranslation(guildId, "imageModuleName"), GetTranslation(guildId, "imageModuleDescription"));
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