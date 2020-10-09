namespace SanaraV3.Game
{
    public struct GameSettings
    {
        public GameSettings(MultiplayerLobby lobby)
        {
            Lobby = lobby;
        }

        public MultiplayerLobby Lobby { get; }
    }
}
