namespace Sanara.Game
{
    public struct GameSettings
    {
        public GameSettings(MultiplayerLobby lobby, bool isCustomGame)
        {
            Lobby = lobby;
            IsCustomGame = isCustomGame;
        }

        public MultiplayerLobby Lobby { get; }
        public bool IsCustomGame { get; }
    }
}
