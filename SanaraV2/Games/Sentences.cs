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
using System.Globalization;

namespace SanaraV2.Games
{
    public static class Sentences
    {
        public static string RulesShiritori(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesShiritori")); }
        public static string RulesShiritoriMulti(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesShiritoriMulti")); }
        public static string RulesShiritori2(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesShiritori2")); }
        public static string RulesKancolle(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesKancolle")); }
        public static string RulesBooru(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesBooru")); }
        public static string RulesAnime(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesAnime")); }
        public static string RulesPokemon(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesPokemon")); }
        public static string RulesCharacter(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesCharacter")); }
        public static string RulesTimer(ulong guildId, int timerRef) { return (Translation.GetTranslation(guildId, "rulesTimer", timerRef.ToString())); }
        public static string RulesReset(ulong guildId) { return (Translation.GetTranslation(guildId, "rulesReset")); }
        public static string ResetNone(ulong guildId) { return (Translation.GetTranslation(guildId, "resetNone")); }
        public static string ResetDone(ulong guildId) { return (Translation.GetTranslation(guildId, "resetDone")); }
        public static string InvalidGameName(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidGameName")); }
        public static string GameAlreadyRunning(ulong guildId) { return (Translation.GetTranslation(guildId, "gameAlreadyRunning")); }
        public static string TimeoutGame(ulong guildId) { return (Translation.GetTranslation(guildId, "timeoutGame")); }
        public static string NewBestScore(ulong guildId, string lastScore, string newScore) { return (Translation.GetTranslation(guildId, "newBestScore", lastScore, newScore)); }
        public static string EqualizedScore(ulong guildId, string score) { return (Translation.GetTranslation(guildId, "equalizedScore", score)); }
        public static string DidntBeatScore(ulong guildId, string lastScore, string newScore) { return (Translation.GetTranslation(guildId, "didntBeatScore", lastScore, newScore)); }
        public static string ShiritoriNoWord(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriNoWord")); }
        public static string ShiritoriTooSmall(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriTooSmall")); }
        public static string OnlyHiraganaKatakanaRomaji(ulong guildId) { return (Translation.GetTranslation(guildId, "onlyHiraganaKatakanaRomaji")); }
        public static string ShiritoriNotNoun(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriNotNoun")); }
        public static string ShiritoriDoesntExist(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriDoesntExist")); }
        public static string ShiritoriMustBegin(ulong guildId, string beginHiragana, string beginRomaji) { return (Translation.GetTranslation(guildId, "shiritoriMustBegin", beginHiragana, beginRomaji)); }
        public static string ShiritoriAlreadySaid(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriAlreadySaid")); }
        public static string ShiritoriEndWithN(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriEndWithN")); }
        public static string ShiritoriNoMoreWord(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriNoMoreWord")); }
        public static string ShiritoriSuggestion(ulong guildId, string suggestionHiragana, string suggestionRomaji, string suggestionTranslation) { return (Translation.GetTranslation(guildId, "shiritoriSuggestion", suggestionHiragana, suggestionRomaji, suggestionTranslation)); }
        public static string ShiritoriExplainBegin(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriExplainBegin")); }
        public static string WaitImage(ulong guildId) { return (Translation.GetTranslation(guildId, "waitImage")); }
        public static string guessDontExist(ulong guildId) { return (Translation.GetTranslation(guildId, "guessDontExist")); }
        public static string GuessGood(ulong guildId) { return (Translation.GetTranslation(guildId, "guessGood")); }
        public static string GuessBad(ulong guildId, string attempt) { return (Translation.GetTranslation(guildId, "guessBad", attempt)); }
        public static string BooruGuessClose(ulong guildId, string attempt) { return (Translation.GetTranslation(guildId, "booruGuessClose", attempt)); }
        public static string InvalidGameArgument(ulong guildId) { return (Translation.GetTranslation(guildId, "invalidGameArgument")); }
        public static string NoDictionnary(ulong guildId) { return (Translation.GetTranslation(guildId, "noDictionnary")); }
        public static string ExceptionGame(ulong guildId, string url) { return (Translation.GetTranslation(guildId, "exceptionGame", url)); }
        public static string ExceptionGameStop(ulong guildId) { return (Translation.GetTranslation(guildId, "exceptionGameStop")); }
        public static string AnimeGame(ulong guildId) { return (Translation.GetTranslation(guildId, "animeGame")); }
        public static string BooruGame(ulong guildId) { return (Translation.GetTranslation(guildId, "booruGame")); }
        public static string KancolleGame(ulong guildId) { return (Translation.GetTranslation(guildId, "kancolleGame")); }
        public static string ShiritoriGame(ulong guildId) { return (Translation.GetTranslation(guildId, "shiritoriGame")); }
        public static string AzurLaneGame(ulong guildId) { return (Translation.GetTranslation(guildId, "azurLaneGame")); }
        public static string FateGOGame(ulong guildId) { return (Translation.GetTranslation(guildId, "fateGOGame")); }
        public static string PokemonGame(ulong guildId) { return (Translation.GetTranslation(guildId, "pokemonGame")); }
        public static string ScoreText(ulong guildId, int rank, int total, int score, int bestScore) { return (Translation.GetTranslation(guildId, "scoreText", rank.ToString(), total.ToString(), score.ToString(), bestScore.ToString())); }
        public static string ScoreContributors(ulong guildId) { return (Translation.GetTranslation(guildId, "scoreContributors")); }
        public static string NoScore(ulong guildId) { return (Translation.GetTranslation(guildId, "noScore")); }
        public static string FullNotAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "fullNotAvailable")); }
        public static string SoloNotAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "soloNotAvailable")); }
        public static string MultiNotAvailable(ulong guildId) { return (Translation.GetTranslation(guildId, "multiNotAvailable")); }
        public static string Meaning(ulong guildId) { return (Translation.GetTranslation(guildId, "meaning")); }
        public static string GoodAnswerWas(ulong guildId, string answer) { return (Translation.GetTranslation(guildId, "goodAnswerWas", answer)); }
        public static string ExceptionGameCheck(ulong guildId) { return (Translation.GetTranslation(guildId, "exceptionGameCheck")); }
        public static string Words(ulong guildId) { return (Translation.GetTranslation(guildId, "words")); }
        public static string NotLoaded(ulong guildId) { return (Translation.GetTranslation(guildId, "notLoaded")); }
        public static string AnimeFull(ulong guildId) { return (AnimeGame(guildId) + " (" + Translation.GetTranslation(guildId, "full") + ")"); }
        public static string NotRanked(ulong guildId) { return (Translation.GetTranslation(guildId, "notRanked")); }
        public static string GlobalRanking(ulong guildId, int myRank, int nbServers, float myScore) { return (Translation.GetTranslation(guildId, "globalRanking", myRank.ToString(), nbServers.ToString(), myScore.ToString("0.00", CultureInfo.InvariantCulture))); }
        public static string NoGlobalRanking(ulong guildId) { return (Translation.GetTranslation(guildId, "noGlobalRanking")); }
        public static string LobbyAlreadyIn(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyAlreadyIn")); }
        public static string LobbyAlreadyInThis(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyAlreadyInThis")); }
        public static string LobbyAlreadyOut(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyAlreadyOut")); }
        public static string LobbyEmpty(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyEmpty")); }
        public static string LobbyJoined(ulong guildId, string gameName) { return (Translation.GetTranslation(guildId, "lobbyJoined", gameName)); }
        public static string LobbyLeaved(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyLeaved")); }
        public static string LobbyNoWaiting(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyNoWaiting")); }
        public static string LobbyFull(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyFull")); }
        public static string LobbyAlreadyStarted(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyAlreadyStarted")); }
        public static string LobbySoloJoin(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbySoloJoin")); }
        public static string LobbySoloLeave(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbySoloLeave")); }
        public static string LobbyCreation(ulong guildId, string timerValue) { return (Translation.GetTranslation(guildId, "lobbyCreation", timerValue)); }
        public static string Rules(ulong guildId) { return (Translation.GetTranslation(guildId, "rules")); }
        public static string LobbyNotEnoughPlayerFatal(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyNotEnoughPlayerFatal")); }
        public static string LobbyNotEnoughPlayer(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyNotEnoughPlayer")); }
        public static string Participants(ulong guildId) { return (Translation.GetTranslation(guildId, "participants")); }
        public static string AnnounceTurn(ulong guildId, string name) { return (Translation.GetTranslation(guildId, "annouceTurn", name)); }
        public static string AnnounceTurnError(ulong guildId, string name) { return (Translation.GetTranslation(guildId, "announceTurnError", name)); }
        public static string LobbyLeftChannel(ulong guildId) { return (Translation.GetTranslation(guildId, "lobbyLeftChannel")); }
        public static string YouLost(ulong guildId) { return (Translation.GetTranslation(guildId, "youLost")); }
        public static string WonMulti(ulong guildId, string lastName) { return (Translation.GetTranslation(guildId, "wonMulti", lastName)); }
    }
}
