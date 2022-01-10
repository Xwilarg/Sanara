namespace Sanara.Game
{
    public struct GameSettings
    {
        public GameSettings(Lobby lobby, bool isCustomGame)
        {
            Lobby = lobby;
            IsCustomGame = isCustomGame;
        }

        public Lobby Lobby { get; }
        public bool IsCustomGame { get; }
    }
}
