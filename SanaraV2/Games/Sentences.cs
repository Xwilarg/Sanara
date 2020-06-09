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
using System.Globalization;

namespace SanaraV2.Games
{
    public static class Sentences
    {
        public static string RulesShiritori(IGuild guild) { return (Translation.GetTranslation(guild, "rulesShiritori")); }
        public static string RulesShiritoriMulti(IGuild guild) { return (Translation.GetTranslation(guild, "rulesShiritoriMulti")); }
        public static string RulesShiritori2(IGuild guild) { return (Translation.GetTranslation(guild, "rulesShiritori2")); }
        public static string RulesKancolle(IGuild guild) { return (Translation.GetTranslation(guild, "rulesKancolle")); }
        public static string RulesArknights(IGuild guild) { return (Translation.GetTranslation(guild, "rulesArknights")); }
        public static string RulesGirlsFrontline(IGuild guild) { return (Translation.GetTranslation(guild, "rulesGirlsFrontline")); }
        public static string RulesBooru(IGuild guild) { return (Translation.GetTranslation(guild, "rulesBooru")); }
        public static string RulesAnime(IGuild guild) { return (Translation.GetTranslation(guild, "rulesAnime")); }
        public static string RulesPokemon(IGuild guild) { return (Translation.GetTranslation(guild, "rulesPokemon")); }
        public static string RulesDestinyChild(IGuild guild) { return (Translation.GetTranslation(guild, "rulesDestinyChild")); }
        public static string RulesReversi(IGuild guild) { return (Translation.GetTranslation(guild, "rulesReversi")); }
        public static string RulesCharacter(IGuild guild) { return (Translation.GetTranslation(guild, "rulesCharacter")); }
        public static string RulesTimer(IGuild guild, int timerRef) { return (Translation.GetTranslation(guild, "rulesTimer", timerRef.ToString())); }
        public static string RulesReset(IGuild guild) { return (Translation.GetTranslation(guild, "rulesReset")); }
        public static string RulesMultiElimination(IGuild guild) { return (Translation.GetTranslation(guild, "rulesMultiElimination")); }
        public static string RulesMultiBestOf(IGuild guild, int nbTries, int nbQuestions) { return (Translation.GetTranslation(guild, "rulesMultiBestOf", nbTries.ToString(), nbQuestions.ToString())); }
        public static string ResetNone(IGuild guild) { return (Translation.GetTranslation(guild, "resetNone")); }
        public static string ResetDone(IGuild guild) { return (Translation.GetTranslation(guild, "resetDone")); }
        public static string InvalidGameName(IGuild guild) { return (Translation.GetTranslation(guild, "invalidGameName")); }
        public static string GameAlreadyRunning(IGuild guild) { return (Translation.GetTranslation(guild, "gameAlreadyRunning")); }
        public static string TimeoutGame(IGuild guild) { return (Translation.GetTranslation(guild, "timeoutGame")); }
        public static string NewBestScore(IGuild guild, string lastScore, string newScore) { return (Translation.GetTranslation(guild, "newBestScore", lastScore, newScore)); }
        public static string EqualizedScore(IGuild guild, string score) { return (Translation.GetTranslation(guild, "equalizedScore", score)); }
        public static string DidntBeatScore(IGuild guild, string lastScore, string newScore) { return (Translation.GetTranslation(guild, "didntBeatScore", lastScore, newScore)); }
        public static string ShiritoriNoWord(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriNoWord")); }
        public static string ShiritoriTooSmall(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriTooSmall")); }
        public static string OnlyHiraganaKatakanaRomaji(IGuild guild) { return (Translation.GetTranslation(guild, "onlyHiraganaKatakanaRomaji")); }
        public static string ShiritoriNotNoun(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriNotNoun")); }
        public static string ShiritoriDoesntExist(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriDoesntExist")); }
        public static string ShiritoriMustBegin(IGuild guild, string beginHiragana, string beginRomaji) { return (Translation.GetTranslation(guild, "shiritoriMustBegin", beginHiragana, beginRomaji)); }
        public static string ShiritoriAlreadySaid(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriAlreadySaid")); }
        public static string ShiritoriEndWithN(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriEndWithN")); }
        public static string ShiritoriNoMoreWord(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriNoMoreWord")); }
        public static string ShiritoriSuggestion(IGuild guild, string suggestionHiragana, string suggestionRomaji, string suggestionTranslation) { return (Translation.GetTranslation(guild, "shiritoriSuggestion", suggestionHiragana, suggestionRomaji, suggestionTranslation)); }
        public static string ShiritoriExplainBegin(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriExplainBegin")); }
        public static string ReversiFinalScore(IGuild guild) { return (Translation.GetTranslation(guild, "reversiFinalScore")); }
        public static string ReversiInvalidMove(IGuild guild) { return (Translation.GetTranslation(guild, "reversiInvalidMove")); }
        public static string ReversiInvalidPos(IGuild guild) { return (Translation.GetTranslation(guild, "reversiInvalidPos")); }
        public static string ReversiGameEnded(IGuild guild) { return (Translation.GetTranslation(guild, "reversiGameEnded")); }
        public static string ReversiCantPlay(IGuild guild, string name) { return (Translation.GetTranslation(guild, "reversiCantPlay", name)); }
        public static string ReversiIntro(IGuild guild, string name) { return (Translation.GetTranslation(guild, "reversiIntro", name)); }
        public static string WaitImage(IGuild guild) { return (Translation.GetTranslation(guild, "waitImage")); }
        public static string guessDontExist(IGuild guild) { return (Translation.GetTranslation(guild, "guessDontExist")); }
        public static string GuessGood(IGuild guild) { return (Translation.GetTranslation(guild, "guessGood")); }
        public static string GuessBad(IGuild guild, string attempt) { return (Translation.GetTranslation(guild, "guessBad", attempt)); }
        public static string BooruGuessClose(IGuild guild, string attempt) { return (Translation.GetTranslation(guild, "booruGuessClose", attempt)); }
        public static string InvalidGameArgument(IGuild guild) { return (Translation.GetTranslation(guild, "invalidGameArgument")); }
        public static string NoDictionnary(IGuild guild) { return (Translation.GetTranslation(guild, "noDictionnary")); }
        public static string ExceptionGame(IGuild guild, string url) { return (Translation.GetTranslation(guild, "exceptionGame", url)); }
        public static string ExceptionGameStop(IGuild guild) { return (Translation.GetTranslation(guild, "exceptionGameStop")); }
        public static string AnimeGame(IGuild guild) { return (Translation.GetTranslation(guild, "animeGame")); }
        public static string BooruGame(IGuild guild) { return (Translation.GetTranslation(guild, "booruGame")); }
        public static string ArknightsGame(IGuild guild) { return (Translation.GetTranslation(guild, "arknightsGame")); }
        public static string KancolleGame(IGuild guild) { return (Translation.GetTranslation(guild, "kancolleGame")); }
        public static string GirlsFrontlineGame(IGuild guild) { return (Translation.GetTranslation(guild, "girlsFrontlineGame")); }
        public static string DestinyChildGame(IGuild guild) { return (Translation.GetTranslation(guild, "destinyChildGame")); }
        public static string ShiritoriGame(IGuild guild) { return (Translation.GetTranslation(guild, "shiritoriGame")); }
        public static string AzurLaneGame(IGuild guild) { return (Translation.GetTranslation(guild, "azurLaneGame")); }
        public static string FateGOGame(IGuild guild) { return (Translation.GetTranslation(guild, "fateGOGame")); }
        public static string PokemonGame(IGuild guild) { return (Translation.GetTranslation(guild, "pokemonGame")); }
        public static string ReversiGame(IGuild guild) { return (Translation.GetTranslation(guild, "reversiGame")); }
        public static string ScoreText(IGuild guild, int rank, int total, int score, int bestScore) { return (Translation.GetTranslation(guild, "scoreText", rank.ToString(), total.ToString(), score.ToString(), bestScore.ToString())); }
        public static string ScoreContributors(IGuild guild) { return (Translation.GetTranslation(guild, "scoreContributors")); }
        public static string NoScore(IGuild guild) { return (Translation.GetTranslation(guild, "noScore")); }
        public static string FullNotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "fullNotAvailable")); }
        public static string SendImageNotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "sendImageNotAvailable")); }
        public static string CropNotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "cropNotAvailable")); }
        public static string ShadowNotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "shadowNotAvailable")); }
        public static string SoloNotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "soloNotAvailable")); }
        public static string MultiNotAvailable(IGuild guild) { return (Translation.GetTranslation(guild, "multiNotAvailable")); }
        public static string Meaning(IGuild guild) { return (Translation.GetTranslation(guild, "meaning")); }
        public static string GoodAnswerWas(IGuild guild, string answer) { return (Translation.GetTranslation(guild, "goodAnswerWas", answer)); }
        public static string ExceptionGameCheck(IGuild guild) { return (Translation.GetTranslation(guild, "exceptionGameCheck")); }
        public static string Words(IGuild guild) { return (Translation.GetTranslation(guild, "words")); }
        public static string NotLoaded(IGuild guild) { return (Translation.GetTranslation(guild, "notLoaded")); }
        public static string AnimeFull(IGuild guild) { return (AnimeGame(guild) + " (" + Translation.GetTranslation(guild, "full") + ")"); }
        public static string NotRanked(IGuild guild) { return (Translation.GetTranslation(guild, "notRanked")); }
        public static string GlobalRanking(IGuild guild, int myRank, int nbServers, float myScore) { return (Translation.GetTranslation(guild, "globalRanking", myRank.ToString(), nbServers.ToString(), myScore.ToString("0.00", CultureInfo.InvariantCulture))); }
        public static string NoGlobalRanking(IGuild guild) { return (Translation.GetTranslation(guild, "noGlobalRanking")); }
        public static string LobbyAlreadyIn(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyAlreadyIn")); }
        public static string LobbyAlreadyInThis(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyAlreadyInThis")); }
        public static string LobbyAlreadyOut(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyAlreadyOut")); }
        public static string LobbyEmpty(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyEmpty")); }
        public static string LobbyJoined(IGuild guild, string gameName) { return (Translation.GetTranslation(guild, "lobbyJoined", gameName)); }
        public static string LobbyLeaved(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyLeaved")); }
        public static string LobbyNoWaiting(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyNoWaiting")); }
        public static string LobbyFull(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyFull")); }
        public static string LobbyAlreadyStarted(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyAlreadyStarted")); }
        public static string LobbySoloJoin(IGuild guild) { return (Translation.GetTranslation(guild, "lobbySoloJoin")); }
        public static string LobbySoloLeave(IGuild guild) { return (Translation.GetTranslation(guild, "lobbySoloLeave")); }
        public static string LobbyCreation(IGuild guild, string timerValue) { return (Translation.GetTranslation(guild, "lobbyCreation", timerValue)); }
        public static string Rules(IGuild guild) { return (Translation.GetTranslation(guild, "rules")); }
        public static string MultiplayerRules(IGuild guild) { return (Translation.GetTranslation(guild, "multiplayerRules")); }
        public static string LobbyNotEnoughPlayerFatal(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyNotEnoughPlayerFatal")); }
        public static string LobbyNotEnoughPlayer(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyNotEnoughPlayer")); }
        public static string Participants(IGuild guild) { return (Translation.GetTranslation(guild, "participants")); }
        public static string AnnounceTurn(IGuild guild, string name) { return (Translation.GetTranslation(guild, "announceTurn", name)); }
        public static string AnnounceTurnError(IGuild guild, string name) { return (Translation.GetTranslation(guild, "announceTurnError", name)); }
        public static string LobbyLeftChannel(IGuild guild) { return (Translation.GetTranslation(guild, "lobbyLeftChannel")); }
        public static string YouLost(IGuild guild) { return (Translation.GetTranslation(guild, "youLost")); }
        public static string WonMulti(IGuild guild, string lastName) { return (Translation.GetTranslation(guild, "wonMulti", lastName)); }
        public static string DictionnaryEmpty(IGuild guild) { return (Translation.GetTranslation(guild, "dictionnaryEmpty")); }
        public static string TurnsRemaining(IGuild guild, int nb, string name) { return (Translation.GetTranslation(guild, "turnsRemaning", nb.ToString(), name)); }
        public static string CurrentScore(IGuild guild) { return (Translation.GetTranslation(guild, "currentScore")); }
        public static string TimeOut(IGuild guild) { return (Translation.GetTranslation(guild, "timeOut")); }
        public static string Draw(IGuild guild) { return (Translation.GetTranslation(guild, "draw")); }
        public static string OutOfTries(IGuild guild) { return (Translation.GetTranslation(guild, "outOfTries")); }
    }
}
