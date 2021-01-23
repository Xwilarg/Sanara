namespace SanaraV3.Game
{
    public struct GameSettings
    {
        public GameSettings(MultiplayerLobby lobby, bool doesSaveScore)
        {
            Lobby = lobby;
            DoesSaveScore = doesSaveScore;
        }

        public MultiplayerLobby Lobby { get; }
        public bool DoesSaveScore { get; }
    }
}
