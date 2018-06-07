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
        public static string onlyMasterStr(ulong guildId) { return (GetTranslation(guildId, "onlyMaster")); }
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
        public static string needLanguage = "Please specify the language you want me to speak in (fr or en)";

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
        private readonly static string noCommandAvailable = "There is no command available for this module";
        public static Embed help(bool isChanNsfw)
        {
            EmbedBuilder embed = new EmbedBuilder
            {
                Title = "Help",
                Color = Color.Purple
            };
            embed.AddField("Anime/Manga Module",
                "**Anime [name]:** Give informations about an anime" + Environment.NewLine +
                "**Manga [code]:** Give informations about a manga");
            embed.AddField("Booru Module",
                "**Safebooru [tags]:** Request a random image from Safebooru (only SFW images)"
                + ((isChanNsfw) ? (Environment.NewLine + "**Konachan [tags]:** Request a random image from Konachan (only wallpapers)"
                                 + Environment.NewLine + "**Gelbooru [tags]:** Request a random image from Gelbooru (no particular rules)"
                                 + Environment.NewLine + "**Rule34 [tags]:** Request a random image from Rule34 (mostly weird images)"
                                 + Environment.NewLine + "**E621 [tags]:** Request a random image from E621 (mostly furries)") : ("")));
            embed.AddField("Code Module",
                "**Indente [code]:** Indente the code given");
            embed.AddField("Communication Module",
                "**Infos [user]:** Give informations about an user" + Environment.NewLine +
                "**BotInfos:** Give informations about the bot");
            embed.AddField("Debug Module",
                "**Print debug:** Give general informations about the bot");
            embed.AddField("Doujinshi Module",
                ((isChanNsfw) ? ("**Doujinshi [tags]:** Request a doujinshi from Nhentai")
                              : (noCommandAvailable)));
            embed.AddField("Game Module",
                "**Play shiritori:** Play the shiritori game (you need to find a japanese word beginning by the ending of the previous one)" + Environment.NewLine +
                "**Play kancolle:** Play the KanColle guess game (you need to identify shipgirls by an image)"
                + ((isChanNsfw) ? (Environment.NewLine + "**Play booru:** Play the booru game (you need to identify tag of Gelbooru images)") : ("")));
            embed.AddField("Google Shortener Module",
                ((isChanNsfw) ? ("**Random url:** Give a random URL from goo.gl")
                              : (noCommandAvailable)));
            embed.AddField("Kantai Collection Module",
                "**Kancolle [shipgirl]:** Give informations about a shipgirl from KanColle wikia" + Environment.NewLine +
                "**Drop [shipgirl]:** Give informations about where you can find a shipgirl in Kancolle" + Environment.NewLine +
                "**Map [world] [level]:** Give informations about a map in Kancolle");
            embed.AddField("Linguistic Module",
                "**Hiragana [word]:** Transcript a word to hiragana" + Environment.NewLine +
                "**Katakana [word]:** Transcript a word to katakana" + Environment.NewLine +
                "**Romaji [word]:** Transcript a word to romaji" + Environment.NewLine +
                "**Definition [word]:** Translate a word in both japanese and english" + Environment.NewLine +
                "**Translation [language] [sentence]:** Translate a sentence in the language given" + Environment.NewLine);
            embed.AddField("Radio Module",
                "**Radio launch:** Make the bot join you in a vocal channel" + Environment.NewLine +
                "**Radio add [YouTube url/keywords]:** Add a song to the playlist" + Environment.NewLine +
                "**Radio playlist:** Display current playlist" + Environment.NewLine +
                "**Radio skip:** Skip the song currently played" + Environment.NewLine +
                "**Radio stop:** Stop the radio and leave the vocal channel");
            embed.AddField("Settings Module", noCommandAvailable);
            embed.AddField("Visual Novel Module",
                "**Vn [visual novel]:** Give informations about a visual novel");
            embed.AddField("Xkcd Module",
                "**Xkcd [(optional) comic id]:** Give a random xkcd comic");
            embed.AddField("YouTube Module",
                "**Youtube [keywords]:** Give a YouTube video given some keywords");
            return (embed.Build());
        }
    }
}