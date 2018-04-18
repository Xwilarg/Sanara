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

namespace SanaraV2
{
    public static class Sentences
    {
        /// --------------------------- ID ---------------------------
        public readonly static ulong idPikyu = 352216646267437059; // Bot that collect informations for statistics
        public readonly static ulong myId = 329664361016721408;
        public readonly static ulong ownerId = 144851584478740481;

        /// --------------------------- General ---------------------------
        public static string introductionError(string userId, string userName)
        {
            return $"<@{ownerId}> The user " + userId + " named " + userName + " is corrupted in my database." + Environment.NewLine +
                                "Please check that manually.";
        }
        public readonly static string onlyMasterStr = "I'm sorry but I only can receive that kind of order from Zirk.";
        public readonly static string needAttachFile = "I need to be allowed to attach files for this.";
        public readonly static string chanIsNotNsfw = "I can't take the risk to post NSFW content here. Please ask again in a NSFW channel.";
        public static string nothingAfterXIterations(int nbIterations) { return ("I didn't find anything after " + nbIterations.ToString() + " iterations."); }
        public static string tooManyRequests(string apiName) { return ("Seam like I exceed the number of requests on the " + apiName + " API. You should wait a bit before retrying."); }
        public static string tagsNotFound(string[] tags)
        {
            if (tags.Length == 1)
                return ("I didn't find anything with the tag '" + tags + "'.");
            string finalStr = "";
            for (int i = 0; i < tags.Length - 1; i++)
                finalStr += "'" + tags[i] + "', ";
            return ("I didn't find anything with the tag '" + finalStr.Substring(0, finalStr.Length - 2) + " and '" + tags[tags.Length - 1] + "'.");
        }
        public readonly static string noCorrespondingGuild = "I'm not in any guild with this name.";
        public readonly static string betaFeature = "I'm sorry but this feature is currently in a closed testing phase.";

        /// --------------------------- Communication ---------------------------
        public readonly static string introductionMsg = "Hi, my name is Sanara" + Environment.NewLine + "Nice to meet you everyone!";
        public readonly static string hiStr = "Hi~";
        public readonly static string whoIAmStr = "My name is Sanaya Miyuki(差成夜 深雪) but just call me Sanara." + Environment.NewLine
                                                + "I'll do my best to learn new things to help you.";

        /// --------------------------- Booru ---------------------------
        public readonly static string fileTooBig = "I wasn't able to post the file I found since it was bigger than 8 MB.";
        public readonly static string prepareImage = "Please wait while I'm looking for what you requested.";

        /// --------------------------- Games ---------------------------
        public readonly static string rulesShiritori = "I'll give you a word in japanese, for example りゅう (ryuu) and you'll have to find another word beginning by the last syllabe." + Environment.NewLine
            + "(In this example, a word starting by う (u), for example うさぎ (usagi).)";
        public readonly static string rulesKancolle = "I'll post an image of a shipgirl, you have to give her name.";
        public readonly static string invalidGameName = "I don't know any game with this name.";
        public readonly static string gameAlreadyrunning = "A game is already running on this channel.";

        /// --------------------------- Settings ---------------------------
        public static string createArchiveStr(string currTime)
        { return ($"I created the new archive {currTime} to save my datas, thanks!"); }
        public readonly static string doneStr = "Done~";
        public static string copyingFiles = "Please let me some time so I can copy all my files.";

        /// --------------------------- Linguist ---------------------------
        public readonly static string toHiraganaHelp = "Please give the word you want me to transcript in hiragana.";
        public readonly static string toRomajiHelp = "Please give the word you want me to transcript in romaji.";
        public readonly static string toKatakanaHelp = "Please give the word you want me to transcript in katakana.";
        public readonly static string japaneseHelp = "Please give the word you want me to translate.";
        public readonly static string translateHelp = "Please give the language you want me to translate in followed by a sentence.";
        public readonly static string invalidLanguage = "I don't know the language you gave.";

        /// --------------------------- KanColle---------------------------
        public readonly static string kancolleHelp = "Please give the shipgirl you want informations about.";
        public readonly static string shipgirlDontExist = "I didn't find any shipgirl with this name.";

        /// --------------------------- VNDB ---------------------------
        public readonly static string vndbHelp = "Please give the visual novel you want informations about.";
        public readonly static string vndbNotFound = "I didn't find any visual novel with this name.";

        /// --------------------------- Code ---------------------------
        public readonly static string indenteHelp = "Please give the code you want to indente.";
        public readonly static string codeHelp = "Please give the code you want to launch.";

        /// --------------------------- MyAnimeList ---------------------------
        public readonly static string mangaHelp = "Please give the manga you want informations about.";
        public readonly static string animeHelp = "Please give the anime you want informations about.";
        public readonly static string mangaNotFound = "I didn't find any manga with this name.";
        public readonly static string animeNotFound = "I didn't find any anime with this name.";

        /// --------------------------- Youtube ---------------------------
        public readonly static string youtubeHelp = "Please give the keywords about the video you want.";
        public readonly static string youtubeNotFound = "I didn't find any video with this keyword.";
        public readonly static string youtubeBadVideo = "The first thing I found wasn't a video.";

        /// --------------------------- Radio ---------------------------
        public readonly static string radioAlreadyStarted = "A radio is already started on this guild.";
        public readonly static string radioNeedChannel = "Please join a voice channel.";

        /// --------------------------- Help ---------------------------
        public static string help(bool isChanNsfw)
        {
            string finalStr = "Hiragana [word]: Transcript a word to hiragana" + Environment.NewLine
                            + "Katakana [word]: Transcript a word to katakana" + Environment.NewLine
                            + "Romaji [word]: Transcript a word to romaji" + Environment.NewLine
                            + "Definition [word]: Translate a word in both japanese and english" + Environment.NewLine
                            + "Translation [language] [sentence]: Translate a sentence in the language given" + Environment.NewLine
                            + "Safebooru [tags]: Request a random image from Safebooru (only SFW images)" + Environment.NewLine
                            + "Vn [visual novel]: Give informations about a visual novel" + Environment.NewLine
                            + "Kancolle [shipgirl]: Give informations about a shipgirl from KanColle wikia" + Environment.NewLine
                            + "Indente [code]: Indente the code given" + Environment.NewLine
                            + "Anime [name]: Give informations about an anime" + Environment.NewLine
                            + "Manga [code]: Give informations about a manga" + Environment.NewLine
                            + "Play [shiritori/kancolle]: Play a game" + Environment.NewLine
                            + "Youtube [keywords]: Give a YouTube video given some keywords" + Environment.NewLine;
            if (isChanNsfw)
                finalStr += "Konachan [tags]: Request a random image from Konachan (only wallpapers)" + Environment.NewLine
                          + "Gelbooru [tags]: Request a random image from Gelbooru (no particular rules)" + Environment.NewLine
                          + "Rule34 [tags]: Request a random image from Safebooru (mostly weird images)" + Environment.NewLine
                          + "Doujinshi [tags]: Request a doujinshi from Nhentai" + Environment.NewLine
                          + "Random url: Give a random URL from goo.gl" + Environment.NewLine;
            else
                finalStr += "(Ask again in a NSFW channel for a full list of features.)";
            return (finalStr);
        }
    }
}